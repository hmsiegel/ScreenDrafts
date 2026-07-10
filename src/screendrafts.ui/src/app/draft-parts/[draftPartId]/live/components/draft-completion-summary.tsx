// app/draft-parts/[draftPartId]/live/components/draft-completion-summary.tsx
'use client';

import { useLiveDraft } from '../live-draft-context';
import type {
  DraftCompletionSummary,
  PickHonorificSummary,
  DrafterHonorificSummary,
  PredictionSummary,
  SeasonStanding,
} from '../live-draft-context';

// ── Honorific name maps ────────────────────────────────────────────────────────

const MOVIE_HONORIFIC_NAMES: Record<number, string> = {
  2: 'Marquee of Fame',
  3: 'Hat Trick',
  4: 'Grand Slam',
  5: 'High Five',
};

const DRAFTER_HONORIFIC_NAMES: Record<number, string> = {
  5: 'All-Star',
  10: 'Hall of Fame',
  15: 'MVP',
  20: 'Legend',
};

function movieHonorificName(newCount: number): string {
  // Named thresholds: 2, 3, 4, 5+. Anything >= 5 is High Five.
  const key = newCount >= 5 ? 5 : newCount;
  return MOVIE_HONORIFIC_NAMES[key] ?? `${newCount} Appearances`;
}

function drafterHonorificName(newCount: number): string {
  // Named thresholds: 5, 10, 15, 20+.
  const key = newCount >= 20 ? 20 : newCount >= 15 ? 15 : newCount >= 10 ? 10 : 5;
  return DRAFTER_HONORIFIC_NAMES[key] ?? `${newCount} Appearances`;
}

// ── Sub-components ────────────────────────────────────────────────────────────

function BoardRow({ position, title }: { position: number; title: string }) {
  return (
    <div className="flex items-baseline gap-4 py-2 border-b border-white/5 last:border-0">
      <span className="font-oswald text-sd-red text-lg w-8 shrink-0">#{position}</span>
      <span className="font-oswald text-sd-paper text-lg leading-tight">{title}</span>
    </div>
  );
}

function MovieHonorificRow({ h }: { h: PickHonorificSummary }) {
  return (
    <div className="flex items-center justify-between py-2 border-b border-white/5 last:border-0">
      <div>
        <p className="font-oswald text-sd-paper">{h.mediaTitle}</p>
        <p className="font-mono text-xs text-white/40 mt-0.5">
          {h.priorCount} → {h.newCount} canonical appearances
        </p>
      </div>
      <span className="font-oswald text-yellow-400 text-sm tracking-wider ml-4 shrink-0">
        {movieHonorificName(h.newCount)}
      </span>
    </div>
  );
}

function DrafterHonorificRow({
  h,
  participants,
}: {
  h: DrafterHonorificSummary;
  participants: { participantId?: string; participantName?: string }[];
}) {
  const name =
    participants.find((p) => p.participantId === h.drafterIdValue)?.participantName ??
    h.drafterIdValue;

  return (
    <div className="flex items-center justify-between py-2 border-b border-white/5 last:border-0">
      <div>
        <p className="font-oswald text-sd-paper">{name}</p>
        <p className="font-mono text-xs text-white/40 mt-0.5">
          {h.priorCount} → {h.newCount} canonical appearances
        </p>
      </div>
      <span className="font-oswald text-yellow-400 text-sm tracking-wider ml-4 shrink-0">
        {drafterHonorificName(h.newCount)}
      </span>
    </div>
  );
}

function PredictorColumn({ p }: { p: PredictionSummary }) {
  return (
    <div className="border border-white/10 px-4 py-3">
      <p className="font-oswald text-sd-paper text-sm tracking-wider uppercase mb-3">
        {p.contestantDisplayName}
      </p>
      <div className="space-y-1.5 mb-3">
        {p.entries.map((e, i) => (
          <div key={i} className="flex items-start gap-2">
            <span
              className={`font-mono text-xs mt-0.5 shrink-0 ${
                e.isCorrect ? 'text-light-blue' : 'text-white/20'
              }`}
            >
              {e.isCorrect ? '✓' : '✗'}
            </span>
            <span
              className={`font-mono text-sm leading-tight ${
                e.isCorrect ? 'text-sd-paper' : 'text-white/30 line-through'
              }`}
            >
              {e.mediaTitle}
            </span>
          </div>
        ))}
      </div>
      <div className="pt-2 border-t border-white/10 flex items-center justify-between">
        <span className="font-oswald text-xs text-white/40 uppercase tracking-wider">
          Total
        </span>
        <span className="font-oswald text-yellow-400 text-lg">
          {p.pointsAwarded} pts
          {p.shootsTheMoon && (
            <span className="ml-1.5 text-[10px] text-light-blue align-middle">MOON</span>
          )}
        </span>
      </div>
    </div>
  );
}

