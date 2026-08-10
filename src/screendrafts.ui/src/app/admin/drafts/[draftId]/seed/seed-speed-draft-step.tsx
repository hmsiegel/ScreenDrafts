// app/admin/drafts/[draftId]/seed/seed-speed-draft-step.tsx
"use client";

import { useEffect, useState } from "react";
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
// per project convention — TV included), or one auto-run title search for a
// Word subject. There's no free-text search here, matching the live panel's
// design: the subject was fixed at setup time, not chosen per-pick.

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
      <ol className="flex gap-2">
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
    <div className="bg-white border border-sd-ink/10 rounded p-8 space-y-6">
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
    <div className="bg-white border border-sd-ink/10 rounded p-8 space-y-6">
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
          const res = await fetch(`${API}/media/search?query=${encodeURIComponent(subjectName)}`, {
            headers: { Authorization: `Bearer ${accessToken}` },
          });
          if (!res.ok) throw new Error(`Failed to search: ${res.status}`);
          const data = await res.json();
          if (cancelled) return;
          const items: TitleSearchItem[] = data.results?.items ?? data.items ?? [];
          setTitleResults(items);
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
  }, [isPersonSubject, subjectImdbId, subjectName, accessToken]);

  const nextPlayOrder = picks.length + 1;
  const lastPick = picks[picks.length - 1] ?? null;
  const landedCount = picks.filter((p) => p.status === "landed").length;
  const boardFull = landedCount >= totalPicks;

  async function handlePick(
    mediaPublicId: string | null,
    item: { tmdbId?: number | null; imdbId?: string | null; mediaType: number },
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
        resolvedPublicId = await importAndResolveTitle(item, accessToken);
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
      <div className="bg-white border border-sd-ink/10 rounded p-4 flex items-center justify-between">
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
                <div className="flex items-center justify-between">
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
            <label className={LABEL}>
              {isPersonSubject ? "Filmography" : `Search results for "${subjectName}"`}
            </label>

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

            {browseLoading && (
              <p className="text-[11px] font-mono text-sd-ink/40">Loading…</p>
            )}

            {!browseLoading && isPersonSubject && credits.length === 0 && !browseError && (
              <p className="text-[11px] font-mono text-sd-ink/40 italic">No filmography found.</p>
            )}

            {!browseLoading && !isPersonSubject && titleResults.length === 0 && !browseError && (
              <p className="text-[11px] font-mono text-sd-ink/40 italic">
                No results for &quot;{subjectName}&quot;.
              </p>
            )}

            {!browseLoading && isPersonSubject && credits.length > 0 && (
              <div className="border border-sd-ink/10 rounded max-h-64 overflow-y-auto">
                {credits.map((c) => {
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

            {!browseLoading && !isPersonSubject && titleResults.length > 0 && (
              <div className="border border-sd-ink/10 rounded max-h-64 overflow-y-auto">
                {titleResults.map((item) => {
                  const submittingKey =
                    item.mediaPublicId || `title-${item.tmdbId ?? item.imdbId}`;
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