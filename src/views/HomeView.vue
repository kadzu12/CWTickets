<template>
    <!-- <router-view></router-view> -->
    <!-- 🔹 HEADER -->
    <!-- <header>
        <div class="logo">
          <img src="@/assets/appAssets/fabula-ai.png" alt="Logo" />
          <div class="brand-name">CWTickets</div>
        </div>
        <nav>
          <ul class="nav-links">
            <li><a href="#">Главная</a></li>
            <<router-link :to="{ name: 'Artist', params: { idArtist: 1 } }">Артист 1</router-link>
            <li><a href="#">Contact</a></li>
          </ul>
        </nav>
        <div class="cta">
          <a href="#" class="login">Войти</a>
        </div>
    </header> -->
  
    <!-- 🔹 MAIN CONTENT -->
    <main>
      <div class="banner-container">
        <img class="banner-vector" src="@/assets/appAssets/Vector.svg" alt="vector">
        <section class="banner">
          <div class="banner-content">
            <h1>Погружайтесь в волну <span>вместе с нами!</span></h1>
            <p>Открывай новые горизонты и наслаждайся живой музыкой.</p>
          </div>
          <div class="ticket-image">
            <img src="@/assets/appAssets/group_main_banner1.png" alt="Tickets" />
          </div>
        </section>
      </div>
      <div class="content-container">
        <!-- 🔹 SLIDER -->
        <section class="slider">
                  <p class="name-slider">Популярные исполнители</p>
                  <swiper
                    :modules="[Navigation, Pagination]"
                    :slides-per-view="1"
                    :space-between="30"
                    navigation
                    pagination
                  >
                  <swiper-slide v-for="(artist, index) in popularArtist" :key="index">
                    <div class="slide">
                      <!-- Затемненный фон -->
                      <div 
                        class="slider-background-blur" 
                        :style="{ backgroundImage: `url('${artist.backgroundPhotoArtist}')` }">
                      </div>
  
                      <div class="slider-content">
                        <!-- Фото профиля -->
                        <div class="slider-profile-photo">
                          <img :src="artist.profilePhotoArtist" alt="Фото профиля">
                        </div>
  
                        <!-- Информация -->
                        <div class="slider-info">
                          <h2 class="group-name">{{ artist.nameArtist }}</h2>
                          <p class="group-description">{{ truncatedDescription(artist.descriptionArtist) }}</p>
                        </div>
  
                        <!-- Кнопка вынесена за пределы info -->
                        <div class="button-container">
                          <router-link :to="{ name: 'ArtistView', params: { idArtist: artist.idArtist } }">
                              <button class="go-to-group">
                                Перейти к исполнителю
                                <img class="slider-buttom-img" src="@/assets/appAssets/arrow_next_icon.png" alt="Перейти">
                              </button>
                            </router-link>
                        </div>
                      </div>
                    </div>
                  </swiper-slide>
                </swiper>
              </section>
<section class="personal-recommendations" v-if="personalRecommendations.length > 0">
<p class="name-slider">
    Персональные рекомендации - 
    <span 
      v-if="topGenre" 
      class="genre-tag"
      :style="{
        backgroundColor: getGenreColor(topGenre),
        color: getTextColor(getGenreColor(topGenre))
      }"
    >
    {{ topGenre }}
      <img 
        :src="getGenreIcon(topGenre)" 
        class="genre-icon-tag"
        :style="{ filter: getIconFilter(topGenre) }"
        alt=""
      >
      
    </span>
</p>
  <div class="artist-cards-recommendations">
    <div 
      class="artist-card-recommendation" 
      v-for="artist in personalRecommendations" 
      :key="artist.idArtist"
      :style="{backgroundImage: `url('${artist.backgroundPhotoArtist}')`}"
    >
      <div class="artist-info-recommendation">
        <h3>{{ artist.nameArtist }}</h3>
        <div class="genres-recommendation">
          <span v-for="(genre, gIndex) in artist.idGenres" :key="genre.idGenre">
            {{ genre.nameGenre }}
            <span v-if="gIndex < artist.idGenres.length - 1"> | </span>
          </span>
        </div>
        <router-link 
          :to="{ name: 'ArtistView', params: { idArtist: artist.idArtist } }"
          class="recommendation-link"
        >
          Подробнее
        </router-link>
      </div>
    </div>
  </div>
</section>

