// src/components/features/predictions/predictions-list.tsx
"use client";

import { useState } from "react";
import Link from "next/link";
import { format } from "date-fns/format";
import { parseISO } from "date-fns/parseISO";
import { PredictionSeasonListItemResponse, PredictionSeasonDraftResponse } from "@/lib/dto";

function formatDate(raw: Date | string | undefined): string {
  if (!raw) return "TBA";
  try {
    // parseISO treats a bare date-only string as local midnight, unlike new Date(...)
    // which treats it as UTC midnight — see drafts-table.tsx's formatAirDate for the
    // full explanation of the one-day-back shift this avoids.
    const date = typeof raw === "string" ? parseISO(raw) : raw;
    return format(date, "MMM d, yyyy").toUpperCase();
  } catch {
    return "TBA";
  }
}

export function PredictionsList({ seasons }: { seasons: PredictionSeasonListItemResponse[] }) {
  const [openSeasons, setOpenSeasons] = useState<Set<string>>(new Set());

  function toggleSeason(publicId: string) {
    setOpenSeasons((prev) => {
      const next = new Set(prev);
      if (next.has(publicId)) {
        next.delete(publicId);
      } else {
        next.add(publicId);
      }
      return next;
    });
  }

  if (seasons.length === 0) {
    return (
      <div className="bg-white border-2 border-sd-ink px-6 py-10 text-center font-mono text-[11px] text-sd-ink/40">
        No prediction seasons yet.
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-4">
      {seasons.map((season) => {
        const seasonId = season.publicId ?? "";
        return (
          <SeasonDrawer
            key={seasonId}
            season={season}
            isOpen={openSeasons.has(seasonId)}
            onToggle={() => toggleSeason(seasonId)}
          />
        );
      })}
    </div>
  );
}

function SeasonDrawer({
  season,
  isOpen,
  onToggle,
}: {
  season: PredictionSeasonListItemResponse;
  isOpen: boolean;
  onToggle: () => void;
}) {
  const drafts = season.drafts ?? [];
  const isSeasonOpen = !season.isClosed;
  const dateRange = season.endsOn
    ? `${formatDate(season.startsOn)} – ${formatDate(season.endsOn)}`
    : `${formatDate(season.startsOn)} – PRESENT`;

  return (
    <div className="bg-white border-2 border-sd-ink">
      <button
        type="button"
        onClick={onToggle}
        aria-expanded={isOpen}
        className="w-full bg-sd-ink px-6 py-5 flex items-center gap-3 flex-wrap text-left hover:bg-sd-ink/90 transition-colors"
      >
        <span
          className={`font-mono text-sm text-white/50 transition-transform duration-150 ${isOpen ? "rotate-90" : ""}`}
        >
          ›
        </span>
        <span className="block w-1 h-4 bg-sd-red shrink-0" />
        <h2 className="font-oswald font-bold text-[20px] tracking-wide text-white">
          SEASON {season.number}
        </h2>
        {isSeasonOpen && (
          <span className="font-mono text-[9px] tracking-widest bg-sd-red text-white px-2 py-1 rounded-sm">
            OPEN
          </span>
        )}
        <span className="font-mono text-[11px] text-white/50 ml-auto">{dateRange}</span>
        <span className="font-mono text-[11px] text-white/50">
          TARGET: {season.targetPoints} PTS
        </span>
      </button>

      {isOpen && (
        drafts.length === 0 ? (
          <div className="px-6 py-8 text-center font-mono text-[11px] text-sd-ink/40">
            No drafts have submitted predictions for this season yet.
          </div>
        ) : (
          <div className="divide-y divide-sd-ink/5">
            {drafts.map((draft) => (
              <DraftRow key={draft.draftPartPublicId} draft={draft} />
            ))}
          </div>
        )
      )}
    </div>
  );
}

function DraftRow({ draft }: { draft: PredictionSeasonDraftResponse }) {
  const scores = draft.scores ?? [];

  return (
    <Link
      href={`/drafts/${draft.draftPublicId}`}
      className="group px-6 py-4 flex items-center gap-4 hover:bg-sd-paper transition-colors"
    >
      <div className="w-16 shrink-0 font-mono text-[11px] text-sd-blue font-bold">
        {draft.episodeNumber ? `EP. ${draft.episodeNumber}` : "—"}
      </div>
      <div className="flex-1 min-w-0 font-oswald font-semibold text-[16px] text-sd-ink truncate">
        {draft.label}
      </div>
      {scores.length > 0 && (
        <div className="shrink-0 font-mono text-[11px] text-sd-ink/60 whitespace-nowrap">
          {scores
            .map((s) => `${s.contestantDisplayName}: ${s.pointsAwarded} PTS`)
            .join(" · ")}
        </div>
      )}
      <div className="text-sd-ink/30 group-hover:text-sd-red transition-colors">›</div>
    </Link>
  );
}