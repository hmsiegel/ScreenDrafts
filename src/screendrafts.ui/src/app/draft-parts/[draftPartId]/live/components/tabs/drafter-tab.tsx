// app/draft-parts/[draftPartId]/live/components/tabs/drafter-tab.tsx
'use client';

import { useState, useCallback } from 'react';
import { useLiveDraft } from '../../live-draft-context';
import { DraftBoard } from '../draft-board';
import { DraftPickList } from '../draft-pick-list';
import { CountdownOverlay } from '../countdown-overlay';

interface Props {
  accessToken: string;
  draftPartId: string;
  userPublicId: string | null;
}

export function DrafterTab({ accessToken, draftPartId, userPublicId }: Props) {
  const {
    gameplay,
    draftPositions,
    picks,
    participants,
    nextExpectedParticipantId,
    countdownTarget,
  } = useLiveDraft();

  const [showCountdown, setShowCountdown] = useState(false);

  // Resolve this user's position
  const myPosition = draftPositions.find(
    (pos) => pos.assignedParticipantId === userPublicId,
  );

  const isMyTurn = nextExpectedParticipantId === userPublicId;

  // Derive active board slot (next unfilled slot in my position's sequence)
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

  // Veto/override eligibility
  const myParticipant = participants.find((p) => p.participantId === userPublicId);
  const mostRecentPick = picks.reduce<(typeof picks)[0] | null>(
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
    mostRecentPick.playedById !== userPublicId &&
    (myParticipant?.overrideTokensRemaining ?? 0) > 0;

  // Show countdown overlay if targeted
  const isCountdownTarget = countdownTarget === userPublicId;

  // Re-show countdown overlay when targeted
  if (isCountdownTarget && !showCountdown) {
    setShowCountdown(true);
  }

  const handleCountdownComplete = useCallback(() => {
    setShowCountdown(false);
  }, []);

  async function handleVeto() {
    if (!mostRecentPick || !canVeto) return;
    const res = await fetch(
      `${process.env.NEXT_PUBLIC_API_URL}/draft-parts/${draftPartId}/picks/${mostRecentPick.playOrder}/veto`,
      {
        method: 'POST',
        headers: { Authorization: `Bearer ${accessToken}`, 'Content-Type': 'application/json' },
      },
    );
    if (!res.ok) console.error('Veto failed', res.status);
  }

  async function handleOverride() {
    if (!mostRecentPick || !canOverride) return;
    const res = await fetch(
      `${process.env.NEXT_PUBLIC_API_URL}/draft-parts/${draftPartId}/picks/${mostRecentPick.playOrder}/veto-override`,
      {
        method: 'POST',
        headers: { Authorization: `Bearer ${accessToken}`, 'Content-Type': 'application/json' },
      },
    );
    if (!res.ok) console.error('Override failed', res.status);
  }

  return (
    <>
      {showCountdown && <CountdownOverlay onComplete={handleCountdownComplete} />}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Left — Position + Board */}
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

          {isMyTurn && (
            <div className="mb-4 px-3 py-2 bg-sd-red/10 border border-sd-red/30">
              <p className="font-oswald text-sd-red text-sm tracking-wider">YOUR TURN — Pick #{activeSlot}</p>
            </div>
          )}

          <h2 className="font-oswald text-sm tracking-widest text-white/50 uppercase mb-3">
            Draft Board
          </h2>
          <DraftBoard activeSlot={isMyTurn ? activeSlot : null} />

          {/* Veto / Override controls */}
          {(canVeto || canOverride) && (
            <div className="mt-4 flex gap-3">
              {canVeto && (
                <button
                  onClick={handleVeto}
                  className="px-4 py-2 border border-sd-red text-sd-red font-oswald text-sm tracking-widest hover:bg-sd-red hover:text-white transition-colors"
                >
                  VETO
                </button>
              )}
              {canOverride && (
                <button
                  onClick={handleOverride}
                  className="px-4 py-2 border border-light-blue text-light-blue font-oswald text-sm tracking-widest hover:bg-light-blue hover:text-sd-ink transition-colors"
                >
                  SAVE (OVERRIDE)
                </button>
              )}
            </div>
          )}

          {/* Pick source panel placeholder */}
          {isMyTurn && (
            <div className="mt-6">
              <h3 className="font-oswald text-xs tracking-widest text-white/50 uppercase mb-2">
                {gameplay.hasDraftPool
                  ? 'Draft Pool'
                  : gameplay.hasDraftBoard
                    ? 'Draft Board'
                    : gameplay.hasCandidateList
                      ? 'Candidate List'
                      : 'Search'}
              </h3>
              {/* Pick source panel is deferred — needs a separate sub-component
                  that renders pool/board/CL items or a search box.
                  For now shows a placeholder. */}
              <div className="h-24 border border-dashed border-white/10 flex items-center justify-center">
                <span className="text-white/20 text-xs font-mono">
                  Pick source — coming next
                </span>
              </div>
            </div>
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
    </>
  );
}