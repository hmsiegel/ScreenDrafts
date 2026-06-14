// app/draft-parts/[draftPartId]/live/components/tabs/co-host-tab.tsx
'use client';

import { useState } from 'react';
import { useLiveDraft } from '../../live-draft-context';
import { DraftBoard } from '../draft-board';
import { DraftPickList } from '../draft-pick-list';

interface Props {
  userPublicId: string | null;
}

export function CoHostTab({ userPublicId: _userPublicId }: Props) {
  const { nextExpectedParticipantId, sendCountdown, connectionState } = useLiveDraft();
  const [counting, setCounting] = useState(false);

  const canCountdown =
    nextExpectedParticipantId !== null &&
    connectionState === "Connected"; // HubConnectionState.Connected = 1

  async function handleCountdown() {
    if (!nextExpectedParticipantId || counting) return;
    setCounting(true);
    await sendCountdown(nextExpectedParticipantId);
    // Re-enable after 7 seconds (countdown is 5s + 2s buffer)
    setTimeout(() => setCounting(false), 7000);
  }

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
      {/* Left — Board + countdown */}
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
        <DraftBoard />

        {/* Zoom placeholder */}
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
  );
}