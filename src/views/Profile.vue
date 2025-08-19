<template>
  <div class="profile-page">
    <div class="profile-content">
      <div class="left-column">
        <div class="avatar-section">
          <div class="avatar-container" :style="{ backgroundColor: getGenreColor(topGenre) }">
            <img :src="avatarSrc || '/avatarUser/Рок avatar_user.png'" 
                alt="User Avatar" 
                class="profile-avatar">
          </div>
        </div>
      <div class="info-section">
        <div class="info-card">
          <h2 v-if="user.idRoleNavigation.nameRole === 'Клиент'">Профиль пользователя</h2>
          <h2 v-else-if="user.idRoleNavigation.nameRole === 'Организатор'">Профиль организатора</h2>
          
          <div class="info-grid">
            <div class="info-item">
              <label>Имя</label>
              <input type="text" v-model="editUser.firstNameUser" v-if="editing" class="edit-input">
              <p v-else class="info-value">{{ user.firstNameUser }}</p>
            </div>
            
            <div class="info-item">
              <label>Фамилия</label>
              <input type="text" v-model="editUser.lastNameUser" v-if="editing" class="edit-input">
              <p v-else class="info-value">{{ user.lastNameUser }}</p>
            </div>
            
            <div class="info-item">
              <label>Почта</label>
              <input type="email" v-model="editUser.emailUser" v-if="editing" class="edit-input" disabled>
              <p v-else class="info-value">{{ user.emailUser }}</p>
            </div>
            
            <div class="info-item">
              <label>Дата рождения</label>
                <input 
                  type="date" 
                  v-model="editUser.birthDateUser" 
                  v-if="editing"
                  class="edit-input"
                  :max="maxBirthDate"
                  @change="validateBirthDate"
                >
                <p v-else class="info-value">{{ formattedBirthDate }}</p>
            </div>
            
            <div class="info-item">
              <label>Город</label>
              <CitySelect
                v-if="editing"
                v-model="editUser.cityUser"
                :cities="cities"
                placeholder="Начните вводить город..."
              />
              <p v-else class="info-value">{{ user.cityUser || 'Не указан' }}</p>
            </div>
          </div>

          <div class="action-buttons">
            <button v-if="!editing" @click="startEditing" class="edit-btn">Редактировать</button>
            <div v-else class="edit-actions">
              <button @click="saveChanges" class="save-btn">Сохранить</button>
              <button @click="cancelEditing" class="cancel-btn">Отмена</button>
            </div>
          </div>
        </div>
          <div class="quick-links">
            <button 
              v-for="status in ticketStatuses" 
              :key="status.value"
              @click="setActiveTab(status.value)"
              class="status-btn"
              :class="{ 'active': activeTab === status.value }"
            >
              {{ status.label }} ({{ getStatusCount(status.value) }})
            </button>
            <button 
              @click="setActiveTab('favorites')"
              class="status-btn"
              :class="{ 'active': activeTab === 'favorites' }"
            >
              Избранное ({{ favorites.length }})
            </button>
            <button 
              @click="setActiveTab('reviews')"
              class="status-btn"
              :class="{ 'active': activeTab === 'reviews' }"
            >
              Мои отзывы ({{ userReviews.length }})
            </button>
          </div>
      </div>
      </div>
      
      <div class="notifications-section">
  <h3>Уведомления 
    <span v-if="unreadCount > 0" class="unread-badge">{{ unreadCount }}</span>
  </h3>

  <div v-if="notificationsLoading" class="loading">Загрузка...</div>
  <div v-else>
    <div v-if="notifications.length === 0" class="empty-message">Нет уведомлений</div>
    <div v-else class="notifications-list">
      <div 
        v-for="notification in notifications" 
        :key="notification.idNotification"
        class="notification-card"
        :class="{ 'unread': !notification.isRead }"
        @click="openNotification(notification)"
      >
        <div class="notification-header">
          <span class="notification-date">{{ formatDate(notification.dateNotification) }}</span>
          <span v-if="!notification.isRead" class="unread-dot"></span>
        </div>

        <p class="notification-message">{{ notification.messageNotification }}</p>

        <div class="notification-actions">
          <!-- Оставить отзыв -->
          <button 
            v-if="notification.messageNotification.includes('Оставьте ваш отзыв')"
            @click.stop="openReviewModal(notification.idConcert)"
          >
            Оставить отзыв
          </button>

          <!-- Перейти к концерту (например, при напоминании или изменении даты) -->
          <router-link
            v-if="notification.idConcert && notification.messageNotification.includes('концерт')"
            :to="{ name: 'ConcertView', params: {idArtist: notification.idArtist, idConcert: notification.idConcert } }"
            @click.stop
          >
            Перейти к концерту
          </router-link>

          <!-- Перейти к артисту -->
          <router-link
            v-if="notification.idArtist"
            :to="{ name: 'ArtistView', params: { idArtist: notification.idArtist } }"
            @click.stop
          >
            Перейти к артисту
          </router-link>
        </div>
      </div>
    </div>
  </div>
