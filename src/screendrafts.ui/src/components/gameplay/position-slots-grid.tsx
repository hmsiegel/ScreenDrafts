import { type DraftPartPosition } from "@/services/drafts/fetch-draft-parts";
import { TMDB_IMAGE_BASE } from "@/services/movies/fetch-tmdb";
import Image from "next/image";

interface PositionSlotsGridProps {
  positions: DraftPartPosition[];
}

export default function PositionSlotsGrid({ positions }: PositionSlotsGridProps) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
      {positions.map((position, i) => (
        <div key={i} className="border border-sd-ink/20 bg-white">
          <div className="flex items-center gap-2 px-3 py-2 border-b border-sd-ink/10 bg-sd-ink/5">
            <span className="font-oswald font-bold text-sm uppercase tracking-wide text-sd-ink">
              {position.name}
            </span>
            {position.participantName && (
              <span className="font-mono text-xs text-sd-ink/60 ml-auto">{position.participantName}</span>
            )}
          </div>
          <div className="divide-y divide-sd-ink/5">
            {position.picks.length === 0 ? (
              <div className="flex items-center justify-center h-16 border-2 border-dashed border-sd-ink/10 mx-3 my-3">
                <span className="font-mono text-xs text-sd-ink/30">Empty</span>
              </div>
            ) : (
              position.picks.map((pick, j) => (
                <div key={j} className="flex items-center gap-3 px-3 py-2">
                  {pick.posterUrl ? (
                    <Image
                      src={`${TMDB_IMAGE_BASE}${pick.posterUrl}`}
                      alt={pick.title ?? ""}
                      width={28}
                      height={42}
                      className="shrink-0 object-cover"
                    />
                  ) : (
                    <div className="w-7 h-10 bg-sd-ink/10 border border-dashed border-sd-ink/20 shrink-0" />
                  )}
                  {pick.title ? (
                    <div>
                      <p className="text-sm font-medium text-sd-ink">{pick.title}</p>
                      {pick.year && <p className="font-mono text-xs text-sd-ink/50">{pick.year}</p>}
                    </div>
                  ) : (
                    <span className="font-mono text-xs text-sd-ink/30">—</span>
                  )}
                  {pick.pickType && (
                    <span className="ml-auto font-mono text-[10px] uppercase tracking-wide px-1.5 py-0.5 bg-sd-ink/10 text-sd-ink/60">
                      {pick.pickType}
                    </span>
                  )}
                </div>
              ))
            )}
          </div>
        </div>
      ))}
    </div>
  );
}