function StandingRow({ s, rank }: { s: SeasonStanding; rank: number }) {
  return (
    <div className="flex items-center justify-between py-2 border-b border-white/5 last:border-0">
      <div className="flex items-baseline gap-3">
        <span className="font-oswald text-sd-red text-lg w-6 shrink-0">{rank}</span>
        <p className="font-oswald text-sd-paper">{s.contestantDisplayName}</p>
      </div>
      <div className="text-right">
        <span className="font-oswald text-sd-paper text-lg">{s.totalPoints} pts</span>
        {s.carryoverPoints > 0 && (
          <p className="font-mono text-xs text-white/40 mt-0.5">
            {s.points} + {s.carryoverPoints} carryover
          </p>
        )}
      </div>
    </div>
  );
}

// ── Main component ────────────────────────────────────────────────────────────

interface Props {
  summary: DraftCompletionSummary;
}

export function DraftCompletionSummaryModal({ summary }: Props) {
  const { dismissCompletionSummary, participants } = useLiveDraft();

  const hasMovieHonorifics = summary.movieHonorifics.length > 0;
  const hasDrafterHonorifics = summary.drafterHonorifics.length > 0;
  const hasAnyHonorifics = hasMovieHonorifics || hasDrafterHonorifics;
  const hasPredictions = summary.predictions.length > 0;
  const hasStandings = summary.standings.length > 0;

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center bg-sd-ink/95 backdrop-blur-sm overflow-y-auto py-8">
      <div className="w-full max-w-5xl mx-6">

        {/* Header */}
        <div className="text-center mb-8">
          <p className="font-oswald text-sd-red text-xs tracking-[0.3em] uppercase mb-2">
            Draft Complete
          </p>
          <h1 className="font-oswald text-4xl text-sd-paper uppercase tracking-wide leading-tight">
            {summary.title}
          </h1>
          <p className="font-mono text-white/40 text-sm mt-2">
            {summary.totalPicks} pick{summary.totalPicks !== 1 ? 's' : ''}
            {summary.vetoCount > 0 && ` · ${summary.vetoCount} veto${summary.vetoCount !== 1 ? 'es' : ''}`}
            {summary.totalParts > 1 && ` · Part ${summary.partIndex} of ${summary.totalParts}`}
          </p>
        </div>

        <div
          className={`grid gap-6 mb-8 ${hasAnyHonorifics ? 'grid-cols-1 lg:grid-cols-2' : 'grid-cols-1'
            }`}
        >
          {/* Row 1 — Final board */}
          <section className="mb-8">
            <p className="font-oswald text-xs tracking-[0.25em] text-white/40 uppercase mb-3">
              Final Board
            </p>
            <div className="border border-white/10 px-4">
              {summary.picks
                .slice()
                .sort((a, b) => a.position - b.position)
                .map((pick) => (
                  <BoardRow
                    key={pick.mediaPublicId}
                    position={pick.position}
                    title={pick.mediaTitle}
                  />
                ))}
            </div>
          </section>

          {/* Row 1 — Honorifics */}
          {hasAnyHonorifics && (
            <section className="mb-8">
              <p className="font-oswald text-xs tracking-[0.25em] text-white/40 uppercase mb-3">
                Honorifics This Draft
              </p>
              <div className="border border-white/10 px-4">
                {summary.movieHonorifics.map((h) => (
                  <MovieHonorificRow key={h.mediaPublicId} h={h} />
                ))}
                {summary.drafterHonorifics.map((h) => (
                  <DrafterHonorificRow
                    key={h.drafterIdValue}
                    h={h}
                    participants={participants}
                  />
                ))}
              </div>
            </section>
          )}
        </div>

        {/* Row 2 — Predictor results, one column per predictor */}
        {hasPredictions && (
          <section className="mb-8">
            <p className="font-oswald text-xs tracking-[0.25em] text-white/40 uppercase mb-3">
              Commissioner Predictions
            </p>
            <div
              className="grid gap-4"
              style={{
                gridTemplateColumns: `repeat(${summary.predictions.length}, minmax(0, 1fr))`,
              }}
            >
              {summary.predictions.map((p) => (
                <PredictorColumn key={p.contestantDisplayName} p={p} />
              ))}
            </div>
          </section>
        )}

        {/* Row 3 — Season standings */}
        {hasStandings && (
          <section className="mb-8">
            <p className="font-oswald text-xs tracking-[0.25em] text-white/40 uppercase mb-3">
              Season Standings
            </p>
            <div className="border border-white/10 px-4">
              {summary.standings.map((s, i) => (
                <StandingRow key={s.contestantDisplayName} s={s} rank={i + 1} />
              ))}
            </div>
          </section>
        )}


        {/* OK button */}
        <div className="text-center">
          <button
            onClick={dismissCompletionSummary}
            className="px-12 py-3 bg-sd-blue text-white font-oswald text-sm tracking-[0.2em] uppercase hover:bg-sd-blue/80 transition-colors"
          >
            OK
          </button>
        </div>

      </div>
    </div>
  );
}