</div>
</div>


    <div id="tickets" class="tickets-section" v-if="ticketStatuses.some(s => s.value === activeTab)">
      <h3>Мои билеты</h3>
      <div v-if="ticketsLoading" class="loading">Загрузка...</div>
      <div v-else>
        <div v-if="tickets.length === 0" class="empty-message">У вас пока нет билетов</div>
        <div v-else class="tickets-list">
          <div v-for="ticket in filteredTickets" :key="ticket.idTicket" class="ticket-card">
            <div class="ticket-header">
              <h4>Концерт: {{ ticket.idConcertNavigation?.idArtistNavigation?.nameArtist || 'Артист не указан' }}</h4>
              <span class="ticket-status" :class="getStatusClass(ticket.statusTicket)">
                {{ formatStatus(ticket.statusTicket) }}
              </span>
            </div>
            
            <div class="ticket-details">
              <div class="detail-item">
                <span class="detail-label">Дата концерта:</span>
                <span class="detail-value">
                  {{ formatDate(ticket.idConcertNavigation?.dateStartConcert + ' ' + ticket.idConcertNavigation?.timeStartConcert)}}
                </span>
              </div>
              
              <div class="detail-item">
                <span  class="detail-label">Место:</span>
                <span class="detail-value">
                  {{ formatSeatInfo(ticket) }}
                </span>
              </div>
              
              <div class="detail-item">
                <span class="detail-label">Дата покупки:</span>
                <span class="detail-value">
                  {{ formatDate(ticket.purchaseDateTicket) }}
                </span>
              </div>
            </div>
            
            <div class="ticket-actions">
              <button v-if="ticket.statusTicket === 'Активен'" @click="downloadTicket(ticket.idTicket, this.user.idUser)" class="action-btn download-btn">Скачать билет</button>
              <button class="action-btn cancel-btn" 
                      v-if="ticket.statusTicket === 'Активен'"
                      @click="cancelTicket(ticket.idTicket)">
                Отменить
              </button>
              <button 
                  v-if="ticket.statusTicket === 'Использован' && !hasReview(ticket.idConcert)"
                  @click="openReviewModal(ticket.idConcert)"
                  class="action-btn review-btn"
                >
                  Оставить отзыв
                </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div id="favorites" class="favorites-section" v-if="activeTab === 'favorites'">
    <h3>Избранное</h3>
    <div v-if="favoritesLoading" class="loading">Загрузка...</div>
    <div v-else>
      <div v-if="favorites.length === 0" class="empty-message">У вас пока нет избранного</div>
      <div v-else class="artists-container">
        <div
          class="artist-card"
          v-for="favorite in favorites"
          :key="favorite.idFavorite"
            :style="{ backgroundImage: favorite.idArtistNavigation?.profilePhotoArtist ? `url('${favorite.idArtistNavigation.profilePhotoArtist}')` : 'none' }"
        >
          <div class="artist-info">
            <div class="border-artist-info">
              <span v-for="(genre, gIndex) in favorite.idArtistNavigation?.idGenres" :key="gIndex">
                {{ genre.nameGenre }}
                <span v-if="gIndex < favorite.idArtistNavigation?.idGenres.length - 1"> | </span>
              </span>
            </div>

          
          </div>

          <div class="artist-hover-info">
            <h4>{{ favorite.idArtistNavigation?.nameArtist }}</h4>
            <p>{{ truncatedDescription(favorite.idArtistNavigation?.descriptionArtist) }}</p>
            
            <router-link :to="{ name: 'ArtistView', params: { idArtist: favorite.idArtistNavigation?.idArtist } }">
              <button>Перейти к артисту</button>
            </router-link>
            <button 
              class="favorite-btn active"
              @click.stop="toggleFavorite(favorite.idArtistNavigation)"
            >
              <svg class="heart" viewBox="0 0 24 24">
                <path class="heart-outline" d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/>
                <path class="heart-filled" d="M16.5 3c-1.74 0-3.41.81-4.5 2.09C10.91 3.81 9.24 3 7.5 3 4.42 3 2 5.42 2 8.5c0 3.78 3.4 6.86 8.55 11.54L12 21.35l1.45-1.32C18.6 15.36 22 12.28 22 8.5 22 5.42 19.58 3 16.5 3z"/>
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>
    <div id="reviews" class="reviews-section" v-if="showReviewsSection">
    <h3>Мои отзывы</h3>
    <div v-if="reviewsLoading" class="loading">Загрузка...</div>
    <div v-else>
      <div v-if="userReviews.length === 0" class="empty-message">У вас пока нет отзывов</div>
      <div v-else class="reviews-list">
        <div v-for="review in userReviews" :key="review.idReview" class="review-card">
          <div class="review-header">
            <h4>{{ review.idConcertNavigation.idArtistNavigation.nameArtist }}</h4>
            <div class="review-rating" v-if="review.ratingReview">
              <span v-for="i in 5" :key="i" :class="{ 'filled': i <= review.ratingReview }">★</span>
            </div>
            <span class="review-date">{{ formatDate(review.dateReview) }}</span>
          </div>
          <p class="review-text">{{ review.textReview }}</p>
          <div class="review-actions">
            <button @click="editReview(review)" class="action-btn edit-btn">Редактировать</button>
            <button @click="deleteReview(review.idReview)" class="action-btn delete-btn">Удалить</button>
          </div>
        </div>
      </div>
    </div>
    <div id="reviews" class="reviews-section" v-if="showReviewsSection">
    <h3>Мои отзывы</h3>
    <div v-if="reviewsLoading" class="loading">Загрузка...</div>
    <div v-else>
      <div v-if="userReviews.length === 0" class="empty-message">У вас пока нет отзывов</div>
      <div v-else class="reviews-list">
        <div v-for="review in userReviews" :key="review.idReview" class="review-card">
          <div class="review-header">
            <h4>{{ review.idConcertNavigation.idArtistNavigation.nameArtist }}</h4>
            <div class="review-rating" v-if="review.ratingReview">
              <span v-for="i in 5" :key="i" :class="{ 'filled': i <= review.ratingReview }">★</span>
            </div>
            <span class="review-date">{{ formatDate(review.dateReview) }}</span>
          </div>
          <p class="review-text">{{ review.textReview }}</p>
          <div class="review-actions">
            <button @click="editReview(review)" class="action-btn edit-btn">Редактировать</button>
            <button @click="deleteReview(review.idReview)" class="action-btn delete-btn">Удалить</button>
          </div>
        </div>
      </div>
    </div>
  </div>
  </div>
  </div>
  <div id="reviews" class="reviews-section" v-if="activeTab === 'reviews'">
      <h3>Мои отзывы</h3>
      <div v-if="reviewsLoading" class="loading">Загрузка...</div>
      <div v-else>
        <div v-if="userReviews.length === 0" class="empty-message">У вас пока нет отзывов</div>
        <div v-else class="reviews-list">
          <div v-for="review in userReviews" :key="review.idReview" class="review-card">
            <div class="review-header">
              <h4>{{ review.idConcertNavigation?.idArtistNavigation?.nameArtist }}</h4>
              <div class="review-rating" v-if="review.ratingReview">
                <span v-for="i in 5" :key="i" :class="{ 'filled': i <= review.ratingReview }">★</span>
              </div>
              <span class="review-date">{{ formatDate(review.dateReview) }}</span>
            </div>
            <p class="review-text">{{ review.textReview }}</p>
            <div class="review-actions">
              <button @click="editReview(review)" class="action-btn edit-btn">Редактировать</button>
              <button @click="deleteReview(review.idReview)" class="action-btn delete-btn">Удалить</button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Модальное окно для отзывов -->
    <div v-if="showReviewModal" class="modal-overlay">
      <div class="modal-content">
        <h3>{{ editingReview ? 'Редактировать отзыв' : 'Оставить отзыв' }}</h3>
        <div class="rating-selector">
          <label>Оценка:</label>
          <div class="stars">
            <span 
              v-for="i in 5" 
              :key="i" 
              @click="currentReview.ratingReview = i"
              :class="{ 'filled': i <= currentReview.ratingReview }"
            >★</span>
          </div>
        </div>
        <textarea 
          v-model="currentReview.textReview" 
          placeholder="Напишите ваш отзыв..."
          class="review-textarea"
        ></textarea>
        <div class="modal-actions">
          <button @click="saveReview" class="save-btn">Сохранить</button>
          <button @click="closeReviewModal" class="cancel-btn">Отмена</button>
        </div>
      </div>
    </div>
  <div class="toasts-container">
      <div 
        v-for="toast in toasts" 
        :key="toast.id"
        :class="['toast', toast.type]"
      >
        {{ toast.message }}
      </div>
    </div>
  </div>
