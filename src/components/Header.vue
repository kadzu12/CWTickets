<template>  
  <header>
      <div class="logo">
        <img src="/logo-CWTICKET.png" alt="Logo" />
        <div class="brand-name">CWTickets</div>
      </div>
      <nav>
        <ul class="nav-links">
          <li>
            <router-link :to="{ name: 'Home' }">Главная</router-link>
          </li>
          <li>
            <router-link :to="{ name: 'ArtistsAllTabView' }">Артисты</router-link>
          </li>
          <li v-if="user?.idRole === 2">
            <router-link :to="{ name: 'Moderation-panel' }">Модерация сайта</router-link>
          </li>
        </ul>
      </nav>
      <div class="cta" v-if="!user">
        <router-link :to="{ name: 'Login'}">
          <a href="#" class="login">Войти</a>
        </router-link>
      </div>
      <div class="user-info" v-if="user">
        <div class="user-city" v-if="user.cityUser">
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 384 512"><path d="M215.7 499.2C267 435 384 279.4 384 192C384 86 298 0 192 0S0 86 0 192c0 87.4 117 243 168.3 307.2c12.3 15.3 35.1 15.3 47.4 0zM192 128a64 64 0 1 1 0 128 64 64 0 1 1 0-128z"/></svg>
          {{ user.cityUser }}
        </div>
        <div class="user-menu" @click="toggleDropdown">
          <span class="user-name">{{ user.loginUser }}</span>
          <div class="dropdown-menu" v-show="dropdownOpen">
            <router-link to="/profile" @click="closeDropdown">
              <i><img class="user-menu-img" src="@/assets/appAssets/user-icon.jpg" alt="user-icon"></i>  Профиль
            </router-link>
            <a href="/profile#tickets" @click="closeDropdown">
              <i><img class="user-menu-img" src="@/assets/appAssets/ticket-icon.jpg" alt="ticket-icon"></i>  Мои билеты
            </a>
            <a href="/profile#favorites" @click="closeDropdown">
              <i><img class="user-menu-img" src="@/assets/appAssets/favorite-icon.jpg" alt="favorite-icon"></i>  Избранное
            </a>
            <router-link :to="{ name: 'Home' }" @click="logout">
              <i><img class="user-menu-img" src="@/assets/appAssets/logout-icon.jpg" alt="logout-icon"></i>  Выйти
            </router-link>
          </div>
        </div>
      </div>
  </header>
</template>

<script>
export default {
  data() {
    return {
      dropdownOpen: false,
      user: null
    };
  },
  created() {
    this.loadUser();
    this.setDefaultCityFilter();
  },
  methods: {
    loadUser() {
      try {
        const storedUser = localStorage.getItem('user');
        this.user = storedUser && storedUser !== 'undefined' ? JSON.parse(storedUser) : null;
      } catch (e) {
        console.error('Ошибка парсинга user из localStorage:', e);
        this.user = null;
      }
    },
    setDefaultCityFilter() {
      if (this.user?.cityUser) {
        // Сохраняем город пользователя в хранилище для использования в других компонентах
        localStorage.setItem('userCity', this.user.cityUser);
        // Можно также отправить событие для обновления фильтров
        this.$emit('city-changed', this.user.cityUser);
      }
    },
    toggleDropdown() {
      this.dropdownOpen = !this.dropdownOpen;
    },
    closeDropdown() {
      this.dropdownOpen = false;
    },
    logout() {
      localStorage.removeItem('user');
      localStorage.removeItem('userCity'); // Очищаем сохраненный город
      this.user = null;
      this.$router.push('/');
    }
  },
  watch: {
    '$route'() {
      this.loadUser();
    }
  }
};
</script>

<style scoped>
.user-menu-img{
  margin-bottom: -6px;
  margin-right: -8px;
  height: 20px;
}
.logo-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: space-between;
  width: 100%;
}

.logo {
  display: flex;
  align-items: center;
  font-size: 22px;
  font-weight: bold;
  color: #333;
}

.logo img {
  height: 50px;
  margin-bottom: -15px;
    margin-top: -15px;
  justify-content: center;
  margin-right: 10px;
}
.user-info {
  font-size: 0.9rem;
  display: flex;
  align-items: center;
  gap: 1rem;
}

.user-city {
  font-weight: 600;
  padding: 0.5rem 0.5rem;

  border-radius: 20px;
  background-color: #f5f5f5;
  display: flex;
  align-items: center;
}

.user-city i {
  margin-right: 0.3rem;
  color: #ff5757;
}

.user-menu {
  position: relative;
  cursor: pointer;
}

.user-name {
  font-weight: 600;
  margin-top: -7px;
  padding: 0.5rem 1rem;
  border-radius: 20px;
  background-color: #f5f5f5;
  display: flex;
  align-items: center;
}

.user-name:hover {
  background-color: #eee;
}

.dropdown-menu {
  position: absolute;
  right: 0;
  top: 100%;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 15px rgba(0,0,0,0.1);
  min-width: 180px;
  z-index: 1000;
  overflow: hidden;
}

.dropdown-menu a {
  display: flex;
  align-items: center;
  padding: 0.75rem 1rem;
  color: #333;
  text-decoration: none;
  transition: all 0.2s;
}

.dropdown-menu a:hover {
  background: #f9f9f9;
  color: #ff5757;
}

.dropdown-menu i {
  margin-right: 0.75rem;
  width: 20px;
  text-align: center;
}

/* Адаптивность */
@media (max-width: 768px) {
  header {
    padding: 1rem;
    flex-wrap: wrap;
  }

  
  .user-city {
    font-size: 0.8rem;
    padding: 0.3rem 0.8rem;
  }
}
.logout-btn {
  align-self: center;
  border: 1px solid #ff5757;
  color: white;
}

.logout-btn:hover {
  background-color: #ff5757;
  color: white;
}
  .user-name {
  margin-bottom: -10px;
    align-self: center;
    margin-right: 10px;
    font-weight: bold;
  }
  .user-menu {
  position: relative;
}

.user-avatar {
  display: flex;
  align-items: center;
  cursor: pointer;
}

.user-avatar img {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  margin-right: 0.5rem;
}

.dropdown-menu {
  position: absolute;
  right: 0;
  left: -10%;
  top: 100%;
  background: white;
  border-radius: 15px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
  min-width: 150px;
  z-index: 100;
}

.dropdown-menu a {
  display: block;
  padding: 0.75rem 1rem;
  color: #333;
  text-decoration: none;
}

.dropdown-menu a:hover {
  background: #f5f5f5;
}

.dropdown-menu i {
  margin-right: 0.5rem;
  color: #666;
}
</style>