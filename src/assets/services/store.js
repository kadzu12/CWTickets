// import Vue from 'vue';
// import Vuex from 'vuex';

// Vue.use(Vuex);

// export default new Vuex.Store({
//   state: {
//     user: JSON.parse(localStorage.getItem('user')) || null
//   },
//   getters: {
//     isAuthenticated: state => !!state.user,
//     userLogin: state => state.user?.LoginUser || '',
//     isModerator: state => state.user?.IdRoleNavigation?.NameRole === 'moderator'
//   },
//   mutations: {
//     setUser(state, user) {
//       state.user = user;
//       localStorage.setItem('user', JSON.stringify(user));
//     },
//     clearUser(state) {
//       state.user = null;
//       localStorage.removeItem('user');
//     }
//   },
//   actions: {
//     loginUser({ commit }, user) {
//       commit('setUser', user);
//     },
//     registerUser({ commit }, user) {
//       commit('setUser', user);
//     },
//     logoutUser({ commit }) {
//       commit('clearUser');
//     }
//   }
// });