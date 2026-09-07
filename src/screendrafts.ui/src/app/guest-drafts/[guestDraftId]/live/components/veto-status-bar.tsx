// app/guest-drafts/[guestDraftId]/live/components/veto-status-bar.tsx
'use client';

import { useGuestDraftLive } from '../guest-draft-context';

function Pip({ filled }: { filled: boolean }) {
  return (
    <span
      className={`inline-block w-2.5 h-2.5 rounded-full border ${
        filled ? 'bg-sd-red border-sd-red' : 'bg-transparent border-white/30'
      }`}
    />
  );
}

export function VetoStatusBar() {
  const { participants, draftPositions, gameplay } = useGuestDraftLive();
  const isExtended = gameplay.type !== 'Standard';

  return (
    <div className="flex flex-wrap gap-px border-b border-white/10 bg-white/5">
      {participants.map((p) => {
        // GuestDrafts has no participant-kind concept (no Team kind), so unlike
        // canonical this matches on participantPublicId alone. ASSUMPTION:
        // GameplayPositionResponse.assignedParticipantPublicId and
        // GameplayParticipantResponse.participantPublicId are populated with
        // the same id — flag if that doesn't hold once this is live.
        const position = draftPositions.find(
          (pos) => pos.assignedParticipantId === p.participantPublicId,
        );

        return (
          <div
            key={p.participantPublicId ?? p.participantId}
            className="flex-1 min-w-[160px] px-4 py-2"
          >
            <div className="flex items-center gap-2 mb-1">
              {position && (
                <span className="font-oswald text-sd-red font-bold text-sm">
                  {position.positionName}
                </span>
              )}
              <span className="font-oswald text-xs tracking-wider text-white/70 truncate">
                {p.participantName}
              </span>
            </div>
            <div className="flex items-center gap-3">
              <div className="flex items-center gap-1">
                <span className="text-[10px] text-white/40 font-mono mr-1">V</span>
                {Array.from({ length: 3 }).map((_, i) => (
                  <Pip key={i} filled={i < (p.vetoTokensRemaining ?? 0)} />
                ))}
              </div>
              {isExtended && (
                <div className="flex items-center gap-1">
                  <span className="text-[10px] text-white/40 font-mono mr-1">O</span>
                  {Array.from({ length: 2 }).map((_, i) => (
                    <Pip key={i} filled={i < (p.overrideTokensRemaining ?? 0)} />
                  ))}
                </div>
              )}
            </div>
          </div>
        );
      })}
    </div>
  );
}