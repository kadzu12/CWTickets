<template>
    <div class="auth-container">
      <div class="auth-header">
        <h2>С возвращением!</h2>
        <p>Нет аккаунта? <router-link :to="{ name: 'RegisterView' }" class="sign-up">Зарегистрируйтесь</router-link></p>
      </div>
  
      <form @submit.prevent="handleLogin" class="auth-form">
        <div v-if="errorMessage" class="error-message">{{ errorMessage }}</div>
        
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
          <label for="password">Пароль</label>
          <input
            type="password"
            id="password"
            v-model="password"
            required
            placeholder="Введите ваш пароль"
          />
        </div>
  
        <div class="remember-me">
          <input
            type="checkbox"
            id="remember"
            v-model="rememberMe"
          />
          <label for="remember">Запомнить меня</label>
        </div>
  
        <button type="submit" class="auth-button" :disabled="loading">
          <span v-if="loading">Вход...</span>
          <span v-else>Войти</span>
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
  
  export default {
    data() {  
      return {
        email: '',
        password: '',
        rememberMe: false,
        loading: false,
        errorMessage: '',
        toasts: []
      }
    },
    methods: {
      async handleLogin() {
        this.loading = true;
        this.errorMessage = '';
        
        try {
          const response = await ApiService.login({
            email: this.email,
            password: this.password
          });
          console.log(response)
          // Сохраняем токен (пример для localStorage)
        //   localStorage.setItem('authToken', response.token);
          localStorage.setItem('user', JSON.stringify(response));
          
          // Показываем уведомление об успехе
          this.showToast('Вы успешно вошли в систему');
          
          // Перенаправляем на главную
          this.$router.push('/');
        } catch (error) {
          console.error("Ошибка входа:", error);
          this.errorMessage = 'Неверный email или пароль';
          this.showToast('Ошибка входа. Проверьте данные', 'error');
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
  
  /* Остальные стили остаются без изменений */
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
  
    .sign-up{
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
    
    .remember-me {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin: 0.5rem 0;
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
    }
    
    .auth-button:hover {
      background-color: #e04a4a;
    }
  </style>