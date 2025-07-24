const axios = require('axios');
const cheerio = require('cheerio');
const fs = require('fs');
const createCsvWriter = require('csv-writer').createObjectCsvWriter;

const BASE_URL = 'https://screendrafts.fandom.com';
const START_URL = `${BASE_URL}/wiki/Category:Episodes`;

const CSV_PATH = 'episodes.csv';
const LOG_PATH = 'scrape_log.txt';
const MAX_RETRIES = 3;

const csvWriter = createCsvWriter({
   path: CSV_PATH,
   header: [
      { id: 'title', title: 'Title' },
      { id: 'url', title: 'URL' },
      { id: 'description', title: 'Description' }
   ],
   append: fs.existsSync(CSV_PATH),
});

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
         .map((line) => line.split(',')[1]?.trim())
         .filter((url) => url && url.startsWith("http"))
   );
}

async function getAllEpisodeUrls() {
   let nextUrl = START_URL;
   const episodeUrls = new Set();
   const previouslyScrapedUrls = loadPreviouslyScrapedUrls();

   while (nextUrl) {
      logMessage(`Fetching: ${nextUrl}`);
      try {
         const response = await axios.get(nextUrl);
         const $ = cheerio.load(response.data);

         $('a.category-page__member-link').each((_, element) => {
            const href = $(element).attr('href');
            if (href && href.startsWith('/wiki/')) {
               const fullUrl = `${BASE_URL}${href}`;
               if (!previouslyScrapedUrls.has(fullUrl)) {
                  episodeUrls.add(fullUrl);
               }
            }
         });

         const nextLink = $('.category-page__pagination-next').attr('href');
         nextUrl = nextLink.length ? new URL(nextLink, BASE_URL).href : null;

      } catch (error) {
         logMessage(`Error fetching ${nextUrl}: ${error.message}`);
         nextUrl = null; // Stop on error
      }

      await delay(100); // Rate limiting
   }

   return [...episodeUrls];
}

async function scrapeEpisodeProfile(url, retries = 0) {
   try {
      logMessage(`Scraping profile: ${url}`);
      const { data: html } = await axios.get(url);
      const $ = cheerio.load(html);

      const title = $('h1.page-header__title').text().trim();
      const container = $('.mw-parser-output');

      let description = [];
      let currentParagraph = [];
      let seenAside = false;
      let stopText = "The following films were drafted";

      for (let i = 0; i < container[0].children.length; i++) {
         const node = container[0].children[i];

         // Skip until we see the <aside>
         if (!seenAside) {
            if (node.tagName === "aside") {
               seenAside = true;
            }
            continue;
         }

         // Stop if this is a <p> with the "stop text"
         if (
            node.type === "tag" &&
            node.tagName === "p" &&
            $(node).text().trim().startsWith(stopText)
         ) {
            break;
         }

         // Paragraphs after the aside and before stop marker
         if (node.type === "tag" && node.tagName === "p") {
            const text = $(node).text().trim();
            if (text) {
               description.push(text);
            }
            continue;
         }

         // Inline nodes after <aside> and before first <p>
         if (["text", "tag"].includes(node.type)) {
            if (node.type === "text") {
               currentParagraph.push(node.data?.trim() ?? "");
            } else if (node.tagName === "b" || node.tagName === "strong") {
               currentParagraph.push(`**${$(node).text().trim()}**`);
            } else if (node.tagName === "a") {
               currentParagraph.push($(node).text().trim());
            } else {
               currentParagraph.push($(node).text().trim());
            }
         }
      }

      // Add collected inline text as a paragraph, if any
      const inlineParagraph = currentParagraph.join(" ").replace(/\s+/g, " ").trim();
      if (inlineParagraph) {
         description.unshift(inlineParagraph);
      }

      const episodeData = {
         title,
         url,
         description: description.join("\n\n"),
      };

      logMessage(`Scraped: ${title} - ${url}`);
      await csvWriter.writeRecords([episodeData]);
      return episodeData;
   } catch (error) {
      if (retries < MAX_RETRIES) {
         logMessage(`Retrying ${url} (${retries + 1}/${MAX_RETRIES})`);
         await delay(2000);
         return scrapeEpisodeProfile(url, retries + 1);
      } else {
         logMessage(`Failed to scrape ${url} after ${MAX_RETRIES} retries`);
         return null;
      }
   }
}

(async () => {
   const alreadyScrapedUrls = loadPreviouslyScrapedUrls();
   const episodeUrls = await getAllEpisodeUrls();
   logMessage(`Found ${episodeUrls.length} episode URLs`);

   const results = [];

   for (const url of episodeUrls) {
      if (alreadyScrapedUrls.has(url)) {
         logMessage(`Skipping already scraped URL: ${url}`);
         continue;
      }

      const episodeData = await scrapeEpisodeProfile(url);

      if (episodeData) {
         results.push(episodeData);
         logMessage(`Successfully scraped: ${url}`);
         await csvWriter.writeRecords([episodeData]);
      } else {
         logMessage(`Failed to scrape: ${url}`);
      }
      await delay(100); // Delay to avoid rate limiting
   }

   logMessage('Scraping completed. Total episodes scraped: ' + results.length);
})();
