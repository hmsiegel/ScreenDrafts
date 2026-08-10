// app/admin/drafts/[draftId]/seed/seed-predictions-step.tsx
"use client";

import { useEffect, useState } from "react";
import {
  getDraftPartPredictionRules,
  getDraftPartPredictors,
  listSeasons,
  seedSubmitPredictionSet,
  type DraftPartPredictionRulesDto,
  type DraftPartPredictorDto,
  type DraftPartHost,
} from "@/services/admin/fetch-admin-drafts";
import { PredictionSeasonSummaryResponse } from "@/lib/dto";
import { type ResolvedMovie } from "@/lib/movie-resolve";
import { useMovieSearch } from "@/lib/use-movie-search";
import { SeedPredictionsSetup } from "./seed-predictions-setup";
import { SurrogateAssignmentPanel } from "../../new/surrogate-assignment-panel";

const LABEL = "block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1";
const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";
const BTN_PRIMARY =
  "bg-sd-red text-white font-oswald font-medium tracking-wide uppercase px-5 py-2.5 hover:bg-sd-red/90 disabled:opacity-50 transition-colors";
const BTN_SECONDARY =
  "border border-sd-ink/20 text-sd-ink font-mono text-[11px] tracking-widest uppercase px-3 py-1.5 hover:bg-sd-ink/5 disabled:opacity-40 transition-colors";

function seasonLabel(s: PredictionSeasonSummaryResponse): string {
  if (s.firstEpisodeNumber != null && s.lastEpisodeNumber != null) {
    return `Season ${s.number} (Ep ${s.firstEpisodeNumber}–${s.lastEpisodeNumber})`;
  }
  return `Season ${s.number}`;
}

interface Props {
  draftPartPublicId: string;
  accessToken: string;
  hosts: DraftPartHost[];
  onDone: () => void;
}

