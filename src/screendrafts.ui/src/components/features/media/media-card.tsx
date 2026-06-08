import { MediaListItemResponse } from "@/lib/dto";
import { mediaTypeLabel } from "@/lib/media-type-display";
import Link from "next/link";

const TMDB_IMAGE_BASE = "https://image.tmdb.org/t/p/w500";

function posterInitials(title: string): string {
  return title
    .split(" ")
    .slice(0, 2)
    .map((w) => w[0]?.toUpperCase() ?? "")
    .join("");
}

interface MediaCardProps {
  item: MediaListItemResponse;
  draftCount?: number;
}

export default function MediaCard({ item, draftCount }: MediaCardProps) {
  const { publicId, title, year, mediaType, image } = item;

  return (
    <Link
      href={`/media/${publicId}`}
      className="group block bg-white border-2 border-sd-ink relative transition-all duration-[140ms] hover:-translate-y-[3px] hover:shadow-[6px_6px_0_#0d1430]"
    >
      {/* Poster */}
      <div className="relative w-full overflow-hidden bg-sd-ink" style={{ aspectRatio: "2 / 3" }}>
        {image ? (
          <img
            src={`${TMDB_IMAGE_BASE}${image}`}
            alt={title}
            className="w-full h-full object-cover"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center font-oswald font-bold text-[40px] text-white/30 select-none">
            {posterInitials(title)}
          </div>
        )}
      </div>

      <div className="p-5">
        {/* Type badge + year */}
        <div className="flex items-center gap-2 mb-2">
          <span className="font-mono text-[9px] tracking-widest bg-sd-ink text-white px-1.5 py-0.5 rounded-sm">
            {mediaTypeLabel(mediaType)}
          </span>
          {year && (
            <span className="font-mono text-[10px] text-[#5a6075]">{year}</span>
          )}
        </div>

        {/* Title */}
        <div className="font-oswald font-bold text-[20px] text-sd-ink leading-tight mb-3">
          {title}
        </div>

        {/* Stat */}
        {draftCount !== undefined && (
          <div className="border-t border-sd-ink/10 pt-3 mt-3">
            <StatCell value={draftCount} label="APPEARANCES" />
          </div>
        )}

        <div className="mt-4 font-oswald font-semibold text-[12px] tracking-wide text-sd-blue group-hover:text-sd-red transition-colors">
          VIEW DETAILS →
        </div>
      </div>
    </Link>
  );
}

function StatCell({ value, label }: { value: number; label: string }) {
  return (
    <div className="flex flex-col items-center gap-0.5">
      <span className="font-oswald font-bold text-[28px] text-sd-ink leading-none">{value}</span>
      <span className="font-mono text-[9px] text-[#5a6075] text-center">{label}</span>
    </div>
  );
}
