// app/guest-drafts/[guestDraftId]/live/live-guest-draft-view.tsx
'use client';

import { useState } from 'react';
import { useGuestDraftLive } from './guest-draft-context';
import { setGuestDraftStatus } from './gameplay-fetchers';
import { VetoStatusBar } from './components/veto-status-bar';
import { DrafterTab } from './components/drafter-tab';
import { DraftBoard } from './components/draft-board';
import { GuestDraftCompletionModal } from './components/guest-draft-completion-modal';
import { GUEST_DRAFT_STATUS_ACTION } from '../../guest-draft-status-actions';

interface Props {
  accessToken: string;
  guestDraftId: string;
}

export function LiveGuestDraftView({ accessToken, guestDraftId }: Props) {
  const {
    gameplay,
    picks,
    draftPositions,
    isOwner,
    completionSummary,
    reconnecting,
    connectionState,
    refetch,
  } = useGuestDraftLive();

  const [completing, setCompleting] = useState(false);
  const [completeError, setCompleteError] = useState<string | null>(null);

  const isComplete = gameplay.status === 'Completed';

  // Board-full check — every position's every owned slot has a currently-
  // landed pick. Owner-only: the Complete action is the same shape as Start
  // (owner-gated server-side), and unlike Start's setup-page precedent, this
  // one should actually be hidden from non-owners rather than shown and left
  // to 403.
  const allSlotsFilled =
    isOwner &&
    !isComplete &&
    draftPositions.length > 0 &&
    draftPositions.every((pos) =>
      (pos.ownedBoardSlots ?? []).every((slot) =>
        picks.some((p) => p.position === slot && p.isActiveOnFinalBoard),
      ),
    );

  async function handleComplete() {
    if (completing) return;
    setCompleting(true);
    setCompleteError(null);
    try {
      await setGuestDraftStatus(accessToken, guestDraftId, GUEST_DRAFT_STATUS_ACTION.Complete);
      // The completion modal itself is driven by the live DraftCompleted
      // SignalR broadcast (completionSummary in context), which reaches
      // everyone including this owner's own connection — not by anything
      // set directly from this call. refetch() here is just a safety net
      // in case that broadcast doesn't arrive for some reason.
      await refetch();
    } catch (e) {
      setCompleteError(e instanceof Error ? e.message : 'Failed to complete draft.');
    } finally {
      setCompleting(false);
    }
  }

  return (
    <div className="min-h-screen bg-sd-ink">
      <div className="max-w-5xl mx-auto px-6 py-8">
        <div className="mb-6">
          <h1 className="font-oswald font-bold text-[32px] leading-none text-sd-paper">
            {gameplay.title}
          </h1>
          <p className="text-xs text-white/40 font-mono mt-1">
            {gameplay.type} · {gameplay.status}
          </p>
        </div>

        {reconnecting && (
          <div className="mb-4 px-3 py-2 bg-yellow-400/10 border border-yellow-400/30">
            <p className="text-yellow-400 text-xs font-mono">Reconnecting…</p>
          </div>
        )}
        {connectionState === 'Disconnected' && !reconnecting && (
          <div className="mb-4 px-3 py-2 bg-sd-red/10 border border-sd-red/30">
            <p className="text-sd-red text-xs font-mono">
              Disconnected — live updates paused. Refresh to reconnect.
            </p>
          </div>
        )}

        <VetoStatusBar />

        {allSlotsFilled && (
          <div className="mt-6 p-4 border border-light-blue/30 bg-light-blue/5 flex items-center justify-between gap-4">
            <div>
              <p className="font-oswald text-light-blue text-sm tracking-wider">
                EVERY POSITION IS FILLED
              </p>
              <p className="text-xs text-white/40 font-mono mt-0.5">
                Mark the draft complete once everyone's done reviewing the board.
              </p>
              {completeError && (
                <p className="text-sd-red text-xs font-mono mt-1">{completeError}</p>
              )}
            </div>
            <button
              onClick={handleComplete}
              disabled={completing}
              className="shrink-0 px-4 py-2 bg-light-blue text-sd-ink font-oswald text-sm tracking-widest hover:bg-light-blue/80 disabled:opacity-50 transition-colors"
            >
              {completing ? 'COMPLETING…' : 'COMPLETE DRAFT'}
            </button>
          </div>
        )}

        {isComplete ? (
          // A later, calm revisit (no live completionSummary this session —
          // e.g. the "View" link from completed-guest-drafts-list.tsx) just
          // shows the board, no modal, no forced navigation. The modal below
          // is specifically for the moment completion actually happens live.
          <div className="mt-6">
            <p className="font-oswald text-sm tracking-widest text-white/50 uppercase mb-3">
              Final Board
            </p>
            <DraftBoard />
          </div>
        ) : (
          <div className="mt-6">
            <DrafterTab accessToken={accessToken} guestDraftId={guestDraftId} />
          </div>
        )}
      </div>

      {completionSummary && (
        <GuestDraftCompletionModal
          title={gameplay.title ?? ''}
          totalPicks={completionSummary.totalPicks}
          vetoCount={completionSummary.vetoCount}
        />
      )}
    </div>
  );
}