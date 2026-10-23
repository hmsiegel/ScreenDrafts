// src/components/features/drafts/listen-links.tsx

import { GetDraftPartResponse, GetDraftReleaseLinkResponse } from "@/lib/dto";
import type { IconType } from "react-icons";
import { FaSpotify } from "react-icons/fa6";
import { SiApplepodcasts } from "react-icons/si";

const PLATFORMS: { key: string; label: string; Icon: IconType }[] = [
  { key: "Spotify", label: "LISTEN ON SPOTIFY", Icon: FaSpotify },
  { key: "Apple", label: "LISTEN ON APPLE PODCASTS", Icon: SiApplepodcasts },
];

export interface ListenLink {
  key: string;
  label: string;
  url: string;
  Icon: IconType;
}

// Episode links live on a part's MainFeed release. Only resolved links reach the client.
export function getPartListenLinks(part: GetDraftPartResponse): ListenLink[] {
  const links: GetDraftReleaseLinkResponse[] = (part.releases ?? []).flatMap(
    (release) => release.links ?? [],
  );

  return PLATFORMS.flatMap(({ key, label, Icon }) => {
    const url = links.find((link) => link.platform === key)?.url;
    return url ? [{ key, label, url, Icon }] : [];
  });
}

interface ListenButtonsProps {
  links: ListenLink[];
  // Show-level URL, used when no episode links exist.
  fallbackUrl?: string;
  className?: string;
}

const BASE =
  "flex w-full items-center justify-center gap-2 text-center font-oswald text-[12px] tracking-wide px-4 py-3";
const PRIMARY = `${BASE} bg-sd-ink text-white hover:opacity-80 transition-opacity`;
const SECONDARY = `${BASE} border border-sd-ink text-sd-ink hover:bg-sd-ink hover:text-white transition-colors`;

export function ListenButtons({ links, fallbackUrl, className }: ListenButtonsProps) {
  if (links.length === 0) {
    if (!fallbackUrl) return null;
    return (
      <div className={className}>
        <a href={fallbackUrl} target="_blank" rel="noopener noreferrer" className={PRIMARY}>
          <FaSpotify aria-hidden className="size-4" />
          LISTEN ON SPOTIFY
        </a>
      </div>
    );
  }

  return (
    <div className={`flex flex-col gap-2 ${className ?? ""}`}>
      {links.map(({ key, label, url, Icon }, i) => (
        <a
          key={key}
          href={url}
          target="_blank"
          rel="noopener noreferrer"
          className={i === 0 ? PRIMARY : SECONDARY}
        >
          <Icon aria-hidden className="size-4" />
          {label}
        </a>
      ))}
    </div>
  );
}