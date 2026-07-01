// app/draft-parts/[draftPartId]/live/components/draft-completion-summary.tsx
'use client';

import { useLiveDraft } from '../live-draft-context';
import type { DraftCompletionSummary, PickHonorificSummary, DrafterHonorificSummary } from '../live-draft-context';

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

// ── Main component ────────────────────────────────────────────────────────────

interface Props {
  summary: DraftCompletionSummary;
}

export function DraftCompletionSummaryModal({ summary }: Props) {
  const { dismissCompletionSummary, participants } = useLiveDraft();

  const hasMovieHonorifics = summary.movieHonorifics.length > 0;
  const hasDrafterHonorifics = summary.drafterHonorifics.length > 0;
  const hasAnyHonorifics = hasMovieHonorifics || hasDrafterHonorifics;

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
          {/* Final board */}
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

          {/* Honorifics */}
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