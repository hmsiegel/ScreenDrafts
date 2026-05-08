const SPOTIFY_URL = process.env.NEXT_PUBLIC_SPOTIFY_URL;
const APPLE_URL = process.env.NEXT_PUBLIC_APPLE_PODCASTS_URL;
const PATREON_URL = process.env.NEXT_PUBLIC_PATREON_URL;
const RSS_URL = process.env.NEXT_PUBLIC_RSS_URL;

export default function AnnouncementBar() {
  return (
    <div className="bg-sd-ink text-white px-8 py-2.5 flex items-center justify-end text-xs tracking-[0.18em]">
      <div className="flex gap-4">
        {RSS_URL && (
          <a href={RSS_URL} target="_blank" rel="noopener noreferrer" className="opacity-70 hover:opacity-100 transition-opacity">
            RSS
          </a>
        )}
        {APPLE_URL && (
          <a href={APPLE_URL} target="_blank" rel="noopener noreferrer" className="opacity-70 hover:opacity-100 transition-opacity">
            APPLE
          </a>
        )}
        {SPOTIFY_URL && (
          <a href={SPOTIFY_URL} target="_blank" rel="noopener noreferrer" className="opacity-70 hover:opacity-100 transition-opacity">
            SPOTIFY
          </a>
        )}
        {PATREON_URL && (
          <a href={PATREON_URL} target="_blank" rel="noopener noreferrer" className="text-sd-red hover:opacity-80 transition-opacity">
            PATREON →
          </a>
        )}
      </div>
    </div>
  );
}
