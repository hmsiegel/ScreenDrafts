// app/draft-parts/[draftPartId]/live/components/tabs/co-host-tab.tsx
'use client';

import { useState } from 'react';
import { useLiveDraft } from '../../live-draft-context';
import { DraftBoard } from '../draft-board';
import { DraftPickList } from '../draft-pick-list';
import type { GameplayPickResponse } from '@/lib/dto';

interface Props {
  accessToken: string;
  draftPartId: string;
  isCommissioner: boolean;
}

export function CoHostTab({ accessToken, draftPartId, isCommissioner }: Props) {
  const { picks, nextExpectedParticipantId, sendCountdown, connectionState } = useLiveDraft();
  const [counting, setCounting] = useState(false);
  const [acting, setActing] = useState(false);

  const canCountdown =
    nextExpectedParticipantId !== null &&
    connectionState === 'Connected';

  const mostRecentPick = picks.reduce<GameplayPickResponse | null>(
    (acc, p) => (!acc || (p.playOrder ?? 0) > (acc.playOrder ?? 0) ? p : acc),
    null,
  );

  async function handleCountdown() {
    if (!nextExpectedParticipantId || counting) return;
    setCounting(true);
    await sendCountdown(nextExpectedParticipantId);
    setTimeout(() => setCounting(false), 7000);
  }

  // Commissioner surrogates can apply a commissioner override on the most
  // recent pick — removes it without counting as a veto or a save.
  async function handleCommissionerOverride() {
    if (!mostRecentPick || acting) return;
    setActing(true);
    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/draft-parts/${draftPartId}/picks/${mostRecentPick.playOrder}/commissioner-override`,
        {
          method: 'POST',
          headers: { Authorization: `Bearer ${accessToken}` },
        },
      );
      if (!res.ok) console.error('Commissioner override failed', res.status);
    } finally {
      setActing(false);
    }
  }

  function renderBoardActions(pick: GameplayPickResponse) {
    if (pick.playOrder !== mostRecentPick?.playOrder) return null;
    if (!isCommissioner) return null;

    return (
      <button
        onClick={handleCommissionerOverride}
        disabled={acting}
        className="px-3 py-1 border border-white/30 text-white/70 font-oswald text-xs tracking-widest hover:border-white hover:text-white disabled:opacity-40 transition-colors"
      >
        {acting ? '…' : 'OVERRIDE'}
      </button>
    );
  }

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
      <div>
        <div className="flex items-center justify-between mb-3">
          <h2 className="font-oswald text-sm tracking-widest text-white/50 uppercase">
            Draft Board
          </h2>
          <button
            onClick={handleCountdown}
            disabled={!canCountdown || counting}
            className="px-4 py-2 border border-sd-red/50 text-sd-red font-oswald text-xs tracking-widest hover:border-sd-red disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
          >
            {counting ? 'COUNTING DOWN…' : 'START COUNTDOWN'}
          </button>
        </div>
        <DraftBoard renderActions={renderBoardActions} />

        <div className="mt-6 h-32 border border-dashed border-white/10 flex items-center justify-center">
          <span className="text-white/20 text-xs font-mono">Zoom — coming soon</span>
        </div>
      </div>

      <div>
        <h2 className="font-oswald text-sm tracking-widest text-white/50 uppercase mb-3">
          Pick List
        </h2>
        <DraftPickList />
      </div>
    </div>
  );
}