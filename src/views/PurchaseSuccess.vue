<template>
    <div class="success-background-container">
      <div class="success-wrapper">
        <div class="success-container">
          <!-- Состояние ожидания оплаты -->
          <div v-if="!paymentSuccess" class="payment-pending">
            <div class="wave-loader"></div>
            <h2 class="loading-text">Ожидание оплаты<span class="dots">{{ animatedDots }}</span></h2>
          </div>
          
          <!-- Состояние успешной оплаты -->
          <div v-if="paymentSuccess" class="payment-success">
            <div class="success-icon">✓</div>
            <h2>Оплата прошла успешно!</h2>
            <p>Ваш билет направлен в личный кабинет и электронную почту</p>
            <button @click="goToProfile" class="profile-button">Перейти в личный кабинет</button>
          </div>
        </div>
      </div>
    </div>
  </template>
  
  <script>
  export default {
    data() {
      return {
        paymentSuccess: false,
        animatedDots: '',
        dotsInterval: null
      };
    },
    methods: {
      animateDots() {
        let dotCount = 0;
        this.dotsInterval = setInterval(() => {
          dotCount = (dotCount + 1) % 4;
          this.animatedDots = '.'.repeat(dotCount);
        }, 500);
      },
      simulatePayment() {
        // Только визуальная симуляция без логики
        setTimeout(() => {
          this.paymentSuccess = true;
          clearInterval(this.dotsInterval);
        }, 3000);
      },
      goToProfile() {
        this.$router.push({ name: 'Profile' });
      }
    },
    mounted() {
      this.animateDots();
      this.simulatePayment();
    },
    beforeDestroy() {
      if (this.dotsInterval) {
        clearInterval(this.dotsInterval);
      }
    }
  };
  </script>
  
  <style scoped>
  .success-background-container {
    position: relative;
    min-height: 100vh;
    width: 100%;
    background-image: url("@/assets/appAssets/back_buy_tickets.jpg");
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
    overflow: hidden;
  }
  
  .success-background-container::before {
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
  
  .success-wrapper {
    position: relative;
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 100vh;
    padding: 20px;
    box-sizing: border-box;
    z-index: 1;
  }
  
  .success-container {
    max-width: 600px;
    width: 100%;
    padding: 40px;
    background-color: rgba(255, 255, 255, 0.9);
    border-radius: 10px;
    box-shadow: 0 0 15px rgba(0, 0, 0, 0.2);
    text-align: center;
  }
  
  .payment-pending {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 300px;
  }
  
  .wave-loader {
    width: 80px;
    height: 80px;
    margin-bottom: 20px;
    position: relative;
  }
  
  .wave-loader:before, .wave-loader:after {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    border-radius: 50%;
    background: rgba(255, 87, 87, 0.2);
    animation: wave 1.5s infinite ease-in-out;
  }
  
  .wave-loader:after {
    animation-delay: -0.5s;
  }
  
  @keyframes wave {
    0%, 100% {
      transform: scale(0);
      opacity: 1;
    }
    50% {
      transform: scale(1);
      opacity: 0;
    }
  }
  
  .loading-text {
    font-size: 24px;
    color: #333;
    margin-top: 20px;
  }
  
  .dots {
    display: inline-block;
    width: 30px;
    text-align: left;
  }
  
  .payment-success {
    animation: fadeIn 0.5s ease-in-out;
  }
  
  @keyframes fadeIn {
    from { opacity: 0; transform: translateY(20px); }
    to { opacity: 1; transform: translateY(0); }
  }
  
  .success-icon {
    width: 80px;
    height: 80px;
    margin: 0 auto 20px;
    background-color: #4CAF50;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: white;
    font-size: 40px;
    font-weight: bold;
    animation: bounce 0.5s;
  }
  
  @keyframes bounce {
    0%, 20%, 50%, 80%, 100% {transform: translateY(0);}
    40% {transform: translateY(-20px);}
    60% {transform: translateY(-10px);}
  }
  
  .profile-button {
    padding: 12px 24px;
    background: #ff5757;
    color: white;
    border: none;
    border-radius: 6px;
    font-size: 16px;
    cursor: pointer;
    transition: all 0.3s;
    margin-top: 20px;
  }
  
  .profile-button:hover {
    background: #e04a4a;
    transform: translateY(-2px);
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
  }
  </style>