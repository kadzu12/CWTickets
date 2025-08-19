<template>
  <div class="purchase-background-container">
    <div class="purchase-wrapper">
      <div class="purchase-container">
        <h2>Оформление билета</h2>
        
        <div v-if="loading" class="loading">Загрузка данных билета...</div>
        
        <div v-if="ticketData && ticketData.artist && ticketData.concert">
          <div class="concert-info-parchase">
            <h3>{{ ticketData.artist.nameArtist }}</h3>
            <p>{{ formatDate(ticketData.concert.dateStartConcert) }}, {{ formatTime(ticketData.concert.timeStartConcert) }}</p>
            <p>{{ ticketData.concert.idHallNavigation?.nameHall }}, {{ ticketData.concert.idHallNavigation?.cityHall }}</p>
          </div>
          
          <div class="section-info">
            <div class="ticket-summary">
              <div v-if="ticketData.seats && ticketData.seats.length">
                <h4>Выбранные места:</h4>
                <ul>
                  <li v-for="(seat, index) in ticketData.seats" :key="index">
                    <template v-if="seat.section_type === 'Столы'">
                      Стол {{ seat.unit_ticket }}, Место {{ seat.seat_ticket }}
                    </template>
                    <template v-else-if="seat.section_type === 'Ряды'">
                      Ряд {{ seat.unit_ticket }}, Место {{ seat.seat_ticket }}
                    </template>
                  </li>
                </ul>
              </div>
              
              <div v-if="ticketData.dancefloor">
                <p>Танцпол: {{ ticketData.dancefloor.quantity }} билет(ов)</p>
              </div>
              
              <h3>Итоговая сумма: {{ ticketData.totalPrice }} ₽</h3>
            </div>
          </div>
          
          <form @submit.prevent="purchaseTicket">
            <div class="payment-form">
              <h3>Платежные данные</h3>
              
              <div class="form-group">
                <label>Номер карты</label>
                <input 
                  v-model="cardNumber"
                  @input="formatCardNumber"
                  placeholder="0000 0000 0000 0000"
                  maxlength="19"
                  required
                >
              </div>
              
              <div class="form-row">
                <div class="form-group">
                  <label>Срок действия</label>
                  <input 
                    v-model="cardExpiry"
                    @input="formatExpiry"
                    placeholder="MM/ГГ"
                    maxlength="5"
                    required
                  >
                </div>
                
                <div class="form-group">
                  <label>CVV</label>
                  <input 
                    v-model="cardCvv"
                    type="password"
                    placeholder="000"
                    maxlength="3"
                    required
                  >
                </div>
              </div>
              
              <button 
                type="submit"
                :disabled="!isFormValid || processing"
                class="pay-button"
              >
                Оплатить {{ ticketData.totalPrice }} ₽
              </button>
            </div>
          </form>
        </div>
        
        <div v-else class="error-message">
          <h3>Данные билета не найдены</h3>
          <p>Пожалуйста, выберите места заново</p>
          <button @click="$router.push({ name: 'Home' })">Вернуться на главную</button>
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

