// app/draft-parts/[draftPartId]/live/components/pick-notification-modal.tsx
'use client';

import { useEffect, useRef, useState } from 'react';
import type { CommunityRuleAppliedPayload, GameplayNotification } from '../live-draft-context';
import { useLiveDraft } from '../live-draft-context';
import { undoVeto } from '../gameplay-fetchers';

interface Props {
  notification: GameplayNotification;
  onDismiss: () => void;
  isPrimaryHost: boolean;
  isCommissioner: boolean;
  accessToken: string;
  draftPartId: string;
}

// ── Honorific name maps ───────────────────────────────────────────────────────

const MOVIE_HONORIFIC_NAMES: Record<number, string> = {
  0: 'None',
  1: 'Marquee of Fame',
  2: 'Hat Trick',
  3: 'Grand Slam',
  4: 'High Five',
};

const DRAFTER_HONORIFIC_NAMES: Record<number, string> = {
  0: 'None',
  1: 'All-Star',
  2: 'Hall of Fame',
  3: 'MVP',
  4: 'Legend',
};

function movieHonorificName(value: number) {
  return MOVIE_HONORIFIC_NAMES[value] ?? `Honorific ${value}`;
}

function drafterHonorificName(value: number) {
  return DRAFTER_HONORIFIC_NAMES[value] ?? `Honorific ${value}`;
}

// ─────────────────────────────────────────────────────────────────────────────

