// app/guest-drafts/[guestDraftId]/summary/guest-draft-summary-view.tsx
import Link from 'next/link';
import type { GuestDraftDetailResponse } from '@/lib/dto';
import { GuestDraftSummaryContent } from '../live/components/guest-draft-summary-content';

interface Props {
  detail: GuestDraftDetailResponse;
}

export function GuestDraftSummaryView({ detail }: Props) {
  // detail.picks is typed as possibly undefined in dto.ts — the same NSwag
  // quirk seen elsewhere in this module: a C# IReadOnlyList<T> property with
  // a `= []` default doesn't come through as required in the generated TS
  // interface, even though the handler always returns an array.
  const picks = detail.picks ?? [];

  const totalPicks = picks.length;
  const vetoCount = picks.filter((p) => p.wasVetoed).length;

  return (
    <div className="min-h-screen bg-sd-ink px-6 py-10">
      <GuestDraftSummaryContent
        title={detail.title}
        totalPicks={totalPicks}
        vetoCount={vetoCount}
        picks={picks}
        action={
          <Link
            href="/guest-drafts"
            className="inline-block px-12 py-3 bg-sd-blue text-white font-oswald text-sm tracking-[0.2em] uppercase hover:bg-sd-blue/80 transition-colors"
          >
            Back to My Guest Drafts
          </Link>
        }
      />
    </div>
  );
}