<template>
  <div class="artist-moderation">
    <!-- Поиск по артистам -->
    <div class="search-container">
      <input 
        type="text" 
        v-model="searchQuery" 
        placeholder="Поиск исполнителей..." 
        @input="searchArtists"
        class="search-input"
      >
      <span class="search-icon"></span>
    </div>

    <!-- Список артистов -->
    <div class="artist-cards-moderation">
      <div 
        class="artist-card-moderation" 
        v-for="artist in artists" 
        :key="artist.idArtist"
        :style="{backgroundImage: `url('${artist.backgroundPhotoArtist || defaultBackground}')`}"
      >
        <div class="artist-info-moderation">
          <h3>{{ artist.nameArtist }}</h3>
          <span v-for="(genre, gIndex) in artist.idGenres" :key="gIndex">
            {{ genre.nameGenre }}
            <span v-if="gIndex < artist.idGenres.length - 1"> | </span>
          </span>
          <button 
            class="moderate-btn"
            @click="openModeration(artist)"
          >
            <span class="material-icons"></span>
            Перейти к модерации
          </button>
        </div>
      </div>
    </div>

    <!-- Модальное окно модерации артиста -->
    <div class="modal-moderation" v-if="selectedArtist">
      <div class="modal-content-moderation">
        <div class="modal-header-moderation">
          <h2>Модерация: {{ selectedArtist.nameArtist }}</h2>
          <button class="moderate-btn" @click="closeModal">
            <img class="material-icons" src="@/assets/appAssets/close_icon.svg" alt="Закрыть">
          </button>
        </div>

        <div class="artist-details-moderation">
          <img :src="selectedArtist.profilePhotoArtist" alt="Фото артиста">
          <div>
            <span v-for="(genre, gIndex) in selectedArtist.idGenres" :key="gIndex">
              {{ genre.nameGenre }}
              <span v-if="gIndex < selectedArtist.idGenres.length - 1"> | </span>
            </span>
            <p><strong>Описание:</strong> {{ selectedArtist.descriptionArtist }}</p>
          </div>
        </div>

        <div class="concerts-list-moderation">
          <h3>Концерты артиста</h3>
          <div 
            class="concert-item-moderation" 
            v-for="concert in filteredConcerts" 
            :key="concert.idConcert"
          >
            <div >
              <p>{{ formatDate(concert.dateStartConcert) }} в {{ concert.idHallNavigation?.cityHall }}</p>
              <p>Зал: {{ concert.idHallNavigation?.nameHall }}</p>
              <p>Возрастное ограничение: {{ concert.ageLimitConcert || 'нет' }}+</p>
            </div>
            <div class="concert-actions-moderation">
              <button @click="openEditConcertModal(concert)">
                <span class="material-icons"></span>
                Редактировать
              </button>
            </div>
          </div>
        </div>

        <div class="modal-actions-moderation">
          <button @click="openAddConcertModal">
            <span class="material-icons"></span>
            Добавить концерт
          </button>
        </div>
      </div>
    </div>

    <!-- Модальное окно редактирования концерта -->
    <div class="modal-moderation" v-if="editingConcert">
      <div class="modal-content-moderation">
        <div class="modal-header-moderation">
          <h2>Редактирование концерта {{ selectedArtist.nameArtist }}</h2>
          <button class="moderate-btn" @click="closeEditModal">
            <img class="material-icons" src="@/assets/appAssets/close_icon.svg" alt="Закрыть">
          </button>
        </div>

        <div class="edit-concert-form">
          <div class="form-group">
            <label>Зал:</label>
            <select v-model="editingConcert.idHall" :disabled="!hallsInSameCity.length">
              <option v-for="hall in hallsInSameCity" :key="hall.idHall" :value="hall.idHall">
                {{ hall.nameHall }} ({{ hall.cityHall }})
              </option>
            </select>
          </div>

          <div class="form-group">
            <label>Статус:</label>
            <select v-model="editingConcert.statusConcert">
              <option value="Событие планируется">Событие планируется</option>
              <option value="Событие отменено">Событие отменено</option>
              <!-- <option 
                value="Событие прошло" 
                :disabled="new Date(editingConcert.dateStartConcert) >= new Date()"
              >
                Событие прошло
              </option> -->
            </select>
          </div>

          <div class="form-group">
            <label>Дата:</label>
            <input type="date" v-model="editingConcert.dateStartConcert">
          </div>

          <div class="form-group">
            <label>Время:</label>
            <input type="time" v-model="editingConcert.timeStartConcert">
          </div>

          <div class="form-group">
            <label>Возрастное ограничение:</label>
            <select v-model="editingConcert.ageLimitConcert">
              <option value="6">6+</option>
              <option value="12">12+</option>
              <option value="16">16+</option>
              <option value="18">18+</option>
            </select>
          </div>

          <div class="form-actions">
            <button class="cancel-btn" @click="closeEditModal">Отмена</button>
            <button class="save-btn" @click="saveConcertChanges">Сохранить</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Модальное окно добавления концерта -->
    <div class="modal-moderation" v-if="addingConcert">
      <div class="modal-content-moderation">
        <div class="modal-header-moderation">
          <h2>Добавление концерта для {{ selectedArtist.nameArtist }}</h2>
          <button class="moderate-btn" @click="closeAddConcertModal">
            <img class="material-icons" src="@/assets/appAssets/close_icon.svg" alt="Закрыть">
          </button>
        </div>

        <div class="edit-concert-form">
          <div class="form-group">
            <label>Зал:</label>
            <select v-model="newConcert.idHall">
              <option v-for="hall in hallsInSameCity" :key="hall.idHall" :value="hall.idHall">
                {{ hall.nameHall }} ({{ hall.cityHall }})
              </option>
            </select>
          </div>

          <div class="form-group">
            <label>Статус:</label>
            <select v-model="newConcert.statusConcert">
              <option value="Событие планируется">Событие планируется</option>
            </select>
          </div>

          <div class="form-group">
            <label>Дата:</label>
            <input type="date" v-model="newConcert.dateStartConcert">
          </div>

          <div class="form-group">
            <label>Время:</label>
            <input type="time" v-model="newConcert.timeStartConcert">
          </div>

          <div class="form-group">
            <label>Возрастное ограничение:</label>
            <select v-model="newConcert.ageLimitConcert">
              <option value="6">6+</option>
              <option value="12">12+</option>
              <option value="16">16+</option>
              <option value="18">18+</option>
            </select>
          </div>

          <div class="form-actions">
            <button class="cancel-btn" @click="closeAddConcertModal">Отмена</button>
            <button class="save-btn" @click="saveNewConcert">Добавить</button>
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
import ApiService from '@/assets/services/apiService';

