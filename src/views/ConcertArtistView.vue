<template>
  <div class="concert-page">
    <!-- Шапка артиста -->
    <div class="artist-background" :style="{backgroundImage: `url('${artist.backgroundPhotoArtist || defaultBackground}')`}"></div>
    
    <div class="artist-header-concert">
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
        
        <!-- Блок информации о конкретном концерте -->
        <div class="concert-info-block">
          <h2>Концерт</h2>
          <div class="concert-meta">
            <div class="concert-date-time">
              <i class="calendar-icon"></i>
              <span>{{ formatConcertDate(concert.dateStartConcert) }}, {{ formatTime(concert.timeStartConcert) }}</span>
            </div>
            <div class="concert-location">
              <i class="location-icon"></i>
              <span>{{ concert.idHallNavigation?.nameHall }}, {{ concert.idHallNavigation?.cityHall }}</span>
            </div>
            <div class="concert-age-limit">
              <i class="age-icon"></i>
              <span>Возраст: {{ concert.ageLimitConcert || 16 }}+</span>
            </div>
          </div>
          
          <div class="concert-price-range">
            <span>Цены от {{ minPrice }} ₽ до {{ maxPrice }} ₽</span>
          </div>
        </div>
        
        <div class="hall-scheme">
          <h2>Схема зала</h2>
          <div class="scheme-container">
            <SectionRender
              :sections="sections"
              :selected-seats="selectedSeats"
              :concert="this.idConcert"
              @toggle-seat="toggleSeatSelection"
              @select-dancefloor="selectDancefloor"
            />
            
            <div class="selected-seats-container" v-if="selectedSeats.length > 0 || dancefloorSelected">
              <h3>Выбранные места</h3>
              
              <!-- Танцпол -->
              <div class="selected-item" v-if="dancefloorSelected">
                <div class="seat-info">
                  <span>Танцпол ({{ dancefloorQuantity }} билетов)</span>
                </div>
                <div class="quantity-controls">
                  <button @click="decreaseDancefloor">
                    <span class="quantity-span">-</span>
                  </button>
                  <span>{{ dancefloorQuantity }}</span>
                  <button @click="increaseDancefloor">
                    <span class="quantity-span">+</span>
                  </button>
                </div>
                <div class="price-tag">{{ dancefloorPrice * dancefloorQuantity }} ₽</div>
                <button class="remove-btn" @click="removeDancefloor">×</button>
              </div>
              
              <div class="selected-item" v-for="(seat, index) in selectedSeats" :key="seat.id">
                <div class="seat-info">
                  <div class="seat-section">{{ seat.section.nameSection }}</div>
                  <div v-if="seat.isTable">
                    Стол {{ seat.unitNumber }}, Место {{ seat.chairNumber }}
                  </div>
                  <div v-else>
                    Ряд {{ seat.unitNumber }}, Место {{ seat.chairNumber }}
                  </div>
                </div>
                <div class="price-tag">{{ seat.price }} ₽</div>
                <button class="remove-btn" @click="removeSeat(index)">×</button>
              </div>
              
              <div class="total-price">
                <strong>Итого:</strong> {{ totalPrice.toFixed(2) }} ₽
              </div>
              
              <button class="clear-all-btn" @click="clearAllSelections">
                Убрать все
              </button>
              
              <button class="buy-button" @click="proceedToCheckout">
                Купить билеты
              </button>
            </div>
            
            <!-- Легенда (если ничего не выбрано) -->
            <div class="section-legend" v-else>
              <h3>Выберите места</h3>
              <p>Нажмите на стулья или танцпол для выбора</p>
              
              <div class="legend-item" v-for="section in sections" :key="section.idSection">
                <div class="legend-color" :style="{ backgroundColor: getSectionColor(section.priceSection) }"></div>
                <div class="legend-range">
                  {{ section.nameSection }} — {{ section.priceSection }} ₽
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
import SectionRender from '@/components/SectionRenderer.vue';
export default {
  components: {
    SectionRender
  },
  name: 'ConcertArtistView',
  props: {
    idArtist: {
      type: [String, Number],
      required: true
    },
    idConcert: {
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
      concert: {
        idArtistNavigation: {},
        idHallNavigation: {}
      },
          dancefloorSelected: false,
      dancefloorPrice: 0,
      sections: [],
      dancefloorQuantity: 0,
      generatedSeats: [],
      defaultAvatar: '/default-avatar.jpg',
      defaultBackground: '/default-background.jpg',
      minPrice: 0,
      maxPrice: 0,
      highlightedSection: null,
      selectedSection: null,
      priceColors: ['#ff9999', '#ff6666', '#ff3333', '#ff0000', '#cc0000'],
      selectedSeats: []
    };
  },
  computed: {

    totalPrice() {
    const seatTotal = this.selectedSeats.reduce((sum, seat) => sum + (Number(seat.price) || 0), 0);
    const danceTotal = this.dancefloorSelected ? 
      (Number(this.dancefloorPrice) || 0) * this.dancefloorQuantity : 0;
    return seatTotal + danceTotal;
  },
    dancefloorTotal() {
      return this.selectedSection ? this.dancefloorQuantity * this.selectedSection.priceSection : 0;
    }
  },
  async created() {
    await this.fetchData();
  },
  methods: {
    selectDancefloor(section) {
      if (!this.dancefloorSelected) {
        this.dancefloorSelected = true;
        this.dancefloorPrice = section.priceSection;
        this.dancefloorQuantity = 1;
        this.selectedSection = section; // Сохраняем информацию о секции
      }
    },
    
    removeDancefloor() {
      this.dancefloorSelected = false;
      this.dancefloorQuantity = 0;
    },
    
    removeSeat(index) {
      this.selectedSeats.splice(index, 1);
    },
    
    clearAllSelections() {
      this.selectedSeats = [];
      this.dancefloorSelected = false;
    },
    
    toggleSeatSelection(seat) {
  // Создаем уникальный идентификатор места
  const seatId = seat.isTable 
    ? `table-${seat.section.idSection}-${seat.unitNumber}-${seat.chairNumber}`
    : `row-${seat.section.idSection}-${seat.unitNumber}-${seat.chairNumber}`;

  // Проверяем, есть ли уже такое место в selectedSeats
  const existingIndex = this.selectedSeats.findIndex(s => {
    if (seat.isTable) {
      return s.section.idSection === seat.section.idSection && 
             s.unitNumber === seat.unitNumber && 
             s.chairNumber === seat.chairNumber;
    } else {
      return s.section.idSection === seat.section.idSection && 
             s.unitNumber === seat.unitNumber && 
             s.chairNumber === seat.chairNumber;
    }
  });

  if (existingIndex === -1) {
    // Добавляем новое место
    this.selectedSeats.push({
      id: seatId,
      section: seat.section,
      unitNumber: seat.unitNumber,
      chairNumber: seat.chairNumber,
      price: seat.price,
      isTable: seat.isTable
    });
  } else {
    // Удаляем место, если оно уже выбрано
    this.selectedSeats.splice(existingIndex, 1);
  }
},
    
    decreaseDancefloor() {
      if (this.dancefloorQuantity > 1) {
        this.dancefloorQuantity--;
      } else {
        this.removeDancefloor();
      }
    },

    handleDancefloorSelect(section) {
      this.selectedSection = section;
      this.dancefloorQuantity = 1; // Начальное количество
    },
    
    removeSeat(index) {
      this.selectedSeats.splice(index, 1);
    },
    
    clearAllSelections() {
      console.log(this.selectedSeats)
      this.selectedSeats = [];
      this.dancefloorQuantity = 0;
    },
    selectSection(section) {
      this.selectedSection = section
    },
    calculateSectionWidth(section) {
      const bounds = this.parsePathBounds(section.schemaSection);
      return bounds ? bounds.width * 0.3 : 300;
    },
    calculateSectionHeight(section) {
      const bounds = this.parsePathBounds(section.schemaSection);
      return bounds ? bounds.height * 0.3 : 200;
    },
    async fetchData() {
      try {
        this.loading = true;
        const artistResponse = await ApiService.getArtistById(this.idArtist);
        if (artistResponse) {
          this.artist = {
            ...artistResponse,
            idGenres: artistResponse.idGenres || [],
            profilePhotoArtist: artistResponse.profilePhotoArtist || this.defaultAvatar,
            backgroundPhotoArtist: artistResponse.backgroundPhotoArtist || this.defaultBackground
          };
        }
        console.log("концерт - " + this.idConcert)
        const concertResponse = await ApiService.getConcertById(this.idConcert);
        if (concertResponse) {
          this.concert = concertResponse;

          if (concertResponse.idHall) {
            const sectionsResponse = await ApiService.getHallSections(concertResponse.idHall);
            this.sections = sectionsResponse || [];
            console.log(sectionsResponse)
            if (this.sections.length > 0) {
              this.minPrice = Math.min(...this.sections.map(s => s.priceSection));
              this.maxPrice = Math.max(...this.sections.map(s => s.priceSection));
              this.generateAllTables();
            }
          }
        }
      } catch (error) {
        console.error('Ошибка загрузки данных:', error);
      } finally {
        this.loading = false;
      }
    },
    
    generateAllTables() {
      this.generatedSeats = [];

      this.sections.forEach((section) => {
        const type = section.typeSection?.toLowerCase() || '';
        if (type.includes('стол')) {
            this.generateTables(section);
          } else if (type.includes('ряд')) {
            this.generateRows(section);
          } else if (type.includes('танц')) {
            this.generateDancefloor(section);
          }

        const tables = section.unitCountSection || 0;
        const seatsPerTable = section.seatsPerUnitSection || 0;

        const bounds = this.parsePathBounds(section.schemaSection);
        if (!bounds) return;

        const startX = bounds.minX + 50;
        const startY = bounds.minY + 50;
        const tableSpacing = Math.min(bounds.width / 5, bounds.height / 5);

        for (let t = 0; t < tables; t++) {
          const centerX = startX + (t % 5) * tableSpacing;
          const centerY = startY + Math.floor(t / 5) * tableSpacing;
          const radius = tableSpacing * 0.3;

          if (!this.seatWithinSection(centerX, centerY, radius, bounds)) {
            continue;
          }

          this.generatedSeats.push({
            x: centerX,
            y: centerY,
            isTable: true,
            sectionId: section.idSection,
            tableNumber: t + 1
          });

          for (let s = 0; s < seatsPerTable; s++) {
            const angle = (Math.PI * 2 * s) / seatsPerTable;

            this.generatedSeats.push({
              x: centerX + radius * Math.cos(angle),
              y: centerY + radius * Math.sin(angle),
              selected: false,
              isTable: false,
              sectionId: section.idSection,
              tableNumber: t + 1,
              chairNumber: s + 1
            });
          }
        }
      });
    },
    generateTables(section) {
      const tables = section.unitCountSection || 0;
      const seatsPerTable = section.seatsPerUnitSection || 0;

      const bounds = this.parsePathBounds(section.schemaSection);
      if (!bounds) return;

      const startX = bounds.minX + 50;
      const startY = bounds.minY + 50;
      const tableSpacing = Math.min(bounds.width / 5, bounds.height / 5);

      for (let t = 0; t < tables; t++) {
        const centerX = startX + (t % 5) * tableSpacing;
        const centerY = startY + Math.floor(t / 5) * tableSpacing;
        const radius = tableSpacing * 0.3;

        if (!this.seatWithinSection(centerX, centerY, radius, bounds)) continue;

        this.generatedSeats.push({
          x: centerX,
          y: centerY,
          isTable: true,
          sectionId: section.idSection,
          tableNumber: t + 1
        });

        for (let s = 0; s < seatsPerTable; s++) {
          const angle = (Math.PI * 2 * s) / seatsPerTable;
          this.generatedSeats.push({
            x: centerX + radius * Math.cos(angle),
            y: centerY + radius * Math.sin(angle),
            selected: false,
            isTable: false,
            sectionId: section.idSection,
            tableNumber: t + 1,
            chairNumber: s + 1
          });
        }
      }
    },
    generateRows(section) {
  const rows = section.unitCountSection || 0;
  const seatsPerRow = section.seatsPerUnitSection || 0;

  const bounds = this.parsePathBounds(section.schemaSection);
  if (!bounds) return;

  const rowSpacing = bounds.height / (rows + 1);
  const seatSpacing = bounds.width / (seatsPerRow + 1);

  for (let r = 0; r < rows; r++) {
    const y = bounds.minY + (r + 1) * rowSpacing;

    for (let s = 0; s < seatsPerRow; s++) {
      const x = bounds.minX + (s + 1) * seatSpacing;

      this.generatedSeats.push({
        x, y,
        isTable: false,
        selected: false,
        sectionId: section.idSection,
        rowNumber: r + 1,
        chairNumber: s + 1
      });
    }
  }
},
generateDancefloor(section) {
  // Можно визуально не рендерить, но выделить центр, чтобы показать активность секции
  const bounds = this.parsePathBounds(section.schemaSection);
  if (!bounds) return;

  const centerX = bounds.minX + bounds.width / 2;
  const centerY = bounds.minY + bounds.height / 2;

  this.generatedSeats.push({
    x: centerX,
    y: centerY,
    isTable: false,
    isDancefloor: true,
    sectionId: section.idSection
  });
},
    parsePathBounds(pathData) {
      if (!pathData) return null;
      
      const points = pathData.match(/[ML]\s*([\d.]+)\s*([\d.]+)/g);
      if (!points || points.length === 0) return null;
      
      let minX = Infinity, minY = Infinity, maxX = -Infinity, maxY = -Infinity;
      
      points.forEach(point => {
        const coords = point.match(/[ML]\s*([\d.]+)\s*([\d.]+)/);
        if (coords) {
          const x = parseFloat(coords[1]);
          const y = parseFloat(coords[2]);
          minX = Math.min(minX, x);
          minY = Math.min(minY, y);
          maxX = Math.max(maxX, x);
          maxY = Math.max(maxY, y);
        }
      });
      
      return {
        minX, minY, maxX, maxY,
        width: maxX - minX,
        height: maxY - minY
      };
    },
    
    
    
    selectSection(section) {
      this.selectedSection = section;
      this.highlightedSection = section.idSection;
    },
    
    getSectionColor(price) {
      if (this.maxPrice === this.minPrice) return this.priceColors[0];
      
      const ratio = (price - this.minPrice) / (this.maxPrice - this.minPrice);
      const index = Math.min(Math.floor(ratio * this.priceColors.length), this.priceColors.length - 1);
      return this.priceColors[index];
    },
    
    highlightSection(sectionId) {
      this.highlightedSection = sectionId;
    },
    
    unhighlightSection() {
      if (!this.selectedSection) {
        this.highlightedSection = null;
      }
    },
    
    getTextX(pathData) {
      if (!pathData) return 100;
      const matches = pathData.match(/M([\d.]+)/);
      return matches ? parseFloat(matches[1]) + 50 : 100;
    },
    
    getTextY(pathData) {
      if (!pathData) return 100;
      const matches = pathData.match(/M[\d.]+\s([\d.]+)/);
      return matches ? parseFloat(matches[1]) + 30 : 100;
    },
    
    formatConcertDate(dateString) {
      if (!dateString) return '';
      const options = { weekday: 'long', day: 'numeric', month: 'long' };
      return new Date(dateString).toLocaleDateString('ru-RU', options);
    },
    
    formatTime(timeString) {
      if (!timeString) return '--:--';
      return timeString.slice(0, 5);
    },
    
    handleImageError(event) {
      if (event.target.classList.contains('profile-photo')) {
        event.target.src = this.defaultAvatar;
      } else if (event.target.classList.contains('artist-background')) {
        event.target.style.backgroundImage = `url('${this.defaultBackground}')`;
      }
    },
    
    increaseDancefloor() {
      if (this.dancefloorQuantity < this.selectedSection.totalSeatsSection) {
        this.dancefloorQuantity++;
      }
    },
    
    decreaseDancefloor() {
      if (this.dancefloorQuantity > 0) this.dancefloorQuantity--;
    },
    
    seatWithinSection(x, y, radius, bounds) {
      return (
        x - radius >= bounds.minX &&
        x + radius <= bounds.maxX &&
        y - radius >= bounds.minY &&
        y + radius <= bounds.maxY
      );
    },
    
    proceedToCheckout() {
  if ((this.selectedSeats.length === 0 && !this.dancefloorSelected)) {
    alert('Пожалуйста, выберите хотя бы одно место или танцпол');
    return;
  }

  // Подготовка данных для передачи
  const ticketData = {
    concert: this.concert,
    artist: this.artist,
    seats: this.selectedSeats.map(seat => ({
      id_section: seat.section.idSection,
      section_name: seat.section.nameSection,
      section_type: seat.section.typeSection,
      unit_ticket: seat.unitNumber,
      seat_ticket: seat.chairNumber,
      price: seat.section.priceSection
    })),
    dancefloor: this.dancefloorSelected ? {
      id_section: this.selectedSection.idSection,
      quantity: this.dancefloorQuantity,
      price: this.selectedSection.priceSection
    } : null,
    totalPrice: this.totalPrice
  };

  // Сохраняем в localStorage как строку
  localStorage.setItem('ticketData', JSON.stringify(ticketData));
  
  // Переходим на страницу оплаты
  this.$router.push({ name: 'PurchaseTickets' });
}
  }
};
</script>

