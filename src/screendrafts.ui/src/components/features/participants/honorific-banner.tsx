import Image from "next/image";
import { HonorificResponse } from "@/lib/dto";

// Map honorific value to banner image filename.
// All banner webp files live in public/honorifics/.
const BANNER_MAP: Record<number, { src: string; alt: string }> = {
  0: { src: "/artifacts/Guest_G.M._Banner.webp", alt: "Guest G.M."},
  1: { src: "/artifacts/All-Star_Banner.webp",    alt: "All-Star" },
  2: { src: "/artifacts/Hall_of_Fame_Banner.webp", alt: "Hall of Famer" },
  3: { src: "/artifacts/MVP_Banner.webp",          alt: "MVP" },
  4: { src: "/artifacts/Legends_Banner.webp",       alt: "Legend" },
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
    <div
      className="relative w-full overflow-hidden"
      style={{ height}}
    >
      <Image
        src={banner.src}
        alt={banner.alt}
        fill
        sizes="(max-width: 320px) 100vw, 320px"
        className="object-cover object-center"
      />
    </div>
  );
}