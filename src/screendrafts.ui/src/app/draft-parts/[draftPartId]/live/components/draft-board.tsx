// app/draft-parts/[draftPartId]/live/components/draft-board.tsx
'use client';

import { useLiveDraft } from '../live-draft-context';

interface DraftBoardProps {
  activeSlot?: number | null; // highlighted slot (drafter's current turn)
}

export function DraftBoard({ activeSlot }: DraftBoardProps) {
  const { gameplay, picks } = useLiveDraft();

  const { minPosition, maxPosition } = resolveRange(gameplay);
  const slots = Array.from(
    { length: maxPosition - minPosition + 1 },
    (_, i) => maxPosition - i,
  );

  return (
    <div className="grid gap-px bg-white/10 border border-white/10">
      {slots.map((slot) => {
        const landedPick = picks.find(
          (p) =>
            p.boardPosition === slot &&
            (!p.wasVetoed || p.wasVetoOverridden),
        );
        const isActive = activeSlot === slot;

        return (
          <div
            key={slot}
            className={`flex items-center gap-4 px-4 py-3 bg-sd-ink transition-colors ${
              isActive ? 'ring-1 ring-sd-red ring-inset' : ''
            }`}
          >
            <span className="font-oswald text-sd-red font-bold text-xl w-8 text-right shrink-0">
              {slot}
            </span>
            {landedPick ? (
              <div className="flex-1 min-w-0">
                <a
                  href={`https://www.themoviedb.org/movie/${landedPick.tmdbId}`}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="font-oswald text-sd-paper hover:text-light-blue transition-colors truncate block"
                >
                  {landedPick.movieTitle}
                  {landedPick.movieYear && (
                    <span className="text-white/40 ml-2 text-sm">({landedPick.movieYear})</span>
                  )}
                </a>
                <span className="text-xs text-white/40 font-mono">{landedPick.playedByName}</span>
                {landedPick.wasVetoOverridden && (
                  <span className="ml-2 text-[10px] bg-light-blue/20 text-light-blue px-1.5 py-0.5 font-oswald tracking-wider">
                    SAVED
                  </span>
                )}
              </div>
            ) : (
              <span className="text-white/20 text-sm font-mono italic">
                {isActive ? 'AWAITING PICK…' : '—'}
              </span>
            )}
          </div>
        );
      })}
    </div>
  );
}

function resolveRange(gameplay: ReturnType<typeof useLiveDraft>['gameplay']) {
  // Derive min/max from owned board slots across all positions
  const allSlots = gameplay.draftPositions?.flatMap((p) => p.ownedBoardSlots) ?? [];
  if (allSlots.length === 0) return { minPosition: 1, maxPosition: 7 };
  return { minPosition: Math.min(...allSlots.filter((s) => s !== undefined)), maxPosition: Math.max(...allSlots.filter((s) => s !== undefined)) };
}