export default {
  data() {
    return {
      ticketData: null,
      cardNumber: '',
      cardExpiry: '',
      cardCvv: '',
      loading: false,
      toasts: [],
      processing: false
    };
  },
  computed: {
    isFormValid() {
      return (
        this.cardNumber.replace(/\s/g, '').length === 16 &&
        this.cardExpiry.length === 5 &&
        this.cardCvv.length === 3
      );
    }
  },
  methods: {
    showToast(message, type = 'success') {
      const id = Date.now();
      this.toasts.push({ id, message, type });
      setTimeout(() => {
        this.toasts = this.toasts.filter(t => t.id !== id);
      }, 3000);
    },
    async purchaseTicket() {
      const storedUser = localStorage.getItem('user');
      if (!storedUser || storedUser === 'undefined') {
        alert('Пожалуйста, авторизуйтесь');
        return;
      }
      
      const user = JSON.parse(storedUser);
      this.processing = true;

      try {
        // Подготовка данных для API
        const ticketsData = [];
        
        // Добавляем обычные места
        if (this.ticketData.seats && this.ticketData.seats.length) {
          ticketsData.push(...this.ticketData.seats.map(seat => ({
            idConcert: this.ticketData.concert.idConcert,
            idUser: user.idUser,
            idSection: seat.id_section,
            rowTicket: seat.unit_ticket,
            seatTicket: seat.seat_ticket
          })));
        }
        
        // Добавляем танцпол
        if (this.ticketData.dancefloor) {
          for (let i = 0; i < this.ticketData.dancefloor.quantity; i++) {
            ticketsData.push({
              idConcert: this.ticketData.concert.idConcert,
              idUser: user.idUser,
              idSection: this.ticketData.dancefloor.id_section,
              rowTicket: null,
              seatTicket: null
            });
          }
        }
        // Отправка на сервер
        const response = await ApiService.purchaseTickets({
          tickets: ticketsData,
          cardNumber: this.cardNumber.replace(/\s/g, '')
        });

        // Очищаем localStorage после успешной покупки
        localStorage.removeItem('ticketData');
        
        this.$router.push({
          name: 'PurchaseSuccess'
        });
        
      } catch (error) {
        console.error('Ошибка покупки:', error);
        this.showToast(error.message || 'Ошибка при оформлении билета', 'error');
      } finally {
        this.processing = false;
      }
    },
    
    formatCardNumber() {
      let value = this.cardNumber.replace(/\s/g, '');
      if (value.length > 16) value = value.substr(0, 16);
      this.cardNumber = value.replace(/(\d{4})/g, '$1 ').trim();
    },
    
    formatExpiry() {
      let value = this.cardExpiry.replace(/\D/g, '');
      if (value.length > 4) value = value.substr(0, 4);
      if (value.length > 2) {
        value = value.substr(0, 2) + '/' + value.substr(2);
      }
      this.cardExpiry = value;
    },
    
    formatDate(dateString) {
      const options = { day: 'numeric', month: 'long', year: 'numeric' };
      return new Date(dateString).toLocaleDateString('ru-RU', options);
    },
    
    formatTime(timeString) {
      if (!timeString) return '--:--';
      return timeString.slice(0, 5);
    },
    
    async loadTicketData() {
      this.loading = true;
      try {
        const data = localStorage.getItem('ticketData');
        
        if (!data) {
          throw new Error('Данные билета не найдены');
        }
        
        this.ticketData = JSON.parse(data);
        
        // Проверка обязательных полей
        if (!this.ticketData?.artist || !this.ticketData?.concert) {
          throw new Error('Неполные данные билета');
        }
        
      } catch (error) {
        console.error('Ошибка загрузки данных:', error);
        localStorage.removeItem('ticketData');
        this.$router.push({ name: 'Home' });
      } finally {
        this.loading = false;
      }
    }
  },
  async mounted() {
    await this.loadTicketData();
  }
};
</script>

<style scoped>
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
.purchase-background-container {
    position: relative;
    min-height: 100vh;
    width: 100%;
    background-image: url("@/assets/appAssets/back_buy_tickets.jpg");
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
    overflow: hidden;
}

.purchase-background-container::before {
    content: "";
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.3);
    backdrop-filter: blur(2px);
    z-index: 0;
}

.purchase-wrapper {
    position: relative;
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 100vh;
    padding: 20px;
    box-sizing: border-box;
    z-index: 1;
}

.purchase-container {
    max-width: 600px;
    width: 100%;
    padding: 20px;
    margin-top: 50px;
    background-color: rgba(255, 255, 255, 0.5);
    border-radius: 10px;
    box-shadow: 0 0 15px rgba(0, 0, 0, 0.2);
}

.concert-info-parchase{
    margin-top: 20px;
    margin-bottom: 20px;
    padding: 15px;
    background: #f8f8f8;
    border-radius: 8px;
}
  
.section-info {
    margin-bottom: 25px;
    padding: 15px;
    border: 1px solid #eee;
    border-radius: 8px;
}
  
.payment-form {
    margin-top: 20px;
}
  
.form-group {
    margin-bottom: 15px;
}
  
.form-row {
    display: flex;
    gap: 15px;
}
  
.form-row .form-group {
    flex: 1;
}
  
label {
    display: block;
    margin-bottom: 5px;
    font-weight: bold;
}
  
input {
    width: 100%;
    padding: 10px;
    border: 1px solid #ddd;
    border-radius: 4px;
}
.ticket-summary {
    margin-top: 20px;
    padding: 15px;
    border: 1px solid #ddd;
    border-radius: 8px;
    background: #f0f0f0;
}

.ticket-summary ul {
    padding-left: 20px;
    margin-bottom: 10px;
}

.ticket-summary li {
    margin-bottom: 5px;
}
.pay-button {
    width: 100%;
    padding: 12px;
    background: #ff5757;
    color: white;
    border: none;
    border-radius: 6px;
    font-size: 16px;
    cursor: pointer;
}
  
.pay-button:disabled {
    background: #ccc;
    cursor: not-allowed;
}
  
.loading {
    text-align: center;
    padding: 20px;
    font-size: 18px;
}
</style>