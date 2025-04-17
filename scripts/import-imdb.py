import csv
import requests
import time

API_KEY = '948c8494'
URL = 'http://www.omdbapi.com/'

def get_imdb_id(movie_title):
   params = {
        't': movie_title,
        'apikey': API_KEY
   }
   try:
        response = requests.get(URL, params=params)
        response.raise_for_status()
        data = response.json()
        if data['Response'] == 'True':
            return data.get['imdbID', None]
        else:
            print(f"Movie not found: {movie_title}. Error: {data.get('Error', None)}")
            return None
   except requests.exceptions.RequestException as e:
         print(f"HTTP error for title '{movie_title}': {e}")
         return None

def update_movies_csv(input_file, output_file):
    with open(input_file, mode='r', newline='', encoding='utf-8') as infile, \
         open(output_file, mode='w', newline='', encoding='utf-8') as outfile:

        reader = csv.DictReader(infile)
        fieldnames = reader.fieldnames + ['imdbID']
        writer = csv.DictWriter(outfile, fieldnames=fieldnames)
        writer.writeheader()

        for row in reader:
         title = row.get('title')
         if title:
               imdb_id = get_imdb_id(title)
               if imdb_id:
                  row['imdbID'] = imdb_id
                  print(f"Updated {title} with imdbID {imdb_id}")
               writer.writerow(row)
               time.sleep(0.2)
         else:
               writer.writerow(row)

if __name__ == '__main__':
    input_csv = 'movies.csv'
    output_csv = 'movies_with_imdb.csv'
    update_movies_csv(input_csv, output_csv)
