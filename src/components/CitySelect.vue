<template>
    <div class="city-select">
      <div class="search-container" @click="toggleDropdown">
        <input
          type="text"
          v-model="searchQuery"
          @input="filterCities"
          @focus="openDropdown"
          :placeholder="placeholder"
          class="search-input"
        />
        <span class="dropdown-icon" :class="{ 'open': isOpen }">▼</span>
      </div>
      
      <ul v-if="isOpen" class="dropdown-list">
        <li
          v-for="city in filteredCities"
          :key="city"
          @click="selectCity(city)"
          :class="{ 'selected': city === modelValue }"
        >
          {{ city }}
        </li>
        <li v-if="filteredCities.length === 0" class="no-results">
          Город не найден
        </li>
      </ul>
    </div>
  </template>
  
  <script>
  export default {
    name: 'CitySelect',
    props: {
      modelValue: {
        type: String,
        default: ''
      },
      placeholder: {
        type: String,
        default: 'Выберите город'
      },
      cities: {
        type: Array,
        required: true,
        default: () => []
      }
    },
    emits: ['update:modelValue'],
    data() {
      return {
        isOpen: false,
        searchQuery: '',
        filteredCities: [...this.cities]
      }
    },
    watch: {
      modelValue(newVal) {
        if (newVal) {
          this.searchQuery = newVal;
        }
      },
      cities(newCities) {
        this.filteredCities = [...newCities];
      }
    },
    methods: {
      toggleDropdown() {
        this.isOpen = !this.isOpen;
        if (this.isOpen) {
          this.filterCities();
        }
      },
      openDropdown() {
        this.isOpen = true;
      },
      closeDropdown() {
        this.isOpen = false;
      },
      filterCities() {
        if (this.searchQuery === '') {
          this.filteredCities = [...this.cities];
        } else {
          this.filteredCities = this.cities.filter(city =>
            city.toLowerCase().includes(this.searchQuery.toLowerCase())
          );
        }
      },
      selectCity(city) {
        this.$emit('update:modelValue', city);
        this.searchQuery = city;
        this.closeDropdown();
      }
    },
    mounted() {
      document.addEventListener('click', (e) => {
        if (!this.$el.contains(e.target)) {
          this.closeDropdown();
        }
      });
    }
  }
  </script>
  
  <style scoped>
  .city-select {
    position: relative;
    width: 100%;
  }
  
  .search-container {
    position: relative;
    display: flex;
    align-items: center;
  }
  
  .search-input {
    width: 100%;
    padding: 0.6rem 0.8rem;
    border: 1px solid #ddd;
    border-radius: 6px;
    font-size: 1rem;
  }
  
  .search-input:focus {
    outline: none;
    border-color: #ff5757;
    box-shadow: 0 0 0 2px rgba(255, 87, 87, 0.2);
  }
  
  .dropdown-icon {
    position: absolute;
    right: 10px;
    font-size: 0.8rem;
    transition: transform 0.2s;
    pointer-events: none;
  }
  
  .dropdown-icon.open {
    transform: rotate(180deg);
  }
  
  .dropdown-list {
    position: absolute;
    width: 100%;
    max-height: 200px;
    overflow-y: auto;
    margin-top: 5px;
    padding: 0;
    list-style: none;
    background: white;
    border: 1px solid #ddd;
    border-radius: 6px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    z-index: 1000;
  }
  
  .dropdown-list li {
    padding: 8px 12px;
    cursor: pointer;
  }
  
  .dropdown-list li:hover {
    background-color: #f5f5f5;
  }
  
  .dropdown-list li.selected {
    background-color: #ff5757;
    color: white;
  }
  
  .no-results {
    color: #888;
    font-style: italic;
    cursor: default;
  }
  
  .no-results:hover {
    background-color: transparent;
  }
  </style>