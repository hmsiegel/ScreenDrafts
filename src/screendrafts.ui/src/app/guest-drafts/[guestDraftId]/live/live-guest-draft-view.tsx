// app/guest-drafts/[guestDraftId]/live/live-guest-draft-view.tsx
'use client';

import { useGuestDraftLive } from './guest-draft-context';
import { VetoStatusBar } from './components/veto-status-bar';
import { DrafterTab } from './components/drafter-tab';

interface Props {
  accessToken: string;
  guestDraftId: string;
}

export function LiveGuestDraftView({ accessToken, guestDraftId }: Props) {
  const { gameplay, completionSummary, reconnecting, connectionState } = useGuestDraftLive();

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

        {completionSummary ? (
          <div className="mt-6 p-6 border border-white/10 bg-white/5 text-center">
            <p className="font-oswald text-sd-paper text-2xl font-bold mb-2">DRAFT COMPLETE</p>
            <p className="text-white/50 text-sm font-mono">
              {completionSummary.totalPicks} picks · {completionSummary.vetoCount} vetoes
            </p>
          </div>
        ) : (
          <div className="mt-6">
            <DrafterTab accessToken={accessToken} guestDraftId={guestDraftId} />
          </div>
        )}
      </div>
    </div>
  );
}