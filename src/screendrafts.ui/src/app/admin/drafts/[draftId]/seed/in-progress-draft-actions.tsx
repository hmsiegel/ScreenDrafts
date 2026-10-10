// app/admin/drafts/[draftId]/seed/seed-speed-draft-step.tsx
"use client";

import { useEffect, useRef, useState } from "react";
import {
  getDraftPartGameplay,
  getSubDraftGameplay,
  assignSubDraftTrivia,
  assignSubDraftPosition,
  playSubDraftPick,
  applySubDraftVeto,
  advanceSubDraft,
  type DraftPartParticipant,
  type GameplaySubDraftSummary,
  type SubDraftGameplay,
  type SubDraftGameplayPick,
} from "@/services/admin/fetch-admin-drafts";
import type { SeedDraftState } from "./seed-draft-wizard";
import { EpisodeSeasonPicker } from "@/components/drafts/episode-season-picker";
import { LIGHT_THEME } from "@/components/drafts/media-picker-theme";
import { searchTvShows, type TvShowSearchResult } from "@/lib/tv-show-resolve";
import {
  importAndResolveEpisode,
  MEDIA_TYPE_TV_EPISODE,
  type SeasonEpisode,
} from "@/lib/tv-episode-resolve";

const LABEL = "block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1";
const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";
const BTN_PRIMARY =
  "bg-sd-red text-white font-oswald font-medium tracking-wide uppercase px-5 py-2.5 hover:bg-sd-red/90 disabled:opacity-50 transition-colors";
const BTN_SECONDARY =
  "border border-sd-ink/20 text-sd-ink font-mono text-[11px] tracking-widest uppercase px-3 py-1.5 hover:bg-sd-ink/5 disabled:opacity-40 transition-colors";

const SUB_DRAFT_STATUS = { Pending: 0, Active: 1, Completed: 2 } as const;

// ── Subject-driven title browsing ────────────────────────────────────────────
// Ported from speed-draft-pick-panel.tsx (live gameplay) rather than reusing
// movie-resolve.ts's searchMovies/importAndResolve — those are hardcoded to
// mediaType: 0 (movies only), which is exactly the gap that was missing TV.
// Sub-draft picks are always drawn from the sub-draft's own subject: full
// filmography for an Actor/Director subject (TMDb movie_credits + tv_credits,
// per project convention — TV included), or an auto-run title search for a
// Word subject (paged via Load more, narrowable with a server-side refine
// box that re-queries "<subject> <text>"). The subject was fixed at setup
// time, not chosen per-pick.

const API = process.env.NEXT_PUBLIC_API_URL;

interface FilmographyCredit {
  tmdbId: number;
  title: string;
  year?: string | null;
  posterUrl?: string | null;
  mediaType: number; // 0 Movie, 1 TvShow
  creditRole?: string | null;
  isInMediaDatabase: boolean;
  mediaPublicId?: string | null;
}

interface TitleSearchItem {
  imdbId?: string | null;
  tmdbId?: number | null;
  title: string;
  year?: string | null;
  posterUrl?: string | null;
  mediaType: number;
  isInMediaDatabase: boolean;
  mediaPublicId?: string | null;
}

const titleKey = (item: TitleSearchItem) =>
  item.mediaPublicId || `title-${item.tmdbId ?? item.imdbId}`;

async function fetchTitlePage(
  query: string,
  page: number,
  loadedBefore: number,
  accessToken: string
): Promise<{ items: TitleSearchItem[]; hasMore: boolean }> {
  const res = await fetch(
    `${API}/media/search?query=${encodeURIComponent(query)}&page=${page}`,
    { headers: { Authorization: `Bearer ${accessToken}` } }
  );
  if (!res.ok) throw new Error(`Failed to search: ${res.status}`);
  const data = await res.json();
  const paged = data.results ?? data;
  // /media/search returns mediaType as a SmartEnum object ({ name, value }),
  // not a number. Flatten it here, or "mediaType=${mediaType}" in the resolve
  // helpers becomes "[object Object]" and /media/by-tmdb-ids returns 400.
  const items: TitleSearchItem[] = (paged.items ?? []).map(
    (i: Omit<TitleSearchItem, "mediaType"> & { mediaType?: number | { value: number } | null }) => ({
      ...i,
      mediaType: typeof i.mediaType === "number" ? i.mediaType : (i.mediaType?.value ?? 0),
    })
  );
  // Raw count vs TMDb's total. Do not trust hasNextPage/totalPages: the
  // handler computes them from request.PageSize, but TMDb pages are fixed at 20.
  const hasMore = items.length > 0 && loadedBefore + items.length < (paged.totalCount ?? 0);
  return { items, hasMore };
}

const MEDIA_TYPE_TV_SHOW = 1;

async function resolveByTmdbIds(
  tmdbIds: number[],
  mediaType: number,
  accessToken: string
): Promise<Map<number, string>> {
  if (tmdbIds.length === 0) return new Map();
  const params = tmdbIds.map((id) => `tmdbIds=${id}`).join("&") + `&mediaType=${mediaType}`;
  const res = await fetch(`${API}/media/by-tmdb-ids?${params}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });
  if (!res.ok) return new Map();
  const data = await res.json();
  const items: { publicId: string; tmdbId: number }[] = data.items ?? data ?? [];
  return new Map(items.map((i) => [i.tmdbId, i.publicId]));
}

async function resolveByImdbIds(
  imdbIds: string[],
  accessToken: string
): Promise<Map<string, string>> {
  if (imdbIds.length === 0) return new Map();
  const params = imdbIds.map((id) => `imdbIds=${id}`).join("&");
  const res = await fetch(`${API}/media/by-imdb-ids?${params}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });
  if (!res.ok) return new Map();
  const data = await res.json();
  const items: { publicId: string; imdbId: string }[] = data.items ?? data ?? [];
  return new Map(items.map((i) => [i.imdbId, i.publicId]));
}

