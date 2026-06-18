// app/draft-parts/[draftPartId]/live/components/pick-notification-modal.tsx
'use client';

import { useEffect, useRef, useState } from 'react';
import type { GameplayNotification } from '../live-draft-context';
import { undoVeto } from '../gameplay-fetchers';

interface Props {
  notification: GameplayNotification;
  onDismiss: () => void;
  isPrimaryHost: boolean;
  isCommissioner: boolean;
  accessToken: string;
  draftPartId: string;
}

export function PickNotificationModal({
  notification,
  onDismiss,
  isPrimaryHost,
  isCommissioner,
  accessToken,
  draftPartId,
}: Props) {
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
              Picked by {payload.participantId}
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
              Play order #{payload.playOrder}
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
            <h2 className="font-oswald text-sd-paper text-2xl font-bold">
              {payload.movieTitle}
            </h2>
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