export default {
  data() {
    return {
      artists: [],
      allArtists: [],
      selectedArtist: null,
      editingConcert: null,
      addingConcert: false,
      hallsInSameCity: [],
      user: null,
      allHalls: [],
      searchQuery: '',
      defaultBackground: '/default-background.jpg',
      newConcert: {
        idArtist: null,
        idHall: null,
        dateStartConcert: new Date().toISOString().split('T')[0],
        timeStartConcert: '19:00',
        ageLimitConcert: '16',
        statusConcert: 'Событие планируется'
      },
      toasts: []
    }
  },
  computed: {
    filteredConcerts() {
      return this.selectedArtist?.concerts?.filter(concert => 
        concert.idHallNavigation?.cityHall === this.user.cityUser && concert.statusConcert == "Событие планируется"
      ) || [];
    }
  },
  methods: {
    async fetchArtists() {
      try {
        const storedUser = localStorage.getItem('user');
        this.user = storedUser && storedUser !== 'undefined' ? JSON.parse(storedUser) : null;
        const response = await ApiService.getArtists();
        this.artists = response;
        this.allArtists = [...response];
      } catch (error) {
        console.error("Ошибка при получении артистов по городу:", error);
        this.showToast('Ошибка загрузки артистов по городу', 'error');
      }
    },
    async fetchHalls() {
      try {
        const response = await ApiService.getHalls();
        this.allHalls = response;
        // this.allHalls = [...response];
      } catch (error) {
        console.error("Ошибка при получении залов:", error);
        this.showToast('Ошибка загрузки залов', 'error');
      }
    },
    
    async refreshArtists() {
      try {
        const response = await ApiService.getArtists();
        this.artists = response;
        this.allArtists = [...response];
        if (this.selectedArtist) {
          this.selectedArtist = this.artists.find(a => a.idArtist === this.selectedArtist.idArtist);
        }
      } catch (error) {
        console.error("Ошибка обновления списка:", error);
      }
    },
    
    searchArtists() {
      if (!this.searchQuery) {
        this.artists = [...this.allArtists];
        return;
      }
      
      const query = this.searchQuery.toLowerCase();
      this.artists = this.allArtists.filter(artist => 
        artist.nameArtist.toLowerCase().includes(query) ||
        artist.idGenres.some(g => g.nameGenre.toLowerCase().includes(query)))
    },
    
    openModeration(artist) {
      this.selectedArtist = JSON.parse(JSON.stringify(artist));
    },
    
    closeModal() {
      this.selectedArtist = null;
    },
    
    formatDate(dateString) {
      return new Date(dateString).toLocaleDateString();
    },
    
    async openEditConcertModal(concert) {
      this.editingConcert = JSON.parse(JSON.stringify(concert));
      
      try {
        const currentCity = concert.idHallNavigation?.cityHall;
        if (currentCity) {
          this.hallsInSameCity = concert.idHallNavigation ? [concert.idHallNavigation] : [];
          const response = await ApiService.getHallsByCity(currentCity);
          const otherHalls = response || [];
          
          otherHalls.forEach(hall => {
            if (!this.hallsInSameCity.some(h => h.idHall === hall.idHall)) {
              this.hallsInSameCity.push(hall);
            }
          });
        } else {
          this.hallsInSameCity = [];
        }
      } catch (error) {
        console.error("Ошибка при получении залов:", error);
        this.hallsInSameCity = concert.idHallNavigation ? [concert.idHallNavigation] : [];
      }
    },
    
    closeEditModal() {
      this.editingConcert = null;
      this.hallsInSameCity = [];
    },
    
    async saveConcertChanges() {
      if (!this.editingConcert?.idConcert) {
        this.showToast('Ошибка: концерт не выбран', 'error');
        return;
      }
      
      try {
        const concertData = {
          idConcert: this.editingConcert.idConcert,
          idArtist: this.editingConcert.idArtist || this.selectedArtist.idArtist,
          idHall: this.editingConcert.idHall,
          statusConcert: this.editingConcert.statusConcert,
          dateStartConcert: this.editingConcert.dateStartConcert,
          timeStartConcert: this.editingConcert.timeStartConcert.includes(':') 
                  ? this.editingConcert.timeStartConcert 
                  : this.editingConcert.timeStartConcert + ":00",
          ageLimitConcert: this.editingConcert.ageLimitConcert
        };

        const response = await ApiService.updateConcert(concertData);
        this.updateLocalConcertData();
        await this.refreshArtists();
        this.closeEditModal();
        this.showToast('Изменения сохранены');
      } catch (error) {
        console.error("Ошибка при сохранении:", error);
        
        let errorMessage = 'Ошибка при сохранении изменений';
        
        // Обработка ошибки от сервера
        if (error.response) {
          // Если сервер вернул сообщение об ошибке
          if (error.response.data) {
            if (typeof error.response.data === 'string') {
              errorMessage = error.response.data;
            } else if (error.response.data.detail) {
              errorMessage = error.response.data.detail;
            } else if (error.response.data.title) {
              errorMessage = error.response.data.title;
            }
          }
        } else if (error.message) {
          errorMessage = error.message;
        }
        
        this.showToast(errorMessage, 'error');
      }
    },
    
    updateLocalConcertData() {
      const artist = this.artists.find(a => a.idArtist === this.selectedArtist?.idArtist);
      if (!artist) return;

      const concertIndex = artist.concerts.findIndex(c => c.idConcert === this.editingConcert.idConcert);
      if (concertIndex === -1) return;

      const hall = this.hallsInSameCity.find(h => h.idHall === this.editingConcert.idHall);
      artist.concerts[concertIndex] = {
        ...this.editingConcert,
        idHallNavigation: hall || artist.concerts[concertIndex].idHallNavigation
      };
    },
    
    async openAddConcertModal() {
      this.addingConcert = true;
      this.newConcert.idArtist = this.selectedArtist.idArtist;
      
      try {
           this.hallsInSameCity = await ApiService.getHallsByCity(this.user.cityUser);
      } catch (error) {
        console.error("Ошибка загрузки залов:", error);
        this.showToast('Ошибка загрузки списка залов', 'error');
      }
    },
    
    closeAddConcertModal() {
      this.addingConcert = false;
      this.resetNewConcert();
    },
    
    resetNewConcert() {
      this.newConcert = {
        idArtist: null,
        idHall: null,
        dateStartConcert: new Date().toISOString().split('T')[0],
        timeStartConcert: '19:00',
        ageLimitConcert: '16',
        statusConcert: 'Событие планируется'
      };
    },
    
    async saveNewConcert() {
      try {
          if (!this.newConcert.idHall) {
            this.showToast('Пожалуйста, выберите зал', 'error');
            return;
          }
        const newConcertData = {
          idArtist: this.selectedArtist.idArtist,
          idHall: this.newConcert.idHall,
          statusConcert: this.newConcert.statusConcert,
          dateStartConcert: this.newConcert.dateStartConcert,
          timeStartConcert: this.newConcert.timeStartConcert + ":00",
          ageLimitConcert: this.newConcert.ageLimitConcert
        };
        
        const response = await ApiService.addConcert(newConcertData);
        await this.refreshArtists();
        this.closeAddConcertModal();
        this.showToast('Концерт успешно добавлен');
      } catch (error) {
        console.error("Ошибка добавления:", error);
        
        let errorMessage = 'Ошибка при добавлении концерта';
        
        // Обработка ошибки от сервера
        if (error.response) {
          // Если сервер вернул сообщение об ошибке
          if (error.response.data) {
            if (typeof error.response.data === 'string') {
              errorMessage = error.response.data;
            } else if (error.response.data.detail) {
              errorMessage = error.response.data.detail;
            } else if (error.response.data.title) {
              errorMessage = error.response.data.title;
            }
          }
        } else if (error.message) {
          errorMessage = error.message;
        }
        
        this.showToast(errorMessage, 'error');
      }
    },
    
    showToast(message, type = 'success') {
      const id = Date.now();
      this.toasts.push({ id, message, type });
      setTimeout(() => {
        this.toasts = this.toasts.filter(t => t.id !== id);
      }, 3000);
    }
  },
  async mounted() {    
    await this.fetchArtists();
  }
}
</script>

