<template>
    <div class="main-container">
        <div class="container">
            <aside class="filters">
                <h3>Фильтры</h3>

                <div class="filter-group">
                    <label for="city">Город:</label>
                    <CitySelectFilter 
                        v-model="filters.selectedCity" 
                        :cities="cities"
                        @update:modelValue="applyFilters"
                        />
                </div>

                <!-- Исправленный выбор жанра -->
                <div class="filter-group">
                    <label for="genre">Жанр:</label>
                    <div class="select-wrapper">
                        <select 
                            v-model="filters.genre" 
                            id="genre" 
                            @change="applyFilters"
                            class="filter-select"
                        >
                            <option :value="null">Все жанры</option>
                            <option 
                                v-for="genre in genres" 
                                :key="genre.idGenre" 
                                :value="genre"
                            >
                                {{ genre.nameGenre }}
                            </option>
                        </select>
                    </div>
                </div>

                <!-- Исправленный выбор даты -->
                <div class="filter-group">
                    <label for="date">Дата:</label>
                    <div class="date-input-wrapper">
                    <input 
                        type="date" 
                        v-model="filters.date" 
                        id="date" 
                        :min="today" 
                        @change="applyFilters">
                    </div>
                </div>

                <!-- Исправленный поиск -->
                <div class="filter-group search-box">
                    <div class="search-input-wrapper">
                        <input 
                            type="text" 
                            v-model="filters.searchQuery" 
                            placeholder="Поиск..." 
                            @input="onSearchInput"
                            class="filter-search"
                        >
                    </div>
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
                <div class="filter-image">
                    <img src="@/assets/appAssets/back_foto_filters.png" alt="Фильтры">
                </div>
            </aside>
            
            <div class="artists-container">
                <div
                    class="artist-card"
                    v-for="(artist, index) in artists"
                    :key="index"
                    :style="{ backgroundImage: `url('${artist.profilePhotoArtist}')`}"
                >
                    <div class="artist-info">
                        <div class="border-artist-info">
                            <span v-for="(genre, gIndex) in artist.idGenres" :key="gIndex">
                                {{ genre.nameGenre }}
                                <span v-if="gIndex < artist.idGenres.length - 1"> | </span>
                            </span>
                        </div>

                        <!-- Количество концертов -->
                        <div class="artist-concerts">
                            <div class="border-artist-info">
                                <p>Концертов: {{ upcomingConcerts(artist.concerts) }}</p>
                            </div>
                        </div>
                        <!-- Избранное (пример) -->
                        <!-- <div class="artist-favorites">
                            <p>{{ artist.favoritesCount }} ♥</p>
                        </div> -->
                    </div>

                    <!-- Блок при наведении: название артиста, описание, кнопка -->
                    <div class="artist-hover-info">
                        <h4>{{ artist.nameArtist }}</h4>
                        <p>{{ truncatedDescription(artist.descriptionArtist) }}</p>
                        
                        <!-- <button @click="goToArtist(artist)">Перейти к артисту</button> -->
                        <router-link :to="{ name: 'ArtistView', params: { idArtist: artist.idArtist } }">
                            <button>Перейти к артисту</button>
                        </router-link>
                        <button 
                            class="favorite-btn"
                            @click.stop="toggleFavorite(artist)"
                            :class="{ active: isFavorite(artist) }"
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
        <!-- Компонент для уведомлений -->
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
import { debounce } from 'lodash';
import ApiService from '../assets/services/apiService';
import CitySelectFilter from '@/components/CitySelectFilter.vue';
  export default {
    components: {
        CitySelectFilter
  },
    name: 'ArtistsAllTabView',
    data() {
        return {
            user:[],
            artists: [],
            favorites: [],
            genres: [],
            dropdownOpen: false,
            selectedGenre: null,
            filters: {
                city: '',
                genre: null,
                date: '',
                searchQuery: '',
                priceRange: 10000
            },
            cities: [],
            isLoading: false,
            toasts: []
        };
    },
    computed: {
        today() {
            const date = new Date();
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            return `${year}-${month}-${day}`;
        },

        filteredArtists() {
            return this.artists.filter(artist => {
                const matchesGenre = !this.filters.genre || 
                    artist.genres.some(g => g.id === this.filters.genre);
                const matchesSearch = !this.filters.search || 
                    artist.name.toLowerCase().includes(this.filters.search.toLowerCase());
                
                return matchesGenre && matchesSearch;
            });
        }
    },
    methods: {
        upcomingConcerts(concerts) {
            return concerts.filter(concert => 
                concert.statusConcert === 'Событие планируется'
            ).length;
        },
        showToast(message, type = 'success') {
            const id = Date.now();
            this.toasts.push({ id, message, type });
            setTimeout(() => {
                this.toasts = this.toasts.filter(t => t.id !== id);
            }, 3000);
        },
        async toggleFavorite(item) {
            try {
                if (!this.user?.idUser) {
                    this.showToast('Пожалуйста, войдите в систему', 'error');
                    return;
                }

                const isFavorite = this.isFavorite(item);
                
                if (isFavorite) {
                    const favoriteId = this.getFavoriteId(item);
                    if (!favoriteId) {
                        throw new Error("Не удалось найти ID избранного");
                    }
                    await ApiService.removeFavorite(favoriteId);
                    this.favorites = this.favorites.filter(fav => 
                        fav.idArtist !== item.idArtist && 
                        fav.idArtist !== item.idArtist
                    );
                    this.showToast('Исполнитель удален из избранного');
                } else {
                    const response = await ApiService.addFavorite({
                        idUser: this.user.idUser,
                        idArtist: item.idArtist
                    });
                    this.favorites.push({
                        idFavorite: response.idFavorite,
                        idArtist: item.idArtist,
                        idUser: this.user.idUser
                    });
                    this.showToast('Исполнитель добавлен в избранные');
                }
            } catch (error) {
                console.error('Ошибка при обновлении избранного:', error);
                this.showToast(error.response?.data?.message || error.message || 'Ошибка при обновлении избранного', 'error');
                await this.fetchFavorites(); // Перезагружаем избранные в случае ошибки
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
            if (!this.user?.idUser) return;
            
            try {
                this.favorites = await ApiService.getUserFavorites(this.user.idUser);
                
            } catch (error) {
                console.error('Ошибка при загрузке избранного:', error);
            }
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
        loadUser() {
            try {
                const storedUser = localStorage.getItem('user');
                this.user = storedUser && storedUser !== 'undefined' ? JSON.parse(storedUser) : null;
            } catch (e) {
                console.error('Ошибка парсинга user из localStorage:', e);
                this.user = null;
            }
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
        async applyFilters() {
            try {
                console.log(this.selectedCity)

                const params = {
                    city: this.filters.city || null,
                    genre: this.filters.genre?.nameGenre || null,
                    date: this.filters.date || null,
                    searchQuery: this.filters.searchQuery || null,
                    priceRange: this.filters.priceRange < 10000 ? this.filters.priceRange : null
                };
                
                this.artists = await ApiService.getFilteredArtists(params);
            } catch (error) {
                console.error('Ошибка фильтрации:', error.response?.data || error.message);
            }
        },
        onSearchInput: debounce(function() {
            this.applyFilters();
        }, 500)
    },

    async mounted() {
        try {
            await this.loadUser(); // Сначала загружаем пользователя
            await this.fetchFavorites(); // Затем загружаем избранные
            
            // Параллельно загружаем остальные данные
            await Promise.all([
                this.fetchArtists(),
                this.fetchGenres(),
                this.fetchCities()
            ]);
        } catch (error) {
            console.error("Ошибка при загрузке данных:", error);
            this.showToast('Ошибка при загрузке данных', 'error');
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

<style scoped>
/* Уведомления */
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
  fill: #ff5757;
}

.heart-outline {
  opacity: 1;
  transform-origin: center;
  transition: all 0.3s ease;
}

.heart-filled {
  opacity: 0;
  transform: scale(0);
  transform-origin: center;
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

/* Анимация при клике */
@keyframes heartBeat {
  0% { transform: scale(1); }
  25% { transform: scale(1.2); }
  50% { transform: scale(0.9); }
  75% { transform: scale(1.1); }
  100% { transform: scale(1); }
}

.favorite-btn:active .heart {
  animation: heartBeat 0.5s ease;
}
.main-container {
    margin-top: 4rem;
}

.container {
    display: flex;
    gap: 10px;
    padding: 20px;
    max-width: 1400px;
    margin: 0 auto;
}

.filters {
  width: 250px;
  background: white;
  padding: 20px;
  flex-shrink: 0;
  border-radius: 10px;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  position: sticky;
  top: 80px;
  height: fit-content;
}

.filters h3 {
  font-size: 20px;
  margin-bottom: 15px;
  color: #333;
}

.filter-group {
  display: flex;
  flex-direction: column;
  margin-bottom: 15px;
}

.filter-group label {
  font-size: 14px;
  margin-bottom: 5px;
  color: #555;
}

.filter-group select,
.filter-group input {
  width: 100%;
  padding: 0.6rem 0.8rem;
  border: 1px solid #ddd;
  border-radius: 6px;
  background-color: rgba(255, 255, 255, 0.8);
  transition: all 0.3s;
}
.filter-group select:focus,
.filter-group input[type="date"]:focus,
.filter-group input[type="text"]:focus {
  border-color: #ff5757;
  outline: none;
  box-shadow: 0 0 0 2px rgba(255, 87, 87, 0.2);
}
/* 🔹 Поле поиска */
.search-box {
  position: relative;
}

.search-box input {
  background-image: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="%23777" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line></svg>');
  background-repeat: no-repeat;
  background-position: 10px center;
  padding-left: 35px;
}

.search-box img {
  position: absolute;
  left: 10px;
  top: 50%;
  transform: translateY(-50%);
  width: 16px;
  opacity: 0.7;
}

/* 🔹 Ползунок цены */
input[type="range"] {
  width: 100%;
  height: 1px;
  -webkit-appearance: none;
  background: #ddd;
  border-radius: 5px;
  margin-top: 0.5rem;
}

input[type="range"]::-webkit-slider-thumb {
  -webkit-appearance: none;
  width: 17px;
  height: 17px;
  background: #ff5757;
  border-radius: 50%;
  cursor: pointer;
}

.artists-container {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 20px;
    width: 100%;
    padding: 0 10px;
}
.favorite-btn .heart-outline {
  opacity: 1;
  fill: none;
  stroke: #ff5757;
  stroke-width: 2px;
}

.favorite-btn .heart-filled {
  opacity: 0;
  fill: #ff5757;
}

.favorite-btn.active .heart-outline {
  opacity: 0;
}

.favorite-btn.active .heart-filled {
  opacity: 1;
}

@media (max-width: 1200px) {
    .artists-container {
        grid-template-columns: repeat(2, 1fr);
    }
}

@media (max-width: 768px) {
    .artists-container {
        grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
        gap: 15px;
    }
}

@media (max-width: 480px) {
    .artists-container {
        grid-template-columns: 1fr;
    }
}

/* Стили карточек остаются без изменений */
.artist-card {
    position: relative;
    background-size: cover;
    background-position: center;
    border-radius: 10px;
    min-height: 350px;
    min-width: 350px;
    overflow: hidden;
    transition: transform 0.3s ease;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
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
    padding: 3.0px;
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

.artist-hover-info p {
    font-size: 16px;
}
.artist-hover-info h4 {
    font-size: 24px;
}

.artist-card:hover .artist-hover-info {
    opacity: 1;
}

button {
    margin-top: 10px;
    padding: 8px 15px;
    border: none;
    background: #ff5757;
    color: white;
    border-radius: 5px;
    cursor: pointer;
}

button:hover {
    background: #e04646;
}
</style>