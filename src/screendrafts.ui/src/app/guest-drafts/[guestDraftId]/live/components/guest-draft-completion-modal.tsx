// app/guest-drafts/[guestDraftId]/live/components/guest-draft-completion-modal.tsx
'use client';

import { useRouter } from 'next/navigation';
import { useGuestDraftLive } from '../guest-draft-context';
import { GuestDraftSummaryContent } from './guest-draft-summary-content';

interface Props {
  title: string;
  totalPicks: number;
  vetoCount: number;
}

// The live, just-happened moment only — appears the instant a draft
// completes while someone's actively on /live (driven by completionSummary
// in context, set by the live DraftCompleted broadcast). A later calm
// revisit uses the standalone /summary page instead, not this modal.
export function GuestDraftCompletionModal({ title, totalPicks, vetoCount }: Props) {
  const router = useRouter();
  const { picks } = useGuestDraftLive();

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center bg-sd-ink/95 backdrop-blur-sm overflow-y-auto py-8 px-6">
      <GuestDraftSummaryContent
        title={title}
        totalPicks={totalPicks}
        vetoCount={vetoCount}
        picks={picks}
        action={
          <button
            onClick={() => router.push('/guest-drafts')}
            className="px-12 py-3 bg-sd-blue text-white font-oswald text-sm tracking-[0.2em] uppercase hover:bg-sd-blue/80 transition-colors"
          >
            OK
          </button>
        }
      />
    </div>
  );
}