</template>

<script>
import ApiService from '@/assets/services/apiService';
import CitySelect from '@/components/CitySelect.vue';

export default {
  components: {
    CitySelect
  },
  data() {
    return {
      user: {},
      toasts: [],
      editUser: {},
      editing: false,
      tickets: [],
      ticketsLoading: false,
      favorites: [],
      favoritesLoading: false,
      ticketStatuses: [
        { value: 'Активен', label: 'Активные' },
        { value: 'Использован', label: 'Использованные' },
        { value: 'Возвращен', label: 'Возвращенные' }
      ],
      activeTab: 'Активен',
      activeStatus: 'Активен',
      showFavoritesSection: false,
      cities: [],
      showReviewsSection: false,
      reviewsLoading: false,
      userReviews: [],
      showReviewModal: false,
      editingReview: false,
      genres: [],
      currentReview: {
        idReview: null,
        idUser: null,
        idConcert: null,
        textReview: '',
        ratingReview: 0,
        dateReview: null
      },
      notifications: [],
      notificationsLoading: false,
      unreadCount: 0,
      pollInterval: null,
      personalRecommendations: [],
      topGenre: '',
      avatarSrc: null,
    };
  },
  computed: {
      showTickets(status) {
        this.activeTab = status;
      },
    formattedBirthDate() {
      if (!this.user.birthDateUser) return 'Не указана';
      return new Date(this.user.birthDateUser).toLocaleDateString('ru-RU');
    },
    filteredTickets() {
      return this.tickets.filter(ticket => ticket.statusTicket === this.activeTab);
    },
    maxBirthDate() {
      const today = new Date();
      const maxDate = new Date(today.getFullYear() - 14, today.getMonth(), today.getDate());
      return this.formatDateForInput(maxDate);
    },
  },
  async created() {
    await this.fetchCities();
    await this.loadUserData()
    this.startPolling();
    await Promise.all([
      this.loadNotifications(),
      this.loadTickets(),
      this.loadUserReviews(),
      this.fetchFavorites(),
      this.fetchPersonalRecommendations(),
      this.fetchGenres(),
    ]);
    this.startPolling();
    if (this.topGenre) {
      this.updateAvatar();
    }
  },
    beforeUnmount() {
    this.stopPolling();
  },
  methods: {
    updateAvatar() {
      if (!this.topGenre) return;
    

      const genreFileName = this.topGenre.toLowerCase()
        .replace(/\s+/g, '_')
        .replace(/[^a-zа-яё0-9_]/gi, '');
      

      try {
        const avatarPath = `/avatarUser/${this.topGenre} avatar_user.png`;
        this.avatarSrc = avatarPath;
      } catch (e) {
        // Если аватарка жанра не найдена, используем дефолтную
        this.avatarSrc = 'avatarUser/Рок avatar_user.png';
      }
    },
    async fetchGenres() {
      try {
        const response = await ApiService.getGenres();
        this.genres = response;
      } catch (error) {
        console.error("Ошибка при получении жанров артиста:", error);
      }
    },
    async fetchPersonalRecommendations() {
      
    if (!this.user?.idUser) return;
    
    this.isLoadingPersonal = true;
    try {
      const recommendations = await ApiService.getRecommendationForUser(this.user.idUser);
      
      if (recommendations && recommendations.length > 0) {
        this.personalRecommendations = recommendations.map(rec => {
          const artist = rec.idArtistNavigation;
          return {
            ...artist,
            idArtist: artist.idArtist,
            nameArtist: artist.nameArtist,
            profilePhotoArtist: artist.profilePhotoArtist,
            backgroundPhotoArtist: artist.backgroundPhotoArtist,
            descriptionArtist: artist.descriptionArtist,
            idGenres: artist.idGenres
          };
        });
        
        // Определяем самый популярный жанр
        this.calculateTopGenre();
      }
    } catch (error) {
      console.error('Ошибка загрузки персональных рекомендаций:', error);
      this.personalRecommendations = [];
    } finally {
      this.isLoadingPersonal = false;
    }
  },
  
  calculateTopGenre() {
    const genreCounts = {};
    
    this.personalRecommendations.forEach(artist => {
      artist.idGenres?.forEach(genre => {
        genreCounts[genre.idGenre] = genreCounts[genre.idGenre] || {count: 0, name: genre.nameGenre};
        genreCounts[genre.idGenre].count++;
      });
    });
    
    const sortedGenres = Object.values(genreCounts).sort((a, b) => b.count - a.count);
    this.topGenre = sortedGenres.length > 0 ? sortedGenres[0].name : '';
    const avatarPath = `/avatarUser/${this.topGenre} avatar_user.png`;
    this.avatarSrc = avatarPath;
  },
        getGenreColor(genreName) {
    const genre = this.genres.find(g => g.nameGenre === genreName);
    return genre ? genre.colorGenre : '#666';
  },
  //     getTopGenreColor() {
  //   // Если у пользователя есть любимые жанры, берем цвет первого
  //   if (this.user?.idGenres?.length > 0) {
  //     const genre = this.genres.find(g => g.idGenre === this.user.idGenres[0].idGenre);
  //     return genre?.colorGenre || '#f5f5f5';
  //   }
  //   return '#f5f5f5'; // Цвет по умолчанию
  // },
    formatDateForInput(date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  },
  validateBirthDate() {
    if (this.editUser.birthDateUser) {
      const birthDate = new Date(this.editUser.birthDateUser);
      const today = new Date();
      const minBirthDate = new Date(today.getFullYear() - 14, today.getMonth(), today.getDate());
      
      if (birthDate > minBirthDate) {
        // Если дата рождения слишком "молодая", сбрасываем ее
        this.editUser.birthDateUser = this.formatDateForInput(minBirthDate);
        // Можно также показать сообщение пользователю
        alert('Вы должны быть старше 14 лет');
      }
    }
  },
    async loadNotifications() {
      if (!this.user?.idUser) return;
      this.notificationsLoading = true;
      try {
        this.notifications = await ApiService.getUserNotifications(this.user.idUser);
        this.unreadCount = await ApiService.getUnreadNotificationsCount(this.user.idUser);
      } catch (error) {
        console.error('Ошибка загрузки уведомлений:', error);
        this.showToast('Не удалось загрузить уведомления', 'error');
      } finally {
        this.notificationsLoading = false;
      }
    },
    
    async openNotification(notification) {
      if (!notification.isRead) {
        try {
          await ApiService.markNotificationAsRead(notification.idNotification);
          notification.isRead = true;
          this.unreadCount--;
        } catch (error) {
          console.error('Ошибка при отметке уведомления как прочитанного:', error);
        }
      }
      
      // Дополнительные действия в зависимости от типа уведомления
      if (notification.idConcert) {
        // Переход на страницу концерта
      } else if (notification.idArtist) {
        // Переход на страницу артиста
      }
    },
    
    startPolling() {
      this.pollInterval = setInterval(() => {
        this.loadNotifications();
      }, 30000); // Обновление каждые 30 секунд
    },
    
    stopPolling() {
      if (this.pollInterval) {
        clearInterval(this.pollInterval);
        this.pollInterval = null;
      }
    },
    setActiveTab(tab) {
      this.activeTab = tab;
    },
    
    filterTickets(status) {
      this.activeTab = status;
    },
    showReviews() {
      this.showReviewsSection = true;
      this.showFavoritesSection = false;
      this.activeStatus = '';
  },
  async loadUserReviews() {
    this.reviewsLoading = true;
    try {
      const response = await ApiService.getUserReviews(this.user.idUser);
      this.userReviews = response || [];
    } catch (error) {
      console.error('Ошибка загрузки отзывов:', error);
      this.userReviews = [];
    } finally {
      this.reviewsLoading = false;
    }
  },
  
  openReviewModal(concertId) {
    this.currentReview = {
      idReview: null,
      idUser: this.user.idUser,
      idConcert: concertId,
      textReview: '',
      ratingReview: 0,
      dateReview: new Date().toISOString()
    };
    this.editingReview = false;
    this.showReviewModal = true;
  },
  
  editReview(review) {
    this.currentReview = {...review};
    this.editingReview = true;
    this.showReviewModal = true;
  },
  
  closeReviewModal() {
    this.showReviewModal = false;
  },
  
  async saveReview() {
      try {
        if (this.editingReview) {
          await ApiService.updateReview(this.currentReview);
        } else {
          await ApiService.createReview(this.currentReview);
        }
        // Обновляем список отзывов
        await this.loadUserReviews();
        this.showReviewModal = false;
        this.showToast('Отзыв успешно сохранен');
      } catch (error) {
        console.error('Ошибка сохранения отзыва:', error);
        this.showToast('Не удалось сохранить отзыв', 'error');
      }
    },
  
  async deleteReview(reviewId) {
    if (confirm('Вы уверены, что хотите удалить этот отзыв?')) {
      try {
        await ApiService.deleteReview(reviewId);
        await this.loadUserReviews();
        this.showToast('Отзыв успешно удален');
      } catch (error) {
        console.error('Ошибка удаления отзыва:', error);
        this.showToast('Не удалось удалить отзыв', 'error');
      }
    }
  },
  
  hasReview(concertId) {
    return this.userReviews.some(review => review.idConcert === concertId);
  },
  showToast(message, type = 'success') {
      const id = Date.now();
      this.toasts.push({ id, message, type });
      setTimeout(() => {
        this.toasts = this.toasts.filter(t => t.id !== id);
      }, 3000);
    },
    async cancelTicket(ticketId) {
        if (!confirm('Вы уверены, что хотите отменить этот билет?')) {
            return;
        }
        
        try {
            const ticket = this.tickets.find(t => t.idTicket === ticketId);
            if (!ticket) {
                alert('Билет не найден');
                return;
            }
            
            const concertDate = new Date(ticket.idConcertNavigation.dateStartConcert);
            const now = new Date();
            
            // Проверка на прошедший концерт
            if (concertDate < now) {
                alert('Нельзя отменить билет на прошедший концерт');
                return;
            }
            
            // Проверка на отмененный концерт
            if (ticket.idConcertNavigation.statusConcert === 'Событие отменено') {
                alert('Концерт отменен, билет автоматически возвращен');
                return;
            }
            
            // Проверка статуса билета
            if (ticket.statusTicket !== 'Активен') {
                alert('Можно отменить только активные билеты');
                return;
            }
            
            const response = await ApiService.cancelTicket(ticketId);
            if (response.success) {
                alert('Билет успешно отменен');
                await this.loadTickets();
            } else {
                alert(response.message || 'Не удалось отменить билет');
            }
        } catch (error) {
            console.error('Ошибка при отмене билета:', error);
            alert(error.response?.data?.message || 'Не удалось отменить билет');
        }
    },

    async fetchCities() {
      try {
        this.cities = await ApiService.getCities();
      } catch (error) {
        console.error("Ошибка при получении городов:", error);
        this.cities = [];
      }
    },
    getStatusCount(status) {
      return this.tickets.filter(t => t.statusTicket === status).length;
    },
    truncatedDescription(text) {
      if (!text) return '';
      return text.length > 100 ? text.slice(0, 100) + "..." : text;
    },
    
    async toggleFavorite(artist) {
      try {
        if (!this.user?.idUser) {
          this.showToast('Пожалуйста, войдите в систему', 'error');
          return;
        }

        const isFavorite = this.favorites.some(f => 
          f.idArtist === artist.idArtist || 
          f.id_artist === artist.idArtist
        );
        
        if (isFavorite) {
          const favoriteId = this.favorites.find(f => 
            f.idArtist === artist.idArtist || 
            f.id_artist === artist.idArtist
          )?.idFavorite;
          
          if (favoriteId) {
            await ApiService.removeFavorite(favoriteId);
            this.favorites = this.favorites.filter(f => 
              f.idArtist !== artist.idArtist && 
              f.id_artist !== artist.idArtist
            );
            this.showToast('Исполнитель удален из избранного');
          }
        } else {
          const response = await ApiService.addFavorite({
            id_user: this.user.idUser,
            id_artist: artist.idArtist
          });
          this.favorites.push({
            ...response,
            idArtistNavigation: artist
          });
          this.showToast('Исполнитель добавлен в избранные');
        }
      } catch (error) {
        console.error('Ошибка при обновлении избранного:', error);
        this.showToast('Ошибка при обновлении избранного', 'error');
      }
    },
    isFavorite(item) {
            if (!this.favorites || !this.favorites.length) return false;
            
            // Проверяем оба варианта именования свойств
            return this.favorites.some(fav => {
                return fav.id_artist === item.idArtist || 
                    fav.idArtist === item.idArtist ||
                    (fav.artist && fav.artist.idArtist === item.idArtist);
            });
        },
        
        getFavoriteId(item) {
            const favorite = this.favorites.find(fav => fav.idArtist === item.idArtist);
            return favorite ? favorite.idFavorite : null;
        },
        
        async fetchFavorites() {
            try {
                if (!this.user?.idUser) {
                    this.favorites = [];
                    return;
                }
                
                const response = await ApiService.getUserFavorites(this.user.idUser);
                this.favorites = Array.isArray(response) ? response : [];
                console.log('Loaded favorites:', this.favorites); // Для отладки
              } catch (error) {
                console.error('Ошибка при загрузке избранного:', error);
                this.favorites = [];
            }
          },
    filterTickets(status) {
      this.activeStatus = status;
      this.showFavoritesSection = false;
    },
    showFavorites() {
      this.showFavoritesSection = true;
      this.activeStatus = '';
    },
    formatSeatInfo(ticket) {
      const section = ticket.idSectionNavigation;
      if (!section) return 'Место не указано';
      
      if (section.sectionType === 'Танцпол') {
        return section.nameSection;
      } else if (section.sectionType === 'Ряды') {
        return `${section.nameSection}, Ряд ${ticket.rowTicket}, Место ${ticket.seatTicket}`;
      } else if (section.sectionType === 'Стол') {
        return `${section.nameSection}, Стол ${ticket.rowTicket}, Место ${ticket.seatTicket}`;
      }
      return `${section.nameSection}`;
  },
  async downloadTicket(ticketId, userId) {
    try {
        window.open(`http://localhost:5199/api/tickets/download/${ticketId}/${userId}`, '_blank');
    } catch (error) {
        console.error('Ошибка при скачивании билета:', error);
        alert('Не удалось скачать билет');
    }
},
    async loadUserData() {
      try {
        const storedUser = localStorage.getItem('user');
        if (storedUser && storedUser !== 'undefined') {
          this.user = JSON.parse(storedUser);
        }
      } catch (error) {
        console.error('Ошибка загрузки данных:', error);
      }
    },
    startEditing() {
      // Клонируем объект пользователя и преобразуем дату для input[type="date"]
      this.editUser = {
        ...this.user,
        cityUser: this.user.cityUser || '',
        birthDateUser: this.user.birthDateUser 
      ? this.formatDateForInput(new Date(this.user.birthDateUser))
      : this.formatDateForInput(new Date(new Date().getFullYear() - 14, 0, 1))
      };
      this.editing = true;
    },
async saveChanges() {
  try {
    // Проверка обязательных полей
    if (!this.editUser.firstNameUser || !this.editUser.lastNameUser) {
      this.showToast('Имя и фамилия обязательны', 'error');
      return;
    }

    // Преобразование даты
    const updateData = {
      ...this.editUser,
      birthDateUser: this.editUser.birthDateUser 
        ? new Date(this.editUser.birthDateUser).toISOString()
        : null
    };

    await ApiService.updateUser(updateData);
    this.user = {...this.editUser};
    localStorage.setItem('user', JSON.stringify(this.user));
    this.editing = false;
    this.showToast("Данные успешно обновлены!");
  } catch (error) {
    console.error('Ошибка сохранения:', error);
    this.showToast(error.response?.data || 'Ошибка при обновлении данных', 'error');
  }
},
    async loadTickets() {
      this.ticketsLoading = true;
      try {
        const response = await ApiService.getUserTickets(this.user.idUser);
        this.tickets = response || [];
      } catch (error) {
        console.error('Ошибка загрузки билетов:', error);
        this.tickets = [];
      } finally {
        this.ticketsLoading = false;
      }
    },
    async loadFavorites() {
      this.favoritesLoading = true;
      try {
        const response = await ApiService.getUserFavorites(this.user.idUser);
        this.favorites = response || [];
      } catch (error) {
        console.error('Ошибка загрузки избранного:', error);
        this.favorites = [];
      } finally {
        this.favoritesLoading = false;
      }
    },
    cancelEditing() {
      this.editing = false;
    },
    formatDate(date) {
      if (!date) return 'Не указана';
      return new Date(date).toLocaleDateString('ru-RU', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    },
    formatStatus(status) {
      return status; // Статусы уже на русском
    },
    getStatusClass(status) {
      return {
        'active': status === 'Активен',
        'used': status === 'Использован',
        'cancelled': status === 'Возвращен'
      };
    },
    async cancelTicket(ticketId) {
      if (confirm('Вы уверены, что хотите отменить этот билет?')) {
        try {
          await ApiService.cancelTicket(ticketId);
          await this.loadTickets();
          this.showToast('Билет успешно возвращен.');
        } catch (error) {
          console.error('Ошибка при отмене билета:', error);
          alert('Не удалось отменить билет');
        }
      }
    }
  }
};
</script>

<style scoped>
.left-column {
  margin-left: -12%;
  flex: 0 0 60%;
  display: flex;

  gap: 2rem;
}
.notifications-section {
  flex: 1;
  background: white;
  border-radius: 10px;
  padding: 1.5rem;
  box-shadow: 0 2px 10px rgba(0,0,0,0.08);
  height: fit-content; /* Автоматическая высота по содержимому */
  max-height: calc(100vh - 100px); /* Ограничение по высоте */
  overflow-y: auto;
  position: sticky;
  top: 20px;
  min-width: 350px; /* Минимальная ширина */
}

.notifications-section h3 {
  margin-top: 0;
  margin-bottom: 1.5rem;
  font-size: 1.3rem;
  color: #333;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.unread-badge {
  background-color: #ff5757;
  color: white;
  border-radius: 50%;
  width: 26px;
  height: 26px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 0.9rem;
  margin-left: 0.5rem;
}

.notifications-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.notification-card {
  padding: 1.25rem;
  border-radius: 10px;
  background-color: #f9f9f9;
  border-left: 4px solid #ddd;
  cursor: pointer;
  transition: all 0.2s;
}

.notification-card.unread {
  background-color: #f0f7ff;
  border-left-color: #4a90e2;
}

.notification-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0,0,0,0.1);
}

