import { format } from "path";
import { Draft } from "./definitions";
import { formatDate } from "./utils";

const pool = require('../../../db');

export async function getLatestDrafts() {
   //  const response = await fetch(`${API_URL}/drafts/latest`);
   //  return await response.json();
   try {
      const data = await pool.query
         (`SELECT title, episode, draft_dates
            FROM drafts
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