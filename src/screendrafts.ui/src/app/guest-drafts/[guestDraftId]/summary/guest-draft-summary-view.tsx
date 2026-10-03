// app/guest-drafts/[guestDraftId]/summary/guest-draft-summary-view.tsx
import Link from 'next/link';
import type { GuestDraftDetailResponse } from '@/lib/dto';
import { GuestDraftSummaryContent } from '../live/components/guest-draft-summary-content';
import { GuestDraftScheduleEditor } from '../../guest-draft-schedule-editor';

interface Props {
  detail: GuestDraftDetailResponse;
  accessToken: string;
}

export function GuestDraftSummaryView({ detail, accessToken }: Props) {
  // detail.picks is typed as possibly undefined in dto.ts — the same NSwag
  // quirk seen elsewhere in this module: a C# IReadOnlyList<T> property with
  // a `= []` default doesn't come through as required in the generated TS
  // interface, even though the handler always returns an array.
  const picks = detail.picks ?? [];

  const totalPicks = picks.length;
  const vetoCount = picks.filter((p) => p.wasVetoed).length;

  return (
    <div className="min-h-screen bg-sd-ink page-x py-8 lg:py-10">
      {/* Owners can correct the date after the fact (backend allows it at any
          status); everyone else only sees it if one was ever set. This page is
          server-rendered with no live context, so the editor falls back to
          router.refresh() after a save. */}
      {(detail.isOwner || detail.scheduledForUtc) && (
        <div className="max-w-3xl mx-auto mb-8">
          <GuestDraftScheduleEditor
            tone="dark"
            accessToken={accessToken}
            guestDraftId={detail.publicId}
            scheduledForUtc={detail.scheduledForUtc}
            isOwner={detail.isOwner}
          />
        </div>
      )}

      <GuestDraftSummaryContent
        title={detail.title}
        totalPicks={totalPicks}
        vetoCount={vetoCount}
        picks={picks}
        action={
          <Link
            href="/guest-drafts"
            className="inline-block px-6 sm:px-12 py-3 bg-sd-blue text-white font-oswald text-sm tracking-[0.12em] sm:tracking-[0.2em] uppercase hover:bg-sd-blue/80 transition-colors"
          >
            Back to My Guest Drafts
          </Link>
        }
      />
    </div>
  );
}