import type {
  DraftPartPredictionResponse,
  PredictionEntryResponse,
  PredictionStandingsResponse,
  ContestantStandingResponse,
} from "@/lib/dto";

// ── Standings table ──────────────────────────────────────────────────────────

function StandingsTable({ standings }: { standings: PredictionStandingsResponse }) {
  if (!standings.standings || standings.standings.length === 0) return null;

  const sorted = [...standings.standings].sort(
    (a, b) => (b.totalPoints ?? 0) - (a.totalPoints ?? 0)
  );

  const hasCarryover = sorted.some((s) => (s.carryoverPoints ?? 0) !== 0);

  return (
    <div>
      <div className="flex items-center justify-between mb-2">
        <h4 className="font-['Oswald'] text-[10px] font-semibold tracking-[0.15em] uppercase text-sd-dim">
          Season {standings.seasonNumber} Standings
        </h4>
        {standings.isClosed && (
          <span className="font-['JetBrains_Mono'] text-[9px] uppercase tracking-widest text-sd-dim border border-sd-dim/30 px-1 py-0.5">
            Final
          </span>
        )}
      </div>

      <table className="w-full text-xs font-['JetBrains_Mono']">
        <thead>
          <tr className="border-b border-sd-ink/10">
            <th className="text-left py-1 pr-3 font-semibold text-sd-dim uppercase tracking-wider text-[9px]">
              Contestant
            </th>
            <th className="text-right py-1 font-semibold text-sd-dim uppercase tracking-wider text-[9px]">
              Running Total
            </th>
          </tr>
        </thead>
        <tbody>
          {sorted.map((s: ContestantStandingResponse, i) => (
            <tr key={s.contestantPublicId} className="border-b border-sd-ink/5 last:border-0">
              <td className="py-1.5 pr-3 text-sd-ink">
                <span className="text-sd-dim mr-1.5">{i + 1}.</span>
                {s.displayName}
                {s.hasCrossedTarget && (
                  <span className="ml-1 text-sd-red text-[10px]" title="Crossed target">★</span>
                )}
              </td>
              <td className="py-1.5 text-right font-semibold tabular-nums">
                <span className={s.hasCrossedTarget ? "text-sd-red" : "text-sd-ink"}>
                  {s.totalPoints ?? 0}
                </span>
                <span className="text-sd-dim text-[9px] ml-0.5">
                  /{standings.targetPoints}
                </span>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {hasCarryover && (
        <p className="mt-1 text-[9px] font-['JetBrains_Mono'] text-sd-dim">
          Total includes carryover handicap
        </p>
      )}
    </div>
  );
}

// ── Single prediction set ────────────────────────────────────────────────────

function PredictionSet({ set }: { set: DraftPartPredictionResponse }) {
  const hasResult = set.result != null;
  const isScored = hasResult && set.entries.some((e) => e.isCorrect != null);

  return (
    <div className="border border-sd-ink/10 bg-sd-paper p-3 space-y-2">
      <div className="flex items-center justify-between">
        <span className="font-['Oswald'] text-sm font-semibold text-sd-ink">
          {set.contestantDisplayName}
        </span>
        <div className="flex items-center gap-2">
          {hasResult && (
            <span className="font-['JetBrains_Mono'] text-xs font-bold text-sd-red">
              +{set.result!.pointsAwarded}pts
            </span>
          )}
          {!set.isLocked && (
            <span className="font-['JetBrains_Mono'] text-[9px] uppercase tracking-widest text-sd-dim border border-sd-dim/30 px-1 py-0.5">
              open
            </span>
          )}
        </div>
      </div>

      {set.entries.length > 0 && (
        <ul className="space-y-0.5">
          {set.entries.map((entry: PredictionEntryResponse, idx) => {
            const correct = isScored && entry.isCorrect === true;
            const wrong = isScored && entry.isCorrect === false;

            return (
              <li
                key={`${set.publicId}-${entry.mediaPublicId ?? idx}`}
                className="flex items-baseline gap-2 font-['JetBrains_Mono'] text-xs"
              >
                {entry.orderIndex != null && (
                  <span className="text-sd-dim tabular-nums w-4 shrink-0">
                    {entry.orderIndex}.
                  </span>
                )}
                <span
                  className={
                    correct ? "text-sd-ink font-medium" :
                      wrong ? "text-sd-dim line-through" :
                        "text-sd-ink"
                  }
                >
                  {entry.mediaTitle}
                </span>
                {correct && (
                  <span className="ml-auto text-[10px] text-green-600 shrink-0">✓</span>
                )}
              </li>
            );
          })}
        </ul>
      )}

      {hasResult && (
        <div className="pt-1 border-t border-sd-ink/10 font-['JetBrains_Mono'] text-[10px] text-sd-dim flex items-center gap-3">
          <span>{set.result!.correctCount}/{set.entries.length} correct</span>
          {set.result!.shootsTheMoon && (
            <span className="text-sd-red font-semibold">🌙 Shoots the Moon</span>
          )}
        </div>
      )}
    </div>
  );
}

// ── Part block ───────────────────────────────────────────────────────────────

interface PartPredictionsProps {
  partLabel: string;
  predictions: DraftPartPredictionResponse[];
  standings: PredictionStandingsResponse | null;
}

function PartPredictions({ partLabel, predictions, standings }: PartPredictionsProps) {
  return (
    <div className="space-y-4">
      {partLabel && (
        <h3 className="font-['Oswald'] text-sm font-semibold tracking-wider uppercase text-sd-dim">
          {partLabel}
        </h3>
      )}
      {predictions.length > 0 && (
        <div className="space-y-2">
          {predictions.map((set) => (
            <PredictionSet key={set.publicId} set={set} />
          ))}
        </div>
      )}
      {standings && <StandingsTable standings={standings} />}
    </div>
  );
}

// ── Root export ──────────────────────────────────────────────────────────────

export interface DraftPartPredictionData {
  draftPartPublicId: string;
  partLabel: string;
  predictions: DraftPartPredictionResponse[];
  standings: PredictionStandingsResponse | null;
}

export function PredictionsSection({ parts }: { parts: DraftPartPredictionData[] }) {
  const hasAny = parts.some(
    (p) =>
      p.predictions.length > 0 ||
      (p.standings?.standings?.length ?? 0) > 0
  );

  if (!hasAny) return null;

  return (
    <section className="mt-8 pt-6 border-t border-sd-ink/10">
      <div className="flex items-center gap-3 mb-6">
        <div className="h-px flex-1 bg-sd-ink/10" />
        <h2 className="font-['Oswald'] text-xs font-semibold tracking-[0.2em] uppercase text-sd-dim whitespace-nowrap">
          Commissioner Predictions
        </h2>
        <div className="h-px flex-1 bg-sd-ink/10" />
      </div>
      <div className="space-y-8">
        {parts
          .filter(
            (p) =>
              p.predictions.length > 0 ||
              (p.standings?.standings?.length ?? 0) > 0
          )
          .map((p) => (
            <PartPredictions
              key={p.draftPartPublicId}
              partLabel={parts.length > 1 ? p.partLabel : ""}
              predictions={p.predictions}
              standings={p.standings}
            />
          ))}
      </div>
    </section>
  );
}