export function PickNotificationModal({
  notification,
  onDismiss,
  isPrimaryHost,
  isCommissioner,
  accessToken,
  draftPartId,
}: Props) {
  const { participants } = useLiveDraft();
  const [undoing, setUndoing] = useState(false);
  const [confirming, setConfirming] = useState(false);
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    timerRef.current = setTimeout(onDismiss, 8000);
    return () => {
      if (timerRef.current) clearTimeout(timerRef.current);
    };
  }, [notification, onDismiss]);

  async function handleUndoVeto() {
    if (notification.kind !== 'VetoApplied') return;
    setUndoing(true);
    try {
      await undoVeto(accessToken, draftPartId, notification.payload.playOrder);
      onDismiss();
    } finally {
      setUndoing(false);
      setConfirming(false);
    }
  }

  function nameFor(participantId: string) {
    return (
      participants.find((p) => p.participantId === participantId)?.participantName ?? participantId
    );
  }

  const canUndoVeto = isPrimaryHost || isCommissioner;
  const { kind, payload } = notification;

  return (
    <div className="fixed inset-0 z-40 flex items-center justify-center bg-black/70">
      <div className="bg-sd-ink border border-white/20 w-full max-w-md mx-4 p-8 relative">

        {kind === 'PickAdded' && (
          <>
            <p className="font-oswald text-sd-red text-5xl font-bold mb-1">
              #{payload.boardPosition}
            </p>
            <p className="text-white/50 font-mono text-xs mb-3 uppercase tracking-widest">
              Picked by {nameFor(payload.participantId)}
            </p>
            <h2 className="font-oswald text-sd-paper text-3xl font-bold leading-tight mb-4">
              {payload.movieTitle}
            </h2>
            <a
              href={`https://www.themoviedb.org/movie/${payload.tmdbId}`}
              target="_blank"
              rel="noopener noreferrer"
              className="text-light-blue text-sm hover:underline font-mono"
            >
              View on TMDb →
            </a>
          </>
        )}

        {kind === 'PickRevealed' && (
          <>
            <p className="font-oswald text-sd-red text-5xl font-bold mb-1">
              #{payload.boardPosition}
            </p>
            <p className="text-white/50 font-mono text-xs mb-3 uppercase tracking-widest">
              Picked by {nameFor(payload.participantId)}
            </p>
            <h2 className="font-oswald text-sd-paper text-3xl font-bold leading-tight mb-4">
              {payload.movieTitle}
            </h2>
            <a
              href={`https://www.themoviedb.org/movie/${payload.tmdbId}`}
              target="_blank"
              rel="noopener noreferrer"
              className="text-light-blue text-sm hover:underline font-mono"
            >
              View on TMDb →
            </a>
          </>
        )}

        {kind === 'VetoApplied' && (
          <>
            <p className="font-oswald text-sd-red text-xl font-bold tracking-widest mb-3 uppercase">
              VETOED
            </p>
            <h2 className="font-oswald text-sd-paper text-2xl font-bold mb-2">
              {payload.movieTitle}
            </h2>
            <p className="text-white/50 font-mono text-xs mb-6">
              {nameFor(payload.vetoedByParticipantId)} vetoed {nameFor(payload.playedByParticipantId)}&apos;s pick
            </p>
            {canUndoVeto && (
              <>
                {confirming ? (
                  <div className="flex gap-3">
                    <button
                      onClick={handleUndoVeto}
                      disabled={undoing}
                      className="px-4 py-2 bg-sd-red text-white font-oswald text-sm tracking-wider hover:bg-sd-red/80 disabled:opacity-50"
                    >
                      {undoing ? 'UNDOING…' : 'CONFIRM UNDO'}
                    </button>
                    <button
                      onClick={() => setConfirming(false)}
                      className="px-4 py-2 border border-white/20 text-white/60 font-oswald text-sm tracking-wider hover:border-white/40"
                    >
                      CANCEL
                    </button>
                  </div>
                ) : (
                  <button
                    onClick={() => setConfirming(true)}
                    className="px-4 py-2 border border-sd-red/50 text-sd-red font-oswald text-sm tracking-wider hover:border-sd-red"
                  >
                    UNDO VETO
                  </button>
                )}
              </>
            )}
          </>
        )}

        {kind === 'VetoOverrideApplied' && (
          <>
            <p className="font-oswald text-light-blue text-xl font-bold tracking-widest mb-3 uppercase">
              SAVED
            </p>
            <h2 className="font-oswald text-sd-paper text-2xl font-bold mb-2">
              {payload.movieTitle}
            </h2>
            <p className="text-white/50 font-mono text-xs">
              Overridden by {nameFor(payload.overriddenByParticipantId)}
            </p>
          </>
        )}

        {kind === 'CommissionerOverrideApplied' && (
          <>
            <p className="font-oswald text-sd-red text-xl font-bold tracking-widest mb-3 uppercase">
              COMMISSIONER OVERRIDE
            </p>
            <h2 className="font-oswald text-sd-paper text-2xl font-bold mb-2">
              {payload.movieTitle}
            </h2>
            <p className="text-white/50 font-mono text-xs">
              Removed by the commissioners — {nameFor(payload.participantId)} picks again
            </p>
          </>
        )}

        {kind === 'VetoUndone' && (
          <>
            <p className="font-oswald text-white/60 text-xl font-bold tracking-widest mb-3 uppercase">
              VETO REVERSED
            </p>
            <h2 className="font-oswald text-sd-paper text-2xl font-bold">
              {payload.movieTitle}
            </h2>
          </>
        )}

        {kind === 'PickUndone' && (
          <>
            <p className="font-oswald text-white/60 text-xl font-bold tracking-widest mb-3 uppercase">
              PICK REMOVED
            </p>
            <h2 className="font-oswald text-sd-paper text-2xl font-bold">
              {payload.movieTitle}
            </h2>
          </>
        )}

        {kind === 'MovieHonorificChanged' && (
          <>
            <p className="font-oswald text-yellow-400 text-xl font-bold tracking-widest mb-3 uppercase">
              {payload.newAppearanceHonorificValue > payload.previousAppearanceHonorificValue
                ? 'HONORIFIC EARNED'
                : 'HONORIFIC REVERTED'}
            </p>
            <h2 className="font-oswald text-sd-paper text-2xl font-bold mb-2">
              {payload.movieTitle}
            </h2>
            <p className="text-white/50 font-mono text-xs">
              {movieHonorificName(payload.newAppearanceHonorificValue)} — {payload.appearanceCount} canonical appearance{payload.appearanceCount !== 1 ? 's' : ''}
            </p>
          </>
        )}

        {kind === 'DrafterHonorificChanged' && (
          <>
            <p className="font-oswald text-yellow-400 text-xl font-bold tracking-widest mb-3 uppercase">
              {payload.newHonorificValue > payload.previousHonorificValue
                ? 'DRAFTER HONORIFIC EARNED'
                : 'DRAFTER HONORIFIC REVERTED'}
            </p>
            <h2 className="font-oswald text-sd-paper text-2xl font-bold mb-2">
              {nameFor(payload.drafterIdValue)}
            </h2>
            <p className="text-white/50 font-mono text-xs">
              {drafterHonorificName(payload.newHonorificValue)} — {payload.appearanceCount} canonical appearance{payload.appearanceCount !== 1 ? 's' : ''}
            </p>
          </>
        )}

        {kind === 'CommunityRuleApplied' && (
          <>
            <p className="font-oswald text-sd-red text-xl font-bold tracking-widest mb-3 uppercase">
              {(payload as CommunityRuleAppliedPayload).ruleKind === 0
                ? 'BOOSTERS VETO'
                : "BOOSTERS' PICK"}
            </p>
            <h2 className="font-oswald text-sd-paper text-2xl font-bold mb-2">
              {(payload as CommunityRuleAppliedPayload).movieTitle}
            </h2>
            <p className="text-white/50 font-mono text-xs">
              {(payload as CommunityRuleAppliedPayload).ruleKind === 0
                ? `The Patreon community voted this must land at slot ${(payload as CommunityRuleAppliedPayload).targetSlot} or higher. Auto-vetoed.`
                : `The Patreon community voted this must be played at slot ${(payload as CommunityRuleAppliedPayload).targetSlot}. Auto-vetoed.`}
            </p>
          </>
        )}

        <button
          onClick={onDismiss}
          className="absolute top-4 right-4 text-white/30 hover:text-white/70 font-mono text-xs"
        >
          DISMISS
        </button>
      </div>
    </div>
  );
}