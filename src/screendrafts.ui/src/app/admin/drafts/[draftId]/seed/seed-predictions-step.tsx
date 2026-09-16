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
import { PredictionSeasonListItemResponse } from "@/lib/dto";
import { MediaPicker, type SelectedMedia } from "@/components/drafts/media-picker";
import { MEDIA_TYPE_TV_EPISODE, importAndResolveEpisode } from "@/lib/tv-episode-resolve";
import { SeedPredictionsSetup } from "./seed-predictions-setup";
import { SurrogateAssignmentPanel } from "../../new/surrogate-assignment-panel";

const LABEL = "block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1";
const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";
const BTN_PRIMARY =
  "bg-sd-red text-white font-oswald font-medium tracking-wide uppercase px-5 py-2.5 hover:bg-sd-red/90 disabled:opacity-50 transition-colors";
const BTN_SECONDARY =
  "border border-sd-ink/20 text-sd-ink font-mono text-[11px] tracking-widest uppercase px-3 py-1.5 hover:bg-sd-ink/5 disabled:opacity-40 transition-colors";

function seasonLabel(s: PredictionSeasonListItemResponse): string {
  const episodeNumbers = (s.drafts ?? [])
    .map((d) => d.episodeNumber)
    .filter((n): n is number => n != null);

  if (episodeNumbers.length === 0) {
    return `Season ${s.number}`;
  }

  const first = Math.min(...episodeNumbers);
  const last = Math.max(...episodeNumbers);
  return first === last
    ? `Season ${s.number} (Ep ${first})`
    : `Season ${s.number} (Ep ${first}–${last})`;
}

interface Props {
  draftPartPublicId: string;
  accessToken: string;
  hosts: DraftPartHost[];
  onDone: () => void;
  // Same field seed-picks-step.tsx already receives from the wizard page —
  // thread it through the same way. Locks MediaPicker to episode mode for
  // TV-restricted drafts instead of a plain movie search.
  restrictedTvSeriesTmdbId?: number | null;
}

export function SeedPredictionsStep({
  draftPartPublicId,
  accessToken,
  hosts,
  onDone,
  restrictedTvSeriesTmdbId,
}: Props) {
  const [loading, setLoading] = useState(true);
  const [rules, setRules] = useState<DraftPartPredictionRulesDto | null>(null);
  const [predictors, setPredictors] = useState<DraftPartPredictorDto[]>([]);
  const [seasons, setSeasons] = useState<PredictionSeasonListItemResponse[]>([]);
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
        onSkip={onDone}
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
            restrictedTvSeriesTmdbId={restrictedTvSeriesTmdbId}
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
  restrictedTvSeriesTmdbId?: number | null;
  submitted: boolean;
  active: boolean;
  onActivate: () => void;
  onSubmitted: () => void;
}

interface RankedEntry {
  rank: number;
  tmdbId: number;
  title: string;
  year: string | null;
  // Set only for a resolved TV episode — see addEntry. Null for a movie
  // entry, matching PredictionEntryDto.MediaPublicId's meaning on the
  // backend.
  mediaPublicId: string | null;
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
  restrictedTvSeriesTmdbId,
  submitted,
  active,
  onActivate,
  onSubmitted,
}: RowProps) {
  const [entries, setEntries] = useState<RankedEntry[]>([]);
  const [pendingRank, setPendingRank] = useState(1);
  const [submitting, setSubmitting] = useState(false);
  const [resolving, setResolving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [rankError, setRankError] = useState<string | null>(null);

  async function addEntry(media: SelectedMedia) {
    setRankError(null);
    if (entries.length >= requiredCount) return;
    if (entries.some((e) => e.tmdbId === media.tmdbId)) return;
    if (entries.some((e) => e.rank === pendingRank)) {
      setRankError(`Rank ${pendingRank} is already used — pick a different rank first.`);
      return;
    }

    if (media.mediaType === MEDIA_TYPE_TV_EPISODE) {
      if (
        media.tvSeriesTmdbId == null ||
        media.seasonNumber == null ||
        media.episodeNumber == null
      ) {
        return;
      }

      setError(null);
      setResolving(true);
      try {
        const mediaPublicId = await importAndResolveEpisode(
          media.tmdbId,
          media.tvSeriesTmdbId,
          media.seasonNumber,
          media.episodeNumber,
          accessToken
        );

        if (!mediaPublicId) {
          setError("Couldn't resolve that episode — please try again.");
          return;
        }

        const next = [
          ...entries,
          { rank: pendingRank, tmdbId: media.tmdbId, title: media.title, year: media.year, mediaPublicId },
        ].sort((a, b) => a.rank - b.rank);
        setEntries(next);
        setPendingRank(nextAvailableRank(next, requiredCount));
      } finally {
        setResolving(false);
      }
      return;
    }

    const next = [
      ...entries,
      { rank: pendingRank, tmdbId: media.tmdbId, title: media.title, year: media.year, mediaPublicId: null },
    ].sort((a, b) => a.rank - b.rank);
    setEntries(next);
    setPendingRank(nextAvailableRank(next, requiredCount));
  }

  function removeEntry(tmdbId: number) {
    const next = entries.filter((e) => e.tmdbId !== tmdbId);
    setEntries(next);
    setPendingRank(nextAvailableRank(next, requiredCount));
  }

  function updateRank(tmdbId: number, newRank: number) {
    if (newRank < 1 || newRank > requiredCount) return;
    if (entries.some((e) => e.tmdbId !== tmdbId && e.rank === newRank)) {
      setRankError(`Rank ${newRank} is already used by another entry.`);
      return;
    }
    setRankError(null);
    setEntries((prev) =>
      prev
        .map((e) => (e.tmdbId === tmdbId ? { ...e, rank: newRank } : e))
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
          tmdbId: e.tmdbId,
          mediaTitle: e.title,
          orderIndex: e.rank,
          mediaPublicId: e.mediaPublicId,
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
                <li key={e.tmdbId} className="flex items-center gap-3 text-sm">
                  <input
                    type="number"
                    min={1}
                    max={requiredCount}
                    value={e.rank}
                    onChange={(ev) => updateRank(e.tmdbId, parseInt(ev.target.value, 10) || e.rank)}
                    className="w-14 border border-sd-ink/20 bg-sd-paper px-2 py-1 text-sm rounded text-center"
                  />
                  <span className="flex-1">
                    {e.title} {e.year ? `(${e.year})` : ""}
                  </span>
                  <button
                    type="button"
                    onClick={() => removeEntry(e.tmdbId)}
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
                <label className={LABEL}>{restrictedTvSeriesTmdbId ? "Episode" : "Movie"}</label>
                <MediaPicker
                  accessToken={accessToken}
                  onSelect={addEntry}
                  disabled={resolving}
                  fixedSeriesTmdbId={restrictedTvSeriesTmdbId ?? undefined}
                />
                {resolving && (
                  <p className="text-[11px] font-mono text-sd-ink/40 mt-1">Resolving episode…</p>
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
            disabled={!seasonPublicId || entries.length === 0 || submitting || resolving}
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