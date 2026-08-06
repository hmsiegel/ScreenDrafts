// app/draft-parts/[draftPartId]/live/components/speed-draft-tabs.tsx
'use client';

import { useState, useEffect, useCallback } from 'react';
import { useLiveDraft } from '../live-draft-context';
import { SpeedDraftPickPanel } from './speed-draft-pick-panel';

// ── Types ─────────────────────────────────────────────────────────────────────
// Replace with dto.ts imports after NSwag regen.

interface SubDraftSummary {
  publicId: string;
  index: number;
  status: number; // 0 Pending, 1 Active, 2 Completed
  subjectKind: number | null;
  subjectName: string | null;
  subjectImdbId: string | null;
}

interface DraftPositionDetail {
  positionPublicId: string;
  positionName: string;
  ownedBoardSlots: number[];
  assignedParticipantId: string | null;
  assignedParticipantKind: number | null;
  assignedParticipantName: string | null;
}

interface PickDetail {
  playOrder: number;
  boardPosition: number;
  movieTitle: string;
  movieYear?: string | null;
  tmdbId?: number | null;
  imdbId?: string | null;
  igdbId?: number | null;
  mediaType?: number | null; // 0 Movie, 1 TvShow, 2 TvEpisode, 3 VideoGame, 4 MusicVideo, 5 Short
  playedById: string;
  playedByKind: number;
  playedByName: string;
  wasVetoed: boolean;
  wasVetoOverridden: boolean;
}

// Prefers a TMDb link when tmdbId + mediaType are both known (structured,
// reliable). Falls back to IMDb when only imdbId is present (OMDb-sourced
// picks, or IMDb-catalogued music videos). No link at all for IGDB games
// (only a numeric ID is stored — IGDB's public URLs need a slug we don't
// have) or YouTube-sourced Shorts/music videos (Movie, the Drafts-side
// denormalized copy, has no ExternalId column at all — a real gap, not
// just an unhandled case here).
function buildMediaUrl(pick: {
  tmdbId?: number | null;
  imdbId?: string | null;
  mediaType?: number | null;
}): string | null {
  if (pick.tmdbId != null) {
    if (pick.mediaType === 1 || pick.mediaType === 2) {
      return `https://www.themoviedb.org/tv/${pick.tmdbId}`;
    }
    // mediaType null/undefined (older rows predating this field) defaults
    // to the movie path, same as the response's own null-handling note.
    if (pick.mediaType === 0 || pick.mediaType == null) {
      return `https://www.themoviedb.org/movie/${pick.tmdbId}`;
    }
  }
  if (pick.imdbId) {
    return `https://www.imdb.com/title/${pick.imdbId}/`;
  }
  return null;
}

interface TriviaResultDetail {
  participantId: string;
  participantKind: number;
  participantName: string;
  questionsWon: number;
  position: number;
}

interface SubDraftDetail {
  subDraftPublicId: string;
  index: number;
  status: number;
  draftPositions: DraftPositionDetail[];
  picks: PickDetail[];
  triviaResults: TriviaResultDetail[];
}

const SUBJECT_KIND_LABELS: Record<number, string> = {
  0: 'Actor',
  1: 'Director',
  2: 'Word',
};

const API = process.env.NEXT_PUBLIC_API_URL;

async function fetchSubDraftDetail(
  accessToken: string,
  draftPartId: string,
  subDraftPublicId: string,
): Promise<SubDraftDetail | null> {
  const res = await fetch(
    `${API}/draft-parts/${draftPartId}/sub-drafts/${subDraftPublicId}/gameplay`,
    { headers: { Authorization: `Bearer ${accessToken}` } },
  );
  if (!res.ok) return null;
  return res.json();
}

// ── Props ─────────────────────────────────────────────────────────────────────

interface Props {
  accessToken: string;
  draftPartId: string;
  isHost: boolean;
  callerParticipantId: string | null;
}

// ── Component ─────────────────────────────────────────────────────────────────

