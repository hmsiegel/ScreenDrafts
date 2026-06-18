// app/draft-parts/[draftPartId]/live/components/draft-board.tsx
'use client';

import { useLiveDraft } from '../live-draft-context';
import type { GameplayPickResponse } from '@/lib/dto';

interface DraftBoardProps {
  activeSlot?: number | null; // highlighted slot (drafter's current turn)
  // Render action buttons (veto / override / commissioner-override / undo)
  // for the most recently played pick at a given slot. Only called for the
  // single most recent play_order in the whole draft part.
  renderActions?: (pick: GameplayPickResponse) => React.ReactNode;
}

export function DraftBoard({ activeSlot, renderActions }: DraftBoardProps) {
  const { gameplay, picks } = useLiveDraft();

  const { minPosition, maxPosition } = resolveRange(gameplay);
  const slots = Array.from(
    { length: maxPosition - minPosition + 1 },
    (_, i) => maxPosition - i,
  );

  const mostRecentPick = picks.reduce<GameplayPickResponse | null>(
    (acc, p) => (!acc || (p.playOrder ?? 0) > (acc.playOrder ?? 0) ? p : acc),
    null,
  );

  return (
    <div className="grid gap-px bg-white/10 border border-white/10">
      {slots.map((slot) => {
        // A slot can hold either a landed pick (counts toward the board) or
        // the most recent pick even if vetoed-and-not-saved, so the host can
        // still see and act on it (override / undo) right where it happened.
        const landedPick = picks.find(
          (p) => p.boardPosition === slot && (!p.wasVetoed || p.wasVetoOverridden),
        );
        const vetoedRecentPick =
          !landedPick &&
          mostRecentPick?.boardPosition === slot &&
          mostRecentPick.wasVetoed &&
          !mostRecentPick.wasVetoOverridden
            ? mostRecentPick
            : null;

        const displayPick = landedPick ?? vetoedRecentPick;
        const isActive = activeSlot === slot;
        const isMostRecent = mostRecentPick?.boardPosition === slot && mostRecentPick.playOrder === displayPick?.playOrder;

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
            {displayPick ? (
              <>
                <div className="flex-1 min-w-0">
                  <a
                    href={`https://www.themoviedb.org/movie/${displayPick.tmdbId}`}
                    target="_blank"
                    rel="noopener noreferrer"
                    className={`font-oswald transition-colors truncate block ${
                      displayPick.wasVetoed && !displayPick.wasVetoOverridden
                        ? 'line-through text-white/30 hover:text-white/50'
                        : 'text-sd-paper hover:text-light-blue'
                    }`}
                  >
                    {displayPick.movieTitle}
                    {displayPick.movieYear && (
                      <span className="text-white/40 ml-2 text-sm">({displayPick.movieYear})</span>
                    )}
                  </a>
                  <span className="text-xs text-white/40 font-mono">{displayPick.playedByName}</span>
                  {displayPick.wasVetoOverridden && (
                    <span className="ml-2 text-[10px] bg-light-blue/20 text-light-blue px-1.5 py-0.5 font-oswald tracking-wider">
                      SAVED
                    </span>
                  )}
                  {displayPick.wasVetoed && !displayPick.wasVetoOverridden && (
                    <span className="ml-2 text-[10px] bg-sd-red/20 text-sd-red px-1.5 py-0.5 font-oswald tracking-wider">
                      VETOED
                    </span>
                  )}
                </div>
                {isMostRecent && renderActions && (
                  <div className="shrink-0">{renderActions(displayPick)}</div>
                )}
              </>
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
  const allSlots = gameplay.draftPositions?.flatMap((p) => p.ownedBoardSlots) ?? [];
  if (allSlots.length === 0) return { minPosition: 1, maxPosition: 7 };
  return {
    minPosition: Math.min(...allSlots.filter((s) => s !== undefined)),
    maxPosition: Math.max(...allSlots.filter((s) => s !== undefined)),
  };
}