<template>
  <div class="artist-page">
    <!-- Шапка артиста -->
    <div class="artist-background" :style="{backgroundImage: `url('${artist.backgroundPhotoArtist || defaultBackground}')`}"></div>
    <div class="artist-header">
      <div class="artist-photo-container">
        <div class="profile-photo-wrapper">
          <img 
            :src="artist.profilePhotoArtist || defaultAvatar" 
            alt="Фото артиста"
            class="profile-photo"
            @error="handleImageError"
          >
        </div>
        <h1 class="artist-name">{{ artist.nameArtist }}</h1>
        <div class="genres-section">
          <div class="genres-list">
            <span v-for="(genre, gIndex) in artist.idGenres" :key="gIndex">
              {{ genre.nameGenre }}
              <span v-if="gIndex < artist.idGenres.length - 1"> | </span>
            </span>
          </div>
        </div>
      </div>
      
      <div class="artist-details">
        <p class="artist-description">{{ artist.descriptionArtist || 'Нет описания' }}</p>
        <div class="concerts-section">
          <h2>Концерты</h2>
          <div class="concerts-info">
          <div class="date-range" v-if="upcomingConcerts.length > 0">
            <i class="calendar-icon"></i>
            {{ formatDate(upcomingConcerts[0]?.dateStartConcert) }} - {{ formatDate(upcomingConcerts[upcomingConcerts.length-1]?.dateStartConcert) }}
          </div>
          <div class="concerts-count">
            {{ upcomingConcerts.length }} предстоящих {{ pluralize(upcomingConcerts.length, ['концерт', 'концерта', 'концертов']) }}
          </div>
        </div>
          <!-- Вкладки -->
          <div class="concerts-tabs">
            <button 
              v-for="tab in tabs" 
              :key="tab.value"
              @click="activeTab = tab.value"
              :class="{ 'active': activeTab === tab.value }"
            >
              {{ tab.label }} ({{ getConcertsCount(tab.value) }})
            </button>
            <button 
              @click="activeTab = 'reviews'"
              :class="{ 'active': activeTab === 'reviews' }"
            >
              Отзывы ({{ artistReviews.length }})
            </button>
          </div>
          
          <!-- Список карточек концертов -->
          <div class="concerts-grid" v-if="activeTab !== 'reviews' && filteredConcerts.length > 0">
            <div 
              class="concert-card-artist" 
              v-for="(concert, index) in filteredConcerts" 
              :key="index"
              :class="{
                'cancelled': concert.statusConcert === 'Событие отменено',
                'past': isPastConcert(concert)
              }"
            >
              <div class="concert-date">
                <div class="concert-day">{{ getDay(concert.dateStartConcert) }}</div>
                <div class="concert-month">{{ getMonth(concert.dateStartConcert) }}</div>
                <div class="concert-weekday">{{ getWeekday(concert.dateStartConcert) }}</div>
                <div class="concert-time">{{ formatTime(concert.timeStartConcert) }}</div>
              </div>
              <div class="concert-location">
                <div class="concert-city">{{ concert.idHallNavigation?.cityHall || 'Город не указан' }}</div>
                <div class="concert-venue">{{ concert.idHallNavigation?.nameHall || 'Площадка не указана' }}</div>
                <div class="concert-age-limit">Возраст: {{ concert.ageLimitConcert || 16 }}+</div>
                <div 
                  v-if="concert.statusConcert === 'Событие отменено'" 
                  class="concert-status"
                >
                  Отменено
                </div>
              </div>
              <div 
                class="concert-availability"
                v-if="!isPastConcert(concert) && concert.statusConcert !== 'Событие отменено'"
              >
                <router-link 
                  :to="{ 
                    name: 'ConcertView', 
                    params: { 
                      idArtist: artist.idArtist,
                      idConcert: concert.idConcert 
                    }
                  }"
                >
                  <button class="info-button">Подробнее</button>
                </router-link>
              </div>
            </div>
          </div>
          <div v-if="activeTab !== 'reviews' && filteredConcerts.length == 0" class="no-concerts">
            Нет концертов в этой категории
          </div>
          <div class="reviews-section" v-if="activeTab === 'reviews'">
            <div v-if="reviewsLoading" class="loading">Загрузка отзывов...</div>
            <div v-else>
              <div v-if="artistReviews.length === 0" class="no-reviews">
                Пока нет отзывов на этого артиста
              </div>
              <div v-else class="reviews-list">
                <div v-for="review in artistReviews" :key="review.idReview" class="review-card">
                  <div class="review-header">
                    <div class="review-user">
                      {{ review.idUserNavigation.firstNameUser }} {{ review.idUserNavigation.lastNameUser }}
                    </div>
                    <div class="review-rating" v-if="review.ratingReview">
                      <span v-for="i in 5" :key="i" :class="{ 'filled': i <= review.ratingReview }">★</span>
                    </div>
                    <div class="review-date">
                      {{ formatDate(review.dateReview) }}
                    </div>
                  </div>
                  <div class="review-text">
                    {{ review.textReview }}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import ApiService from '@/assets/services/apiService';