.notification-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.75rem;
}

.notification-date {
  font-size: 0.85rem;
  color: #666;
}

.unread-dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background-color: #4a90e2;
}

.notification-message {
  margin: 0;
  color: #333;
  line-height: 1.5;
  font-size: 1rem;
}

.notification-actions {
  margin-top: 1rem;
  display: flex;
  justify-content: flex-end;
}

.notification-actions a {
  color: #ff5757;
  text-decoration: none;
  font-size: 0.9rem;
  font-weight: 500;
}

.notification-actions a:hover {
  text-decoration: underline;
}

.empty-message {
  padding: 2rem;
  text-align: center;
  color: #888;
  font-style: italic;
  font-size: 1.1rem;
}

.loading {
  padding: 1rem;
  text-align: center;
  color: #666;
}
.reviews-section {
  margin-top: 3rem;
  padding-top: 2rem;
  border-top: 1px solid #eee;
}

.reviews-list {
  display: grid;
  gap: 1.5rem;
}

.review-card {
  background: white;
  border-radius: 10px;
  padding: 1.5rem;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
}

.review-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.review-rating {
  color: #ffc107;
  font-size: 1.2rem;
}

.review-rating .filled {
  color: #ffc107;
}

.review-date {
  color: #888;
  font-size: 0.9rem;
}

