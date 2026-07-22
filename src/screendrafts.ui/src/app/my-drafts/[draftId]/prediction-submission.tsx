"use client";

import { useState, useEffect, useRef, useCallback } from "react";
import * as signalR from "@microsoft/signalr";
import MovieSearchInput from "@/components/drafts/movie-search-input";
import { type MovieSearchResult } from "@/services/movies/fetch-tmdb";
import {
  getCurrentPredictionSeason,
  getDraftPartPredictionRules,
  getDraftPartPredictions,
  submitPredictionSet,
  SubmitPredictionEntry,
} from "@/services/drafts/fetch-my-drafts";
import { parseApiErrorMessage } from "@/lib/parse-api-error";

interface Props {
  accessToken: string;
  draftPartId: string;
  contestantPublicId: string;
  // Kept for backward compatibility with my-draft-tabs.tsx — not the
  // source of truth, just an initial hint for the loading-state label.
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
  const [loading, setLoading] = useState(true);
  const [seasonPublicId, setSeasonPublicId] = useState<string | null>(null);

  const [entries, setEntries] = useState<SubmitPredictionEntry[]>([]);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isLocked, setIsLocked] = useState(false);
  const [lastSavedAt, setLastSavedAt] = useState<Date | null>(null);
  const [justLocked, setJustLocked] = useState(false);

  const loadState = useCallback(async () => {
    const [r, season, sets] = await Promise.all([
      getDraftPartPredictionRules(accessToken, draftPartId),
      getCurrentPredictionSeason(accessToken),
      getDraftPartPredictions(accessToken, draftPartId),
    ]);

    setRules(r);
    setSeasonPublicId(season?.publicId ?? null);

    const mySet = sets.find((s) => s.contestantPublicId === contestantPublicId);
    if (mySet) {
      setIsLocked(mySet.isLocked);
      setEntries(
        [...mySet.entries]
          .sort((a, b) => a.orderIndex - b.orderIndex)
          .map((e) => ({
            tmdbId: e.tmdbId,
            mediaTitle: e.mediaTitle,
            orderIndex: e.orderIndex,
            notes: e.notes,
          }))
      );
    }
  }, [accessToken, draftPartId, contestantPublicId]);

  // Initial load.
  useEffect(() => {
    loadState().finally(() => setLoading(false));
  }, [loadState]);

  // Real-time: if the draft part starts while this tab is open, re-fetch
  // to pick up the server's authoritative locked state — rather than
  // guessing client-side what got locked, since a partial save could
  // still be sitting in local state that never made it to the server.
  const previousTeardownRef = useRef<Promise<void> | null>(null);

  useEffect(() => {
    const hubUrl = `${process.env.NEXT_PUBLIC_API_URL}/drafts/hub`;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => accessToken })
      .withAutomaticReconnect()
      .build();

    connection.on("DraftPartStarted", () => {
      setJustLocked(true);
      loadState();
    });

    let mounted = true;

    async function start() {
      if (previousTeardownRef.current) {
        await previousTeardownRef.current;
      }
      if (!mounted) return;
      try {
        await connection.start();
        if (!mounted) return;
        await connection.invoke("JoinDraftPartAsync", draftPartId);
      } catch {
        // Silent — same as MyDraftsRealtimeRefresher. A failed connection
        // just means a manual reload is needed to see the lock, same as
        // before this existed. The save-time DraftPartAlreadyStarted
        // error is still the hard backstop either way.
      }
    }

    const startPromise = start();

    return () => {
      mounted = false;
      const teardown = startPromise.catch(() => undefined).then(() => connection.stop());
      previousTeardownRef.current = teardown;
    };
  }, [accessToken, draftPartId, loadState]);

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

  async function handleSave() {
    if (!rules || !seasonPublicId || entries.length === 0) return;
    setError(null);
    setSaving(true);
    try {
      await submitPredictionSet(accessToken, draftPartId, {
        seasonPublicId,
        contestantPublicId,
        submittedByPersonPublicId: null,
        sourceKind: 0, // PredictionSourceKind.UI
        entries,
      });
      setLastSavedAt(new Date());
    } catch (err) {
      setError(parseApiErrorMessage(err, "Save failed. Please try again."));
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return (
      <p className="text-sm text-sd-ink/40 font-mono">
        {hasSubmitted ? "Loading your predictions…" : "Loading…"}
      </p>
    );
  }

  if (!rules) {
    return (
      <p className="text-sm text-sd-ink/40 font-mono">
        Predictions aren&apos;t open for this part.
      </p>
    );
  }

  if (isLocked) {
    return (
      <div className="border border-sd-ink/10 rounded p-4 bg-white space-y-2">
        {justLocked && (
          <p className="text-sm text-sd-blue">The draft just started — your picks are locked in.</p>
        )}
        <p className="text-sm text-sd-ink">
          Your predictions are locked in — the draft has started.
        </p>
        {entries.length > 0 && (
          <ul className="text-sm text-sd-ink/70 list-decimal list-inside space-y-0.5">
            {entries.map((e) => (
              <li key={e.tmdbId}>{e.mediaTitle}</li>
            ))}
          </ul>
        )}
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

          <div className="flex items-center gap-3">
            <button
              type="button"
              onClick={handleSave}
              disabled={entries.length === 0 || saving}
              className="bg-sd-red text-white font-oswald font-medium uppercase tracking-wide text-xs px-4 py-2 hover:bg-sd-red/90 disabled:opacity-50"
            >
              {saving ? "Saving…" : complete ? "Save Final Picks" : "Save Progress"}
            </button>
            {lastSavedAt && !saving && (
              <span className="text-[11px] font-mono text-sd-ink/40">
                Saved
              </span>
            )}
          </div>

          <p className="text-[11px] font-mono text-sd-ink/40">
            You can keep editing and saving until the draft starts.
            {!complete && ` ${rules.requiredCount - entries.length} more needed.`}
          </p>
        </>
      )}
    </div>
  );
}