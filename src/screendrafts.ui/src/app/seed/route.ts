import bcrypt from 'bcrypt';
import { users } from '@/app/lib/placeholder-data/users';
import { drafts } from '@/app/lib/placeholder-data/drafts';
import { drafters } from '@/app/lib/placeholder-data/drafters';

const pool = require('../../../db');

async function seedUsers() {
   await pool.query(`CREATE EXTENSION IF NOT EXISTS "uuid-ossp"`);
   await pool.query(
      `CREATE TABLE IF NOT EXISTS users (
         id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
         first_name VARCHAR(255) NOT NULL,
         last_name VARCHAR(255) NOT NULL,
         email TEXT NOT NULL UNIQUE,
         password TEXT NOT NULL
      );
   `);

   const insertedUsers = await Promise.all(
      users.map(async (user) => {
         const hashedPassword = await bcrypt.hash(user.password, 10);
         return pool.query(`
            INSERT INTO users (id, first_name, last_name, email, password)
            VALUES ('${user.id}', '${user.firstname}', '${user.lastname}', '${user.email}', '${hashedPassword}')
            ON CONFLICT (id) DO NOTHING;
         `);
      }),
   );

   return insertedUsers;
}

async function seedDrafts() {
   await pool.query(`CREATE EXTENSION IF NOT EXISTS "uuid-ossp"`);

   await pool.query(`
      CREATE TABLE IF NOT EXISTS drafts (
         id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
         title TEXT NOT NULL,
         episode TEXT NOT NULL,
         number_of_films INT NOT NULL,
         number_of_drafters INT NOT NULL,
         draft_dates DATE []
      );
   `);


   const insertedDrafts = await Promise.all(
      drafts.map(
         (draft) => pool.query(`
            INSERT INTO drafts (id, title, episode, number_of_films, number_of_drafters, draft_dates)
            VALUES ('${draft.id}', '${draft.title}', '${draft.episode}', '${draft.numberOfFilms}', '${draft.numberOfDrafters}', '{${draft.draftDate.join(',')}}')
            ON CONFLICT (id) DO NOTHING;
         `,
      ),
   ));
   return insertedDrafts;
}

async function seedDrafters() {
   await pool.query(`CREATE EXTENSION IF NOT EXISTS "uuid-ossp"`);

   await pool.query(`
      CREATE TABLE IF NOT EXISTS drafters (
         primary_id INT NOT NULL,
         id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
         first_name TEXT NOT NULL,
         middle TEXT,
         last_name TEXT NOT NULL,
         full_name TEXT NOT NULL,
         image_url TEXT
      );
   `);

   const insertedDrafters = await Promise.all(
      drafters.map(
         (drafter) => pool.query(`
            INSERT INTO drafters (primary_id, id, first_name, middle, last_name, full_name, image_url)
            VALUES ('${drafter.primaryId}', '${drafter.id}', '${drafter.firstName}', '${drafter.middle}', '${drafter.lastName}', '${drafter.fullName}', '${drafter.image_url}')
            ON CONFLICT (id) DO NOTHING;
         `,
      ),
   ));

   return insertedDrafters;
}

export async function GET(){
   try {
      // await seedUsers();
      await seedDrafts();
      // await seedDrafters();

      return Response.json({ message: 'Database seeded successfully' });
   } catch (error) {
      return Response.json({ error }, { status: 500 });
   }
}