export function SeedPredictionsStep({ draftPartPublicId, accessToken, hosts, onDone }: Props) {
  const [loading, setLoading] = useState(true);
  const [rules, setRules] = useState<DraftPartPredictionRulesDto | null>(null);
  const [predictors, setPredictors] = useState<DraftPartPredictorDto[]>([]);
  const [seasons, setSeasons] = useState<PredictionSeasonSummaryResponse[]>([]);
  const [seasonPublicId, setSeasonPublicId] = useState("");
  const [submittedContestants, setSubmittedContestants] = useState<Set<string>>(new Set());
  const [activeContestant, setActiveContestant] = useState<string | null>(null);

  async function loadAll() {
    setLoading(true);
    const [rulesResult, predictorsResult, seasonsResult] = await Promise.all([
      getDraftPartPredictionRules(accessToken, draftPartPublicId),
      getDraftPartPredictors(accessToken, draftPartPublicId),
      listSeasons(accessToken),
    ]);
    setRules(rulesResult);
    setPredictors(predictorsResult);
    setSeasons(seasonsResult);
    setLoading(false);
  }

  useEffect(() => {
    loadAll();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [draftPartPublicId]);

  if (loading) {
    return <p className="text-sm text-sd-ink/50 font-mono">Loading…</p>;
  }

  if (!rules) {
    return (
      <SeedPredictionsSetup
        draftPartPublicId={draftPartPublicId}
        accessToken={accessToken}
        hosts={hosts}
        onSaved={loadAll}
      />
    );
  }

  if (predictors.length === 0) {
    return (
      <div className="bg-white border border-sd-ink/10 rounded p-8 max-w-md space-y-4 text-center">
        <p className="text-sm text-sd-ink/60">
          Rules are set but no predictors are configured — nothing to submit.
        </p>
        <button type="button" onClick={onDone} className={BTN_PRIMARY}>
          Continue →
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-2xl">
      <div className="bg-white border border-sd-ink/10 rounded p-4">
        <label className={LABEL}>Season</label>
        <select
          className={INPUT}
          value={seasonPublicId}
          onChange={(e) => setSeasonPublicId(e.target.value)}
        >
          <option value="">Select a season…</option>
          {seasons.map((s) => (
            <option key={s.publicId} value={s.publicId}>
              {seasonLabel(s)}
            </option>
          ))}
        </select>
      </div>

      <div className="space-y-3">
        {predictors.map((p) => (
          <ContestantEntryRow
            key={p.contestantPublicId}
            predictor={p}
            requiredCount={rules.requiredCount}
            seasonPublicId={seasonPublicId}
            draftPartPublicId={draftPartPublicId}
            accessToken={accessToken}
            submitted={submittedContestants.has(p.contestantPublicId)}
            active={activeContestant === p.contestantPublicId}
            onActivate={() => setActiveContestant(p.contestantPublicId)}
            onSubmitted={() =>
              setSubmittedContestants((prev) => new Set(prev).add(p.contestantPublicId))
            }
          />
        ))}
      </div>

      {submittedContestants.size > 0 && (
        <SurrogateAssignmentPanel draftPartPublicId={draftPartPublicId} accessToken={accessToken} />
      )}

      <button type="button" onClick={onDone} className={BTN_PRIMARY}>
        Continue →
      </button>
    </div>
  );
}

interface RowProps {
  predictor: DraftPartPredictorDto;
  requiredCount: number;
  seasonPublicId: string;
  draftPartPublicId: string;
  accessToken: string;
  submitted: boolean;
  active: boolean;
  onActivate: () => void;
  onSubmitted: () => void;
}

interface RankedEntry {
  rank: number;
  movie: ResolvedMovie;
}

function nextAvailableRank(entries: RankedEntry[], requiredCount: number): number {
  const used = new Set(entries.map((e) => e.rank));
  for (let r = 1; r <= requiredCount; r++) {
    if (!used.has(r)) return r;
  }
  return requiredCount;
}

function ContestantEntryRow({
  predictor,
  requiredCount,
  seasonPublicId,
  draftPartPublicId,
  accessToken,
  submitted,
  active,
  onActivate,
  onSubmitted,
}: RowProps) {
  const [entries, setEntries] = useState<RankedEntry[]>([]);
  const [pendingRank, setPendingRank] = useState(1);
  const [query, setQuery] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [rankError, setRankError] = useState<string | null>(null);

  // Passing "" while inactive rather than adding an active flag to the hook
  // — useMovieSearch already treats anything under 2 chars as "no search",
  // so this keeps the row from searching while collapsed without needing a
  // second gating mechanism.
  const { results, searching } = useMovieSearch(active ? query : "", accessToken);

  function addEntry(movie: ResolvedMovie) {
    setRankError(null);
    if (entries.length >= requiredCount) return;
    if (entries.some((e) => e.movie.tmdbId === movie.tmdbId)) return;
    if (entries.some((e) => e.rank === pendingRank)) {
      setRankError(`Rank ${pendingRank} is already used — pick a different rank first.`);
      return;
    }
    const next = [...entries, { rank: pendingRank, movie }].sort((a, b) => a.rank - b.rank);
    setEntries(next);
    setQuery("");
    setPendingRank(nextAvailableRank(next, requiredCount));
  }

  function removeEntry(tmdbId: number) {
    const next = entries.filter((e) => e.movie.tmdbId !== tmdbId);
    setEntries(next);
    setPendingRank(nextAvailableRank(next, requiredCount));
  }

  function updateRank(tmdbId: number, newRank: number) {
    if (newRank < 1 || newRank > requiredCount) return;
    if (entries.some((e) => e.movie.tmdbId !== tmdbId && e.rank === newRank)) {
      setRankError(`Rank ${newRank} is already used by another entry.`);
      return;
    }
    setRankError(null);
    setEntries((prev) =>
      prev
        .map((e) => (e.movie.tmdbId === tmdbId ? { ...e, rank: newRank } : e))
        .sort((a, b) => a.rank - b.rank)
    );
  }

  async function handleSubmit() {
    if (!seasonPublicId || entries.length === 0 || submitting) return;
    setSubmitting(true);
    setError(null);
    try {
      await seedSubmitPredictionSet(accessToken, {
        draftPartId: draftPartPublicId,
        seasonPublicId,
        contestantPublicId: predictor.contestantPublicId,
        submittedByPersonPublicId: null,
        entries: entries.map((e) => ({
          tmdbId: e.movie.tmdbId,
          mediaTitle: e.movie.title,
          orderIndex: e.rank,
        })),
      });
      onSubmitted();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to submit predictions.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="bg-white border border-sd-ink/10 rounded">
      <button
        type="button"
        onClick={onActivate}
        className="w-full flex items-center justify-between px-4 py-3 text-left"
      >
        <span className="text-sm font-medium text-sd-ink">{predictor.contestantDisplayName}</span>
        <span className="flex items-center gap-2">
          {submitted && (
            <span className="text-[10px] font-mono uppercase tracking-widest text-green-700">
              submitted
            </span>
          )}
          <span className="text-[11px] font-mono text-sd-ink/40">
            {entries.length}/{requiredCount}
          </span>
        </span>
      </button>

      {active && (
        <div className="border-t border-sd-ink/10 p-4 space-y-4">
          <p className="text-[11px] font-mono text-sd-ink/40">
            Entries don&apos;t need to cover every rank — enter only the ones you actually
            know. Rank is independent of the order you add them in.
          </p>

          {entries.length > 0 && (
            <ol className="space-y-1">
              {entries.map((e) => (
                <li key={e.movie.tmdbId} className="flex items-center gap-3 text-sm">
                  <input
                    type="number"
                    min={1}
                    max={requiredCount}
                    value={e.rank}
                    onChange={(ev) => updateRank(e.movie.tmdbId, parseInt(ev.target.value, 10) || e.rank)}
                    className="w-14 border border-sd-ink/20 bg-sd-paper px-2 py-1 text-sm rounded text-center"
                  />
                  <span className="flex-1">
                    {e.movie.title} {e.movie.year ? `(${e.movie.year})` : ""}
                  </span>
                  <button
                    type="button"
                    onClick={() => removeEntry(e.movie.tmdbId)}
                    className="text-sd-ink/30 hover:text-sd-red"
                  >
                    ×
                  </button>
                </li>
              ))}
            </ol>
          )}

          {rankError && <p className="text-[11px] font-mono text-sd-red">{rankError}</p>}

          {entries.length < requiredCount && (
            <div className="flex items-start gap-3">
              <div className="shrink-0">
                <label className={LABEL}>Rank</label>
                <input
                  type="number"
                  min={1}
                  max={requiredCount}
                  value={pendingRank}
                  onChange={(e) => setPendingRank(parseInt(e.target.value, 10) || 1)}
                  className="w-14 border border-sd-ink/20 bg-sd-paper px-2 py-2 text-sm rounded text-center"
                />
              </div>
              <div className="flex-1">
                <label className={LABEL}>Movie</label>
                <input
                  type="text"
                  className={INPUT}
                  placeholder="Search movies…"
                  value={query}
                  onChange={(e) => setQuery(e.target.value)}
                />
                {searching && (
                  <p className="text-[11px] font-mono text-sd-ink/40 mt-1">Searching…</p>
                )}
                {results.length > 0 && (
                  <div className="border border-sd-ink/10 rounded mt-2 max-h-40 overflow-y-auto">
                    {results.map((m) => (
                      <button
                        key={m.tmdbId}
                        type="button"
                        onClick={() => addEntry(m)}
                        className="w-full text-left px-3 py-2 text-sm hover:bg-sd-paper/60 border-b border-sd-ink/5 last:border-0"
                      >
                        {m.title} {m.year ? `(${m.year})` : ""}
                      </button>
                    ))}
                  </div>
                )}
              </div>
            </div>
          )}

          {error && (
            <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-3 py-2 rounded">
              {error}
            </div>
          )}

          <button
            type="button"
            onClick={handleSubmit}
            disabled={!seasonPublicId || entries.length === 0 || submitting}
            className={BTN_SECONDARY}
          >
            {submitting ? "Submitting…" : `Submit ${predictor.contestantDisplayName}'s Predictions`}
          </button>
          {!seasonPublicId && (
            <p className="text-[11px] font-mono text-sd-red">Select a season above first.</p>
          )}
        </div>
      )}
    </div>
  );
}