async function importAndResolveTitle(
  item: { tmdbId?: number | null; imdbId?: string | null; mediaType: number },
  accessToken: string,
  timeoutMs = 25000
): Promise<string | null> {
  const source: "tmdb" | "imdb" = item.tmdbId != null ? "tmdb" : "imdb";

  await fetch(`${API}/integrations/movies/import`, {
    method: "POST",
    headers: { Authorization: `Bearer ${accessToken}`, "Content-Type": "application/json" },
    body: JSON.stringify({
      mediaType: item.mediaType,
      tmdbId: item.tmdbId ?? null,
      imdbId: item.imdbId ?? null,
    }),
  });

  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    await new Promise((r) => setTimeout(r, 600));

    if (source === "tmdb" && item.tmdbId != null) {
      const resolved = await resolveByTmdbIds([item.tmdbId], item.mediaType, accessToken);
      const publicId = resolved.get(item.tmdbId);
      if (publicId) return publicId;
    } else if (item.imdbId) {
      const resolved = await resolveByImdbIds([item.imdbId], accessToken);
      const publicId = resolved.get(item.imdbId);
      if (publicId) return publicId;
    }
  }
  return null;
}

interface Props {
  draft: SeedDraftState;
  // Same shared DraftPartParticipant row used across all three sub-drafts —
  // per project convention this is one row per participant for the whole
  // part, not re-created per sub-draft, so it's passed down once.
  participants: DraftPartParticipant[];
  accessToken: string;
  // Fires once all three sub-drafts report Completed — the wizard has no
  // separate Trivia/Picks steps for Speed Drafts, this one step covers both
  // across all three rounds.
  onDone: () => void;
}

