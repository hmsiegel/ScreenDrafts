// components/features/home/commissioner-standings.tsx
import type { MappedStandings } from "@/services/home/fetch-home-data";

const rankColors = ['bg-sd-blue', 'bg-sd-red', 'bg-sd-ink', 'bg-gray-400'];

/** standings is null when there is no current season or it failed to load. */
export default function CommissionerStandings({
  standings,
}: {
  standings: MappedStandings | null;
}) {
  const episodeRange = !standings
    ? null
    : standings.firstEpisodeNumber && standings.lastEpisodeNumber
      ? `EPS ${standings.firstEpisodeNumber}-${standings.lastEpisodeNumber}`
      : standings.firstEpisodeNumber
        ? `FROM EP ${standings.firstEpisodeNumber}`
        : null;

  return (
    <div className="bg-white border-2 border-sd-ink rounded-sm min-w-0">
      <div className="bg-sd-red text-white px-5 py-3.5">
        <div className="font-oswald font-bold text-[19px] sm:text-[22px] tracking-[0.06em]">
          COMMISSIONER PREDICTIONS
        </div>
        {standings && (
          <div className="flex flex-wrap items-center gap-x-1.5 text-[11px] tracking-[0.18em] opacity-85 mt-0.5">
            <span>SEASON {standings.seasonNumber}</span>
            {episodeRange && (
              <>
                <span className="opacity-50">·</span>
                <span>{episodeRange}</span>
              </>
            )}
            {standings.isClosed && (
              <>
                <span className="opacity-50">·</span>
                <span className="bg-white/20 px-1.5 py-0.5 text-[9px] tracking-widest">FINAL</span>
              </>
            )}
          </div>
        )}
      </div>

      {standings && standings.entries.length > 0 ? (
        <div className="px-5 py-5 space-y-0 divide-y divide-gray-100">
          {standings.entries.map((s, i) => (
            <div key={s.name} className="flex items-center gap-3 py-3">
              <div
                className={`w-7 h-7 rounded ${rankColors[i] ?? 'bg-gray-400'} text-white font-oswald font-bold text-sm flex items-center justify-center flex-shrink-0`}
              >
                {s.rank}
              </div>
              {/* min-w-16 rather than w-16: a longer name grows the column instead of overflowing it */}
              <div className="font-oswald font-semibold text-lg tracking-[0.04em] min-w-16 shrink-0">{s.name}</div>
              <div className="flex-1 min-w-8 h-2 bg-gray-100 rounded overflow-hidden">
                <div
                  className={`h-full ${i === 0 ? 'bg-sd-red' : 'bg-sd-blue'}`}
                  style={{ width: `${standings.targetPoints > 0 ? Math.min((s.totalPoints / standings.targetPoints) * 100, 100) : 0}%` }}
                />
              </div>
              <div className="font-mono text-lg font-bold w-9 shrink-0 text-right">{s.totalPoints}</div>
            </div>
          ))}
        </div>
      ) : (
        <div className="px-5 py-6 text-center text-[13px] text-gray-500">
          Standings will show here once the current season is underway.
        </div>
      )}

      <div className="mx-5 mb-5 p-3.5 bg-sd-paper border border-dashed border-sd-ink text-sm leading-relaxed">
        <div className="text-[10px] tracking-[0.22em] text-sd-red mb-1.5 font-bold">★ HOW TO PLAY</div>
          The <strong>Predictions Game</strong> is a side game where commissioners Clay and Ryan attempt to
          predict which movies will be played on each episode of Screen Drafts by the Guest G.M.s.
          {standings && <> The game is played to seasons of {standings.targetPoints} points.</>}
      </div>
    </div>
  );
}