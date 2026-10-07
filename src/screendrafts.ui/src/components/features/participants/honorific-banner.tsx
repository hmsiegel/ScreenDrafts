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

  if (size === "profile") {
    // The profile card's banner runs the card's full width at the image's own aspect ratio. A fixed 36px box
    // with object-cover cropped the lettering top and bottom as soon as the card got wider than the sidebar
    // (a phone-width card is far wider than the 320px lg sidebar), so the height has to follow the width.
    return (
      // eslint-disable-next-line @next/next/no-img-element -- the banners are already webp on the CDN, as in the card size below
      <img src={banner.src} alt={banner.alt} className="block w-full h-auto" />
    );
  }

  return (
    <div className="relative w-full overflow-hidden" style={{ height: 48 }}>
      {/* unoptimized: the banners are already webp and the CDN caches them, so
          Next's optimizer (and a remotePatterns entry) is not needed. */}
      <Image
        src={banner.src}
        alt={banner.alt}
        fill
        unoptimized
        className="object-center object-contain"
      />
    </div>
  );
}
