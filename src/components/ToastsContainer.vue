<template>
    <div class="toasts-container">
      <div 
        v-for="toast in toasts" 
        :key="toast.id"
        :class="['toast', toast.type]"
      >
        {{ toast.message }}
      </div>
    </div>
  </template>
  
  <script>
  import { toasts } from '@/plugins/toast'
  
  export default {
    setup() {
      return { toasts }
    }
  }
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
  }
  
  .toast.error {
    background-color: #F44336;
  }
  </style>