<div v-else-if="isLoadingPersonal" class="loading-message">Загрузка персональных рекомендаций...</div>

    <div class="container">
                <!-- Боковая панель с фильтрами -->
    <aside class="filters">
      <h3>Фильтры</h3>

      <div class="filter-group">
        <label for="city">Город:</label>
        <CitySelectFilter 
          v-model="filters.city" 
          :cities="cities"
          @update:modelValue="applyFilters"
        />
      </div>

      <div class="filter-group">
        <label for="genre">Жанр:</label>
        <select v-model="filters.genre" id="genre" @change="applyFilters">
          <option value=null>Все жанры</option>
          <option 
            v-for="genre in genres" 
            :key="genre.idGenre" 
            :value="genre">
            {{ genre.nameGenre }}
          </option>
        </select>
      </div>

      <div class="filter-group">
        <label for="date">Дата:</label>
        <input 
          type="date" 
          v-model="filters.date" 
          id="date" 
          :min="today" 
          @change="applyFilters">
      </div>

      <div class="filter-group search-box">
        <input 
          type="text" 
          v-model="filters.searchQuery" 
          placeholder="Поиск..." 
          @input="onSearchInput">
      </div>

      <div class="filter-group">
  <label style="background-color: #ddd; margin: auto; border-radius: 5px; padding: 3px;" for="price">Цена до: {{ filters.priceRange }} ₽</label>
        <div class="range-container">
          <input 
            type="range" 
            v-model="filters.priceRange" 
            min="0" 
            max="10000" 
            step="500" 
            id="price"
            @input="applyFilters">
          <div class="range-background"></div>
        </div>
      </div>

      <!-- Декоративная картинка снизу -->
      <div class="filter-image">
        <img src="@/assets/appAssets/back_foto_filters.png" alt="Фильтры">
      </div>
    </aside>
    <div class="concerts-content">
            <!-- Теги дат над карточками -->
            <div class="date-tags">
              <button 
                :class="{ active: activeDateTag === 'today' }"
                @click="setDateFilter('today')">Сегодня</button>
              <button 
                :class="{ active: activeDateTag === 'tomorrow' }"
                @click="setDateFilter('tomorrow')">Завтра</button>
              <button 
                :class="{ active: activeDateTag === 'week' }"
                @click="setDateFilter('week')">Эта неделя</button>
              <button 
                :class="{ active: activeDateTag === 'all' }"
                @click="setDateFilter('all')">Все даты</button>
            </div>
    <div class="concerts-container">
  <div
    class="concert-card"
    v-for="(concert, index) in events"
    :key="index"
    :style="{ backgroundImage: concert.profilePhotoArtist ? `url('${concert.profilePhotoArtist}')` : '' }"
  >
    <div class="concert-info">
      <div class="border-concert-info date-info">
        <span>{{ formatConcertDate(concert.dateStartConcert) }}</span>
      </div>
      
      <div class="border-concert-info location-info">
        <span>{{ concert.cityHall }}, {{ concert.nameHall }}</span>
      </div>
      
      <div class="border-concert-info price-info">
        <span>От {{ concert.minPrice || '—' }} ₽</span>
      </div>
    </div>

    <div class="concert-hover-info">
      <h4>{{ concert.nameArtist }}</h4>
      <p>{{ truncatedDescription(concert.descriptionArtist) }}</p>
      <!-- Убедитесь, что concert.idConcert существует -->
      <router-link 
                  :to="{ 
                    name: 'ConcertView', 
                    params: { 
                      idArtist: concert.idArtist,
                      idConcert: concert.idConcert 
                    }
                  }">
                  <button class="info-button">Подробнее...</button>
                </router-link>
      <!-- <router-link 
        v-if="concert.idConcert" 
        :to="{ name: 'ConcertView', params: { id: concert.idConcert } }"
      >
        <button>Купить билеты</button>
      </router-link> -->
          </div>
        </div>
      </div>
      </div>

        </div>
      </div>
    </main>
  </template>
  
  <script>
  import '@/assets/styleAssets/styleHome.css'
  import ApiService from '../assets/services/apiService';
  import { Swiper, SwiperSlide } from "swiper/vue";
  import "swiper/css";
  import "swiper/css/navigation";
  import "swiper/css/pagination";
  import { Navigation, Pagination } from "swiper/modules";
  import { Text } from 'vue';
  import { RouterLink, RouterView } from 'vue-router'
  import { debounce } from 'lodash';
  import CitySelectFilter from '@/components/CitySelectFilter.vue';

  export default {
  components: { Swiper, SwiperSlide,CitySelectFilter },
  data() {
    return {
      events: [],
      artists: [],
      popularArtist:[],
      user:[],
      genres: [],
      allEvents: [],
      dropdownOpen: false,
      selectedGenre: null,
      activeDateTag: 'week',
      filters: {
        city: '',
        genre: null,
        date: '',
        searchQuery: '',
        priceRange: 10000
      },
      cities: [],
      Navigation,
      Pagination,
      isLoading: false,
      error: null,
      personalRecommendations: [],
      topGenre: '',
      isLoadingPersonal: false

    };
  },
  computed: {
    today() {
      const date = new Date();
      const year = date.getFullYear();
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const day = String(date.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
    }
  },
  methods: {
      getGenreIcon(genreName) {
        const genre = this.genres.find(g => g.nameGenre === genreName);
        console.log(genre.iconUrlGenre)
        return genre ? genre.iconUrlGenre : '';
  },
  
  getIconFilter(genreName) {
    const bgColor = this.getGenreColor(genreName);
    const textColor = this.getTextColor(bgColor);
    
    // Если текст должен быть белым (темный фон), инвертируем иконку
    return textColor === '#fff' ? 'brightness(0) invert(1)' : 'none';
  },
      getGenreColor(genreName) {
    const genre = this.genres.find(g => g.nameGenre === genreName);
    return genre ? genre.colorGenre : '#666';
  },
  
  getTextColor(bgColor) {
    // Простая функция для определения, должен ли текст быть белым или черным
    // на основе яркости фона
    if (!bgColor) return '#fff';
    
    const color = bgColor.charAt(0) === '#' ? bgColor.substring(1, 7) : bgColor;
    const r = parseInt(color.substring(0, 2), 16);
    const g = parseInt(color.substring(2, 4), 16);
    const b = parseInt(color.substring(4, 6), 16);
    const brightness = (r * 299 + g * 587 + b * 114) / 1000;
    return brightness > 128 ? '#000' : '#fff';
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
  },
    loadUser() {
      try {
        const storedUser = localStorage.getItem('user');
        this.user = storedUser && storedUser !== 'undefined' ? JSON.parse(storedUser) : null;
      } catch (e) {
        console.error('Ошибка парсинга user из localStorage:', e);
        this.user = null;
      }
    },
    async fetchPopularArtist() {
      try {
        const response = await ApiService.popularArtists();
        
        if (!response || !Array.isArray(response)) {
          this.popularArtist = [];
          return;
        }

        const sortedArtists = response.sort((a, b) => (b.popularity_score || 0) - (a.popularity_score || 0));

        const enrichedArtists = await Promise.all(
          sortedArtists.map(async (artist) => {
            try {
              const fullArtistInfo = await ApiService.getArtistById(artist.id_artist);
              
              return {
                ...artist,
                ...(fullArtistInfo || {}), 
                popularity_score: artist.popularity_score 
              };
            } catch (error) {
              console.error(`Ошибка загрузки исполнителя ${artist.id_artist}:`, error);
              return artist; 
            }
          })
        );

        this.popularArtist = enrichedArtists.filter(artist => artist !== null && artist !== undefined);
        
      } catch (error) {
        console.error("Ошибка загрузки исполнителей:", error);
        this.popularArtist = [];
      }
    },
    formatConcertDate(dateString) {
      const options = { day: 'numeric', month: 'short' };
      return new Date(dateString).toLocaleDateString('ru-RU', options);
    },
    async fetchArtists() {
      try {
        const response = await ApiService.getArtists();
        this.artists = response;
      } catch (error) {
        console.error("Ошибка при получении артистов:", error);
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
    truncatedDescription(text) {
      return text.length > 100 ? text.slice(0, 100) + "..." : text;
    },
    // toggleDropdown() {
    //   this.dropdownOpen = !this.dropdownOpen
    // },
    // closeDropdown() {
    //   this.dropdownOpen = false
    // },
    // selectGenre(genre) {
    //   this.filters.genre = genre
    //   this.dropdownOpen = false
    //   // Можно добавить emit для уведомления родителя
    //   this.$emit('genre-selected', genre)
    // },
    // selectGenre(genre) {
    //   this.selectedGenre = genre;
    //   this.dropdownOpen = false;
    //   this.$emit('input', genre);
    // },
    async fetchCities() {
      try {
        const response = await ApiService.getCities();
        this.cities = response;
      } catch (error) {
        console.error("Ошибка при получении городов:", error);
      }
    },
    async fetchFilteredArtists() {
    try {
      const params = {
        city: this.selectedCity || null,
        genre: this.selectedGenre ? this.selectedGenre.name : null,
        date: this.selectedDate || null,
        searchQuery: this.searchQuery || null,
        priceRange: this.priceRange || null,
        sortBy: this.sortBy || null,
      };
  
      console.log("Отправляем параметры запроса:", params);
      const response = await ApiService.getFilteredArtists(params);
      
    
      this.artists = response;
      } catch (error) {
        console.error("Ошибка при загрузке артистов:", error);
      }
    },
    async fetchInitialData() {
      try {
        this.genres = await ApiService.getGenres();
        this.cities = await ApiService.getCities();
        // await this.applyFilters(); 
      } catch (error) {
        console.error('Ошибка загрузки данных:', error);
      }
    },
    formatDate(dateString) {
      const options = { day: 'numeric', month: 'long', year: 'numeric', hour: '2-digit', minute: '2-digit' };
      return new Date(dateString).toLocaleDateString('ru-RU', options);
    },
    formatShortDate(dateString) {
      const options = { day: 'numeric', month: 'short' };
      return new Date(dateString).toLocaleDateString('ru-RU', options);
    },
    goToEvent(event) {
      // Переход на страницу события
      this.$router.push({ name: 'ConcertArtistView', params: { idEvent: event.idEvent } });
    },
    setDateFilter(type) {
  this.activeDateTag = type;
  this.filters.date = ''; // Сбрасываем конкретную дату
  
  // Создаем копию всех событий для фильтрации
  const filtered = this.allEvents.filter(event => {
    const eventDate = new Date(event.dateStartConcert);
    eventDate.setHours(0, 0, 0, 0);
    
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    
    if (type === 'today') {
      return eventDate.getTime() === today.getTime();
    } 
    else if (type === 'tomorrow') {
      const tomorrow = new Date(today);
      tomorrow.setDate(today.getDate() + 1);
      return eventDate.getTime() === tomorrow.getTime();
    } 
    else if (type === 'week') {
      const weekLater = new Date(today);
      weekLater.setDate(today.getDate() + 7);
      return eventDate >= today && eventDate <= weekLater;
    }
    // Для 'all' возвращаем true - все события подходят
    return true;
  });
  
  // Присваиваем отфильтрованные события
  this.events = filtered;
},
    async fetchEvents() {
  this.isLoading = true;
  this.error = null;
  try {
    const response = await ApiService.getEvents();
    if (!Array.isArray(response)) {
      throw new Error("Некорректный формат данных концертов");
    }
    this.allEvents = response;
    this.events = response;
    this.setDateFilter('week');

  } catch (error) {
    console.error("Ошибка при получении событий:", error);
    this.error = "Не удалось загрузить события. Пожалуйста, попробуйте позже.";
  } finally {
    this.isLoading = false;
  }
},
    async applyFilters() {
      try {
        const params = {
          city: this.filters.city || null,
          genre: this.filters.genre?.nameGenre || null,
          date: this.filters.date || null,
          searchQuery: this.filters.searchQuery || null,
          priceRange: this.filters.priceRange < 10000 ? this.filters.priceRange : null
        };
        
        console.log('Отправляемые параметры:', params); // Для отладки
        this.events = await ApiService.getFilteredConcerts(params);
        this.activeDateTag = 'all'
      } catch (error) {
        console.error('Ошибка фильтрации:', error.response?.data || error.message);
      }
    },
    onCustomDateChange() {
      this.activeDateTag = null;
      this.filters.dateRange = null;
      this.fetchEvents(); // Изменено с fetchConcerts на fetchEvents
    },
    onSearchInput: debounce(function() {
      this.applyFilters();
    }, 500)
  },

  async mounted() {
  try {
    this.loadUser(),
    await Promise.all([
      await this.fetchPopularArtist(),
      this.fetchArtists(),
      this.fetchEvents(),
      this.fetchGenres(),
      this.fetchCities(),
      this.fetchPersonalRecommendations()
    ]);
  } catch (error) {
    console.error("Ошибка при загрузке данных:", error);
    this.error = "Произошла ошибка при загрузке данных";
  }

  },
  
  watch: {
  selectedCity: "fetchFilteredArtists",
  selectedGenre: "fetchFilteredArtists",
  selectedDate: "fetchFilteredArtists",
  searchQuery: "fetchFilteredArtists",
  priceRange: "fetchFilteredArtists"
  }
  }
  
  </script>