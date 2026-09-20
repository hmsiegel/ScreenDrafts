// components/features/participants/honorific-banner.tsx

import Image from "next/image";
import { HonorificResponse } from "@/lib/dto";
import { artifactUrl } from "@/lib/cdn";

// Map honorific value to banner image. Files live in R2 under artifacts/.
const BANNER_MAP: Record<number, { src: string; alt: string }> = {
  0: { src: artifactUrl("Guest_G.M._Banner.webp"), alt: "Guest G.M." },
  1: { src: artifactUrl("All-Star_Banner.webp"), alt: "All-Star" },
  2: { src: artifactUrl("Hall_of_Fame_Banner.webp"), alt: "Hall of Fame" },
  3: { src: artifactUrl("MVP_Banner.webp"), alt: "MVP" },
  4: { src: artifactUrl("Legends_Banner.webp"), alt: "Legend" },
};

export function HonorificBanner({
  honorific,
  isGM = false,
  size = "card",
}: {
  honorific: HonorificResponse | null;
  isGM?: boolean;
  size?: "card" | "profile";
}) {
  const value = honorific?.honorificValue ?? (isGM ? 0 : null);
  if (value === null) return null;

  const banner = BANNER_MAP[value];
  if (!banner) return null;

  const height = size === "profile" ? 36 : 48;

  return (
    <div className="relative w-full overflow-hidden" style={{ height }}>
      {/* unoptimized: the banners are already webp and the CDN caches them, so
          Next's optimizer (and a remotePatterns entry) is not needed. */}
      <Image
        src={banner.src}
        alt={banner.alt}
        fill
        unoptimized
        className="object-cover object-center"
      />
    </div>
  );
}
