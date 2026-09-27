// app/guest-drafts/page.tsx
import { auth } from '@/auth';
import { redirect } from 'next/navigation';
import Link from 'next/link';
import { Metadata } from 'next';
import { getMyGuestDrafts } from './[guestDraftId]/live/gameplay-fetchers';
import { UpcomingGuestDraftsList } from './upcoming-guest-drafts-list';
import { InProgressGuestDraftsList } from './in-progress-guest-drafts-list';
import { CompletedGuestDraftsList } from './completed-guest-drafts-list';
import GuestDraftsRealtimeRefresher from './guest-drafts-realtime-refresher';

export const metadata: Metadata = { title: 'My Guest Drafts' };
export const dynamic = 'force-dynamic';

function GuestDraftsCard({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="bg-white border border-sd-ink/10 mb-8">
      <div className="flex items-center gap-3 px-6 py-4 border-b border-sd-ink/10 bg-sd-ink">
        <div className="w-1 h-5 bg-sd-red shrink-0" />
        <h2 className="font-oswald font-bold text-[16px] tracking-wide uppercase text-white">
          {title}
        </h2>
      </div>
      <div className="p-6">{children}</div>
    </div>
  );
}

export default async function GuestDraftsPage() {
  const session = await auth();
  if (!session?.accessToken) redirect('/');

  const { upcoming, inProgress, completed } = await getMyGuestDrafts(session.accessToken);

  // Only "Created" status parts can transition to InProgress while someone's
  // looking at this list — "Paused" also lands in the upcoming bucket
  // server-side but isn't watchable the same way, so filter it back out here.
  const watchedGuestDraftIds = upcoming
    .filter((d) => d.status === 'Created')
    .map((d) => d.publicId);

  return (
    <div className="min-h-screen bg-light-blue">
      <GuestDraftsRealtimeRefresher
        accessToken={session.accessToken}
        watchedGuestDraftIds={watchedGuestDraftIds}
      />
      <div className="px-6 md:px-10 py-10 max-w-[1200px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">
          / GUEST DRAFTS
        </p>

        <div className="flex items-end justify-between mb-10">
          <h1 className="font-oswald font-bold text-[56px] leading-none text-sd-ink">
            MY GUEST DRAFTS
          </h1>
          <Link
            href="/guest-drafts/new"
            className="bg-sd-red text-white font-oswald font-medium tracking-wide uppercase px-5 py-3 hover:bg-sd-red/90 transition-colors"
          >
            + Create New Draft
          </Link>
        </div>

        <GuestDraftsCard title="Upcoming">
          <UpcomingGuestDraftsList drafts={upcoming} />
        </GuestDraftsCard>

        <GuestDraftsCard title="In Progress">
          <InProgressGuestDraftsList drafts={inProgress} />
        </GuestDraftsCard>

        <GuestDraftsCard title="Completed">
          <CompletedGuestDraftsList drafts={completed} />
        </GuestDraftsCard>
      </div>
    </div>
  );
}