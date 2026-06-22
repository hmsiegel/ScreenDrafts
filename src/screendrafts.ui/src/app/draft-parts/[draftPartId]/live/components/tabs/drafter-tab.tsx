// app/draft-parts/[draftPartId]/live/components/tabs/drafter-tab.tsx
'use client';

import { useState, useCallback, useEffect } from 'react';
import { useLiveDraft } from '../../live-draft-context';
import { DraftBoard } from '../draft-board';
import { DraftPickList } from '../draft-pick-list';
import { CountdownOverlay } from '../countdown-overlay';
import { PickSourcePanel } from '../pick-source-panel';
import type { GameplayPickResponse } from '@/lib/dto';

interface Props {
  accessToken: string;
  draftPartId: string;
}

export function DrafterTab({ accessToken, draftPartId }: Props) {
  const {
    gameplay,
    draftPositions,
    picks,
    participants,
    nextExpectedParticipantId,
    callerParticipantId,
    countdownTarget,
    dismissCountdown,
  } = useLiveDraft();

  const [showCountdown, setShowCountdown] = useState(false);
  // Tracks a pick this drafter just submitted, awaiting the host's
  // announcement (PickRevealed). Cleared once it actually lands in `picks`
  // or the drafter moves off their turn (e.g. the host undoes it).
  const [pendingSubmission, setPendingSubmission] = useState<
    { playOrder: number; movieTitle: string } | null
  >(null);

  // Once the pick we're waiting on actually appears in `picks` (revealed),
  // clear the "awaiting announcement" banner.
  useEffect(() => {
    if (!pendingSubmission) return;
    const landed = picks.some((p) => p.playOrder === pendingSubmission.playOrder);
    if (landed) setPendingSubmission(null);
  }, [picks, pendingSubmission]);

  const myPosition = draftPositions.find(
    (pos) => pos.assignedParticipantId?.toString() === callerParticipantId,
  );

  const isMyTurn =
    callerParticipantId !== null &&
    nextExpectedParticipantId === callerParticipantId;

  const landedPositions = new Set(
    picks
      .filter((p) => !p.wasVetoed || p.wasVetoOverridden)
      .map((p) => p.boardPosition),
  );
  const activeSlot =
    myPosition?.ownedBoardSlots
      ?.slice()
      .sort((a, b) => b - a)
      .find((s) => !landedPositions.has(s)) ?? null;

  const myParticipant = participants.find(
    (p) => p.participantId?.toString() === callerParticipantId,
  );
  const mostRecentPick = picks.reduce<GameplayPickResponse | null>(
    (acc, p) => (!acc || (p.playOrder ?? 0) > (acc.playOrder ?? 0) ? p : acc),
    null,
  );
  const canVeto =
    mostRecentPick !== null &&
    !mostRecentPick.wasVetoed &&
    (myParticipant?.vetoTokensRemaining ?? 0) > 0;
  const canOverride =
    mostRecentPick !== null &&
    mostRecentPick.wasVetoed &&
    !mostRecentPick.wasVetoOverridden &&
    mostRecentPick.playedById?.toString() !== callerParticipantId &&
    (myParticipant?.overrideTokensRemaining ?? 0) > 0;

  const isCountdownTarget = countdownTarget === callerParticipantId;
  if (isCountdownTarget && !showCountdown) {
    setShowCountdown(true);
  }

  const handleCountdownComplete = useCallback(() => {
    setShowCountdown(false);
  }, []);

  // Manual dismiss (distinct from natural timeout completion above). Must
  // clear BOTH the local showCountdown flag AND the context's countdownTarget
  // — clearing only showCountdown would leave isCountdownTarget true, and
  // the reactive check above (`if (isCountdownTarget && !showCountdown)`)
  // would immediately flip showCountdown back to true on the next render,
  // undoing the dismiss.
  const handleCountdownDismiss = useCallback(() => {
    setShowCountdown(false);
    dismissCountdown();
  }, [dismissCountdown]);

  async function handleVeto() {
    if (!mostRecentPick || !canVeto || !myParticipant) return;
    const res = await fetch(
      `${process.env.NEXT_PUBLIC_API_URL}/draft-parts/${draftPartId}/picks/${mostRecentPick.playOrder}/veto`,
      {
        method: 'POST',
        headers: { Authorization: `Bearer ${accessToken}`, 'Content-Type': 'application/json' },
        body: JSON.stringify({
          participantPublicId: myParticipant.participantPublicId,
          participantKind: myParticipant.participantKind,
        }),
      },
    );
    if (!res.ok) console.error('Veto failed', res.status);
  }

  async function handleOverride() {
    if (!mostRecentPick || !canOverride || !myParticipant) return;
    const res = await fetch(
      `${process.env.NEXT_PUBLIC_API_URL}/draft-parts/${draftPartId}/veto-override/${mostRecentPick.playOrder}`,
      {
        method: 'POST',
        headers: { Authorization: `Bearer ${accessToken}`, 'Content-Type': 'application/json' },
        body: JSON.stringify({
          participantIdValue: myParticipant.participantPublicId,
          participantKind: myParticipant.participantKind,
        }),
      },
    );
    if (!res.ok) console.error('Override failed', res.status);
  }

  // Inline action buttons rendered directly on the board row for the most
  // recently played pick — only shown if this drafter is eligible to act.
  function renderBoardActions(pick: GameplayPickResponse) {
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

  return (
    <>
      {showCountdown && (
        <CountdownOverlay onComplete={handleCountdownComplete} onDismiss={handleCountdownDismiss} />
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Left — Position + Board + Pick source */}
        <div>
          {myPosition && (
            <div className="mb-4 p-3 border border-white/10 bg-white/5">
              <p className="font-oswald text-sd-red font-bold text-sm tracking-wider">
                YOU ARE POSITION {myPosition.positionName}
              </p>
              <p className="text-xs text-white/40 font-mono mt-0.5">
                Your picks: {myPosition.ownedBoardSlots?.sort((a, b) => b - a).join(', ')}
              </p>
            </div>
          )}

          {pendingSubmission ? (
            <div className="mb-4 px-3 py-2 bg-sd-blue/10 border border-sd-blue/30">
              <p className="font-oswald text-light-blue text-sm tracking-wider">
                PICK SUBMITTED — {pendingSubmission.movieTitle}
              </p>
              <p className="text-[11px] text-white/40 font-mono mt-0.5">
                Awaiting host announcement…
              </p>
            </div>
          ) : (
            isMyTurn && (
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
          <DraftBoard activeSlot={isMyTurn ? activeSlot : null} renderActions={renderBoardActions} />

          {/* Pick source panel — visible only on the drafter's turn, and only
              until they've submitted a pick that's awaiting the host's
              announcement (prevents submitting a second pick before the
              first one is even revealed). */}
          {isMyTurn && !pendingSubmission && activeSlot !== null && callerParticipantId !== null && (
            <PickSourcePanel
              accessToken={accessToken}
              draftPartId={draftPartId}
              draftId={gameplay.draftId!}
              activeSlot={activeSlot}
              callerParticipantId={myParticipant?.participantPublicId ?? ""}
              callerParticipantKind={myParticipant?.participantKind ?? 0}
              onPickSubmitted={(playOrder, movieTitle) => {
                setPendingSubmission({ playOrder, movieTitle });
              }}
            />
          )}

          {/* Zoom placeholder — drafters join the same session as hosts */}
          <div className="mt-6 h-32 border border-dashed border-white/10 flex items-center justify-center">
            <span className="text-white/20 text-xs font-mono">Zoom — coming soon</span>
          </div>
        </div>

        {/* Right — Pick list */}
        <div>
          <h2 className="font-oswald text-sm tracking-widest text-white/50 uppercase mb-3">
            Pick List
          </h2>
          <DraftPickList />
        </div>
      </div>
    </>
  );
}