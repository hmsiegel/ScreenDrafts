const axios = require('axios');
const cheerio = require('cheerio');
const { log } = require('console');
const fs = require('fs');
const { url } = require('inspector');
const createCsvWriter = require('csv-writer').createObjectCsvWriter;

const BASE_URL = 'https://screendrafts.fandom.com';
const START_URL = `${BASE_URL}/wiki/Category:Drafters`;

const CSV_PATH = 'drafters_socials.csv';
const LOG_PATH = 'scrape_log.txt';
const MAX_RETRIES = 3;

const csvWriter = createCsvWriter({
   path: CSV_PATH,
   header: [
      { id: 'name', title: 'Name' },
      { id: 'twitter', title: 'Twitter' },
      { id: 'instagram', title: 'Instagram' },
      { id: 'letterboxd', title: 'Letterboxd' },
      { id: 'blueSky', title: 'Blue Sky' },
      { id: 'url', title: 'Wiki Url' },
   ],
   append: fs.existsSync(CSV_PATH),
});

function extractHandle(url, platform) {
   if (!url) return "";
   const parts = url.split('/');
   return platform === 'letterboxd'
      ? parts.at(-2) || ""
      : parts.at(-1)?.replace("@", "") || "";
}

async function delay(ms) {
   return new Promise(resolve => setTimeout(resolve, ms));
}

function logMessage(message) {
   const entry = `[${new Date().toISOString()}] ${message}`;
   console.log(entry);
   fs.appendFileSync(LOG_PATH, `${entry}\n`);
}

function loadPreviouslyScrapedUrls() {
   if (!fs.existsSync(CSV_PATH)) return new Set();
   const data = fs.readFileSync(CSV_PATH, 'utf8');
   return new Set(
      data
         .split('\n')
         .slice(1) // Skip header
         .map((line) => line.split(',').at(-1)?.trim())
         .filter((url) => url && url.startsWith("http"))
   );
}

async function getAllDrafterUrls() {
   let nextUrl = START_URL;
   const profileUrls = new Set();

   while (nextUrl) {
      logMessage(`Fetching page: ${nextUrl}`);
      const { data: html } = await axios.get(nextUrl);
      const $ = cheerio.load(html);
      $('.category-page__member-link').each((_, element) => {
         const href = $(element).attr('href');
         if (href && href.startsWith('/wiki/')) {
            profileUrls.add(new URL(href, BASE_URL).href);
         }
      });

      const nextLink = $('.category-page__pagination-next').attr('href');
      nextUrl = nextLink ? new URL(nextLink, BASE_URL).href : null;
      await delay(1000); // Delay to avoid rate limiting
   }
   return [...profileUrls];
}

async function scrapeDrafterProfile(url, retries = 0) {
   try {
      logMessage(`Scraping profile: ${url}`);
      const { data: html } = await axios.get(url);
      const $ = cheerio.load(html);

      const name = $('h1.page-header__title').text().trim();
      const links = $(`.mw-parser-output a[href]`);

      const socialHandles = {
         twitter: null,
         instagram: null,
         letterboxd: null,
         blueSky: null
      }

      links.each((_, link) => {
         const href = $(link).attr('href');

         if (href.includes('twitter.com') && !href.includes("getfandom")) {
            socialHandles.twitter ??= href
         } else if (href.includes('instagram.com')) {
            socialHandles.instagram ??= href
         } else if (href.includes('letterboxd.com')) {
            socialHandles.letterboxd ??= href
         } else if (href.includes('bsky.app') || href.includes('bluesky.social')) {
            socialHandles.blueSky ??= href
         }
      });

      console.log(`Found social handles for ${name}:`, socialHandles);

      return {
         name,
         ...socialHandles,
         url
      };
   } catch (error) {
      if (retries < MAX_RETRIES) {
         logMessage(`Error scraping ${url}: ${error.message}. Retrying... (${retries + 1}/${MAX_RETRIES})`);
         await delay(2000); // Wait before retrying
         return scrapeDrafterProfile(url, retries + 1);
      } else {
         logMessage(`Failed to scrape ${url} after ${MAX_RETRIES} attempts.`);
         return null;
      }
   }
}

(async () => {
   const alreadyScrapedUrls = loadPreviouslyScrapedUrls();
   const drafterUrls = await getAllDrafterUrls();
   logMessage(`Found ${drafterUrls.length} drafter profiles.`);

   const results = [];

   for (const url of drafterUrls) {
      if (alreadyScrapedUrls.has(url)) {
         logMessage(`Skipping already scraped URL: ${url}`);
         continue;
      }

      const profileData = await scrapeDrafterProfile(url);
      if (profileData) {
         results.push(profileData);
         logMessage(`Successfully scraped: ${profileData.name} (${url})`);
         await csvWriter.writeRecords([profileData]);
      } else {
         logMessage(`Failed to scrape profile at ${url}`);
      }
      await delay(1000); // Delay between requests
   }

   logMessage(`Scraping completed. Total profiles scraped: ${results.length}`);
})();
