// app/guest-drafts/[guestDraftId]/live/components/static-draft-board.tsx
// No 'use client' needed — takes everything as props, no hooks.

export interface StaticBoardPosition {
  ownedBoardSlots?: number[];
}

// Deliberately minimal and deliberately all-optional — matches the
// overlapping subset of fields between GameplayPickResponse (live context)
// and GuestDraftDetailPickResponse (the new standalone endpoint), and every
// field on both of those comes through dto.ts as optional regardless of
// whether the underlying C# property is actually nullable. Fallbacks live in
// the render logic below rather than forcing a defensive mapping step at
// every call site.
export interface StaticBoardPick {
  position?: number;
  tmdbId?: number | null;
  movieTitle?: string | null;
  movieYear?: string | null;
  playedByDisplayName?: string;
  wasVetoed?: boolean;
  wasVetoOverridden?: boolean;
  isActiveOnFinalBoard?: boolean;
}

interface Props {
  positions: StaticBoardPosition[];
  picks: StaticBoardPick[];
}

export function StaticDraftBoard({ positions, picks }: Props) {
  const allSlots = positions.flatMap((p) => p.ownedBoardSlots ?? []);
  const minPosition = allSlots.length > 0 ? Math.min(...allSlots) : 1;
  const maxPosition = allSlots.length > 0 ? Math.max(...allSlots) : 7;
  const slots = Array.from(
    { length: maxPosition - minPosition + 1 },
    (_, i) => maxPosition - i,
  );

  return (
    <div className="grid gap-px bg-white/10 border border-white/10">
      {slots.map((slot) => {
        const landedPick = picks.find(
          (p) => p.position === slot && (p.isActiveOnFinalBoard ?? false),
        );
        const anyPick = landedPick ?? picks.find((p) => p.position === slot);

        const wasVetoed = anyPick?.wasVetoed ?? false;
        const wasVetoOverridden = anyPick?.wasVetoOverridden ?? false;
        const struckThrough = wasVetoed && !wasVetoOverridden;

        return (
          <div key={slot} className="flex items-center gap-4 px-4 py-3 bg-sd-ink">
            <span className="font-oswald text-sd-red font-bold text-xl w-8 text-right shrink-0">
              {slot}
            </span>
            {anyPick ? (
              <div className="flex-1 min-w-0">
                {anyPick.tmdbId ? (
                  <a
                    href={`https://www.themoviedb.org/movie/${anyPick.tmdbId}`}
                    target="_blank"
                    rel="noopener noreferrer"
                    className={`font-oswald transition-colors truncate block ${
                      struckThrough
                        ? 'line-through text-white/30 hover:text-white/50'
                        : 'text-sd-paper hover:text-light-blue'
                    }`}
                  >
                    {anyPick.movieTitle}
                    {anyPick.movieYear && (
                      <span className="text-white/40 ml-2 text-sm">({anyPick.movieYear})</span>
                    )}
                  </a>
                ) : (
                  <span
                    className={`font-oswald truncate block ${
                      struckThrough ? 'line-through text-white/30' : 'text-sd-paper'
                    }`}
                  >
                    {anyPick.movieTitle}
                    {anyPick.movieYear && (
                      <span className="text-white/40 ml-2 text-sm">({anyPick.movieYear})</span>
                    )}
                  </span>
                )}
                <span className="text-xs text-white/40 font-mono">
                  {anyPick.playedByDisplayName}
                </span>
                {wasVetoOverridden && (
                  <span className="ml-2 text-[10px] bg-light-blue/20 text-light-blue px-1.5 py-0.5 font-oswald tracking-wider">
                    SAVED
                  </span>
                )}
                {struckThrough && (
                  <span className="ml-2 text-[10px] bg-sd-red/20 text-sd-red px-1.5 py-0.5 font-oswald tracking-wider">
                    VETOED
                  </span>
                )}
              </div>
            ) : (
              <span className="text-white/20 text-sm font-mono italic">—</span>
            )}
          </div>
        );
      })}
    </div>
  );
}