.review-text {
  margin: 1rem 0;
  line-height: 1.5;
  color: #333;
}

.review-actions {
  display: flex;
  gap: 1rem;
  margin-top: 1rem;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  padding: 2rem;
  border-radius: 10px;
  width: 100%;
  max-width: 500px;
}

.rating-selector {
  margin: 1rem 0;
  display: flex;
  align-items: center;
  gap: 1rem;
}

.stars {
  display: flex;
  gap: 0.5rem;
  font-size: 1.5rem;
  color: #ddd;
  cursor: pointer;
}

.stars .filled {
  color: #ffc107;
}

.review-textarea {
  width: 100%;
  min-height: 150px;
  padding: 1rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  margin: 1rem 0;
  resize: vertical;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
}

.review-btn {
  background-color: #4CAF50;
  color: white;
}

.review-btn:hover {
  background-color: #3e8e41;
}

.edit-btn {
  background-color: #2196F3;
  color: white;
}

.edit-btn:hover {
  background-color: #0b7dda;
}

.delete-btn {
  background-color: #f44336;
  color: white;
}

.delete-btn:hover {
  background-color: #da190b;
}

@media (max-width: 600px) {
  .modal-content {
    width: 90%;
    padding: 1.5rem;
  }
  
  .review-actions {
    flex-direction: column;
  }
  
  .review-actions button {
    width: 100%;
  }
}
.toasts-container {
    position: fixed;
    bottom: 20px;
    right: 20px;
    z-index: 1000;
  }
  
  .toast {
    padding: 12px 24px;
    margin-bottom: 10px;
    border-radius: 4px;
    color: white;
    background-color: #4CAF50;
    animation: fadeIn 0.3s;
  }
  
  .toast.error {
    background-color: #F44336;
  }
