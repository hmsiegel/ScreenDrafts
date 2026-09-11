// app/guest-drafts/[guestDraftId]/live/components/static-draft-pick-list.tsx
// No 'use client' needed — takes everything as props, no hooks.

// Same all-optional, tolerant-of-undefined pattern as StaticBoardPick — both
// real caller types (GameplayPickResponse, GuestDraftDetailPickResponse)
// come through dto.ts with every field optional regardless of the C#
// property's actual nullability.
export interface StaticListPick {
  playOrder?: number;
  position?: number;
  tmdbId?: number | null;
  movieTitle?: string | null;
  movieYear?: string | null;
  playedByDisplayName?: string;
  wasVetoed?: boolean;
  wasVetoOverridden?: boolean;
  wasCommissionerOverride?: boolean;
  vetoedByDisplayName?: string | null;
  savedByDisplayName?: string | null;
}

interface Props {
  picks: StaticListPick[];
}

export function StaticDraftPickList({ picks }: Props) {
  // Ascending by play order — every pick ever played, not just the one
  // currently active per position. This is the actual fix: a summary needs
  // to show a vetoed pick (struck through) AND whatever eventually landed
  // at that same position, not collapse down to one winner per slot the way
  // a live board view does.
  const sorted = [...picks].sort((a, b) => (a.playOrder ?? 0) - (b.playOrder ?? 0));

  if (sorted.length === 0) {
    return <p className="text-white/30 text-sm font-mono italic py-4">No picks recorded.</p>;
  }

  return (
    <div className="divide-y divide-white/10">
      {sorted.map((pick, i) => {
        const wasVetoed = pick.wasVetoed ?? false;
        const wasVetoOverridden = pick.wasVetoOverridden ?? false;
        const wasCommissionerOverride = pick.wasCommissionerOverride ?? false;
        const isStruck = (wasVetoed && !wasVetoOverridden) || wasCommissionerOverride;

        return (
          <div key={pick.playOrder ?? i} className="flex items-center gap-3 py-2">
            <span className="font-mono text-white/30 text-xs w-6 text-right shrink-0">
              {pick.playOrder}
            </span>
            <span className="font-oswald text-white/30 text-xs w-6 text-center shrink-0">
              {pick.position}
            </span>
            <div className="flex-1 min-w-0">
              {pick.tmdbId ? (
                <a
                  href={`https://www.themoviedb.org/movie/${pick.tmdbId}`}
                  target="_blank"
                  rel="noopener noreferrer"
                  className={`font-oswald text-sm transition-colors hover:text-light-blue ${
                    isStruck ? 'line-through text-white/30' : 'text-sd-paper'
                  }`}
                >
                  {pick.movieTitle}
                  {pick.movieYear && (
                    <span className="text-white/40 ml-1 text-xs">({pick.movieYear})</span>
                  )}
                </a>
              ) : (
                <span
                  className={`font-oswald text-sm ${
                    isStruck ? 'line-through text-white/30' : 'text-sd-paper'
                  }`}
                >
                  {pick.movieTitle}
                  {pick.movieYear && (
                    <span className="text-white/40 ml-1 text-xs">({pick.movieYear})</span>
                  )}
                </span>
              )}
              <span className="block text-[11px] text-white/40 font-mono">
                {pick.playedByDisplayName}
              </span>
            </div>
            {wasVetoed && !wasVetoOverridden && (
              <span className="text-[10px] bg-sd-red/20 text-sd-red px-1.5 py-0.5 font-oswald tracking-wider shrink-0">
                VETOED{pick.vetoedByDisplayName ? ` BY ${pick.vetoedByDisplayName.toUpperCase()}` : ''}
              </span>
            )}
            {wasVetoOverridden && (
              <span className="text-[10px] bg-light-blue/20 text-light-blue px-1.5 py-0.5 font-oswald tracking-wider shrink-0">
                SAVED{pick.savedByDisplayName ? ` BY ${pick.savedByDisplayName.toUpperCase()}` : ''}
              </span>
            )}
            {wasCommissionerOverride && (
              <span className="text-[10px] bg-white/10 text-white/50 px-1.5 py-0.5 font-oswald tracking-wider shrink-0">
                REMOVED
              </span>
            )}
          </div>
        );
      })}
    </div>
  );
}