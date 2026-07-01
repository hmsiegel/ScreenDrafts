// app/draft-parts/[draftPartId]/live/components/draft-pick-list.tsx
'use client';

import { useLiveDraft } from '../live-draft-context';

export function DraftPickList() {
  const { picks } = useLiveDraft();

  // Ascending by play order: the first pick played (typically the highest
  // board slot, e.g. #7) appears at the top, with later picks added below —
  // matching the natural top-to-bottom flow of the draft as it happens.
  const sorted = [...picks].sort((a, b) => (a.playOrder ?? 0) - (b.playOrder ?? 0));

  if (sorted.length === 0) {
    return (
      <p className="text-white/30 text-sm font-mono italic py-4">No picks yet.</p>
    );
  }

  return (
    <div className="divide-y divide-white/10">
      {sorted.map((pick) => (
        <div key={pick.playOrder} className="flex items-center gap-3 py-2">
          <span className="font-mono text-white/30 text-xs w-6 text-right shrink-0">
            {pick.playOrder}
          </span>
          <span className="font-oswald text-white/30 text-xs w-6 text-center shrink-0">
            {pick.boardPosition}
          </span>
          <div className="flex-1 min-w-0">
            <a
              href={`https://www.themoviedb.org/movie/${pick.tmdbId}`}
              target="_blank"
              rel="noopener noreferrer"
              className={`font-oswald text-sm transition-colors hover:text-light-blue ${
                pick.wasVetoed && !pick.wasVetoOverridden
                  ? 'line-through text-white/30'
                  : 'text-sd-paper'
              }`}
            >
              {pick.movieTitle}
              {pick.movieYear && (
                <span className="text-white/40 ml-1 text-xs">({pick.movieYear})</span>
              )}
            </a>
            <span className="block text-[11px] text-white/40 font-mono">{pick.playedByName}</span>
          </div>
          {pick.wasVetoed && !pick.wasVetoOverridden && (
            <span className="text-[10px] bg-sd-red/20 text-sd-red px-1.5 py-0.5 font-oswald tracking-wider shrink-0">
              VETOED{pick.vetoedByName ? ` BY ${pick.vetoedByName.toUpperCase()}` : ''}
            </span>
          )}
          {pick.wasVetoOverridden && (
            <span className="text-[10px] bg-light-blue/20 text-light-blue px-1.5 py-0.5 font-oswald tracking-wider shrink-0">
              SAVED{pick.savedByName ? ` BY ${pick.savedByName.toUpperCase()}` : ''}
            </span>
          )}
          {pick.wasCommissionerOverride && (
            <span className="text-[10px] bg-white/10 text-white/50 px-1.5 py-0.5 font-oswald tracking-wider shrink-0">
              REMOVED
            </span>
          )}
        </div>
      ))}
    </div>
  );
}