.artists-container {
  display: grid;
  grid-template-columns: repeat(3, 1fr); 
  gap: 20px;
  width: 100%;
  padding: 0 10px;
}

.artist-card {
  position: relative;
  background-size: cover;
  background-position: center;
  border-radius: 10px;
  min-height: 350px;
  overflow: hidden;
  transition: transform 0.3s ease;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  background-color: #f5f5f5; /* Фон, если нет изображения */
}

.artist-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 6px 12px rgba(0, 0, 0, 0.15);
}

.artist-info {
  position: absolute;
  display: flex;
  flex-direction: row;
  bottom: 0;
  left: 0;
  width: 100%;
  padding: 10px;
  justify-content: space-between;
  text-align: center;
}

.border-artist-info {
  background: #ff5757;
  border-radius: 10px;
  box-sizing: border-box;
  padding: 3px;
  color: #000;
}

.artist-hover-info {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(255, 255, 255, 0.95);
  color: #000;
  border-radius: 10px;
  padding: 20px;
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  opacity: 0;
  transition: opacity 0.3s ease;
}

.artist-hover-info h4 {
  font-size: 24px;
  margin-bottom: 10px;
}

.artist-hover-info p {
  font-size: 16px;
  margin-bottom: 20px;
  text-align: center;
}

.artist-card:hover .artist-hover-info {
  opacity: 1;
}

