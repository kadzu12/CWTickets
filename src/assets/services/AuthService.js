// export class AuthService {
//     static isAuthenticated() {
//       return !!localStorage.getItem('authToken');
//     }
  
//     static getCurrentUser() {
//       const user = localStorage.getItem('user');
//       return user ? JSON.parse(user) : null;
//     }
  
//     static logout() {
//       localStorage.removeItem('authToken');
//       localStorage.removeItem('user');
//     }
  
//     static getAuthHeader() {
//       const token = localStorage.getItem('authToken');
//       return token ? { 'Authorization': `Bearer ${token}` } : {};
//     }
//   }