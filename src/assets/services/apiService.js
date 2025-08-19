// apiService.js
import ArtistView from '@/views/ArtistView.vue';
import axios from 'axios';

const API_URL_AUTH = 'http://localhost:5199/api/Auth';
const API_URL_USERS = 'http://localhost:5199/api/Users';
const API_URL_ARTIST = 'http://localhost:5199/api/Artist';
const API_URL_GENRES ='http://localhost:5199/api/Genres';
const API_URL_HALLS ='http://localhost:5199/api/Hall';
const API_URL_CONCERTS ='http://localhost:5199/api/Concerts';
const API_URL_TICKETS ='http://localhost:5199/api/Tickets';
const API_URL_FAVORITE = 'http://localhost:5199/api/Favorite';
const API_URL_REVIEWS = 'http://localhost:5199/api/Review';
const API_URL_NOTIFICATION = 'http://localhost:5199/api/Notification';
const API_URL_RECOMENDATION = 'http://localhost:5199/api/Recommendation';
export default class ApiService {
  // Метод для получения всех артистов
  static async getArtists() {
    try {
      const response = await axios.get(`${API_URL_ARTIST}/GetArtists`);
      return response.data;  // Возвращаем данные
    } catch (error) {
      console.error('Ошибка при запросе данных артистов:', error);
      throw error;  
    }
  }
  static async getEvents() {
    try {
      const response = await axios.get(`${API_URL_CONCERTS}/GetConcerts`);
      return response.data;  
    } catch (error) {
      console.error('Ошибка при запросе данных концертов:', error);
      throw error; 
    }
  }
  static async getArtistById(id) {
    try {
      const response = await axios.get(`${API_URL_ARTIST}/GetArtistById/${id}`); // Пример URL (подставьте ваш)
      return response.data;
    } catch (error) {
      console.error('Ошибка при загрузке артиста:', error);
      throw error;
    }
  }
  static async getArtistByCity(city) {
    try {
      const response = await axios.get(`${API_URL_ARTIST}/GetArtistByCity/${city}`); 
      return response.data;
    } catch (error) {
      console.error('Ошибка при загрузке артиста:', error);
      throw error;
    }
  }
  static async getConcertById(id) {
    try {
      const response = await axios.get(`${API_URL_ARTIST}/GetConcertById/${id}`); // Пример URL (подставьте ваш)
      return response.data;
    } catch (error) {
      console.error('Ошибка при загрузке концерта артиста:', error);
      throw error;
    }
  }
  // Метод для получения артистов с фильтрами
  static async getFilteredArtists(params) {
    try {
      const response = await axios.get(`${API_URL_ARTIST}/GetFilteredArtists`, {
        params: params,
        paramsSerializer: {
          encode: encodeURIComponent,
          serialize: (params) => {
            return Object.entries(params)
              .filter(([_, value]) => value !== undefined && value !== null)
              .map(([key, value]) => `${key}=${encodeURIComponent(value)}`)
              .join('&');
          }
        }
      });
      return response.data;
    } catch (error) {
      console.error('Ошибка при запросе данных артистов фильтрации:', error);
      console.error('Детали ошибки:', {
        message: error.message,
        response: error.response?.data,
        config: error.config
      });
      throw error;
    }
  }
  static async getFilteredConcerts(params) {
    try {
      const response = await axios.get(`${API_URL_CONCERTS}/GetFilteredConcerts`, {
        params: params,
        paramsSerializer: {
          encode: encodeURIComponent,
          serialize: (params) => {
            return Object.entries(params)
              .filter(([_, value]) => value !== undefined && value !== null)
              .map(([key, value]) => `${key}=${encodeURIComponent(value)}`)
              .join('&');
          }
        }
      });
      return response.data;
    } catch (error) {
      console.error('Ошибка при запросе данных концертов фильтрации:', error);
      console.error('Детали ошибки:', {
        message: error.message,
        response: error.response?.data,
        config: error.config
      });
      throw error;
    }
  }
  static async getGenres() {
    try {
      const response = await axios.get(`${API_URL_GENRES}/GetGenres`);
      return response.data;  
    } catch (error) {
      console.error('Ошибка при запросе данных жанров:', error);
      throw error;
    }
  }
  static async getCities() {
    try {
      const response = await axios.get(`${API_URL_HALLS}/GetCities`);
      return response.data;
    } catch (error) {
      console.error('Ошибка при получении городов:', error);
      throw error;
    }
  }
  static async getHalls() {
    try {
      const response = await axios.get(`${API_URL_HALLS}/GetHalls`);
      return response.data;
    } catch (error) {
      console.error('Ошибка при получении залов:', error);
      throw error;
    }
  }
  static  async getHallsByCity(city) {
    try {
      const response = await axios.get(`${API_URL_HALLS}/GetHallsByCity?city=${encodeURIComponent(city)}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching halls:', error);
      throw error;
    }
  }

static async updateConcert(concertData) {
  try {
    // Преобразуем данные перед отправкой
    const payload = {
      IdConcert: concertData.idConcert,
      IdArtist: concertData.idArtist,
      IdHall: concertData.idHall,
      StatusConcert: concertData.statusConcert,
      DateStartConcert: concertData.dateStartConcert, // уже в формате 'YYYY-MM-DD'
      TimeStartConcert: concertData.timeStartConcert.endsWith(':00') 
        ? concertData.timeStartConcert 
        : concertData.timeStartConcert + ':00',
      AgeLimitConcert: concertData.ageLimitConcert?.toString() || '16'
    };

    console.log('Sending concert update:', payload); // для отладки

    const response = await axios.put(`${API_URL_CONCERTS}/UpdateConcert`, payload, {
      headers: {
        'Content-Type': 'application/json'
      }
    });
    return response.data;
  } catch (error) {
    const errorMessage = error.response?.data?.title || 
                        error.response?.data || 
                        'Ошибка при обновлении концерта';
    console.error('Update concert error:', error.response?.data || error.message);
    throw new Error(errorMessage);
  }
}
  static async addConcert(concertData) {
    try {
      const response = await axios.post(`${API_URL_CONCERTS}/AddConcert`, concertData);
      return response.data;
    } catch (error) {
      const apiError = new Error(error.response?.data?.message || 'Ошибка при обновлении концерта');
      apiError.response = error.response;
      throw error;
    }
  }
  static async login(credentials) {
    try {
      const response = await axios.post(`${API_URL_USERS}/login`, credentials);
      return response.data;
    } catch (error) {
      console.error('Ошибка входа:', error);
      throw new Error(error.response?.data?.message || 'Ошибка входа');
    }
  }
  
  static async register(userData) {
    try {
      const response = await axios.post(`${API_URL_USERS}/register`, userData);
      return response.data;
    } catch (error) {
      console.error('Ошибка регистрации:', error);
      throw new Error(error.response?.data?.message || 'Ошибка регистрации');
    }
  }
  static async updateUser(userData) {
    try {
      const response = await axios.put(`${API_URL_USERS}/UpdateUser`, {
        Id: userData.idUser,
        FirstName: userData.firstNameUser,
        LastName: userData.lastNameUser,
        BirthDate: userData.birthDateUser,
        Email: userData.emailUser,
        City: userData.cityUser
      }, {
        headers: {
          'Content-Type': 'application/json'
        }
      });
      return response.data;
    } catch (error) {
      console.error('Ошибка обновления информации пользователя:', error);
      throw error;
    }
  }
  static async getHallSections(hallId) {
    try {
        // console.log(hallId);
        const response = await axios.get(`${API_URL_HALLS}/GetHallSections/${hallId}`);
        return response.data;
    } catch (error) {
        if (error.response && error.response.status === 404) {
            console.warn('Секции зала не найдены');
            return [];
        }
        console.error('Ошибка при загрузке секций зала:', error);
        throw error;
    }
  }


  static async purchaseTickets(purchaseData) {
    try {
      const response = await axios.post(`${API_URL_TICKETS}/Purchase`, purchaseData);
      return response.data;
    } catch (error) {
      if (error.response) {
        if (error.response.status === 400) {
          throw new Error(error.response.data || 'Неверные данные платежа');
        } else if (error.response.status === 401) {
          throw new Error('Требуется авторизация');
        } else if (error.response.status === 404) {
          throw new Error('Концерт или секция не найдены');
        }
      }
      throw new Error('Произошла ошибка при обработке платежа');
    }
  }
  static async checkSeatAvailability({ concertId, sectionId, rowNumber = null, seatNumber = null }) {
    try {
      const response = await axios.get(`${API_URL_TICKETS}/CheckSeatAvailability`, {
        params: { concertId, sectionId, rowNumber, seatNumber }
      });
      return response.data;
    } catch (error) {
      console.error('Ошибка получения занятости места:', error);
      return { isAvailable: false };
    }
  }
  static async getOccupiedSeats (concertId) {
    try {
      const response = await axios.get(`${API_URL_TICKETS}/GetOccupiedSeats/${concertId}`);
      return response.data;
    } catch (error) {
      console.error('Ошибка получения занятости места:', error);
      return { isAvailable: false };
    }
  }
  static async getUserTickets(userId) {
    try {
      // console.log(hallId);
      const response = await axios.get(`${API_URL_USERS}/GetTicketsUser/${userId}`)
      return response.data;
  } catch (error) {
      if (error.response && error.response.status === 404) {
          console.warn('Билеты пользователя не найдены');
          return [];
      }
      console.error('Ошибка при загрузке билетов пользователя:', error);
      throw error;
  }
    // return this.axios.get(`${API_URL_USERS}/GetTicketsUser/${userId}`)
    //     .then(response => ({
    //         data: response.data.tickets,
    //         count: response.data.count
    //     }));
  }
  static async cancelTicket(ticketId) {
    try {
        const response = await axios.post(`${API_URL_USERS}/CancelTicket/${ticketId}`);
        return response.data;
    } catch (error) {
        console.error('Error canceling ticket:', error);
        throw error;
    }
}

  static async getUserFavorites(userId) {
    try {
      const response = await axios.get(`${API_URL_FAVORITE}/user/${userId}`)
      return response.data;
    } catch (error) {
      if (error.response && error.response.status === 404) {
          console.warn('Избранные пользователя не найдены');
          return [];
      }
      console.error('Ошибка при загрузке избранных пользователя:', error);
      throw error;
    }
  }
  static async addFavorite(favoriteData) {
    try {

        const response = await axios.post(`${API_URL_FAVORITE}/addFavorite`, favoriteData);
        return response.data;
    } catch (error) {
        const message = error.response?.data || error.message;
        console.error('Ошибка при добавлении в избранное:', message);
        throw error;
    }
}
  static async removeFavorite(favoriteId) {
    try {
      const response = await axios.delete(`${API_URL_FAVORITE}/removeFavorite/${favoriteId}`);
      return response.data;
    } catch (error) {
      if (error.response) {
          console.warn('Не удалось удалить избранное');
          return [];
      }
      console.error('Ошибка при удалении избранного пользователя:', error);
      throw error;
    }
  }
  // Получить отзывы пользователя
    static async getUserReviews(userId) {
        try {
            const response = await axios.get(`${API_URL_REVIEWS}/user/${userId}`);
            return response.data;
        } catch (error) {
            console.error('Ошибка при загрузке отзывов пользователя:', error);
            throw error;
        }
    }

    // Получить отзывы для концерта
  static async getArtistReviews(artistId) {
    try {
      const response = await axios.get(`${API_URL_REVIEWS}/artist/${artistId}`);
      return response.data;
    } catch (error) {
      console.error('Ошибка при загрузке отзывов артиста:', error);
      throw error;
    }
  }

    // Создать отзыв
    static async createReview(reviewData) {
        try {
            const response = await axios.post(API_URL_REVIEWS, reviewData);
            return response.data;
        } catch (error) {
            console.error('Ошибка при создании отзыва:', error);
            throw error;
        }
    }

    static async updateReview(reviewData) {
        try {
            const response = await axios.put(`${API_URL_REVIEWS}/${reviewData.idReview}`, reviewData);
            return response.data;
        } catch (error) {
            console.error('Ошибка при обновлении отзыва:', error);
            throw error;
        }
    }

    static async deleteReview(reviewId) {
        try {
            const response = await axios.delete(`${API_URL_REVIEWS}/${reviewId}`);
            return response.data;
        } catch (error) {
            console.error('Ошибка при удалении отзыва:', error);
            throw error;
        }
    }

    static async hasUserReviewedConcert(userId, concertId) {
        try {
            const reviews = await this.getUserReviews(userId);
            return reviews.some(review => review.idConcert === concertId);
        } catch (error) {
            console.error('Ошибка при проверке отзыва:', error);
            throw error;
        }
    }
    static async getUserNotifications(userId) {
        try {
            const response = await axios.get(`${API_URL_NOTIFICATION}/user/${userId}`);
            return response.data;
        } catch (error) {
            console.error('Ошибка при получении уведомлений:', error);
            throw error;
        }
    }

    static async markNotificationAsRead(notificationId) {
        try {
            const response = await axios.patch(`${API_URL_NOTIFICATION}/${notificationId}/read`);
            return response.data;
        } catch (error) {
            console.error('Ошибка при отметке уведомления как прочитанного:', error);
            throw error;
        }
    }

    static async getUnreadNotificationsCount(userId) {
        try {
            const response = await axios.get(`${API_URL_NOTIFICATION}/user/${userId}/unread-count`);
            return response.data;
        } catch (error) {
            console.error('Ошибка при получении количества непрочитанных уведомлений:', error);
            throw error;
        }
    }

    static async getRecommendationForUser(idUser) {
    try {
      const response = await axios.get(`${API_URL_RECOMENDATION}/GetRecommendationForUser/${idUser}`);
      return response.data;
      } catch (error) {
        console.error('Ошибка получения рекомендация пользователя:', error);
        throw error;
      }
    }
    static async popularArtists() {
    try {
      const response = await axios.get(`${API_URL_RECOMENDATION}/PopularArtists`);
      return response.data;
      } catch (error) {
        console.error('Ошибка получения популярных исполнителей:', error);
        throw error;
      }
    }
}
