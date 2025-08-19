// import './assets/main.css'

// import { createApp } from 'vue'
// import App from './views/Home.vue'
// // import router from './router'
// createApp(App).mount('#app')
// // const app = createApp(App)

// // app.use(router)

// // app.mount('#app')

import { createApp } from 'vue'
import App from './App.vue'
import router from './router' // <-- Должен быть импорт роутера
import Toast from 'vue-toastification';
import 'vue-toastification/dist/index.css';

const app = createApp(App)

app.use(router) // <-- Подключаем его к приложению
const toastOptions = {
    timeout: 3000,
    closeOnClick: true,
    draggable: true
  }
router.afterEach((to) => {
  if (to.hash) {
    setTimeout(() => {
      const element = document.querySelector(to.hash);
        if (element) {
          element.scrollIntoView({ behavior: 'smooth' });
        }
      }, 100);
    }
});
app.use(Toast, toastOptions)
app.mount('#app')
