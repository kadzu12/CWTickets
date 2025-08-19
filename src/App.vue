<!-- App.vue -->
<template>
  <div class="app">
    <HeaderView /> 
    <main>
      <router-view />
    </main>
  </div>
</template>

<script>
import HeaderView from '@/components/Header.vue';

export default {
  components: { HeaderView }
}
</script>

<style>
.app {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

main {
  flex: 1;
}
</style>