<style scoped>
.selected-seats-container {
  padding: 20px;
  background: #f8f8f8;
  border-radius: 8px;
}
.quantity-controls {
  display: flex;
  align-items: center;
  gap: 5px;
}

.quantity-span {
  color: white;
  font-size: 20px;
  line-height: 1;
  display: inline-block;
  vertical-align: middle;
}
.quantity-controls button {
  margin-top: 0px;
  width: 25px;
  height: 25px;
  border: 1px solid #ddd;
  background: #ff5757;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0;
}
.selected-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px;
  margin-bottom: 8px;
  background: white;
  border-radius: 4px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
}
.price-buy-tickets {
  font-weight: bold;
  color: #ff5757;
}

.remove-btn {
  background: none;
  border: none;
  color: #999;
  font-size: 1.2rem;
  cursor: pointer;
  padding: 0 5px;
}

.remove-btn:hover {
  color: #ff5757;
}

.total-price {
  text-align: right;
  margin: 15px 0;
  font-size: 1.2rem;
}

.clear-all-btn {
  background: #b3b3b3;
  border: none;
  padding: 8px 15px;
  border-radius: 4px;
  margin-right: 10px;
  cursor: pointer;
}

.clear-all-btn:hover {
  background: #e0e0e0;
}

.buy-button {
  background: #ff5757;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 4px;
  cursor: pointer;
  font-weight: bold;
}