import '@/assets/styleAssets/styleArtist.css'

export default {
  name: 'ArtistView',
  props: {
    idArtist: {
      type: [String, Number],
      required: true
    }
  },
  data() {
    return {
      loading: true,
      artist: {
        nameArtist: '',
        descriptionArtist: '',
        profilePhotoArtist: '',
        backgroundPhotoArtist: '',
        idGenres: [],
        concerts: []
      },
      defaultAvatar: '/default-avatar.jpg',
      defaultBackground: '/default-background.jpg',
      activeTab: 'upcoming',
      tabs: [
        { value: 'upcoming', label: 'Предстоящие' },
        { value: 'past', label: 'Прошедшие' },
        { value: 'cancelled', label: 'Отмененные' }
      ],
      artistReviews: [],
      reviewsLoading: false
    };
  },
  computed: {
    upcomingConcerts() {
      const now = new Date();
      return this.artist.concerts.filter(concert => {
        if (concert.statusConcert === 'Событие отменено') return false;
        return new Date(concert.dateStartConcert) > now;
      });
    },
    filteredConcerts() {
      const now = new Date();
      
      switch(this.activeTab) {
        case 'upcoming':
          return this.artist.concerts.filter(concert => {
            if (concert.statusConcert === 'Событие отменено') return false;
            return new Date(concert.dateStartConcert) > now;
          });
          
        case 'past':
          return this.artist.concerts.filter(concert => {
            if (concert.statusConcert === 'Событие отменено') return false;
            return new Date(concert.dateStartConcert) <= now;
          });
          
        case 'cancelled':
          return this.artist.concerts.filter(concert => 
            concert.statusConcert === 'Событие отменено'
          );
          
        default:
          return [];
      }
    }
  },
  async created() {
    await this.fetchArtistData();
    await this.loadArtistReviews();
  },
  methods: {
    async loadArtistReviews() {
      this.reviewsLoading = true;
      try {
        const response = await ApiService.getArtistReviews(this.idArtist);
        this.artistReviews = response || [];
      } catch (error) {
        console.error('Ошибка загрузки отзывов:', error);
        this.artistReviews = [];
      } finally {
        this.reviewsLoading = false;
      }
    },

    async fetchArtistData() {
      try {
        this.loading = true;
        const response = await ApiService.getArtistById(this.idArtist);
        
        if (response) {
          this.artist = {
            ...response,
            idGenres: response.idGenres || [],
            concerts: response.concerts ? response.concerts.map(c => ({
              ...c,
              timeStartConcert: c.timeStartConcert || '00:00:00',
              ageLimitConcert: c.ageLimitConcert || 16
            })) : [],
            profilePhotoArtist: response.profilePhotoArtist || this.defaultAvatar,
            backgroundPhotoArtist: response.backgroundPhotoArtist || this.defaultBackground
          };
          
          this.artist.concerts.sort((a, b) => 
            new Date(a.dateStartConcert) - new Date(b.dateStartConcert)
          );
        }
      } catch (error) {
        console.error('Ошибка загрузки данных:', error);
      } finally {
        this.loading = false;
      }
    },
    isPastConcert(concert) {
      if (concert.statusConcert === 'Событие отменено') return false;
      return new Date(concert.dateStartConcert) <= new Date();
    },
    
    getConcertsCount(tab) {
      switch(tab) {
        case 'upcoming': 
          return this.artist.concerts.filter(c => 
            !this.isPastConcert(c) && c.statusConcert !== 'Событие отменено'
          ).length;
          
        case 'past': 
          return this.artist.concerts.filter(c => 
            this.isPastConcert(c) && c.statusConcert !== 'Событие отменено'
          ).length;
          
        case 'cancelled': 
          return this.artist.concerts.filter(c => 
            c.statusConcert === 'Событие отменено'
          ).length;
          
        default: return 0;
      }
    },
    handleImageError(event) {
      if (event.target.classList.contains('artist-photo')) {
        event.target.src = this.defaultAvatar;
      } else if (event.target.classList.contains('artist-background')) {
        event.target.style.backgroundImage = `url('${this.defaultBackground}')`;
      }
    },
    
    formatDate(dateString) {
      if (!dateString) return '';
      const options = { day: '2-digit', month: '2-digit', year: 'numeric' };
      return new Date(dateString).toLocaleDateString('ru-RU', options);
    },
    
    getDay(dateString) {
      if (!dateString) return '';
      return new Date(dateString).getDate();
    },
    
    getMonth(dateString) {
      if (!dateString) return '';
      const months = ['Янв', 'Фев', 'Мар', 'Апр', 'Май', 'Июн', 'Июл', 'Авг', 'Сен', 'Окт', 'Ноя', 'Дек'];
      return months[new Date(dateString).getMonth()];
    },
    
    getWeekday(dateString) {
      if (!dateString) return '';
      const weekdays = ['Вс', 'Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб'];
      return weekdays[new Date(dateString).getDay()];
    },
    
    formatTime(timeString) {
      if (!timeString) return '--:--';
      return timeString.slice(0, 5);
    },
    
    getStatusText(status) {
      const statusMap = {
        'Cобытие планируется': 'Планируется',
        'Идут продажи': 'Билеты в продаже',
        'Концерт отменен': 'Отменен',
        'Концерт завершен': 'Завершен'
      };
      return statusMap[status] || status;
    },
    
    pluralize(number, titles) {
      const cases = [2, 0, 1, 1, 1, 2];
      return titles[
        number % 100 > 4 && number % 100 < 20
          ? 2
          : cases[number % 10 < 5 ? number % 10 : 5]
      ];
    }
  }
};
</script>