<style scoped>
.artist-moderation {
  font-family: Arial, sans-serif;
  margin: 5% 5% 0 5%;
  max-width: 1200px;
  padding: 20px;
}

/* Стили поиска */
.search-container {
  position: relative;
  margin-bottom: 20px;
  max-width: 400px;
}

.search-input {
  width: 100%;
  padding: 10px 15px 10px 40px;
  border: 1px solid #ddd;
  border-radius: 20px;
  font-size: 16px;
}

.search-icon {
  position: absolute;
  left: 15px;
  top: 50%;
  transform: translateY(-50%);
}

/* Карточки артистов */
.artist-cards-moderation {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.artist-card-moderation {
  height: 200px;
  border-radius: 10px;
  background-size: cover;
  background-position: center;
  position: relative;
  overflow: hidden;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
  transition: transform 0.3s;
}

.artist-card-moderation:hover {
  transform: translateY(-5px);
}

.artist-info-moderation {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  background: rgba(0, 0, 0, 0.7);
  color: white;
  padding: 15px;
}

.artist-info-moderation h3 {
  margin: 0 0 5px 0;
}

/* Кнопки */
.moderate-btn {
  background: #ff5757;
  color: white;
  border: none;
  padding: 8px 8px;
  border-radius: 5px;
  cursor: pointer;
  display: flex;
  align-items: center;
  margin-top: 10px;
}

.add-concert-btn {
  background: #4CAF50;
  color: white;
  border: none;
  padding: 10px 15px;
  border-radius: 5px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 20px;
}

.add-concert-btn:hover {
  background: #3e8e41;
}

.material-icons {
  align-self: center;
  justify-self: center;
  width: 17px;
}

/* Модальные окна */
.modal-moderation {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal-content-moderation {
  background: white;
  border-radius: 10px;
  width: 90%;
  max-width: 800px;
  max-height: 90vh;
  overflow-y: auto;
  padding: 20px;
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
}

.modal-header-moderation {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding-bottom: 10px;
  border-bottom: 1px solid #eee;
}

.close-btn-moderation {
  background: none;
  border: none;
  cursor: pointer;
  color: #666;
}

/* Детали артиста */
.artist-details-moderation {
  display: flex;
  gap: 20px;
  margin-bottom: 20px;
}

.artist-details-moderation img {
  width: 150px;
  height: 150px;
  object-fit: cover;
  border-radius: 8px;
}

/* Список концертов */
.concerts-list-moderation {
  margin-top: 20px;
}

.concert-item-moderation {
  display: flex;
  justify-content: space-between;
  padding: 15px;
  border: 1px solid #eee;
  border-radius: 5px;
  margin-bottom: 10px;
}

.concert-actions-moderation {
  display: flex;
  align-items: center;
  gap: 10px;
}

/* Формы */
.edit-concert-form {
  padding: 20px;
}

.form-group {
  margin-bottom: 15px;
}

.form-group label {
  display: block;
  margin-bottom: 5px;
  font-weight: bold;
}

.form-group select,
.form-group input[type="date"],
.form-group input[type="time"] {
  width: 100%;
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
  gap: 10px;
}

.save-btn {
  background: #ff5757;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 4px;
  cursor: pointer;
}

.cancel-btn {
  background: #ddd;
  color: #333;
  border: none;
  padding: 8px 16px;
  border-radius: 4px;
  cursor: pointer;
}

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

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>