<template>
    <div class="auth-container">
      <div class="auth-header">
        <h2>Создать аккаунт</h2>
        <p>Уже есть аккаунт? <router-link to="/login" class="sign-up">Войдите</router-link></p>
      </div>
  
      <form @submit.prevent="handleRegister" class="auth-form">
        <div v-if="errorMessage" class="error-message">{{ errorMessage }}</div>
        
        <div class="form-group">
          <label for="firstName">Имя</label>
          <input
            type="text"
            id="firstName"
            v-model="firstName"
            required
            placeholder="Введите ваше имя"
          />
        </div>
  
        <div class="form-group">
          <label for="lastName">Фамилия</label>
          <input
            type="text"
            id="lastName"
            v-model="lastName"
            required
            placeholder="Введите вашу фамилию"
          />
        </div>
        <div class="form-group">
          <label for="login">Логин</label>
          <input
            type="text"
            id="login"
            v-model="login"
            required
            placeholder="Придумайте логин"
          />
        </div>
        <div class="form-group">
          <label for="email">Электронная почта</label>
          <input
            type="email"
            id="email"
            v-model="email"
            required
            placeholder="Введите ваш email"
          />
        </div>
        <div class="form-group">
          <label for="birthDate">Дата рождения</label>
          <input
            type="date"
            id="birthDate"
            v-model="birthDate"
            required
            :max="maxBirthDate"
            @change="validateBirthDate"
          />
        </div>

        <div class="form-group">
          <label for="city">Город</label>
          <CitySelect
              v-model="city"
              :cities="cities"
              placeholder="Начните вводить город..."
              :loading="cities.length === 0"
            />
        </div>
        <div class="form-group">
          <label for="password">Пароль</label>
          <input
            type="password"
            id="password"
            v-model="password"
            required
            placeholder="Введите пароль (минимум 6 символов)"
            minlength="6"
          />
        </div>
  
        <div class="form-group">
          <label for="confirmPassword">Подтвердите пароль</label>
          <input
            type="password"
            id="confirmPassword"
            v-model="confirmPassword"
            required
            placeholder="Повторите пароль"
            minlength="6"
          />
        </div>
  
        <button type="submit" class="auth-button" :disabled="loading">
          <span v-if="loading">Регистрация...</span>
          <span v-else>Зарегистрироваться</span>
        </button>
      </form>
      
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
  import CitySelect from '@/components/CitySelect.vue';
  export default {
    components: {
      CitySelect
  },
    data() {
      return {
        firstName: '',
        lastName: '',
        email: '',
        login: '',
        birthDate: '',
        city: '',
        password: '',
        confirmPassword: '',
        loading: false,
        errorMessage: '',
        toasts: [],
        cities: [],
        city: ''
      }
    },
    async created() {
        await this.fetchCities();
      },
    computed: {
      maxBirthDate() {
        const today = new Date();
        const maxDate = new Date(today.getFullYear() - 14, today.getMonth(), today.getDate());
        return this.formatDateForInput(maxDate);
      },
      formattedBirthDate() {
        // ваш существующий код форматирования даты
      }
    },
        methods: {
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
      async fetchCities() {
        try {
          const response = await ApiService.getCities();
          this.cities = response;
        } catch (error) {
          console.error("Ошибка при получении городов:", error);
          this.cities = [];
        }
      },

      async handleRegister() {
        if (this.password !== this.confirmPassword) {
          this.errorMessage = 'Пароли не совпадают';
          this.showToast('Пароли не совпадают', 'error');
          return;
        }
  
        this.loading = true;
        this.errorMessage = '';
        
        try {
          const userData = {
            firstName: this.firstName,
            lastName: this.lastName,
            birthDate: this.birthDate,
            login: this.login,
            email: this.email,
            city: this.city,
            password: this.password
          };
  
          const response = await ApiService.register(userData);
          
          // Показываем уведомление об успехе
          this.showToast('Регистрация прошла успешно!');
          
          // Автоматически входим после регистрации
          // localStorage.setItem('authToken', response.token);
          // localStorage.setItem('user', JSON.stringify(response.user));
          
          // Перенаправляем на главную
          this.$router.push('/login');
        } catch (error) {
          console.error("Ошибка регистрации:", error);
          this.errorMessage = error.message || 'Ошибка при регистрации';
          this.showToast(this.errorMessage, 'error');
        } finally {
          this.loading = false;
        }
      },
      
      showToast(message, type = 'success') {
        const id = Date.now();
        this.toasts.push({ id, message, type });
        setTimeout(() => {
          this.toasts = this.toasts.filter(t => t.id !== id);
        }, 3000);
      }
      
    }
  }
  </script>
  
  <style scoped>
  .error-message {
    color: #ff5757;
    margin-bottom: 1rem;
    text-align: center;
  }
  
  .auth-button:disabled {
    background-color: #ccc;
    cursor: not-allowed;
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
  
  @keyframes fadeIn {
    from { opacity: 0; transform: translateY(20px); }
    to { opacity: 1; transform: translateY(0); }
  }
  
  /* Остальные стили аналогичны компоненту входа */
  .auth-container {
      max-width: 400px;
      margin: 5% auto 0 auto;
      padding: 2rem;
      background: #fff;
      border-radius: 8px;
      box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
    }
    
    .auth-header {
      text-align: center;
      margin-bottom: 2rem;
    }
    
    .auth-header h2 {
      font-size: 1.5rem;
      margin-bottom: 0.5rem;
    }
  
    .sign-up {
      color: #ff5757;
    }
    
    .auth-form {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    
    .form-group {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }
    
    .form-group label {
      font-weight: 500;
      align-self: flex-start;
    }
    
    .form-group input {
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 1rem;
    }
    
    .auth-button {
      padding: 0.75rem;
      background-color: #ff5757;
      color: white;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 1rem;
      transition: background-color 0.3s;
      margin-top: 1rem;
    }
    
    .auth-button:hover {
      background-color: #e04a4a;
    }
  </style>