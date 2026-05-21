"use client";

interface YouTubeEmbedProps {
  url: string;
  title?: string;
}

function extractYouTubeId(url: string): string | null {
  try {
    const u = new URL(url);
    if (u.hostname === "youtu.be") return u.pathname.slice(1).split("?")[0] || null;
    const v = u.searchParams.get("v");
    if (v) return v;
    const segments = u.pathname.split("/");
    const embedIdx = segments.indexOf("embed");
    if (embedIdx !== -1 && !u.hostname.includes("imdb")) return segments[embedIdx + 1] || null;
    return null;
  } catch {
    return null;
  }
}

function isImdbUrl(url: string): boolean {
  try {
    return new URL(url).hostname.includes("imdb.com");
  } catch {
    return false;
  }
}

function LinkCard({ url, title }: { url: string; title: string }) {
  return (
    <a
      href={url}
      target="_blank"
      rel="noopener noreferrer"
      className="flex items-center gap-3 px-5 py-4 hover:bg-sd-paper transition-colors group"
    >
      <div className="w-10 h-10 shrink-0 flex items-center justify-center border-2 border-sd-red text-sd-red group-hover:bg-sd-red group-hover:text-white transition-colors">
        <svg viewBox="0 0 24 24" fill="currentColor" className="w-4 h-4">
          <path d="M8 5v14l11-7z" />
        </svg>
      </div>
      <div className="flex flex-col">
        <span className="font-oswald font-semibold text-[14px] text-sd-ink group-hover:text-sd-red transition-colors">
          WATCH TRAILER
        </span>
        <span className="font-mono text-[9px] tracking-widest text-sd-ink/40">
          {title.toUpperCase()}
        </span>
      </div>
    </a>
  );
}

export default function YouTubeEmbed({ url, title = "Trailer" }: YouTubeEmbedProps) {
  // IMDb embed URLs are DRM-restricted and cannot be iframed from third-party origins.
  // Render a link card instead.
  if (isImdbUrl(url)) {
    return <LinkCard url={url} title={title} />;
  }

  const videoId = extractYouTubeId(url);

  if (!videoId) {
    return <LinkCard url={url} title={title} />;
  }

  return (
    <div className="w-full" style={{ aspectRatio: "16 / 9" }}>
      <iframe
        src={`https://www.youtube-nocookie.com/embed/${videoId}`}
        title={title}
        allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
        allowFullScreen
        className="w-full h-full border-0"
      />
    </div>
  );
}