.buy-button:hover {
  background: #e04a4a;
}
.sections-container {
  display: flex;
  flex-wrap: wrap;
  gap: 20px;
  padding: 20px;
  justify-content: center;
  background: #f8f8f8;
}
.sections-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
  padding: 20px;
}
/* Основные стили страницы */
.concert-page {
  position: relative;
  min-height: 100vh;
}

.artist-background {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 400px;
  background-size: cover;
  background-position: center;
  z-index: 0;
  filter: brightness(0.7);
}

.artist-header-concert {
  position: relative;
  z-index: 1;
  padding-top: 90px;
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.artist-photo-container {
  text-align: center;
  margin-bottom: 20px;
}

.profile-photo-wrapper {
  width: 150px;
  height: 150px;
  border-radius: 20%;
  overflow: hidden;
  margin: 0 auto 15px;
  border: 4px solid white;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}

.profile-photo {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.artist-name {
  font-size: 2.5rem;
  margin: 0;
  color: white;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.5);
}

.genres-list {
  color: #eee;
  font-size: 1.1rem;
  margin-top: 10px;
}

.artist-details {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  padding: 30px;
  width: 100%;
  margin-top: 20px;
}

.artist-description {
  font-size: 1.1rem;
  line-height: 1.6;
  color: #555;
  margin-bottom: 30px;
}

/* Стили блока информации о концерте */
.concert-info-block {
  margin: 30px 0;
  padding: 25px;
  background: #f9f9f9;
  border-radius: 10px;
  border-left: 4px solid #ff5757;
}

.concert-info-block h2 {
  font-size: 1.8rem;
  margin-top: 0;
  margin-bottom: 20px;
  color: #333;
}

.concert-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 25px;
  margin-bottom: 15px;
}

