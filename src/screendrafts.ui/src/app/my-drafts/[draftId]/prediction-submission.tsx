"use client";

import { useState, useEffect } from "react";
import MovieSearchInput from "@/components/drafts/movie-search-input";
import { type MovieSearchResult } from "@/services/movies/fetch-tmdb";
import {
  getCurrentPredictionSeason,
  getDraftPartPredictionRules,
  submitPredictionSet,
  SubmitPredictionEntry,
} from "@/services/drafts/fetch-my-drafts";

interface Props {
  accessToken: string;
  draftPartId: string;
  contestantPublicId: string;
  hasSubmitted: boolean;
}

const LABEL = "block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1";

const MODE_LABELS: Record<number, string> = {
  0: "Unordered — all picks count",
  1: "Unordered — top picks count",
  2: "Ordered — top picks count",
  3: "Ordered — all picks count",
};

function isOrderedMode(mode: number) {
  return mode === 2 || mode === 3;
}

export default function PredictionSubmission({
  accessToken,
  draftPartId,
  contestantPublicId,
  hasSubmitted,
}: Props) {
  const [rules, setRules] = useState<{
    predictionMode: number;
    requiredCount: number;
    topN: number | null;
    deadlineUtc: string | null;
  } | null>(null);
  const [rulesLoading, setRulesLoading] = useState(true);
  const [seasonPublicId, setSeasonPublicId] = useState<string | null>(null);

  const [entries, setEntries] = useState<SubmitPredictionEntry[]>([]);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [submitted, setSubmitted] = useState(hasSubmitted);

  useEffect(() => {
    Promise.all([
      getDraftPartPredictionRules(accessToken, draftPartId),
      getCurrentPredictionSeason(accessToken),
    ]).then(([r, season]) => {
      setRules(r);
      setSeasonPublicId(season?.publicId ?? null);
      setRulesLoading(false);
    });
  }, [accessToken, draftPartId]);

  function handleSelect(movie: MovieSearchResult) {
    if (!rules) return;
    if (entries.some((e) => e.tmdbId === movie.tmdbId)) return;
    if (entries.length >= rules.requiredCount) return;
    setEntries([
      ...entries,
      {
        tmdbId: movie.tmdbId,
        mediaTitle: movie.title,
        orderIndex: isOrderedMode(rules.predictionMode) ? entries.length + 1 : null,
        notes: null,
      },
    ]);
  }

  function removeEntry(tmdbId: number) {
    if (!rules) return;
    const filtered = entries.filter((e) => e.tmdbId !== tmdbId);
    setEntries(
      isOrderedMode(rules.predictionMode)
        ? filtered.map((e, i) => ({ ...e, orderIndex: i + 1 }))
        : filtered
    );
  }

  function moveEntry(index: number, direction: -1 | 1) {
    const target = index + direction;
    if (target < 0 || target >= entries.length) return;
    const next = [...entries];
    [next[index], next[target]] = [next[target], next[index]];
    setEntries(next.map((e, i) => ({ ...e, orderIndex: i + 1 })));
  }

  async function handleSubmit() {
    if (!rules || !seasonPublicId) return;
    setError(null);
    setSubmitting(true);
    try {
      await submitPredictionSet(accessToken, draftPartId, {
        seasonPublicId,
        contestantPublicId,
        submittedByPersonPublicId: null,
        sourceKind: 0, // PredictionSourceKind.UI
        entries,
      });
      setSubmitted(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Submission failed.");
    } finally {
      setSubmitting(false);
    }
  }

  if (rulesLoading) {
    return <p className="text-sm text-sd-ink/40 font-mono">Loading…</p>;
  }

  if (!rules) {
    return (
      <p className="text-sm text-sd-ink/40 font-mono">
        Predictions aren&apos;t open for this part.
      </p>
    );
  }

  if (submitted) {
    return (
      <div className="border border-sd-ink/10 rounded p-4 bg-white">
        <p className="text-sm text-sd-ink">Your predictions are in.</p>
      </div>
    );
  }

  const deadlinePassed = rules.deadlineUtc ? new Date() > new Date(rules.deadlineUtc) : false;
  const complete = entries.length === rules.requiredCount;
  const ordered = isOrderedMode(rules.predictionMode);

  return (
    <div className="border border-sd-ink/10 rounded p-4 bg-white space-y-4">
      <div>
        <p className="text-sm text-sd-ink font-medium">{MODE_LABELS[rules.predictionMode]}</p>
        <p className="text-[11px] font-mono text-sd-ink/50">
          Pick {rules.requiredCount}
          {rules.topN ? ` (top ${rules.topN} score)` : ""}
          {rules.deadlineUtc && <> — deadline {new Date(rules.deadlineUtc).toLocaleString()}</>}
        </p>
      </div>

      {deadlinePassed ? (
        <p className="text-sm text-sd-red">The submission deadline has passed.</p>
      ) : (
        <>
          {entries.length > 0 && (
            <div className="space-y-1.5">
              {entries.map((e, i) => (
                <div
                  key={e.tmdbId}
                  className="flex items-center gap-2 border border-sd-ink/10 rounded px-3 py-1.5 bg-sd-paper"
                >
                  {ordered && (
                    <span className="text-[11px] font-mono text-sd-ink/40 w-5">
                      {e.orderIndex}
                    </span>
                  )}
                  <span className="text-sm text-sd-ink flex-1">{e.mediaTitle}</span>
                  {ordered && (
                    <>
                      <button
                        type="button"
                        onClick={() => moveEntry(i, -1)}
                        disabled={i === 0}
                        className="text-sd-ink/40 hover:text-sd-ink disabled:opacity-30 text-xs px-1"
                      >
                        ▲
                      </button>
                      <button
                        type="button"
                        onClick={() => moveEntry(i, 1)}
                        disabled={i === entries.length - 1}
                        className="text-sd-ink/40 hover:text-sd-ink disabled:opacity-30 text-xs px-1"
                      >
                        ▼
                      </button>
                    </>
                  )}
                  <button
                    type="button"
                    onClick={() => removeEntry(e.tmdbId)}
                    className="text-sd-ink/40 hover:text-sd-red text-lg leading-none"
                  >
                    ×
                  </button>
                </div>
              ))}
            </div>
          )}

          {entries.length < rules.requiredCount && (
            <div>
              <label className={LABEL}>
                Add Film ({entries.length}/{rules.requiredCount})
              </label>
              <MovieSearchInput
                onSelect={handleSelect}
                accessToken={accessToken}
                placeholder="Search to predict a film…"
              />
            </div>
          )}

          {error && <p className="text-sm text-sd-red">{error}</p>}

          <button
            type="button"
            onClick={handleSubmit}
            disabled={!complete || submitting || !seasonPublicId}
            className="bg-sd-red text-white font-oswald font-medium uppercase tracking-wide text-xs px-4 py-2 hover:bg-sd-red/90 disabled:opacity-50"
          >
            {submitting ? "Submitting…" : "Submit Predictions"}
          </button>
        </>
      )}
    </div>
  );
}