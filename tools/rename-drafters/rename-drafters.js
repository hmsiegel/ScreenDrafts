const { readdirSync, existsSync, renameSync, writeFileSync, appendFileSync } = require('fs');
const { join, basename, extname } = require('path');

const dir = join(__dirname, '../../src/screendrafts.ui/public/drafters');
const dryRun = process.argv.includes('--dry-run');
const logFile = join(__dirname, 'skipped-files.log');

const lowercaseParticles = new Set(["de", "da", "del", "di", "la", "le", "van", "von", "st"]);

// Normalize accents
function normalizeAccents(text) {
   return text.normalize('NFD').replace(/[\u0300-\u036f]/g, '');
}

function normalizeToSlug(parts){
   return parts
      .map(p => {
         const cleaned = normalizeAccents(p.toLowerCase());
         return lowercaseParticles.has(cleaned) ? cleaned : cleaned;
      })
      .join('-');
}

function extractNameParts(fileName){
   const base = basename(fileName, ".webp");

   const noDots = base.replace(/\./g, ' ');

   if (noDots.includes('_')) {
      return noDots.split('_').filter(Boolean);
   }

  const parts = noDots.match(/[A-Z][a-z]+|[a-z]+|[A-Z]+(?![a-z])/g);
  return parts ?? [];
}

function logSkippedFiles(reason, fileName) {
   const entry = `[${new Date().toISOString()}] Skipped: ${fileName} - ${reason}\n`;
   appendFileSync(logFile, entry);
   console.warn(`Skipped: ${fileName} - ${reason}`);
}


readdirSync(dir).forEach(fileName => {
   console.log(`Looking in: ${dir}`);
   console.log(`Processing file: ${fileName}`);

   if (extname(fileName).toLowerCase() !== '.webp') {
      logSkippedFiles(`Not a .webp file`, fileName);
      return;
   }

   const parts = extractNameParts(fileName);

   if (parts.length < 1) {
      const reason = `Skipping file with no valid parts: ${fileName}`;
      logSkippedFiles(reason, fileName);
      return;
   }

   const slug = normalizeToSlug(parts);
   const newFileName = `${slug}.webp`;

   const oldPath = join(dir, fileName);
   const newPath = join(dir, newFileName);

   if (existsSync(newPath)) {
      logSkippedFiles(`File already exists: ${newFileName}`, newFileName);
      return;
   }

   if (dryRun) {
      console.log(`Dry run: would rename ${fileName} to ${newFileName}`);
      return;
   } else {
      renameSync(oldPath, newPath);
      console.log(`Renamed ${fileName} to ${newFileName}`);
   }
});