.concert-date-time,
.concert-location,
.concert-age-limit {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 1.1rem;
  color: #555;
}

.concert-price-range {
  font-size: 1.2rem;
  font-weight: bold;
  color: #ff5757;
  padding: 8px 16px;
  background: rgba(255, 87, 87, 0.1);
  border-radius: 20px;
  display: inline-block;
}

/* Стили схемы зала */
.hall-scheme {
  margin-top: 30px;
}

.scheme-container {
  border: 1px solid #eee;
  border-radius: 8px;
  overflow: hidden;
  margin-top: 15px;
}

.scheme-svg-container {
  width: 100%;
  height: 600px;
  background: #f8f8f8;
  overflow: hidden;
  display: flex;
  justify-content: center;
  align-items: center;
}

.hall-svg {
  margin-top: 5%;
  margin-left: 25%;
  width: auto;
  height: 100%;
}

.section-path {
  stroke: #fff;
  stroke-width: 2;
  cursor: pointer;
  transition: all 0.3s;
  opacity: 0.8;
}

.section-path:hover {
  opacity: 1;
  stroke-width: 3;
}

.section-path.highlighted {
  opacity: 1;
  stroke: #ff5757;
  stroke-width: 4;
}

.section-label {
  font-size: 30px;
  font-weight: bold;
  fill: #fff;
  pointer-events: none;
  dominant-baseline: middle;
  text-anchor: middle;
}