export function SeedSpeedDraftStep({ draft, participants, accessToken, onDone }: Props) {
  const [subDrafts, setSubDrafts] = useState<GameplaySubDraftSummary[] | null>(null);
  const [current, setCurrent] = useState<SubDraftGameplay | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Discovers the 3 sub-drafts from /gameplay, then loads whichever one
  // isn't Completed yet. Re-run after every state-changing call below
  // instead of patching local state — the trivia -> position -> picks ->
  // advance sequence has enough server-side side effects (activation, veto
  // rollover) that trusting the server's view is worth the extra round trip.
  async function refresh() {
    setLoading(true);
    setError(null);
    const gameplay = await getDraftPartGameplay(accessToken, draft.draftPartPublicId);
    const list = (gameplay?.subDrafts ?? []).slice().sort((a, b) => a.index - b.index);
    setSubDrafts(list);

    const next = list.find((s) => s.status !== SUB_DRAFT_STATUS.Completed);
    if (!next) {
      setCurrent(null);
      setLoading(false);
      if (list.length > 0) onDone();
      return;
    }

    const detail = await getSubDraftGameplay(accessToken, draft.draftPartPublicId, next.publicId);
    setCurrent(detail);
    setLoading(false);
  }

  useEffect(() => {
    refresh();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [draft.draftPartPublicId]);

  if (loading) {
    return <p className="text-sm text-sd-ink/50 font-mono">Loading…</p>;
  }

  if (error) {
    return (
      <div className="space-y-4 max-w-2xl">
        <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
          {error}
        </div>
        <button type="button" onClick={refresh} className={BTN_SECONDARY}>
          Retry
        </button>
      </div>
    );
  }

  if (!current || !subDrafts) {
    return <p className="text-sm text-sd-ink/50 font-mono">Finishing up…</p>;
  }

  const totalPicks = current.draftPositions.reduce((sum, p) => sum + p.ownedBoardSlots.length, 0);
  const positionsAssigned = current.draftPositions.every((p) => p.assignedParticipantId != null);
  // Subject fields live on the /gameplay summary (GameplaySubDraftSummary),
  // not on GetSubDraftGameplayResponse — the sub-draft query never selects
  // subject_kind/name/imdb_id at all, only status/index/publicId.
  const currentSummary = subDrafts.find((s) => s.publicId === current.subDraftPublicId);

  return (
    <div className="space-y-6 max-w-2xl">
      <ol className="flex flex-wrap gap-2">
        {subDrafts.map((s) => (
          <li
            key={s.publicId}
            className={`px-3 py-1.5 text-[11px] font-mono tracking-widest uppercase rounded border ${
              s.publicId === current.subDraftPublicId
                ? "bg-sd-ink text-white border-sd-ink"
                : s.status === SUB_DRAFT_STATUS.Completed
                  ? "bg-white text-sd-ink border-sd-ink/30"
                  : "bg-sd-paper text-sd-ink/30 border-sd-ink/10"
            }`}
          >
            Sub-Draft {s.index}
            {s.status === SUB_DRAFT_STATUS.Completed && " ✓"}
          </li>
        ))}
      </ol>

      {current.triviaResults.length === 0 && (
        <SpeedDraftTriviaSection
          draft={draft}
          subDraftPublicId={current.subDraftPublicId}
          participants={participants}
          accessToken={accessToken}
          onSubmitted={refresh}
        />
      )}

      {current.triviaResults.length > 0 && !positionsAssigned && (
        <SpeedDraftPositionSection
          draft={draft}
          subDraftPublicId={current.subDraftPublicId}
          triviaResults={current.triviaResults}
          positions={current.draftPositions}
          participants={participants}
          accessToken={accessToken}
          onAssigned={refresh}
        />
      )}

      {current.triviaResults.length > 0 && positionsAssigned && !currentSummary?.subjectName && (
        <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
          This sub-draft has no subject set — set it (actor, director, or word) before
          entering picks. The seed wizard doesn&apos;t create subjects.
        </div>
      )}

      {current.triviaResults.length > 0 && positionsAssigned && currentSummary?.subjectName && (
        <SpeedDraftPicksSection
          draft={draft}
          subDraftPublicId={current.subDraftPublicId}
          initialPicks={current.picks}
          totalPicks={totalPicks}
          participants={participants}
          accessToken={accessToken}
          subjectKind={currentSummary.subjectKind ?? 2}
          subjectName={currentSummary.subjectName}
          subjectImdbId={currentSummary.subjectImdbId}
          onSubDraftComplete={refresh}
        />
      )}
    </div>
  );
}

// ── Trivia ───────────────────────────────────────────────────────────────────
// No skip option here, unlike the part-level SeedTriviaStep — submitting
// results is what activates the sub-draft server-side
// (DraftPart.AssignSubDraftTriviaResults -> SubDraft.Activate()); there's no
// other way to move it off Pending.

interface TriviaSectionProps {
  draft: SeedDraftState;
  subDraftPublicId: string;
  participants: DraftPartParticipant[];
  accessToken: string;
  onSubmitted: () => void;
}

function SpeedDraftTriviaSection({
  draft,
  subDraftPublicId,
  participants,
  accessToken,
  onSubmitted,
}: TriviaSectionProps) {
  // Speed Drafts require exactly 2 participants (domain-enforced), no
  // Community row is possible — filtering is defensive, matching
  // SeedTriviaStep's convention rather than assuming the invariant holds.
  const eligible = participants.filter(
    (p) => p.participantKindValue.name !== "Community" && p.participantPublicId != null
  );

  const [rows, setRows] = useState(
    eligible.map((p) => ({
      participantIdValue: p.participantIdValue,
      displayName: p.displayName ?? p.participantIdValue,
      position: "" as number | "",
      questionsWon: "" as number | "",
    }))
  );
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  function updateRow(idx: number, field: "position" | "questionsWon", value: number | "") {
    setRows((prev) => prev.map((r, i) => (i === idx ? { ...r, [field]: value } : r)));
  }

  const filledPositions = rows.map((r) => r.position).filter((p) => p !== "");
  const hasDuplicatePositions = new Set(filledPositions).size !== filledPositions.length;
  const allFilled = rows.length > 0 && rows.every((r) => r.position !== "" && r.questionsWon !== "");

  async function handleSubmit() {
    if (!allFilled || hasDuplicatePositions || submitting) return;
    setSubmitting(true);
    setError(null);
    try {
      const byIdValue = new Map(eligible.map((p) => [p.participantIdValue, p]));
      await assignSubDraftTrivia(accessToken, {
        draftPartId: draft.draftPartPublicId,
        subDraftId: subDraftPublicId,
        results: rows.flatMap((r) => {
          const participant = byIdValue.get(r.participantIdValue);
          if (!participant?.participantPublicId || participant.participantKindValue.value == null) {
            return [];
          }
          return [
            {
              participantPublicId: participant.participantPublicId,
              kind: participant.participantKindValue.value,
              position: Number(r.position),
              questionsWon: Number(r.questionsWon),
            },
          ];
        }),
      });
      onSubmitted();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save trivia results.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="bg-white border border-sd-ink/10 rounded p-4 sm:p-8 space-y-6">
      <p className="text-sm text-sd-ink/60">
        Position is finishing place for this sub-draft — 1 for whoever won and gets to
        choose a board slot next.
      </p>

      <div className="space-y-4">
        {rows.map((row, idx) => (
          <div key={row.participantIdValue} className="grid grid-cols-[1fr_auto_auto] gap-3 items-end">
            <div className="text-sm font-medium text-sd-ink pb-2">{row.displayName}</div>
            <div>
              <label className={LABEL}>Position</label>
              <input
                type="number"
                min={1}
                className={`${INPUT} w-20`}
                value={row.position}
                onChange={(e) =>
                  updateRow(idx, "position", e.target.value === "" ? "" : parseInt(e.target.value, 10))
                }
              />
            </div>
            <div>
              <label className={LABEL}>Questions Won</label>
              <input
                type="number"
                min={0}
                className={`${INPUT} w-24`}
                value={row.questionsWon}
                onChange={(e) =>
                  updateRow(idx, "questionsWon", e.target.value === "" ? "" : parseInt(e.target.value, 10))
                }
              />
            </div>
          </div>
        ))}
      </div>

      {hasDuplicatePositions && (
        <p className="text-[11px] font-mono text-sd-red">
          Two participants can&apos;t share the same finishing position.
        </p>
      )}

      {error && (
        <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
          {error}
        </div>
      )}

      <button
        type="button"
        onClick={handleSubmit}
        disabled={!allFilled || hasDuplicatePositions || submitting}
        className={BTN_PRIMARY}
      >
        {submitting ? "Saving…" : "Save & Continue →"}
      </button>
    </div>
  );
}

// ── Position choice ──────────────────────────────────────────────────────────

interface PositionSectionProps {
  draft: SeedDraftState;
  subDraftPublicId: string;
  triviaResults: SubDraftGameplay["triviaResults"];
  positions: SubDraftGameplay["draftPositions"];
  participants: DraftPartParticipant[];
  accessToken: string;
  onAssigned: () => void;
}

function SpeedDraftPositionSection({
  draft,
  subDraftPublicId,
  triviaResults,
  positions,
  participants,
  accessToken,
  onAssigned,
}: PositionSectionProps) {
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const winnerTrivia = triviaResults.find((t) => t.position === 1);
  const winner = winnerTrivia
    ? participants.find((p) => p.participantIdValue === winnerTrivia.participantId)
    : undefined;

  const slotA = positions.find((p) => p.positionName === "A");
  const slotB = positions.find((p) => p.positionName === "B");

  async function handleChoose(choice: "A" | "B") {
    if (!winner?.participantPublicId || winner.participantKindValue.value == null || submitting) return;
    setSubmitting(true);
    setError(null);
    try {
      await assignSubDraftPosition(accessToken, {
        draftPartId: draft.draftPartPublicId,
        subDraftId: subDraftPublicId,
        winnerParticipantPublicId: winner.participantPublicId,
        winnerParticipantKind: winner.participantKindValue.value,
        choice,
      });
      onAssigned();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to assign board position.");
    } finally {
      setSubmitting(false);
    }
  }

  if (!winnerTrivia || !winner) {
    return (
      <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
        No trivia result has finishing position 1 for this sub-draft — can&apos;t determine
        who chooses a board slot. Check the trivia entries just submitted.
      </div>
    );
  }

  return (
    <div className="bg-white border border-sd-ink/10 rounded p-4 sm:p-8 space-y-6">
      <p className="text-sm text-sd-ink/60">
        <span className="font-medium text-sd-ink">
          {winner.displayName ?? winnerTrivia.participantId}
        </span>{" "}
        won trivia — which board slot did they choose?
      </p>

      <div className="grid grid-cols-2 gap-4">
        {(["A", "B"] as const).map((choice) => {
          const slot = choice === "A" ? slotA : slotB;
          return (
            <button
              key={choice}
              type="button"
              onClick={() => handleChoose(choice)}
              disabled={submitting}
              className="border border-sd-ink/20 rounded p-4 text-left hover:bg-sd-paper/60 disabled:opacity-50 transition-colors"
            >
              <div className="font-oswald text-lg text-sd-ink">Slot {choice}</div>
              <div className="text-[11px] font-mono text-sd-ink/40 uppercase tracking-widest mt-1">
                {slot ? `Picks: ${slot.ownedBoardSlots.join(", ")}` : "—"}
              </div>
            </button>
          );
        })}
      </div>

      {error && (
        <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
          {error}
        </div>
      )}
    </div>
  );
}

// ── Picks & vetoes ────────────────────────────────────────────────────────────
// Trimmed version of SeedPicksStep: no Veto Override, no Commissioner
// Override (both domain-blocked for sub-drafts), no host picker (ActedByPublicId
// comes from the JWT, not user-selectable — see PlaySubDraftPickCommandHandler).

interface LocalSubDraftPick {
  playOrder: number;
  position: number;
  movieTitle: string;
  tmdbId: number | null;
  participantIdValue: string;
  participantDisplayName: string;
  status: "landed" | "vetoed";
  vetoedByName: string | null;
}

interface PicksSectionProps {
  draft: SeedDraftState;
  subDraftPublicId: string;
  initialPicks: SubDraftGameplayPick[];
  totalPicks: number;
  participants: DraftPartParticipant[];
  accessToken: string;
  subjectKind: number; // 0 Actor, 1 Director, 2 Word
  subjectName: string;
  subjectImdbId: string | null;
  onSubDraftComplete: () => void;
}

function SpeedDraftPicksSection({
  draft,
  subDraftPublicId,
  initialPicks,
  totalPicks,
  participants,
  accessToken,
  subjectKind,
  subjectName,
  subjectImdbId,
  onSubDraftComplete,
}: PicksSectionProps) {
  const isPersonSubject = subjectKind === 0 || subjectKind === 1;

  const [picks, setPicks] = useState<LocalSubDraftPick[]>(() =>
    initialPicks
      .slice()
      .sort((a, b) => a.playOrder - b.playOrder)
      .map((p) => ({
        playOrder: p.playOrder,
        position: p.boardPosition,
        movieTitle: p.movieTitle,
        tmdbId: p.tmdbId,
        participantIdValue: p.playedById,
        participantDisplayName: p.playedByName,
        status: p.wasVetoed ? "vetoed" : "landed",
        vetoedByName: null,
      }))
  );

  const [position, setPosition] = useState<number | "">("");
  const [participantIdValue, setParticipantIdValue] = useState(participants[0]?.participantIdValue ?? "");
  const [advancing, setAdvancing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [pendingVeto, setPendingVeto] = useState<{ playOrder: number; issuerIdValue: string } | null>(
    null
  );

  // submitting tracks a per-item key (not a bool) — with a browsable list
  // instead of a single search result, more than one row can show a PICK
  // button at once, so each needs its own loading state.
  const [submitting, setSubmitting] = useState<string | null>(null);

  const [browseLoading, setBrowseLoading] = useState(true);
  const [browseError, setBrowseError] = useState<string | null>(null);
  const [personPhotoUrl, setPersonPhotoUrl] = useState<string | null>(null);
  const [credits, setCredits] = useState<FilmographyCredit[]>([]);
  const [titleResults, setTitleResults] = useState<TitleSearchItem[]>([]);
  const [titlePage, setTitlePage] = useState(1);
  const [titleLoaded, setTitleLoaded] = useState(0);
  const [titleHasMore, setTitleHasMore] = useState(false);
  const [loadingMore, setLoadingMore] = useState(false);

  const [refine, setRefine] = useState("");
  const [debouncedRefine, setDebouncedRefine] = useState("");

  useEffect(() => {
    const t = setTimeout(() => setDebouncedRefine(refine.trim()), 350);
    return () => clearTimeout(t);
  }, [refine]);

  // New sub-draft subject: drop any leftover refine text.
  useEffect(() => {
    setRefine("");
    setDebouncedRefine("");
  }, [subjectName]);

  // Person subjects filter the loaded filmography client-side, so refine text
  // stays out of the query (and out of the filmography load effect).
  const searchQuery = isPersonSubject
    ? subjectName
    : `${subjectName} ${debouncedRefine}`.trim();
  const searchQueryRef = useRef(searchQuery);
  searchQueryRef.current = searchQuery;

  // "subject" = the subject's own list; "episode" = browse a series' seasons
  // and episodes. drillSeries pre-locks the picker to one series (Episodes
  // button on a TV row); null means search for any series.
  const [view, setView] = useState<"subject" | "episode">("subject");
  const [drillSeries, setDrillSeries] = useState<{ tmdbId: number; title: string } | null>(null);

  // Word subjects: /media/search is movies-only, so TV shows come from the
  // TMDb TV search (/integrations/movies/tv/search) behind a Movies / TV Shows
  // switch. Same query as the movie list, including the refine text.
  const [kind, setKind] = useState<"all" | "movie" | "tv">(isPersonSubject ? "all" : "movie");
  const [tvResults, setTvResults] = useState<TvShowSearchResult[]>([]);
  const [tvLoading, setTvLoading] = useState(false);

  const visibleCredits = credits
    .filter((c) => kind === "all" || (kind === "tv" ? c.mediaType === 1 : c.mediaType === 0))
    .filter((c) => c.title.toLowerCase().includes(refine.trim().toLowerCase()));

  useEffect(() => {
    setKind(isPersonSubject ? "all" : "movie");
  }, [subjectName, isPersonSubject]);

  useEffect(() => {
    if (isPersonSubject || kind !== "tv") return;
    const controller = new AbortController();
    setTvLoading(true);
    searchTvShows(searchQuery, accessToken, controller.signal)
      .then((r) => {
        if (!controller.signal.aborted) setTvResults(r);
      })
      .finally(() => {
        if (!controller.signal.aborted) setTvLoading(false);
      });
    return () => controller.abort();
  }, [isPersonSubject, kind, searchQuery, accessToken]);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setBrowseLoading(true);
      setBrowseError(null);
      try {
        if (isPersonSubject && subjectImdbId) {
          const res = await fetch(
            `${API}/media/person-filmography?imdbId=${encodeURIComponent(subjectImdbId)}`,
            { headers: { Authorization: `Bearer ${accessToken}` } }
          );
          if (!res.ok) throw new Error(`Failed to load filmography: ${res.status}`);
          const data = await res.json();
          if (cancelled) return;
          setPersonPhotoUrl(data.personPhotoUrl ?? null);
          setCredits(data.credits ?? []);
        } else if (!isPersonSubject) {
          const { items, hasMore } = await fetchTitlePage(searchQuery, 1, 0, accessToken);
          if (cancelled) return;
          setTitleResults(items);
          setTitlePage(1);
          setTitleLoaded(items.length);
          setTitleHasMore(hasMore);
        } else {
          // Person subject with no IMDb ID — SubDraft.SetSubject requires
          // one for Actor/Director, so this shouldn't happen once subjects
          // are set up correctly, but surface it rather than fetch nothing
          // silently.
          setBrowseError("This subject has no IMDb ID on file — filmography can't load.");
        }
      } catch (err) {
        if (!cancelled) setBrowseError(err instanceof Error ? err.message : "Failed to load titles.");
      } finally {
        if (!cancelled) setBrowseLoading(false);
      }
    }

    void load();
    return () => {
      cancelled = true;
    };
  }, [isPersonSubject, subjectImdbId, subjectName, searchQuery, accessToken]);

  async function loadMoreTitles() {
    if (loadingMore || !titleHasMore) return;
    setLoadingMore(true);
    try {
      const next = titlePage + 1;
      const q = searchQuery;
      const { items, hasMore } = await fetchTitlePage(q, next, titleLoaded, accessToken);
      if (q !== searchQueryRef.current) return; // query changed mid-flight
      setTitleResults((prev) => {
        const seen = new Set(prev.map(titleKey));
        return [...prev, ...items.filter((i) => !seen.has(titleKey(i)))];
      });
      setTitlePage(next);
      setTitleLoaded(titleLoaded + items.length);
      setTitleHasMore(hasMore);
    } catch (err) {
      setBrowseError(err instanceof Error ? err.message : "Failed to load more titles.");
    } finally {
      setLoadingMore(false);
    }
  }

  const nextPlayOrder = picks.length + 1;
  const lastPick = picks[picks.length - 1] ?? null;
  const landedCount = picks.filter((p) => p.status === "landed").length;
  const boardFull = landedCount >= totalPicks;

  async function handlePick(
    mediaPublicId: string | null,
    item: {
      tmdbId?: number | null;
      imdbId?: string | null;
      mediaType: number;
      tvSeriesTmdbId?: number;
      seasonNumber?: number;
      episodeNumber?: number;
    },
    title: string,
    submittingKey: string
  ) {
    if (position === "" || !participantIdValue || submitting !== null || boardFull) return;
    const participant = participants.find((p) => p.participantIdValue === participantIdValue);
    if (!participant || participant.participantKindValue.value == null || !participant.participantPublicId) {
      setError("Selected participant is missing a public ID or kind — try re-selecting them.");
      return;
    }
    setSubmitting(submittingKey);
    setError(null);
    try {
      let resolvedPublicId = mediaPublicId;
      if (!resolvedPublicId) {
        resolvedPublicId =
          item.mediaType === MEDIA_TYPE_TV_EPISODE &&
          item.tmdbId != null &&
          item.tvSeriesTmdbId &&
          item.seasonNumber &&
          item.episodeNumber
            ? await importAndResolveEpisode(
                item.tmdbId,
                item.tvSeriesTmdbId,
                item.seasonNumber,
                item.episodeNumber,
                accessToken,
                25000
              )
            : await importAndResolveTitle(item, accessToken);
        if (!resolvedPublicId) {
          setError("Title could not be imported in time — try again in a moment.");
          return;
        }
      }

      await playSubDraftPick(accessToken, {
        draftPartId: draft.draftPartPublicId,
        subDraftId: subDraftPublicId,
        position: Number(position),
        playOrder: nextPlayOrder,
        participantPublicId: participant.participantPublicId,
        participantKind: participant.participantKindValue.value,
        moviePublicId: resolvedPublicId,
      });

      setPicks((prev) => [
        ...prev,
        {
          playOrder: nextPlayOrder,
          position: Number(position),
          movieTitle: title,
          tmdbId: item.tmdbId ?? null,
          participantIdValue,
          participantDisplayName: participant.displayName ?? "—",
          status: "landed",
          vetoedByName: null,
        },
      ]);
      setPosition("");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to record pick.");
    } finally {
      setSubmitting(null);
    }
  }

  function openEpisodes(tmdbId: number, title: string) {
    setDrillSeries({ tmdbId, title });
    setView("episode");
  }

  function pickShow(show: TvShowSearchResult) {
    void handlePick(
      null,
      { tmdbId: show.tmdbId, imdbId: null, mediaType: MEDIA_TYPE_TV_SHOW },
      show.title,
      `tv-${show.tmdbId}`
    );
  }

  function handleEpisodeSelect(ep: SeasonEpisode, seriesTitle: string | null, seriesTmdbId: number) {
    const code = `S${String(ep.seasonNumber).padStart(2, "0")}E${String(ep.episodeNumber).padStart(2, "0")}`;
    void handlePick(
      null,
      {
        tmdbId: ep.tmdbId,
        imdbId: null,
        mediaType: MEDIA_TYPE_TV_EPISODE,
        tvSeriesTmdbId: seriesTmdbId,
        seasonNumber: ep.seasonNumber,
        episodeNumber: ep.episodeNumber,
      },
      seriesTitle ? `${seriesTitle} — ${code} — ${ep.name}` : `${code} — ${ep.name}`,
      `episode-${ep.tmdbId}`
    );
  }

  async function handleVetoLast(issuerIdValue: string) {
    if (!lastPick || lastPick.status !== "landed" || submitting !== null) return;
    const issuer = participants.find((p) => p.participantIdValue === issuerIdValue);
    if (!issuer || issuer.participantKindValue.value == null || !issuer.participantPublicId) {
      setError("Select who's issuing this veto.");
      return;
    }
    setSubmitting("veto");
    setError(null);
    try {
      await applySubDraftVeto(accessToken, {
        draftPartId: draft.draftPartPublicId,
        subDraftId: subDraftPublicId,
        playOrder: lastPick.playOrder,
        issuerPublicId: issuer.participantPublicId,
        issuerKind: issuer.participantKindValue.value,
      });
      setPicks((prev) =>
        prev.map((p) =>
          p.playOrder === lastPick.playOrder
            ? { ...p, status: "vetoed", vetoedByName: issuer.displayName ?? null }
            : p
        )
      );
      setPendingVeto(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to apply veto.");
    } finally {
      setSubmitting(null);
    }
  }

  async function handleAdvance() {
    if (advancing) return;
    setAdvancing(true);
    setError(null);
    try {
      await advanceSubDraft(accessToken, {
        draftPartId: draft.draftPartPublicId,
        subDraftId: subDraftPublicId,
      });
      onSubDraftComplete();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to complete this sub-draft.");
      setAdvancing(false);
    }
  }

  return (
    <div className="space-y-6">
      <div className="bg-white border border-sd-ink/10 rounded p-4 flex flex-wrap items-center justify-between gap-x-4 gap-y-1">
        <p className="text-sm text-sd-ink/70">{landedCount} of {totalPicks} positions landed</p>
        <p className="font-mono text-[11px] text-sd-ink/40 uppercase tracking-widest">
          Next: Play Order {nextPlayOrder}
        </p>
      </div>

      {picks.length > 0 && (
        <div className="bg-white border border-sd-ink/10 rounded divide-y divide-sd-ink/5">
          {picks.map((p) => {
            const showingPicker = pendingVeto?.playOrder === p.playOrder;
            return (
              <div key={p.playOrder} className="px-4 py-2.5 text-sm">
                <div className="flex flex-wrap items-center justify-between gap-x-3 gap-y-1">
                  <div className="min-w-0">
                    <span className="font-mono text-[11px] text-sd-ink/40 mr-2">#{p.playOrder}</span>
                    <span className="font-medium text-sd-ink">{p.movieTitle}</span>
                    <span className="text-sd-ink/50"> — slot {p.position}, {p.participantDisplayName}</span>
                    {p.status === "vetoed" && (
                      <span className="ml-2 text-[10px] font-mono uppercase tracking-widest text-sd-red">
                        vetoed{p.vetoedByName ? ` by ${p.vetoedByName}` : ""} — slot open
                      </span>
                    )}
                  </div>
                  {!showingPicker && p.status === "landed" && p.playOrder === lastPick?.playOrder && (
                    <button
                      type="button"
                      onClick={() => setPendingVeto({ playOrder: p.playOrder, issuerIdValue: "" })}
                      disabled={submitting !== null}
                      className={BTN_SECONDARY}
                    >
                      Veto
                    </button>
                  )}
                </div>

                {showingPicker && pendingVeto && (
                  <div className="flex items-center gap-2 mt-2 pt-2 border-t border-sd-ink/5">
                    <label className="text-[11px] font-mono text-sd-ink/50 uppercase tracking-widest">
                      Vetoed by
                    </label>
                    <select
                      className="border border-sd-ink/20 bg-sd-paper px-2 py-1 text-sm rounded flex-1"
                      value={pendingVeto.issuerIdValue}
                      onChange={(e) => setPendingVeto({ ...pendingVeto, issuerIdValue: e.target.value })}
                    >
                      <option value="">Select…</option>
                      {participants.map((participant) => (
                        <option key={participant.participantIdValue} value={participant.participantIdValue}>
                          {participant.displayName ?? participant.participantIdValue}
                          {participant.participantIdValue === p.participantIdValue ? " (self)" : ""}
                        </option>
                      ))}
                    </select>
                    <button
                      type="button"
                      onClick={() => handleVetoLast(pendingVeto.issuerIdValue)}
                      disabled={!pendingVeto.issuerIdValue || submitting !== null}
                      className={BTN_SECONDARY}
                    >
                      Confirm
                    </button>
                    <button
                      type="button"
                      onClick={() => setPendingVeto(null)}
                      className="text-[11px] font-mono text-sd-ink/40 uppercase tracking-widest hover:underline"
                    >
                      Cancel
                    </button>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}

      {!boardFull && (
        <div className="bg-white border border-sd-ink/10 rounded p-6 space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className={LABEL}>Board Slot</label>
              <input
                type="number"
                min={1}
                className={INPUT}
                value={position}
                onChange={(e) => setPosition(e.target.value === "" ? "" : parseInt(e.target.value, 10))}
              />
            </div>
            <div>
              <label className={LABEL}>Played By</label>
              <select
                className={INPUT}
                value={participantIdValue}
                onChange={(e) => setParticipantIdValue(e.target.value)}
              >
                {participants.map((p) => (
                  <option key={p.participantIdValue} value={p.participantIdValue}>
                    {p.displayName ?? p.participantIdValue}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {(position === "" || !participantIdValue) && (
            <p className="text-[11px] font-mono text-sd-red">
              Set a board slot and participant above, then click Pick on a title below.
            </p>
          )}

          <div>
            <div className="flex gap-1 mb-3 text-[11px] font-mono uppercase tracking-wide">
              <button
                type="button"
                onClick={() => {
                  setView("subject");
                  setDrillSeries(null);
                }}
                className={`px-2 py-1 border ${
                  view === "subject" ? LIGHT_THEME.toggleActive : LIGHT_THEME.toggleInactive
                }`}
              >
                Titles
              </button>
              <button
                type="button"
                onClick={() => {
                  setView("episode");
                  setDrillSeries(null);
                }}
                className={`px-2 py-1 border ${
                  view === "episode" ? LIGHT_THEME.toggleActive : LIGHT_THEME.toggleInactive
                }`}
              >
                TV Episode
              </button>
            </div>

            {view === "subject" && (
              <>
                <label className={LABEL}>
                  {isPersonSubject ? "Filmography" : `Search results for "${subjectName}"`}
                </label>

                {(
                  <div className="flex gap-1 mb-2 text-[11px] font-mono uppercase tracking-wide">
                    {isPersonSubject && (
                      <button
                        type="button"
                        onClick={() => setKind("all")}
                        className={`px-2 py-1 border ${
                          kind === "all" ? LIGHT_THEME.toggleActive : LIGHT_THEME.toggleInactive
                        }`}
                      >
                        All
                      </button>
                    )}
                    <button
                      type="button"
                      onClick={() => setKind("movie")}
                      className={`px-2 py-1 border ${
                        kind === "movie" ? LIGHT_THEME.toggleActive : LIGHT_THEME.toggleInactive
                      }`}
                    >
                      Movies
                    </button>
                    <button
                      type="button"
                      onClick={() => setKind("tv")}
                      className={`px-2 py-1 border ${
                        kind === "tv" ? LIGHT_THEME.toggleActive : LIGHT_THEME.toggleInactive
                      }`}
                    >
                      TV Shows
                    </button>
                  </div>
                )}

                <input
                  type="text"
                  className={`${INPUT} mb-2`}
                  placeholder={
                    isPersonSubject
                      ? `Filter "${subjectName}" filmography…`
                      : `Refine within "${subjectName}"… (e.g. "of soul")`
                  }
                  value={refine}
                  onChange={(e) => setRefine(e.target.value)}
                />

                {isPersonSubject && personPhotoUrl && (
                  <img
                    src={personPhotoUrl}
                    alt=""
                    className="w-12 h-12 rounded-full object-cover mb-2"
                  />
                )}

                {browseError && (
                  <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-3 py-2 rounded mb-2">
                    {browseError}
                  </div>
                )}

                {!isPersonSubject && kind === "tv" && (
                  <>
                    {tvLoading && <p className="text-[11px] font-mono text-sd-ink/40">Loading…</p>}
                    {!tvLoading && tvResults.length === 0 && (
                      <p className="text-[11px] font-mono text-sd-ink/40 italic">
                        No shows for &quot;{searchQuery}&quot;.
                      </p>
                    )}
                    {!tvLoading && tvResults.length > 0 && (
                      <div className="border border-sd-ink/10 rounded max-h-64 overflow-y-auto">
                        {tvResults.map((show) => {
                          const submittingKey = `tv-${show.tmdbId}`;
                          return (
                            <div
                              key={submittingKey}
                              className="flex items-center gap-3 px-3 py-2 border-b border-sd-ink/5 last:border-0 hover:bg-sd-paper/60"
                            >
                              <div className="flex-1 min-w-0">
                                <span className="text-sm text-sd-ink">{show.title}</span>
                                <span className="text-sd-ink/40 text-xs ml-2">
                                  {[show.year, "TV"].filter(Boolean).join(" · ")}
                                </span>
                              </div>
                              <button
                                type="button"
                                onClick={() => openEpisodes(show.tmdbId, show.title)}
                                disabled={submitting !== null}
                                className={BTN_SECONDARY}
                              >
                                Episodes
                              </button>
                              <button
                                type="button"
                                onClick={() => pickShow(show)}
                                disabled={submitting !== null || position === "" || !participantIdValue}
                                className={BTN_SECONDARY}
                              >
                                {submitting === submittingKey ? "…" : "Pick"}
                              </button>
                            </div>
                          );
                        })}
                      </div>
                    )}
                  </>
                )}

                {browseLoading && (isPersonSubject || kind === "movie") && (
                  <p className="text-[11px] font-mono text-sd-ink/40">Loading…</p>
                )}

                {!browseLoading && isPersonSubject && visibleCredits.length === 0 && !browseError && (
                  <p className="text-[11px] font-mono text-sd-ink/40 italic">
                    {credits.length === 0 ? "No filmography found." : "No credits match your filter."}
                  </p>
                )}

                {!browseLoading && !isPersonSubject && kind === "movie" && titleResults.length === 0 && !browseError && (
                  <p className="text-[11px] font-mono text-sd-ink/40 italic">
                    No results for &quot;{searchQuery}&quot;.
                  </p>
                )}

                {!browseLoading && isPersonSubject && visibleCredits.length > 0 && (
                  <div className="border border-sd-ink/10 rounded max-h-64 overflow-y-auto">
                    {visibleCredits.map((c) => {
                      const submittingKey = `credit-${c.tmdbId}-${c.mediaType}`;
                      return (
                        <div
                          key={submittingKey}
                          className="flex items-center gap-3 px-3 py-2 border-b border-sd-ink/5 last:border-0 hover:bg-sd-paper/60"
                        >
                          <div className="flex-1 min-w-0">
                            <span className="text-sm text-sd-ink">{c.title}</span>
                            <span className="text-sd-ink/40 text-xs ml-2">
                              {[c.year, c.mediaType === 1 ? "TV" : null, c.creditRole]
                                .filter(Boolean)
                                .join(" · ")}
                            </span>
                          </div>
                          {c.mediaType === 1 && (
                            <button
                              type="button"
                              onClick={() => openEpisodes(c.tmdbId, c.title)}
                              disabled={submitting !== null}
                              className={BTN_SECONDARY}
                            >
                              Episodes
                            </button>
                          )}
                          <button
                            type="button"
                            onClick={() =>
                              handlePick(
                                c.mediaPublicId ?? null,
                                { tmdbId: c.tmdbId, imdbId: null, mediaType: c.mediaType },
                                c.title,
                                submittingKey
                              )
                            }
                            disabled={submitting !== null || position === "" || !participantIdValue}
                            className={BTN_SECONDARY}
                          >
                            {submitting === submittingKey ? "…" : "Pick"}
                          </button>
                        </div>
                      );
                    })}
                  </div>
                )}

                {!browseLoading && !isPersonSubject && kind === "movie" && titleResults.length > 0 && (
                  <div className="border border-sd-ink/10 rounded max-h-64 overflow-y-auto">
                    {titleResults.map((item) => {
                      const submittingKey = titleKey(item);
                      return (
                        <div
                          key={submittingKey}
                          className="flex items-center gap-3 px-3 py-2 border-b border-sd-ink/5 last:border-0 hover:bg-sd-paper/60"
                        >
                          <div className="flex-1 min-w-0">
                            <span className="text-sm text-sd-ink">{item.title}</span>
                            <span className="text-sd-ink/40 text-xs ml-2">
                              {[item.year, item.mediaType === 1 ? "TV" : null]
                                .filter(Boolean)
                                .join(" · ")}
                            </span>
                          </div>
                          {item.mediaType === 1 && item.tmdbId != null && (
                            <button
                              type="button"
                              onClick={() => openEpisodes(item.tmdbId as number, item.title)}
                              disabled={submitting !== null}
                              className={BTN_SECONDARY}
                            >
                              Episodes
                            </button>
                          )}
                          <button
                            type="button"
                            onClick={() =>
                              handlePick(
                                item.mediaPublicId ?? null,
                                { tmdbId: item.tmdbId, imdbId: item.imdbId, mediaType: item.mediaType },
                                item.title,
                                submittingKey
                              )
                            }
                            disabled={submitting !== null || position === "" || !participantIdValue}
                            className={BTN_SECONDARY}
                          >
                            {submitting === submittingKey ? "…" : "Pick"}
                          </button>
                        </div>
                      );
                    })}
                    {titleHasMore && (
                      <div className="px-3 py-2">
                        <button
                          type="button"
                          onClick={loadMoreTitles}
                          disabled={loadingMore}
                          className={BTN_SECONDARY}
                        >
                          {loadingMore ? "Loading…" : "Load more"}
                        </button>
                      </div>
                    )}
                  </div>
                )}
              </>
            )}
            {view === "episode" && (
              <div className="space-y-2">
                {drillSeries && (
                  <div className="flex items-center justify-between gap-2">
                    <p className="text-sm text-sd-ink truncate">Episodes of {drillSeries.title}</p>
                    <button
                      type="button"
                      onClick={() => {
                        setView("subject");
                        setDrillSeries(null);
                      }}
                      className="text-[11px] font-mono text-sd-blue uppercase tracking-widest hover:underline"
                    >
                      Back
                    </button>
                  </div>
                )}
                <EpisodeSeasonPicker
                  key={drillSeries?.tmdbId ?? "search"}
                  accessToken={accessToken}
                  fixedSeriesTmdbId={drillSeries?.tmdbId}
                  onSelect={handleEpisodeSelect}
                  disabled={submitting !== null || position === "" || !participantIdValue}
                />
                {submitting?.startsWith("episode-") && (
                  <p className="text-[11px] font-mono text-sd-ink/40">Importing episode…</p>
                )}
              </div>
            )}
          </div>

          {error && (
            <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
              {error}
            </div>
          )}
        </div>
      )}

      {boardFull && (
        <div className="bg-white border border-sd-ink/10 rounded p-6 space-y-4">
          <p className="text-sm text-sd-ink/60">
            Board full for this sub-draft. Completing it rolls any unused veto tokens into
            the next sub-draft.
          </p>
          {error && (
            <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
              {error}
            </div>
          )}
          <button type="button" onClick={handleAdvance} disabled={advancing} className={BTN_PRIMARY}>
            {advancing ? "Completing…" : "Complete Sub-Draft →"}
          </button>
        </div>
      )}
    </div>
  );
}