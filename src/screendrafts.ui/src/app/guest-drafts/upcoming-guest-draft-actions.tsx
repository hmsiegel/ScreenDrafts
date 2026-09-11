// app/guest-drafts/upcoming-guest-draft-actions.tsx
'use client';

import Link from 'next/link';
import type { GuestDraftSummaryResponse } from '@/lib/dto';

interface Props {
  draft: GuestDraftSummaryResponse;
}

// AddParticipant/AssignParticipantToPosition/setGuestDraftStatus(Start) are
// all owner-gated server-side (OnlyOwnerCanPerformThisAction) — a
// participant has nothing to do on this screen yet, so no Setup link for
// them, just a status line. Once started they move to the In Progress
// section instead, where everyone gets the Play link.
export function UpcomingGuestDraftActions({ draft }: Props) {
  if (!draft.isOwner) {
    return (
      <div className="flex items-center justify-end">
        <span className="text-xs font-mono text-sd-ink/40">Waiting to start</span>
      </div>
    );
  }

  return (
    <div className="flex items-center justify-end">
      <Link
        href={`/guest-drafts/${draft.publicId}/setup`}
        className="bg-sd-blue text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-blue/90"
      >
        Setup
      </Link>
    </div>
  );
}