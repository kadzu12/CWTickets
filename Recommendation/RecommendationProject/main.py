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
        top[['id_artist', 'name_artist', 'popularity_score']].to_json(filepath, orient='records', force_ascii=False,
                                                                      indent=4)
        logger.info(f"Популярные исполнители сохранены в {filepath}")

    def get_user_reviewed_artists(self, user_id):
        query = f"""
        SELECT a.id_artist, a.name_artist, 
               COALESCE(AVG(r.rating_review), 0) as avg_rating
        FROM artist a
        LEFT JOIN concert c ON a.id_artist = c.id_artist
        LEFT JOIN review r ON c.id_concert = r.id_concert AND r.id_user = {user_id}
        GROUP BY a.id_artist, a.name_artist
        HAVING COUNT(r.id_review) > 0
        """
        return pd.read_sql(query, self.engine)

    def load_user_preferences(self, user_id):
        favorites_query = f"""
            SELECT id_artist FROM favorite WHERE id_user = {user_id}
        """
        favorites = pd.read_sql(favorites_query, self.engine)['id_artist'].tolist()

        tickets_query = f"""
            SELECT c.id_artist, t.status_ticket
            FROM ticket t
            JOIN concert c ON t.id_concert = c.id_concert
            WHERE t.id_user = {user_id}
        """
        tickets_df = pd.read_sql(tickets_query, self.engine)

        return favorites, tickets_df

    def recommend_loved_artists(self, user_id, popularity_data, limit=5):
        reviewed = self.get_user_reviewed_artists(user_id)

        if not reviewed.empty:
            loved = reviewed.merge(popularity_data, on=['id_artist', 'name_artist'], how='left')
            if 'avg_rating' in loved.columns and 'popularity_score' in loved.columns:
                loved['final_score'] = loved['avg_rating'] * loved['popularity_score']
            else:
                if 'avg_rating' in loved.columns:
                    loved['final_score'] = loved['avg_rating']
                elif 'popularity_score' in loved.columns:
                    loved['final_score'] = loved['popularity_score']
                else:
                    loved['final_score'] = 0
        else:
            favorites, _ = self.load_user_preferences(user_id)
            if not favorites:
                return pd.DataFrame()
            loved_query = f"""
                SELECT a.id_artist, a.name_artist 
                FROM artist a 
                WHERE a.id_artist IN ({','.join(map(str, favorites))})
            """
            loved = pd.read_sql(loved_query, self.engine)
            loved = loved.merge(popularity_data, on=['id_artist', 'name_artist'], how='left')
            loved['final_score'] = loved['popularity_score'] if 'popularity_score' in loved.columns else 0

        loved = loved.drop_duplicates(subset='id_artist')
        if 'final_score' in loved.columns:
            loved = loved.sort_values('final_score', ascending=False)

        return loved.head(limit)

    def get_similar_genre_artists(self, user_id, popularity_data, exclude_ids, limit=5):
        genre_query = f"""
            SELECT DISTINCT ag.id_genre
            FROM artist_genre ag
            JOIN favorite f ON ag.id_artist = f.id_artist
            WHERE f.id_user = {user_id}

            UNION

            SELECT DISTINCT ag.id_genre
            FROM artist_genre ag
            JOIN concert c ON ag.id_artist = c.id_artist
            JOIN ticket t ON c.id_concert = t.id_concert
            WHERE t.id_user = {user_id}
        """
        genres = pd.read_sql(genre_query, self.engine)['id_genre'].tolist()

        if not genres:
            return pd.DataFrame()

        genre_artists_query = f"""
            SELECT DISTINCT a.id_artist, a.name_artist
            FROM artist a
            JOIN artist_genre ag ON a.id_artist = ag.id_artist
            WHERE ag.id_genre IN ({','.join(map(str, genres))})
              AND a.id_artist NOT IN ({','.join(map(str, exclude_ids)) if exclude_ids else '0'})
        """
        genre_artists = pd.read_sql(genre_artists_query, self.engine)
        genre_artists = genre_artists.merge(popularity_data, on=['id_artist', 'name_artist'], how='left')
        genre_artists = genre_artists.sort_values('popularity_score', ascending=False)

        return genre_artists.head(limit)

    def get_full_personal_recommendation(self, user_id, loved_limit=5, similar_limit=5):
        popularity_data = self.load_data_for_popular_artists()
        popularity_data['popularity_score'] = popularity_data.apply(self.calculate_popularity_score, axis=1)

        loved = self.recommend_loved_artists(user_id, popularity_data, limit=loved_limit)

        interacted_ids = set()
        if not loved.empty and 'id_artist' in loved.columns:
            interacted_ids.update(loved['id_artist'].tolist())

        favorites, tickets_df = self.load_user_preferences(user_id)
        interacted_ids.update(favorites)
        if not tickets_df.empty:
            interacted_ids.update(tickets_df['id_artist'].tolist())

        similar = self.get_similar_genre_artists(user_id, popularity_data, interacted_ids, limit=similar_limit)

        loved_cols = ['id_artist', 'name_artist', 'final_score']
        similar_cols = ['id_artist', 'name_artist', 'popularity_score']

        if not loved.empty:
            loved = loved[[col for col in loved_cols if col in loved.columns]]
        else:
            loved = pd.DataFrame(columns=loved_cols)

        if not similar.empty:
            similar = similar[[col for col in similar_cols if col in similar.columns]]
        else:
            similar = pd.DataFrame(columns=similar_cols)

        return loved, similar

    def get_popular_artists(self, limit=10):
        data = self.load_data_for_popular_artists()
        data['popularity_score'] = data.apply(self.calculate_popularity_score, axis=1)
        return data.sort_values('popularity_score', ascending=False).head(limit)

    def update_recommendation_table(self):
        logger.info("Обновление персональных рекомендаций...")
        users = pd.read_sql("SELECT id_user FROM user", self.engine)
        recommendations = []

        for user_id in users['id_user']:
            loved, similar = self.get_full_personal_recommendation(user_id)
            for id_artist in pd.concat([loved['id_artist'], similar['id_artist']]).drop_duplicates():
                recommendations.append((user_id, int(id_artist)))

        try:
            with self.connection.cursor() as cursor:
                cursor.execute("DELETE FROM recommendation")
                for id_user, id_artist in recommendations:
                    cursor.execute(
                        "INSERT INTO recommendation (id_user, id_artist) VALUES (%s, %s)",
                        (id_user, id_artist)
                    )
            self.connection.commit()
            logger.info(f"Добавлено {len(recommendations)} рекомендаций в таблицу.")
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