"use client";

const API_URL = process.env.NEXT_PUBLIC_API_URL;

interface EpisodeImageProps {
  /** publicId-based filename e.g. "d_abc123.jpg" — from API imagePath field */
  imagePath?: string | null;
  /** Fallback: draft title for legacy title-based lookup */
  title: string;
}

export default function EpisodeImage({ imagePath, title }: EpisodeImageProps) {

  // If we have an imagePath from the API, serve from the API static files
  // Otherwise fall back to the legacy title-based lookup in the UI public folder
  const primarySrc = imagePath
    ? `${API_URL}/drafts/${imagePath}`
    : `/episodes/${encodeURIComponent(title)}.jpg`;

  function handleError(e: React.SyntheticEvent<HTMLImageElement>) {
    const img = e.currentTarget;

    if (imagePath) {
      // Cycle through extensions for API-served images
      const base = `${API_URL}/drafts/${imagePath.replace(/\.(jpg|png|webp)$/, "")}`;
      if (img.src.endsWith(".jpg")) {
        img.src = base + ".webp";
      } else if (img.src.endsWith(".webp")) {
        img.src = base + ".png";
      } else {
        img.src = "/screen-drafts.jpg";
      }
    } else {
      // Legacy fallback chain
      const encoded = encodeURIComponent(title);
      if (img.src.includes("/episodes/") && img.src.endsWith(".jpg")) {
        img.src = `/episodes/${encoded}.webp`;
      } else {
        img.src = "/screen-drafts.jpg";
      }
    }
  }

  return (
    <div className="mt-4 mb-4 border border-sd-ink/10 overflow-hidden">
      <img
        src={primarySrc}
        alt={title}
        className="w-full object-cover"
        onError={handleError}
      />
    </div>
  );
}