export function SpeedDraftTabs({ accessToken, draftPartId, isHost, callerParticipantId }: Props) {
  const { gameplay } = useLiveDraft();
  const subDrafts = (gameplay.subDrafts ?? []) as SubDraftSummary[];

  const firstUnlockedIndex = subDrafts.findIndex((sd) => sd.status !== 2 /* not Completed */);
  const [activeIndex, setActiveIndex] = useState(
    firstUnlockedIndex === -1 ? subDrafts[0]?.index ?? 1 : subDrafts[firstUnlockedIndex]?.index ?? 1,
  );

  function isUnlocked(sd: SubDraftSummary): boolean {
    if (sd.index === 1) return true;
    const prev = subDrafts.find((s) => s.index === sd.index - 1);
    return prev?.status === 2; // Completed
  }

  // activeIndex's useState initializer above only runs once, on mount — it
  // does not re-run when gameplay.subDrafts updates later (e.g. after
  // AdvanceSubDraftButton's refetch()). Without this, completing a
  // sub-draft correctly unlocks the next tab in the data, but the user
  // stays parked on the tab they were already viewing (now Completed) and
  // never sees it, until a full page reload re-runs the initializer fresh.
  // This only fires when the ACTIVE tab itself just completed, so it won't
  // yank a host forward if they've manually clicked back to review an
  // earlier, already-completed sub-draft.
  useEffect(() => {
    const active = subDrafts.find((sd) => sd.index === activeIndex);
    if (active?.status === 2 /* Completed */) {
      const next = subDrafts.find((sd) => sd.index === activeIndex + 1);
      if (next) {
        setActiveIndex(next.index);
      }
    }
  }, [subDrafts, activeIndex]);

  if (subDrafts.length === 0) {
    return (
      <p className="text-white/40 text-sm font-mono italic">
        No sub-drafts set up for this part yet.
      </p>
    );
  }

  return (
    <div>
      <div className="flex border-b border-white/10 mb-6">
        {subDrafts.map((sd) => {
          const unlocked = isUnlocked(sd);
          return (
            <button
              key={sd.publicId}
              onClick={() => unlocked && setActiveIndex(sd.index)}
              disabled={!unlocked}
              className={`px-5 py-3 font-oswald text-sm tracking-widest transition-colors ${
                activeIndex === sd.index
                  ? 'text-sd-paper border-b-2 border-sd-red'
                  : unlocked
                    ? 'text-white/40 hover:text-white/70'
                    : 'text-white/15 cursor-not-allowed'
              }`}
            >
              SUBJECT {sd.index}
              {sd.status === 2 && <span className="ml-1.5 text-light-blue">✓</span>}
              {!unlocked && <span className="ml-1.5">🔒</span>}
            </button>
          );
        })}
      </div>

      {subDrafts
        .filter((sd) => sd.index === activeIndex)
        .map((sd) => (
          <SubDraftPanel
            key={sd.publicId}
            accessToken={accessToken}
            draftPartId={draftPartId}
            summary={sd}
            isHost={isHost}
            callerParticipantId={callerParticipantId}
          />
        ))}
    </div>
  );
}

// ── Per-sub-draft panel ───────────────────────────────────────────────────────

