// components/features/drafts/predictions-section.tsx
import { arrangePredictionRows } from "@/lib/prediction-rows";
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

// ── Prediction table ─────────────────────────────────────────────────────────
// One table, one column per contestant. Row order and alignment come from
// arrangePredictionRows (ranked: highest slot first; unranked: hits on top,
// shared titles on one row).

function PredictionTable({ sets }: { sets: DraftPartPredictionResponse[] }) {
  const { ranked, rows } = arrangePredictionRows(
    sets.map((s) => ({ name: s.contestantDisplayName, entries: s.entries })),
    sets.some((s) => s.isRanked)
  );

  // A set shows ✓ / strike-through only once it has been scored.
  const scored = sets.map((s) => s.result != null && s.entries.some((e) => e.isCorrect != null));
  const anyResult = sets.some((s) => s.result != null);

  return (
    <div className="border border-sd-ink/10 bg-sd-paper p-3 overflow-x-auto">
      <table className="w-full table-fixed text-xs font-['JetBrains_Mono']">
        <thead>
          <tr className="border-b border-sd-ink/10 align-bottom">
            {ranked && <th className="w-6" />}
            {sets.map((set) => (
              <th key={set.publicId} className="text-left py-1 pr-3 font-normal">
                <span className="font-['Oswald'] text-sm font-semibold text-sd-ink">
                  {set.contestantDisplayName}
                </span>
                {set.result != null && (
                  <span className="ml-2 text-xs font-bold text-sd-red">
                    +{set.result.pointsAwarded}pts
                  </span>
                )}
                {!set.isLocked && (
                  <span className="ml-2 text-[9px] uppercase tracking-widest text-sd-dim border border-sd-dim/30 px-1 py-0.5">
                    open
                  </span>
                )}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((row, r) => (
            <tr key={r} className="align-baseline">
              {ranked && (
                <td className="py-0.5 pr-1 text-sd-dim tabular-nums">{row.rank}.</td>
              )}
              {row.cells.map((entry: PredictionEntryResponse | null, c) => {
                const correct = scored[c] && entry?.isCorrect === true;
                const wrong = scored[c] && entry?.isCorrect === false;

                return (
                  <td key={sets[c].publicId} className="py-0.5 pr-3 break-words">
                    {entry && (
                      <span
                        className={
                          correct
                            ? "text-sd-ink font-medium"
                            : wrong
                              ? "text-sd-dim line-through"
                              : "text-sd-ink"
                        }
                      >
                        {entry.mediaTitle}
                        {correct && <span className="ml-1.5 text-[10px] text-green-600">✓</span>}
                      </span>
                    )}
                  </td>
                );
              })}
            </tr>
          ))}
        </tbody>
        {anyResult && (
          <tfoot>
            <tr className="border-t border-sd-ink/10 text-[10px] text-sd-dim">
              {ranked && <td />}
              {sets.map((set) => (
                <td key={set.publicId} className="pt-1.5 pr-3">
                  {set.result != null && (
                    <>
                      <span>
                        {set.result.correctCount}/{set.entries.length} correct
                      </span>
                      {set.result.shootsTheMoon && (
                        <span className="ml-3 text-sd-red font-semibold">🌙 Shoots the Moon</span>
                      )}
                    </>
                  )}
                </td>
              ))}
            </tr>
          </tfoot>
        )}
      </table>
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
        <PredictionTable sets={predictions} />
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