import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '@/views/HomeView.vue'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: HomeView
  },
  {
    path: '/artist/:idArtist',
    name: 'ArtistView',
    component: () => import('../views/ArtistView.vue'),
    props: true
  },
  {
    path: '/artists',
    name: 'ArtistsAllTabView',
    component: () => import('../views/ArtistsAllTabView.vue'),
    props: true
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/LoginView.vue'),
    props: true
  },
  ,
  {
    path: '/moderation-panel',
    name: 'Moderation-panel',
    component: () => import('../views/ModerationPanel.vue'),
    props: true
  },
  {
    path: '/artist/:idArtist/concert/:idConcert',
    name: 'ConcertView',
    component: () => import('../views/ConcertArtistView.vue'),
    props: true
  },
  {
    path: '/register',
    name: 'RegisterView',
    component: () => import('../views/RegisterView.vue'),
    props: true
  },
  {
    path: '/purchase',
    name: 'PurchaseTickets',
    component: () => import('@/views/PurchaseTicket.vue'),
    // props: true
  },
  {
    path: '/purchase/success',
    name: 'PurchaseSuccess',
    component: () => import('@/views/PurchaseSuccess.vue'),
    // props: true
  },
  {
    path: '/profile',
    name: 'Profile',
    component: () => import('@/views/Profile.vue'),
    // props: true
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router