function SubDraftPanel({
  accessToken,
  draftPartId,
  summary,
  isHost,
  callerParticipantId,
}: {
  accessToken: string;
  draftPartId: string;
  summary: SubDraftSummary;
  isHost: boolean;
  callerParticipantId: string | null;
}) {
  const { joinSubDraftGroup, leaveSubDraftGroup, lastSubDraftEvent, gameplay, refetch } = useLiveDraft();

  const [detail, setDetail] = useState<SubDraftDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [vetoSubmitting, setVetoSubmitting] = useState(false);
  const [vetoError, setVetoError] = useState<string | null>(null);
  const [undoActing, setUndoActing] = useState<'undo-pick' | 'undo-veto' | null>(null);
  const [undoError, setUndoError] = useState<string | null>(null);

  const load = useCallback(async () => {
    const d = await fetchSubDraftDetail(accessToken, draftPartId, summary.publicId);
    setDetail(d);
    setLoading(false);
  }, [accessToken, draftPartId, summary.publicId]);

  useEffect(() => {
    setLoading(true);
    void load();
  }, [load]);

  // Join this sub-draft's SignalR group while its tab is active; leave on
  // switch-away or unmount. PickAdded/PickRevealed/VetoApplied for this
  // sub-draft arrive via lastSubDraftEvent below, not a direct listener here
  // — live-draft-context.tsx owns the connection and does the routing.
  useEffect(() => {
    void joinSubDraftGroup(summary.publicId);
    return () => {
      void leaveSubDraftGroup(summary.publicId);
    };
  }, [summary.publicId, joinSubDraftGroup, leaveSubDraftGroup]);

  useEffect(() => {
    if (lastSubDraftEvent?.subDraftPublicId === summary.publicId) {
      void load();
    }
  }, [lastSubDraftEvent, summary.publicId, load]);

  // Long-interval fallback, doing two jobs now: re-fetching in case a
  // message was missed, and re-invoking JoinSubDraftGroupAsync as a safety
  // net against reconnects — SignalR group membership doesn't survive a
  // dropped-and-restored connection, and nothing else in this component
  // (or live-draft-context) currently rejoins sub-draft groups after one.
  // AddToGroupAsync is idempotent, so re-joining when already a member is
  // harmless. SignalR is still the primary path via the effects above;
  // this only covers what they miss.
  useEffect(() => {
    if (summary.status !== 1) return;
    const interval = setInterval(() => {
      void load();
      void joinSubDraftGroup(summary.publicId);
    }, 20000);
    return () => clearInterval(interval);
  }, [summary.status, load, joinSubDraftGroup, summary.publicId]);

  if (loading || !detail) {
    return <p className="text-white/30 text-sm font-mono italic">Loading…</p>;
  }

  if (summary.status === 0 /* Pending */) {
    return isHost ? (
      <TriviaWinnerPicker
        accessToken={accessToken}
        draftPartId={draftPartId}
        summary={summary}
        onResolved={load}
      />
    ) : (
      <p className="text-white/40 text-sm font-mono italic">
        Waiting for the host to reveal this subject…
      </p>
    );
  }

  // Active, positions exist (created at setup via SetSpeedDraftPositions),
  // but not yet assigned for this round — trivia resolved, winner hasn't
  // chosen A or B yet.
  if (
    summary.status === 1 &&
    detail.draftPositions.length > 0 &&
    detail.draftPositions.every((p) => p.assignedParticipantId === null)
  ) {
    const winnerResult = detail.triviaResults.find((t) => t.position === 1);
    // TriviaResultDetail.participantId is the internal GUID (same shape as
    // GameplayTriviaResultResponse.ParticipantId on the main gameplay
    // response) — never a public ID. The public ID has to come from
    // gameplay.participants, cross-referenced by that GUID, the same place
    // TriviaWinnerPicker already sources it from for the trivia-results call.
    const winnerParticipant = winnerResult
      ? gameplay.participants?.find((p) => p.participantId === winnerResult.participantId)
      : undefined;
    return isHost ? (
      <PositionChoicePicker
        accessToken={accessToken}
        draftPartId={draftPartId}
        summary={summary}
        winner={
          winnerResult && winnerParticipant
            ? {
                participantPublicId: winnerParticipant.participantPublicId ?? '',
                participantKind: winnerParticipant.participantKind ?? 0,
                participantName: winnerResult.participantName,
              }
            : null
        }
        onResolved={load}
      />
    ) : (
      <p className="text-white/40 text-sm font-mono italic">
        {winnerResult
          ? `Waiting for ${winnerResult.participantName} to choose a position…`
          : 'Waiting for the host…'}
      </p>
    );
  }

  const myPosition = detail.draftPositions.find(
    (pos) => pos.assignedParticipantId === callerParticipantId,
  );
  // myPosition.assignedParticipantId is the internal GUID (GetSubDraftGameplay's
  // AssignedParticipantId = pos.AssignedToId) — never a public ID. Same
  // mismatch as the trivia-winner/position-choice fix earlier; this is the
  // occurrence in the actual pick-submission path, which I missed going
  // back to check at the time.
  const myParticipant = gameplay.participants?.find(
    (p) => p.participantId === myPosition?.assignedParticipantId,
  );
  const landedSlots = new Set(
    detail.picks
      .filter((p) => !p.wasVetoed || p.wasVetoOverridden)
      .map((p) => p.boardPosition),
  );
  // The actual next pick is the highest unfilled slot across BOTH
  // positions combined, not just whichever slots happen to belong to the
  // viewing participant — A and B's slots interleave (7,6,5,4,3,2,1
  // alternating), so checking only "do I have any slot left" made both
  // participants show as having a turn simultaneously almost the whole
  // draft. This checks whose slot the true next one actually is.
  const nextGlobalSlot =
    detail.draftPositions
      .flatMap((pos) => pos.ownedBoardSlots)
      .filter((s) => !landedSlots.has(s))
      .sort((a, b) => b - a)[0] ?? null;

  const activeSlot =
    nextGlobalSlot !== null && myPosition?.ownedBoardSlots.includes(nextGlobalSlot)
      ? nextGlobalSlot
      : null;

  const isMyTurn = activeSlot !== null;
  const subjectLabel = summary.subjectKind != null ? SUBJECT_KIND_LABELS[summary.subjectKind] : null;

  const mostRecentPick = detail.picks.reduce<PickDetail | null>(
    (acc, p) => (!acc || p.playOrder > acc.playOrder ? p : acc),
    null,
  );

  // No veto overrides for sub-drafts (blocked at the domain level), so
  // there's no "round is over" state the way the regular DrafterTab has —
  // a vetoed pick is just gone, full stop.
  const canVeto =
    mostRecentPick !== null &&
    !mostRecentPick.wasVetoed &&
    (myParticipant?.vetoTokensRemaining ?? 0) > 0;

  async function handleVeto() {
    if (!mostRecentPick || !canVeto || !myParticipant || vetoSubmitting) return;
    setVetoSubmitting(true);
    setVetoError(null);
    try {
      const res = await fetch(
        `${API}/draft-parts/${draftPartId}/sub-drafts/${summary.publicId}/picks/${mostRecentPick.playOrder}/veto`,
        {
          method: 'POST',
          headers: {
            Authorization: `Bearer ${accessToken}`,
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            draftPartPublicId: draftPartId,
            subDraftPublicId: summary.publicId,
            playOrder: mostRecentPick.playOrder,
            issuerPublicId: myParticipant.participantPublicId,
            issuerKind: myParticipant.participantKind,
          }),
        },
      );
      if (!res.ok) {
        const body = await res.json().catch(() => null);
        throw new Error(body?.detail ?? `Veto failed: ${res.status}`);
      }
      void load();
      await refetch();
    } catch (e) {
      setVetoError(e instanceof Error ? e.message : 'Failed to veto.');
    } finally {
      setVetoSubmitting(false);
    }
  }

  // Host-only "break glass" actions. Unlike handleVeto above, these hit the
  // flat /draft-parts/{draftPartId}/picks/{playOrder}/... routes (no
  // /sub-drafts/{subDraftId}/ segment) — that's how UndoPickRequest and
  // UndoVetoRequest are actually shaped, disambiguating by an optional
  // subDraftPublicId in the body instead of the URL. Mirrors
  // primary-host-tab.tsx's handleUndoPick/handleUndoVeto.
  async function handleUndoPick(playOrder: number) {
    setUndoActing('undo-pick');
    setUndoError(null);
    try {
      const res = await fetch(`${API}/draft-parts/${draftPartId}/picks/${playOrder}`, {
        method: 'DELETE',
        headers: {
          Authorization: `Bearer ${accessToken}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          draftPartId,
          playOrder,
          subDraftPublicId: summary.publicId,
        }),
      });
      if (!res.ok) throw new Error(`Undo pick failed: ${res.status}`);
      void load();
      await refetch();
    } catch (e) {
      setUndoError(e instanceof Error ? e.message : 'Failed to undo pick.');
    } finally {
      setUndoActing(null);
    }
  }

  async function handleUndoVeto(playOrder: number) {
    setUndoActing('undo-veto');
    setUndoError(null);
    try {
      const res = await fetch(`${API}/draft-parts/${draftPartId}/picks/${playOrder}/undo-veto`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${accessToken}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          draftPartId,
          playOrder,
          subDraftPublicId: summary.publicId,
        }),
      });
      if (!res.ok) throw new Error(`Undo veto failed: ${res.status}`);
      void load();
      await refetch();
    } catch (e) {
      setUndoError(e instanceof Error ? e.message : 'Failed to undo veto.');
    } finally {
      setUndoActing(null);
    }
  }

  return (
    <div>
      <div className="mb-6 p-4 border border-white/10 bg-white/5">
        <p className="font-mono text-[11px] tracking-widest text-white/40 uppercase mb-1">
          {subjectLabel ?? 'Subject'}
        </p>
        <p className="font-oswald text-2xl font-bold text-sd-paper">{summary.subjectName}</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <div>
          <h3 className="font-oswald text-sm tracking-widest text-white/50 uppercase mb-3">
            Board
          </h3>
          <div className="space-y-2 mb-4">
            {detail.draftPositions.map((pos) => (
              <div key={pos.positionPublicId} className="border border-white/10 p-3">
                <p className="font-oswald text-sm text-sd-paper">
                  {pos.positionName} — {pos.assignedParticipantName ?? '—'}
                </p>
                <p className="text-[11px] text-white/30 font-mono">
                  Picks: {pos.ownedBoardSlots.slice().sort((a, b) => b - a).join(', ')}
                </p>
              </div>
            ))}
          </div>

          {!isHost && isMyTurn && activeSlot !== null && callerParticipantId && myPosition && (
            <>
              <div className="mb-3 px-3 py-2 bg-sd-red/10 border border-sd-red/30">
                <p className="font-oswald text-sd-red text-sm tracking-wider">
                  YOUR TURN — Pick #{activeSlot}
                </p>
              </div>
              <SpeedDraftPickPanel
                accessToken={accessToken}
                draftPartId={draftPartId}
                subDraftId={summary.publicId}
                activeSlot={activeSlot}
                callerParticipantId={myParticipant?.participantPublicId ?? ''}
                callerParticipantKind={myParticipant?.participantKind ?? myPosition.assignedParticipantKind ?? 0}
                subjectKind={summary.subjectKind ?? 2}
                subjectName={summary.subjectName ?? ''}
                subjectImdbId={summary.subjectImdbId}
                existingPicks={detail.picks}
                onPickSubmitted={() => void load()}
              />
            </>
          )}
        </div>

        <div>
          <h3 className="font-oswald text-sm tracking-widest text-white/50 uppercase mb-3">
            Picks
          </h3>
          <div className="space-y-2">
            {detail.picks.length === 0 && (
              <p className="text-white/30 text-sm font-mono italic">No picks yet.</p>
            )}
            {detail.picks
              .slice()
              .sort((a, b) => a.playOrder - b.playOrder)
              .map((p) => (
                <div
                  key={p.playOrder}
                  className={`border p-3 ${
                    p.wasVetoed && !p.wasVetoOverridden
                      ? 'border-sd-red/30 opacity-50'
                      : 'border-white/10'
                  }`}
                >
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p
                        className={`font-oswald text-sm text-sd-paper ${
                          p.wasVetoed && !p.wasVetoOverridden ? 'line-through decoration-sd-red' : ''
                        }`}
                      >
                        #{p.boardPosition} —{' '}
                        {(() => {
                          const url = buildMediaUrl(p);
                          return url ? (
                            <a
                              href={url}
                              target="_blank"
                              rel="noopener noreferrer"
                              className="hover:text-sd-blue underline decoration-white/20 hover:decoration-sd-blue transition-colors"
                            >
                              {p.movieTitle}
                            </a>
                          ) : (
                            p.movieTitle
                          );
                        })()}
                        {p.movieYear && <span className="text-white/30"> ({p.movieYear})</span>}
                      </p>
                      <p className="text-[11px] text-white/40 font-mono">
                        {p.playedByName}
                        {p.wasVetoed && !p.wasVetoOverridden && ' — VETOED'}
                        {p.wasVetoOverridden && ' — SAVED'}
                      </p>
                    </div>
                    {!isHost && p.playOrder === mostRecentPick?.playOrder && canVeto && (
                      <button
                        onClick={handleVeto}
                        disabled={vetoSubmitting}
                        className="shrink-0 px-3 py-1 border border-sd-red text-sd-red font-oswald text-xs tracking-widest hover:bg-sd-red hover:text-white disabled:opacity-40 transition-colors"
                      >
                        {vetoSubmitting ? '…' : 'VETO'}
                      </button>
                    )}
                    {isHost && p.playOrder === mostRecentPick?.playOrder && (
                      <div className="flex gap-2 shrink-0">
                        {p.wasVetoed && !p.wasVetoOverridden && (
                          <button
                            onClick={() => handleUndoVeto(p.playOrder)}
                            disabled={undoActing !== null}
                            className="px-3 py-1 border border-light-blue text-light-blue font-oswald text-xs tracking-widest hover:bg-light-blue hover:text-sd-ink disabled:opacity-40 transition-colors"
                          >
                            {undoActing === 'undo-veto' ? '…' : 'UNDO VETO'}
                          </button>
                        )}
                        <button
                          onClick={() => handleUndoPick(p.playOrder)}
                          disabled={undoActing !== null}
                          className="px-3 py-1 border border-sd-red text-sd-red font-oswald text-xs tracking-widest hover:bg-sd-red hover:text-white disabled:opacity-40 transition-colors"
                        >
                          {undoActing === 'undo-pick' ? '…' : 'UNDO'}
                        </button>
                      </div>
                    )}
                  </div>
                </div>
              ))}
            {vetoError && <p className="text-sd-red text-xs font-mono">{vetoError}</p>}
            {undoError && <p className="text-sd-red text-xs font-mono">{undoError}</p>}
          </div>

          {summary.status === 1 && isHost && (
            <AdvanceSubDraftButton
              accessToken={accessToken}
              draftPartId={draftPartId}
              summary={summary}
              detail={detail}
              isLast={
                summary.index ===
                Math.max(
                  ...((gameplay.subDrafts ?? []) as SubDraftSummary[]).map((sd) => sd.index),
                )
              }
              onAdvanced={load}
            />
          )}
        </div>
      </div>
    </div>
  );
}

// ── Trivia winner picker (host only, Pending sub-drafts) ────────────────────

function TriviaWinnerPicker({
  accessToken,
  draftPartId,
  summary,
  onResolved,
}: {
  accessToken: string;
  draftPartId: string;
  summary: SubDraftSummary;
  onResolved: () => void;
}) {
  const { gameplay, refetch } = useLiveDraft();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Speed Drafts are always exactly 2 participants — reuse the main
  // gameplay payload's list rather than fetching sub-draft-scoped
  // participants (there's no such thing; participants are DraftPart-level).
  const participants = gameplay.participants ?? [];

  async function handleWinner(winnerId: string) {
    if (submitting || participants.length !== 2) return;
    setSubmitting(true);
    setError(null);
    try {
      const results = participants.map((p, i) => {
        const isWinner = p.participantId === winnerId;
        return {
          participantPublicId: p.participantPublicId ?? '',
          kind: p.participantKind ?? 0,
          position: isWinner ? 1 : 2,
          questionsWon: isWinner ? 1 : 0,
        };
      });

      const res = await fetch(
        `${API}/draft-parts/${draftPartId}/sub-drafts/${summary.publicId}/trivia-results`,
        {
          method: 'POST',
          headers: {
            Authorization: `Bearer ${accessToken}`,
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            draftPartPublicId: draftPartId,
            subDraftPublicId: summary.publicId,
            results,
          }),
        },
      );
      if (!res.ok) {
        const body = await res.json().catch(() => null);
        throw new Error(body?.detail ?? `Failed to record trivia winner: ${res.status}`);
      }
      await refetch(); // updates gameplay.subDrafts summary (reveals subject)
      onResolved();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to record trivia winner.');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="max-w-md">
      <p className="font-oswald text-sd-paper text-sm tracking-wider mb-1">
        Subject {summary.index} — Trivia
      </p>
      <p className="text-white/40 text-xs font-mono mb-4">Who won the trivia question?</p>

      {participants.length !== 2 ? (
        <p className="text-sd-red text-xs font-mono">
          Expected exactly 2 participants, found {participants.length}.
        </p>
      ) : (
        <div className="grid grid-cols-2 gap-3">
          {participants.map((p) => (
            <button
              key={p.participantId}
              onClick={() => p.participantId && handleWinner(p.participantId)}
              disabled={submitting}
              className="px-4 py-3 border border-sd-red/50 text-sd-red font-oswald text-sm tracking-widest hover:bg-sd-red hover:text-white disabled:opacity-40 transition-colors"
            >
              {submitting ? '…' : p.participantName}
            </button>
          ))}
        </div>
      )}

      {error && <p className="text-sd-red text-xs font-mono mt-3">{error}</p>}
    </div>
  );
}

// ── Position choice (host relays the trivia winner's pick) ──────────────────
// A: 4 picks (7,5,3,1). B: 3 picks (6,4,2) but goes earlier on even slots —
// a real strategic tradeoff, which is why it's a choice and not automatic.

interface ResolvedWinner {
  participantPublicId: string;
  participantKind: number;
  participantName: string;
}

function PositionChoicePicker({
  accessToken,
  draftPartId,
  summary,
  winner,
  onResolved,
}: {
  accessToken: string;
  draftPartId: string;
  summary: SubDraftSummary;
  winner: ResolvedWinner | null;
  onResolved: () => void;
}) {
  const { refetch } = useLiveDraft();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleChoice(choice: 'A' | 'B') {
    if (!winner || submitting) return;
    setSubmitting(true);
    setError(null);
    try {
      const res = await fetch(
        `${API}/draft-parts/${draftPartId}/sub-drafts/${summary.publicId}/position`,
        {
          method: 'POST',
          headers: {
            Authorization: `Bearer ${accessToken}`,
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            draftPartPublicId: draftPartId,
            subDraftPublicId: summary.publicId,
            winnerParticipantPublicId: winner.participantPublicId,
            winnerParticipantKind: winner.participantKind,
            choice,
          }),
        },
      );
      if (!res.ok) {
        const body = await res.json().catch(() => null);
        throw new Error(body?.detail ?? `Failed to set position: ${res.status}`);
      }
      await refetch();
      onResolved();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to set position.');
    } finally {
      setSubmitting(false);
    }
  }

  if (!winner) {
    return (
      <p className="text-sd-red text-xs font-mono">
        No trivia winner found for this sub-draft — can&rsquo;t choose a position yet.
      </p>
    );
  }

  return (
    <div className="max-w-md">
      <p className="font-oswald text-sd-paper text-sm tracking-wider mb-1">
        {winner.participantName} won trivia
      </p>
      <p className="text-white/40 text-xs font-mono mb-4">Which position do they want?</p>

      <div className="grid grid-cols-2 gap-3">
        <button
          onClick={() => handleChoice('A')}
          disabled={submitting}
          className="px-4 py-3 border border-sd-red/50 text-sd-red font-oswald text-sm tracking-widest hover:bg-sd-red hover:text-white disabled:opacity-40 transition-colors text-left"
        >
          <span className="block">A</span>
          <span className="block text-[10px] text-white/40 font-mono mt-1">Picks 7, 5, 3, 1</span>
        </button>
        <button
          onClick={() => handleChoice('B')}
          disabled={submitting}
          className="px-4 py-3 border border-light-blue/50 text-light-blue font-oswald text-sm tracking-widest hover:bg-light-blue hover:text-sd-ink disabled:opacity-40 transition-colors text-left"
        >
          <span className="block">B</span>
          <span className="block text-[10px] text-white/40 font-mono mt-1">Picks 6, 4, 2</span>
        </button>
      </div>

      {error && <p className="text-sd-red text-xs font-mono mt-3">{error}</p>}
    </div>
  );
}

// ── Advance to next sub-draft (host only, once all 7 slots landed) ──────────

function AdvanceSubDraftButton({
  accessToken,
  draftPartId,
  summary,
  detail,
  isLast,
  onAdvanced,
}: {
  accessToken: string;
  draftPartId: string;
  summary: SubDraftSummary;
  detail: SubDraftDetail;
  isLast: boolean;
  onAdvanced: () => void;
}) {
  const { refetch } = useLiveDraft();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const totalSlots = detail.draftPositions.flatMap((p) => p.ownedBoardSlots).length;
  const landedCount = detail.picks.filter((p) => !p.wasVetoed || p.wasVetoOverridden).length;
  const ready = totalSlots > 0 && landedCount === totalSlots;

  async function handleAdvance() {
    if (!ready || submitting) return;
    setSubmitting(true);
    setError(null);
    try {
      const res = await fetch(
        `${API}/draft-parts/${draftPartId}/sub-drafts/${summary.publicId}/advance`,
        {
          method: 'POST',
          headers: {
            Authorization: `Bearer ${accessToken}`,
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            draftPartPublicId: draftPartId,
            subDraftPublicId: summary.publicId,
          }),
        },
      );
      if (!res.ok) {
        const body = await res.json().catch(() => null);
        throw new Error(body?.detail ?? `Failed to advance: ${res.status}`);
      }
      await refetch(); // updates gameplay.subDrafts summary — unlocks next tab
      onAdvanced();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to advance.');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="mt-4">
      <button
        onClick={handleAdvance}
        disabled={!ready || submitting}
        className="w-full px-4 py-2.5 bg-sd-blue text-white font-oswald text-sm tracking-widest hover:bg-sd-blue/80 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
      >
        {submitting
          ? isLast
            ? 'COMPLETING…'
            : 'ADVANCING…'
          : ready
            ? isLast
              ? 'COMPLETE DRAFT'
              : 'COMPLETE & ADVANCE'
            : `${landedCount}/${totalSlots} PICKS LANDED`}
      </button>
      {error && <p className="text-sd-red text-xs font-mono mt-2">{error}</p>}
    </div>
  );
}