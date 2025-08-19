<template>
    <div class="city-filter">
      <div class="select-container" @click.stop="toggleDropdown">
        <input
          type="text"
          v-model="searchQuery"
          @input="filterCities"
          @focus="openDropdown"
          :placeholder="displayValue"
          class="filter-input"
        />
        <span v-if="modelValue" class="clear-btn" @click.stop="clearSelection">×</span>
        <span class="dropdown-icon" :class="{ 'open': isOpen }">▼</span>
        
        <transition name="fade">
          <ul v-if="isOpen" class="dropdown-list">
            <li 
              v-if="showAllOption"
              @click="selectCity('')"
              :class="{ 'selected': modelValue === '' }"
            >
              Все города
            </li>
            <li
              v-for="city in filteredCities"
              :key="city"
              @click="selectCity(city)"
              :class="{ 'selected': city === modelValue, 'highlighted': city === highlightedCity }"
            >
              <span v-html="highlightMatch(city)"></span>
              <span v-if="city === modelValue" class="check-mark">✓</span>
            </li>
            <li v-if="filteredCities.length === 0" class="no-results">
              Город не найден
            </li>
          </ul>
        </transition>
      </div>
    </div>
  </template>
  
  <script>
  import { debounce } from 'lodash';
  
  export default {
    name: 'CitySelectFilter',
    props: {
      modelValue: {
        type: String,
        default: ''
      },
      cities: {
        type: Array,
        required: true,
        default: () => []
      },
      showAllOption: {
        type: Boolean,
        default: true
      }
    },
    emits: ['update:modelValue'],
    data() {
      return {
        isOpen: false,
        searchQuery: '',
        filteredCities: [...this.cities],
        highlightedCity: null,
        keyboardIndex: -1
      };
    },
    computed: {
      displayValue() {
        return this.modelValue || 'Фильтр по городу';
      }
    },
    watch: {
      cities(newCities) {
        this.filteredCities = [...newCities];
      },
      isOpen(newVal) {
        if (newVal) {
          this.filterCities();
          document.addEventListener('keydown', this.handleKeyboardNav);
        } else {
          document.removeEventListener('keydown', this.handleKeyboardNav);
        }
      }
    },
    methods: {
      toggleDropdown() {
        this.isOpen ? this.closeDropdown() : this.openDropdown();
      },
      openDropdown() {
        this.isOpen = true;
      },
      closeDropdown() {
        this.isOpen = false;
        this.keyboardIndex = -1;
        this.highlightedCity = null;
      },
      filterCities: debounce(function() {
        if (this.searchQuery === '') {
          this.filteredCities = [...this.cities];
        } else {
          this.filteredCities = this.cities.filter(city =>
            city.toLowerCase().includes(this.searchQuery.toLowerCase())
          );
        }
        this.keyboardIndex = -1;
      }, 300),
selectCity(city) {
  this.$emit('update:modelValue', city);
  this.searchQuery = '';
  this.closeDropdown();

  // Новый код — закрыть все открытые dropdown'ы на странице
  this.$emit('closeAllDropdowns');
},
      clearSelection() {
        this.selectCity('');
      },
      highlightMatch(text) {
        if (!this.searchQuery) return text;
        const regex = new RegExp(this.searchQuery, 'gi');
        return text.replace(regex, match => `<span class="match">${match}</span>`);
      },
      handleKeyboardNav(e) {
        if (!this.isOpen) return;
        
        if (e.key === 'ArrowDown') {
          e.preventDefault();
          this.navigate(1);
        } else if (e.key === 'ArrowUp') {
          e.preventDefault();
          this.navigate(-1);
        } else if (e.key === 'Enter' && this.highlightedCity !== null) {
          e.preventDefault();
          this.selectCity(this.highlightedCity);
        } else if (e.key === 'Escape') {
          this.closeDropdown();
        }
      },
      navigate(direction) {
        if (this.filteredCities.length === 0) return;
        
        this.keyboardIndex = Math.max(
          -1,
          Math.min(
            this.keyboardIndex + direction,
            this.filteredCities.length - 1
          )
        );
        
        this.highlightedCity = this.keyboardIndex >= 0 
          ? this.filteredCities[this.keyboardIndex] 
          : null;
        
        // Scroll to highlighted item
        if (this.highlightedCity) {
          const element = this.$el.querySelector('.highlighted');
          if (element) {
            element.scrollIntoView({ block: 'nearest' });
          }
        }
      }
    },
    mounted() {
      document.addEventListener('click', this.closeOnClickOutside);
    },
    beforeUnmount() {
      document.removeEventListener('click', this.closeOnClickOutside);
      document.removeEventListener('keydown', this.handleKeyboardNav);
    }
  };
  </script>
  
  <style scoped>
  .city-filter {
    position: relative;
  }
  
  .city-filter label {
    display: block;
    font-size: 14px;
    margin-bottom: 5px;
    color: #555;
  }
  
  .select-container {
    position: relative;
  }
  
  .filter-input {
    width: 100%;
    padding: 0.6rem 2.5rem 0.6rem 0.8rem;
    border: 1px solid #ddd;
    border-radius: 6px;
    background-color: rgba(255, 255, 255, 0.8);
    font-size: 1rem;
    transition: all 0.3s;
  }
  
  .filter-input:focus {
    border-color: #ff5757;
    outline: none;
    box-shadow: 0 0 0 2px rgba(255, 87, 87, 0.2);
  }
  
  .dropdown-icon {
    position: absolute;
    right: 10px;
    top: 50%;
    transform: translateY(-50%);
    font-size: 0.7rem;
    color: #777;
    pointer-events: none;
    transition: transform 0.2s;
  }
  
  .dropdown-icon.open {
    transform: translateY(-50%) rotate(180deg);
  }
  
  .clear-btn {
    position: absolute;
    right: 25px;
    top: 50%;
    transform: translateY(-50%);
    cursor: pointer;
    font-size: 1.2rem;
    color: #999;
    padding: 0 5px;
    z-index: 2;
  }
  
  .clear-btn:hover {
    color: #ff5757;
  }
  
  .dropdown-list {
    position: absolute;
    width: 100%;
    max-height: 300px;
    overflow-y: auto;
    margin-top: 5px;
    padding: 5px 0;
    list-style: none;
    background: white;
    border: 1px solid #ddd;
    border-radius: 6px;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
    z-index: 1000;
  }
  
  .dropdown-list li {
    padding: 8px 15px;
    cursor: pointer;
    font-size: 14px;
    color: #333;
    transition: all 0.2s;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
  
  .dropdown-list li:hover {
    background-color: #f5f5f5;
  }
  
  .dropdown-list li.selected {
    background-color: #ff5757;
    color: white;
  }
  
  .dropdown-list li.highlighted {
    background-color: #f0f0f0;
  }
  
  .dropdown-list li .check-mark {
    font-weight: bold;
  }
  
  .no-results {
    color: #888;
    font-style: italic;
    cursor: default;
  }
  
  .no-results:hover {
    background-color: transparent;
  }
  
  .match {
    font-weight: bold;
    background-color: rgba(255, 215, 0, 0.3);
  }
  
  .fade-enter-active,
  .fade-leave-active {
    transition: opacity 0.2s, transform 0.2s;
  }
  
  .fade-enter-from,
  .fade-leave-to {
    opacity: 0;
    transform: translateY(-10px);
  }
  </style>