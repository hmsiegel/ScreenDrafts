// app/guest-drafts/[guestDraftId]/live/components/drafter-tab.tsx
'use client';

import { useState, useEffect } from 'react';
import { useGuestDraftLive } from '../guest-draft-context';
import {
  applyGuestDraftVeto,
  applyGuestDraftVetoOverride,
  fetchMediaByPublicId,
  revealGuestDraftPick,
} from '../gameplay-fetchers';
import type { GuestDraftGameplayPickResponse, MediaResponse } from '@/lib/dto';
import { DraftPickList } from './draft-pick-list';
import { DraftBoard } from './draft-board';
import { PickSourcePanel } from './pick-source-panel';

interface Props {
  accessToken: string;
  guestDraftId: string;
}

// No SpeedDraft branch here — GuestDrafts is single-part only, no sub-drafts.
// No CountdownOverlay either — DraftHub.cs's GuestDrafts surface is just
// JoinGuestDraftAsync/LeaveGuestDraftAsync, no StartCountdownAsync equivalent,
// so there's nothing for GuestDrafts to trigger a countdown from.

export function DrafterTab({ accessToken, guestDraftId }: Props) {
  const {
    picks,
    participants,
    draftPositions,
    callerParticipantId,
    pendingReveal,
    refetch,
  } = useGuestDraftLive();

  // Tracks a pick this participant just submitted, awaiting reveal by
  // whoever's authorized to reveal it — could be the other drafter in a
  // 2-person draft, or a random drafter chosen at submit time in larger ones.
  // Cleared once the pick actually appears in `picks` (revealed) or the
  // submitter navigates off their turn.
  const [pendingSubmission, setPendingSubmission] = useState<
    { playOrder: number; movieTitle: string } | null
  >(null);

  useEffect(() => {
    if (!pendingSubmission) return;
    const landed = picks.some((p) => p.playOrder === pendingSubmission.playOrder);
    if (landed) setPendingSubmission(null);
  }, [picks, pendingSubmission]);

  const myPosition = draftPositions.find(
    (pos) => pos.assignedParticipantId === callerParticipantId,
  );

  // isActiveOnFinalBoard comes straight from the response now (backend
  // applies the Landed formula) instead of being re-derived locally.
  const landedPositions = new Set(
    picks.filter((p) => p.isActiveOnFinalBoard).map((p) => p.position),
  );
  // Next open slot in my own position — highest slot number first, matching
  // the serpentine pick order (e.g. position A picks 7, then 6, then 4...).
  //
  // ASSUMPTION: GetGuestDraftGameplayResponse has no nextExpectedParticipantId
  // (or equivalent) the way DraftParts' gameplay response does, so there's no
  // backend-authoritative "whose turn" signal to check against. Rather than
  // guess at turn-blocking logic client-side (e.g. whether a pending
  // unrevealed pick should block the next drafter from picking), this shows
  // the pick panel to anyone with an open slot in their own position and lets
  // the server reject an out-of-turn attempt — surfaced through the existing
  // error state in PickSourcePanel. Flag if GuestDrafts needs strict
  // client-side turn gating instead; that needs a real "whose turn" field.
  const activeSlot =
    myPosition?.ownedBoardSlots
      ?.slice()
      .sort((a, b) => b - a)
      .find((s) => !landedPositions.has(s)) ?? null;

  const myParticipant = participants.find((p) => p.participantId === callerParticipantId);
  const mostRecentPick = picks.reduce<GuestDraftGameplayPickResponse | null>(
    (acc, p) => (!acc || (p.playOrder ?? 0) > (acc.playOrder ?? 0) ? p : acc),
    null,
  );

  const roundIsOver = mostRecentPick?.wasVetoOverridden === true;

  const canVeto =
    !roundIsOver &&
    mostRecentPick !== null &&
    !mostRecentPick.wasVetoed &&
    (myParticipant?.vetoTokensRemaining ?? 0) > 0;

  const canOverride =
    !roundIsOver &&
    callerParticipantId != null &&
    mostRecentPick !== null &&
    mostRecentPick.wasVetoed === true &&
    !mostRecentPick.wasVetoOverridden &&
    mostRecentPick.playedByParticipantId !== callerParticipantId &&
    (myParticipant?.overrideTokensRemaining ?? 0) > 0;

  async function handleVeto() {
    if (!mostRecentPick || !canVeto) return;
    try {
      await applyGuestDraftVeto(accessToken, guestDraftId, mostRecentPick.playOrder ?? 0);
      await refetch();
    } catch (e) {
      console.error('Veto failed', e);
    }
  }

  async function handleOverride() {
    if (!mostRecentPick || !canOverride) return;
    try {
      await applyGuestDraftVetoOverride(accessToken, guestDraftId, mostRecentPick.playOrder ?? 0);
      await refetch();
    } catch (e) {
      console.error('Override failed', e);
    }
  }

  // Inline action buttons rendered directly on the board row for the most
  // recently played pick — only shown if this drafter is eligible to act.
  function renderBoardActions(pick: GuestDraftGameplayPickResponse) {
    if (pick.playOrder !== mostRecentPick?.playOrder) return null;
    if (!canVeto && !canOverride) return null;

    return (
      <div className="flex gap-2">
        {canVeto && (
          <button
            onClick={handleVeto}
            className="px-3 py-1 border border-sd-red text-sd-red font-oswald text-xs tracking-widest hover:bg-sd-red hover:text-white transition-colors"
          >
            VETO
          </button>
        )}
        {canOverride && (
          <button
            onClick={handleOverride}
            className="px-3 py-1 border border-light-blue text-light-blue font-oswald text-xs tracking-widest hover:bg-light-blue hover:text-sd-ink transition-colors"
          >
            OVERRIDE
          </button>
        )}
      </div>
    );
  }

  // ── Reveal duty ─────────────────────────────────────────────────────────
  // pendingReveal comes from context — set the moment this connection
  // receives PickSubmitted, which only reaches the designated revealer's own
  // group, so its presence alone means "this is your reveal to make." The
  // payload carries moviePublicId but no title, so resolve it separately.
  const [revealMovie, setRevealMovie] = useState<MediaResponse | null>(null);
  const [revealing, setRevealing] = useState(false);
  const [revealError, setRevealError] = useState<string | null>(null);

  useEffect(() => {
    if (!pendingReveal) {
      setRevealMovie(null);
      return;
    }
    let cancelled = false;
    setRevealError(null);
    fetchMediaByPublicId(accessToken, pendingReveal.moviePublicId)
      .then((m) => {
        if (!cancelled) setRevealMovie(m);
      })
      .catch(() => {
        if (!cancelled) setRevealError('Could not load the pick — try refreshing.');
      });
    return () => {
      cancelled = true;
    };
  }, [pendingReveal, accessToken]);

  async function handleReveal() {
    if (!pendingReveal) return;
    setRevealing(true);
    setRevealError(null);
    try {
      await revealGuestDraftPick(accessToken, guestDraftId, pendingReveal.playOrder);
      await refetch();
    } catch (e) {
      setRevealError(e instanceof Error ? e.message : 'Failed to reveal pick.');
    } finally {
      setRevealing(false);
    }
  }

  const revealSubmittedByName = pendingReveal
    ? participants.find((p) => p.participantId === pendingReveal.playedByParticipantId)
        ?.displayName ?? 'another drafter'
    : null;

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
      {/* Left — Position + Board + Pick source */}
      <div>
        {pendingReveal && (
          <div className="mb-4 p-4 border border-yellow-400/40 bg-yellow-400/5">
            <p className="font-oswald text-yellow-400 text-xs tracking-widest uppercase mb-2">
              Reveal duty — pick #{pendingReveal.boardPosition}
            </p>
            {revealMovie ? (
              <p className="font-oswald text-sd-paper text-xl font-bold mb-1">
                {revealMovie.title}
                {revealMovie.year && (
                  <span className="text-white/40 ml-2 text-sm">({revealMovie.year})</span>
                )}
              </p>
            ) : (
              <p className="text-white/40 text-sm font-mono mb-1 animate-pulse">Loading pick…</p>
            )}
            <p className="text-xs text-white/40 font-mono mb-3">
              Submitted by {revealSubmittedByName}
            </p>
            {revealError && (
              <p className="text-sd-red text-xs font-mono mb-2">{revealError}</p>
            )}
            <button
              onClick={handleReveal}
              disabled={revealing || !revealMovie}
              className="px-4 py-2 bg-yellow-400 text-sd-ink font-oswald text-sm tracking-widest hover:bg-yellow-300 disabled:opacity-50 transition-colors"
            >
              {revealing ? 'REVEALING…' : 'REVEAL PICK'}
            </button>
          </div>
        )}

        {myPosition && (
          <div className="mb-4 p-3 border border-white/10 bg-white/5">
            <p className="font-oswald text-sd-red font-bold text-sm tracking-wider">
              YOU ARE POSITION {myPosition.positionName}
            </p>
            <p className="text-xs text-white/40 font-mono mt-0.5">
              Your picks: {myPosition.ownedBoardSlots?.slice().sort((a, b) => b - a).join(', ')}
            </p>
          </div>
        )}

        {pendingSubmission ? (
          <div className="mb-4 px-3 py-2 bg-sd-blue/10 border border-sd-blue/30">
            <p className="font-oswald text-light-blue text-sm tracking-wider">
              PICK SUBMITTED — {pendingSubmission.movieTitle}
            </p>
            <p className="text-[11px] text-white/40 font-mono mt-0.5">
              Awaiting reveal…
            </p>
          </div>
        ) : (
          activeSlot !== null && (
            <div className="mb-4 px-3 py-2 bg-sd-red/10 border border-sd-red/30">
              <p className="font-oswald text-sd-red text-sm tracking-wider">
                YOUR TURN — Pick #{activeSlot}
              </p>
            </div>
          )
        )}

        <h2 className="font-oswald text-sm tracking-widest text-white/50 uppercase mb-3">
          Draft Board
        </h2>
        <DraftBoard activeSlot={activeSlot} renderActions={renderBoardActions} />

        {!pendingSubmission && activeSlot !== null && callerParticipantId !== null && (
          <PickSourcePanel
            accessToken={accessToken}
            guestDraftId={guestDraftId}
            activeSlot={activeSlot}
            onPickSubmitted={(playOrder, movieTitle) => {
              setPendingSubmission({ playOrder, movieTitle });
            }}
          />
        )}
      </div>

      {/* Right — Pick list */}
      <div>
        <h2 className="font-oswald text-sm tracking-widest text-white/50 uppercase mb-3">
          Pick List
        </h2>
        <DraftPickList />
      </div>
    </div>
  );
}