const SPOTIFY_URL = process.env.NEXT_PUBLIC_SPOTIFY_URL;
const APPLE_URL = process.env.NEXT_PUBLIC_APPLE_PODCASTS_URL;
const PATREON_URL = process.env.NEXT_PUBLIC_PATREON_URL;
const RSS_URL = process.env.NEXT_PUBLIC_RSS_URL;
const WIKI_URL = process.env.NEXT_PUBLIC_WIKI_URL;

export default function SiteFooter() {
  return (
    <footer className="bg-sd-ink text-light-blue px-8 py-5 flex justify-between text-[11px] tracking-[0.18em]">
      <div>© 2026 SCREEN DRAFTS · A PODCAST IN GOOD STANDING</div>
<div className="flex gap-4">
        {WIKI_URL && (
          <a href={WIKI_URL} target="_blank" rel="noopener noreferrer" className="hover:text-white transition-colors">
            WIKI
          </a>
        )}
        {PATREON_URL && (
          <a href={PATREON_URL} target="_blank" rel="noopener noreferrer" className="hover:text-white transition-colors">
            PATREON
          </a>
        )}
        {APPLE_URL && (
          <a href={APPLE_URL} target="_blank" rel="noopener noreferrer" className="hover:text-white transition-colors">
            APPLE
          </a>
        )}
        {SPOTIFY_URL && (
          <a href={SPOTIFY_URL} target="_blank" rel="noopener noreferrer" className="hover:text-white transition-colors">
            SPOTIFY
          </a>
        )}
        {RSS_URL && (
          <a href={RSS_URL} target="_blank" rel="noopener noreferrer" className="hover:text-white transition-colors">
            RSS
          </a>
        )}
      </div>
    </footer>
  );
}