/* Стили мест */
.seat {
  cursor: pointer;
  transition: all 0.2s;
}

.seat:hover {
  stroke: #ff5722;
  stroke-width: 2;
}

.seat.selected {
  stroke: #ff5722;
  stroke-width: 2;
  filter: drop-shadow(0 0 4px rgba(255, 87, 87, 0.7));
}

/* Информация о секции */
.sections-info {
  padding: 20px;
  background: #fff;
  border-top: 1px solid #eee;
}

.section-details {
  text-align: center;
}

.section-details h3 {
  font-size: 1.5rem;
  margin-bottom: 15px;
  color: #333;
}

.section-details p {
  font-size: 1.1rem;
  margin-bottom: 10px;
}

.buy-button {
  width: 100%;
  padding: 12px;
  background: #ff5757;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 1.1rem;
  font-weight: bold;
  margin-top: 20px;
  cursor: pointer;
  transition: all 0.3s;
}

.buy-button:hover {
  background: #e04a4a;
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
}

.buy-button:disabled {
  background: #ccc;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

/* Легенда */
.section-legend {
  text-align: center;
}

.section-legend h3 {
  font-size: 1.5rem;
  margin-bottom: 15px;
  color: #333;
}

.section-legend p {
  font-size: 1.1rem;
  color: #666;
  margin-bottom: 20px;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 8px;
  justify-content: center;
}

.legend-color {
  width: 20px;
  height: 20px;
  border-radius: 4px;
}

.legend-range {
  font-size: 1.1rem;
}

/* Адаптивные стили */
@media (max-width: 768px) {
  .artist-header {
    padding-top: 100px;
  }
  
  .artist-details {
    padding: 20px;
  }
  
  .concert-meta {
    flex-direction: column;
    gap: 12px;
  }
  
  .scheme-svg-container {
    height: 400px;
  }
  
  .section-label {
    font-size: 20px;
  }
}
</style>