.favorite-btn {
  position: absolute;
  top: 15px;
  right: 15px;
  background: rgba(255, 255, 255, 0.8);
  border: none;
  border-radius: 50%;
  width: 56px;
  height: 56px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  z-index: 10;
  transition: all 0.3s ease;
}

.favorite-btn:hover {
  background: rgba(255, 255, 255, 0.95);
  transform: scale(1.1);
}

.heart {
  width: 20px;
  height: 20px;
}

.heart-outline {
  fill: none;
  stroke: #ff5757;
  stroke-width: 2px;
  opacity: 1;
  transition: all 0.3s ease;
}

.heart-filled {
  fill: #ff5757;
  opacity: 0;
  transform: scale(0);
  transition: all 0.3s ease;
}

.favorite-btn.active .heart-outline {
  opacity: 0;
  transform: scale(0);
}

.favorite-btn.active .heart-filled {
  opacity: 1;
  transform: scale(1);
}

@media (max-width: 1024px) {
.profile-content {
    flex-direction: column;
  }
  
  .left-column {
    align-items: center;
    margin-left: 0;
    flex-direction: column;
    width: 100%;
    flex: 1 1 100%;
  }
  
  .notifications-section {
    width: 100%;
    position: static;
    max-height: none;
  }
}


@media (max-width: 600px) {
  .artists-container {
    grid-template-columns: 1fr;
  }
}
.profile-page {
    max-width: 1200px;
    margin: 0 auto;
    padding: 2rem 1rem;
  }
  
  .profile-content {
    margin-top: 6%;
    display: flex;
    gap: 3rem;
    align-items: flex-start;
  }
  
  .avatar-section {
    flex: 0 0 180px;
  }
  
  .avatar-container {
    width: 180px;
    height: 180px;
    border-radius: 50%;
    overflow: hidden;
    background-color: #f5f5f5;
    border: 3px solid #e0e0e0;
  }
  
  .profile-avatar {
    align-self: center;
    justify-self: center;
    margin-top: -20px;
    margin-right: -5px;
    width: 100%;
    height: 100%;
    object-fit: cover;
    z-index: 2;
  }
  
  .info-section {
    flex: 1;
  }
  
  .info-card {
    background: white;
    border-radius: 10px;
    padding: 2rem;
    box-shadow: 0 2px 10px rgba(0,0,0,0.08);
    width: 100%;
  }
  
  .info-card h2 {
    margin-top: 0;
    margin-bottom: 2rem;
    font-size: 1.6rem;
    color: #333;
    text-align: left;
  }
  
  .info-grid {
    display: grid;
    grid-template-columns: 1fr;
    gap: 1.5rem;
  }
  
  .info-item {
    display: grid;
    grid-template-columns: 120px 1fr;
    gap: 1rem;
    align-items: center;
  }
  
  .info-item label {
    font-weight: 600;
    color: #555;
    font-size: 0.95rem;
    text-align: left;
  }
  
  .info-value, .edit-input {
    text-align: left;
    margin: 0;
    font-size: 1rem;
    color: #333;
  }
  
  .edit-input {
    padding: 0.5rem 0.75rem;
    border: 1px solid #ddd;
    border-radius: 4px;
    transition: border-color 0.3s;
    width: 100%;
    max-width: 300px;
  }
  
  .edit-input:focus {
    border-color: #ff5757;
    outline: none;
  }
  
  .action-buttons {
    margin-top: 2.5rem;
    display: flex;
    justify-content: flex-end;
  }
  
  .edit-btn, .save-btn, .cancel-btn {
    padding: 0.7rem 1.5rem;
    border-radius: 6px;
    font-size: 1rem;
    cursor: pointer;
    transition: all 0.2s;
    border: none;
  }
  
  .edit-btn {
    background-color: #ff5757;
    color: white;
  }
  
  .edit-btn:hover {
    background-color: #e04646;
  }
  
  .edit-actions {
    display: flex;
    gap: 1rem;
  }
  
  .save-btn {
    background-color: #ff5757;
    color: white;
  }
  
  .save-btn:hover {
    background-color: #e04646;
  }
  
  .cancel-btn {
    background-color: #f5f5f5;
    color: #333;
    border: 1px solid #ddd;
  }
  
  .cancel-btn:hover {
    background-color: #e0e0e0;
  }
  
  .quick-links {
    display: flex;
    gap: 1.5rem;
    margin-top: 2rem;
    padding-top: 1.5rem;
    border-top: 1px solid #eee;
  }
  
  /* .quick-links {
  display: flex;
  gap: 0.5rem;
  margin-top: 2rem;
  padding-top: 1.5rem;
  border-top: 1px solid #eee;
  flex-wrap: wrap;
} */

