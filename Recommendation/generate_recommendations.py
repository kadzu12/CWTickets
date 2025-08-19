import pandas as pd
import numpy as np
import pymysql
from sqlalchemy import create_engine
import json
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class MusicRecommendationSystem:
    def __init__(self, db_config):
        self.db_config = db_config
        self.engine = create_engine(
            f"mysql+pymysql://{db_config['user']}:{db_config['password']}@{db_config['host']}/{db_config['database']}"
        )
        self.connection = pymysql.connect(**db_config)

    def __del__(self):
        if hasattr(self, 'connection') and self.connection:
            self.connection.close()

    def load_data_for_popular_artists(self):
        reviews_query = """
        SELECT a.id_artist, a.name_artist, 
               COUNT(r.id_review) as review_count, 
               AVG(r.rating_review) as avg_rating
        FROM artist a
        LEFT JOIN concert c ON a.id_artist = c.id_artist
        LEFT JOIN review r ON c.id_concert = r.id_concert
        GROUP BY a.id_artist, a.name_artist
        """
        tickets_query = """
        SELECT a.id_artist, a.name_artist, 
               COUNT(t.id_ticket) as tickets_sold
        FROM artist a
        LEFT JOIN concert c ON a.id_artist = c.id_artist
        LEFT JOIN ticket t ON c.id_concert = t.id_concert
        GROUP BY a.id_artist, a.name_artist
        """
        favorites_query = """
        SELECT a.id_artist, a.name_artist, 
               COUNT(f.id_favorite) as favorites_count
        FROM artist a
        LEFT JOIN favorite f ON a.id_artist = f.id_artist
        GROUP BY a.id_artist, a.name_artist
        """
        reviews = pd.read_sql(reviews_query, self.engine)
        tickets = pd.read_sql(tickets_query, self.engine)
        favorites = pd.read_sql(favorites_query, self.engine)

        data = reviews.merge(tickets, on=['id_artist', 'name_artist'], how='outer') \
                      .merge(favorites, on=['id_artist', 'name_artist'], how='outer') \
                      .fillna(0)
        return data

    def calculate_popularity_score(self, row):
        review_score = np.log1p(row['review_count']) * row['avg_rating']
        tickets_score = np.log1p(row['tickets_sold'])
        favorites_score = np.log1p(row['favorites_count'])
        return 0.4 * review_score + 0.3 * tickets_score + 0.3 * favorites_score

    def save_popular_artists_to_json(self, filepath="popular_artists.json", limit=10):
        df = self.load_data_for_popular_artists()
        df['popularity_score'] = df.apply(self.calculate_popularity_score, axis=1)
        top = df.sort_values('popularity_score', ascending=False).head(limit)
        top[['id_artist', 'name_artist', 'popularity_score']].to_json(filepath, orient='records', force_ascii=False, indent=4)
        logger.info(f"Популярные исполнители сохранены в {filepath}")

    def generate_personal_recommendations(self):
        users = pd.read_sql("SELECT id_user FROM user", self.engine)
        popularity_data = self.load_data_for_popular_artists()
        popularity_data['popularity_score'] = popularity_data.apply(self.calculate_popularity_score, axis=1)

        recommendations = []
        for user_id in users['id_user']:
            reviewed_query = f"""
                SELECT a.id_artist, AVG(r.rating_review) AS avg_rating
                FROM artist a
                JOIN concert c ON a.id_artist = c.id_artist
                JOIN review r ON c.id_concert = r.id_concert
                WHERE r.id_user = {user_id}
                GROUP BY a.id_artist
            """
            reviewed = pd.read_sql(reviewed_query, self.engine)
            if not reviewed.empty:
                reviewed = reviewed.merge(popularity_data, on='id_artist', how='left')
                reviewed['final_score'] = reviewed['avg_rating'] * reviewed['popularity_score']
                top_artists = reviewed.sort_values('final_score', ascending=False).head(10)
                for artist_id in top_artists['id_artist']:
                    recommendations.append((user_id, int(artist_id)))

        return recommendations

    def update_recommendation_table(self):
        logger.info("Обновление персональных рекомендаций...")
        recs = self.generate_personal_recommendations()

        try:
            with self.connection.cursor() as cursor:
                cursor.execute("DELETE FROM recommendation")
                for id_user, id_artist in recs:
                    cursor.execute(
                        "INSERT INTO recommendation (id_user, id_artist) VALUES (%s, %s)",
                        (id_user, id_artist)
                    )
            self.connection.commit()
            logger.info(f"Добавлено {len(recs)} рекомендаций в таблицу.")
        except Exception as e:
            logger.error(f"Ошибка при обновлении recommendation: {e}")
            self.connection.rollback()

if __name__ == "__main__":
    db_config = {
        'host': 'localhost',
        'user': 'root',
        'password': 'qwertyyou123',
        'database': 'ticket_db'
    }

    system = MusicRecommendationSystem(db_config)
    system.save_popular_artists_to_json()
    system.update_recommendation_table()
