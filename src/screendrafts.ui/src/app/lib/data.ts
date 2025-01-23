import { Commissioner, Draft, Drafter } from "./definitions";

const pool = require('../../../db');

export async function getLatestDrafts() {
   //  const response = await fetch(`${API_URL}/drafts/latest`);
   //  return await response.json();
   try {
      const data = await pool.query
         (`SELECT title, episode, draft_dates
            FROM drafts
            WHERE episode IS NOT NULL
            ORDER BY episode DESC
            LIMIT 5`);



      const latestDrafts = data.rows.map((draft:Draft) => ({
         ...draft,
         draft_dates: draft.draft_dates.map((date: Date) => date.toLocaleDateString(
            "en-US",
            {
               year: "numeric",
               month: "short",
               day: "numeric",
            }
         )).join(", "),
      }));
      return latestDrafts;
   }
   catch (error) {
      console.error('Error fetching latest drafts:', error);
      throw new Error('Error fetching latest drafts');
   }
}

export async function getDrafters() {
   try {
      const data = await pool.query
         (`SELECT primary_id, full_name
            FROM drafters
            ORDER BY full_name`);

      const drafters = data.rows.map((drafter: Drafter) => ({
         ...drafter,
      }));
      return drafters;
   }
   catch (error) {
      console.error('Error fetching drafters:', error);
      throw new Error('Error fetching drafters');
   }
}

export async function getCommissioners() {
   try {
      const data = await pool.query
         (`SELECT primary_id, full_name
            FROM drafters
            ORDER BY full_name`);

      const commissioners = data.rows.map((commissioner: Commissioner) => ({
         ...commissioner,
      }));
      return commissioners;
   }
   catch (error) {
      console.error('Error fetching drafters:', error);
      throw new Error('Error fetching drafters');
   }
}