.status-btn {
  padding: 0.5rem 1rem;
  border-radius: 20px;
  border: 1px solid #ddd;
  background-color: #f5f5f5;
  color: #333;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 0.9rem;
  white-space: nowrap;
}

.status-btn:hover {
  background-color: #e0e0e0;
}

.status-btn.active {
  background-color: #ff5757;
  color: white;
  border-color: #ff5757;
}

  .tickets-section, .favorites-section {
    margin-top: 3rem;
    padding-top: 2rem;
    border-top: 1px solid #eee;
  }
  
  .tickets-section h3, .favorites-section h3 {
    font-size: 1.4rem;
    margin-bottom: 1.5rem;
    color: #333;
  }
  
  .loading {
    padding: 1rem;
    text-align: center;
    color: #666;
  }
  
  .empty-message {
    padding: 1rem;
    text-align: center;
    color: #888;
    font-style: italic;
  }
  
  .tickets-list {
    display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
  }
  
  .ticket-card {
    background: white;
    border-radius: 10px;
    padding: 1.5rem;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
    border-left: 4px solid #ff5757;
    height: 100%;
    display: flex;
    flex-direction: column;
  }
  
  .ticket-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;
  }
  
  .ticket-header h4 {
    margin: 0;
    font-size: 1.2rem;
    color: #333;
  }
  
  .ticket-status {
    padding: 0.25rem 0.75rem;
    border-radius: 20px;
    font-size: 0.85rem;
    font-weight: 500;
  }
  
  .ticket-status.active {
    background-color: #e6f7ee;
    color: #00a859;
  }
  
  .ticket-status.used {
    background-color: #f0f0f0;
    color: #666;
  }
  
  .ticket-status.cancelled {
    background-color: #fee;
    color: #d32f2f;
  }
  
  .ticket-details {
    flex-grow: 1;
    /* display: grid;
    gap: 0.75rem;
    margin-bottom: 1.5rem; */
  }
  
  .detail-item {
    display: flex;
    gap: 0.5rem;
  }
  
  .detail-label {
    font-weight: 500;
    color: #666;
    min-width: 120px;
  }
  
  .detail-value {
    color: #333;
  }
  
  .ticket-actions {
    display: flex;
    gap: 1rem;
    margin-top: 1rem;
  }
  
  .action-btn {
    padding: 0.5rem 1rem;
    border-radius: 6px;
    font-size: 0.9rem;
    cursor: pointer;
    transition: all 0.2s;
    border: none;
  }
  
  .download-btn {
    background-color: #ff5757;
    color: white;
  }
  
  .download-btn:hover {
    background-color: #e04646;
  }
  
  .cancel-btn {
    background-color: #f5f5f5;
    color: #333;
    border: 1px solid #ddd;
  }
  
  .cancel-btn:hover {
    background-color: #e0e0e0;
  }
  
  .favorites-list {
    display: grid;
    gap: 1rem;
  }
  .avatar-container {
  width: 180px;
  height: 180px;
  border-radius: 50%;
  overflow: hidden;
  border: 3px solid #e0e0e0;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background-color 0.3s ease;
}

.profile-avatar {
  width: 90%;
  height: 90%;
  object-fit: cover;
  border-radius: 50%;
}
  @media (max-width: 768px) {
  .profile-page {
    padding: 1rem;
  }
  
  .notifications-section {
    min-width: auto;
    padding: 1rem;
  }
  
  .notification-card {
    padding: 1rem;
  }
}
  @media (min-width: 1200px) {
  .tickets-list {
    grid-template-columns: repeat(3, 1fr);
  }
}
  @media (max-width: 900px) {
    .quick-links {
      gap: 0.5rem;
      justify-content: center;
    }
    
    .status-btn {
      padding: 0.5rem 0.8rem;
      font-size: 0.8rem;
    }
    .profile-content {
      flex-direction: column;
      align-items: center;
      gap: 2rem;
    }
    
  .left-column,
  .notifications-section {
    flex: 1 1 100%;
    width: 100%;
  }
    .quick-links {
      flex-direction: column;
      gap: 1rem;
    }
    
    .info-item {
      grid-template-columns: 1fr;
      gap: 0.5rem;
    }
    
    .info-item label {
      margin-bottom: 0;
    }
    
    .edit-input {
      max-width: 100%;
    }
    
    .detail-item {
      flex-direction: column;
      gap: 0.25rem;
    }
    .tickets-list {
    grid-template-columns: 1fr;
  }
    .ticket-actions {
      flex-direction: column;
    }
    
    .action-btn {
      width: 100%;
    }
  }
</style>