<style scoped>
.reviews-section {
  margin-top: 2rem;
}

.reviews-list {
  display: grid;
  gap: 1.5rem;
}

.review-card {
  background: white;
  border-radius: 8px;
  padding: 1.5rem;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.review-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.review-user {
  font-weight: 600;
}

.review-rating {
  color: #ffc107;
}

.review-rating .filled {
  color: #ffc107;
}

.review-date {
  color: #888;
  font-size: 0.9rem;
}

.review-text {
  line-height: 1.5;
}

.no-reviews {
  padding: 2rem;
  text-align: center;
  color: #666;
  font-style: italic;
}
.concerts-tabs {
  display: flex;
  gap: 1rem;
  margin: 1rem 0;
  padding-bottom: 1rem;
  border-bottom: 1px solid #eee;
}

.concerts-tabs button {
  padding: 0.5rem 1rem;
  border-radius: 20px;
  border: 1px solid #ddd;
  background-color: #f5f5f5;
  color: #333;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 0.9rem;
}

.concerts-tabs button:hover {
  background-color: #e0e0e0;
}

.concerts-tabs button.active {
  background-color: #ff5757;
  color: white;
  border-color: #ff5757;
}

/* Стили для отмененных и прошедших концертов */
.concert-card-artist.cancelled {
  opacity: 0.7;
  position: relative;
}

.concert-card-artist.cancelled::after {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.05);
}

.concert-card-artist.past {
  opacity: 0.8;
}

.concert-status {
  color: #d32f2f;
  font-weight: bold;
  margin-top: 0.5rem;
}

.no-concerts {
  padding: 20px;
  text-align: center;
  color: #666;
  font-size: 16px;
}

/* Адаптация для мобильных */
@media (max-width: 768px) {
  .concerts-tabs {
    gap: 0.5rem;
    flex-wrap: wrap;
  }
  
  .concerts-tabs button {
    padding: 0.5rem 0.8rem;
    font-size: 0.8rem;
  }
}
.no-concerts {
  padding: 20px;
  text-align: center;
  color: #666;
  font-size: 16px;
}
</style>