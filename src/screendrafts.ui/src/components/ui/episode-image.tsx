"use client";

import { DRAFT_IMAGE_FALLBACKS, draftImageCandidates } from "@/lib/cdn";
import FallbackImage from "@/components/features/drafts/fallback-image";

interface EpisodeImageProps {
  /** Exact filename from the API's imagePath field, e.g. "d_abc123-1a2b3c4d.webp". */
  imagePath?: string | null;
  /** Draft publicId. Finds pre-CDN images stored in R2 as "{publicId}.jpg" / ".webp". */
  publicId?: string | null;
  /** Alt text, and the key for the legacy title-based lookup in the UI public folder. */
  title: string;
}

const DEFAULT_IMAGE = "/screen-drafts.jpg";

export default function EpisodeImage({ imagePath, publicId, title }: EpisodeImageProps) {
  const cdnSources = draftImageCandidates(publicId, imagePath);

  // With nothing to look up in R2, fall back to the old title-based files.
  const sources =
    cdnSources.length > 0
      ? [...cdnSources, ...DRAFT_IMAGE_FALLBACKS]
      : [
          `/episodes/${encodeURIComponent(title)}.jpg`,
          `/episodes/${encodeURIComponent(title)}.webp`,
          ...DRAFT_IMAGE_FALLBACKS,
        ];

  return (
    <div className="mt-4 mb-4 border border-sd-ink/10 overflow-hidden">
      <FallbackImage sources={sources} alt={title} className="w-full object-cover" />
    </div>
  );
}