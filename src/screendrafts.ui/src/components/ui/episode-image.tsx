"use client";

import { cdnUrl } from "@/lib/cdn";

interface EpisodeImageProps {
  /** publicId-based filename e.g. "d_abc123-1a2b3c4d.jpg" — from API imagePath field */
  imagePath?: string | null;
  /** Fallback: draft title for legacy title-based lookup */
  title: string;
}

const DEFAULT_IMAGE = "/screen-drafts.jpg";

export default function EpisodeImage({ imagePath, title }: EpisodeImageProps) {
  // imagePath from the API -> CDN (drafts/).
  // Otherwise the legacy title-based lookup in the UI public folder.
  const primarySrc = imagePath
    ? cdnUrl("drafts", imagePath)
    : `/episodes/${encodeURIComponent(title)}.jpg`;

  function handleError(e: React.SyntheticEvent<HTMLImageElement>) {
    const img = e.currentTarget;

    // Already on the default: stop, or a missing default loops forever.
    if (img.src.endsWith(DEFAULT_IMAGE)) return;

    // The API stores the exact filename, so an API-served image has nothing to retry.
    if (imagePath) {
      img.src = DEFAULT_IMAGE;
      return;
    }

    // Legacy: .jpg -> .webp -> default
    if (img.src.includes("/episodes/") && img.src.endsWith(".jpg")) {
      img.src = `/episodes/${encodeURIComponent(title)}.webp`;
    } else {
      img.src = DEFAULT_IMAGE;
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