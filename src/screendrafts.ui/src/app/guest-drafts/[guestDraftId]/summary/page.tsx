// app/guest-drafts/[guestDraftId]/summary/page.tsx
import { auth } from '@/auth';
import { redirect } from 'next/navigation';
import { Metadata } from 'next';
import { fetchGuestDraftDetails } from '../live/gameplay-fetchers';
import { GuestDraftSummaryView } from './guest-draft-summary-view';

export const metadata: Metadata = { title: 'Guest Draft Summary' };
export const dynamic = 'force-dynamic';

interface Props {
  params: Promise<{ guestDraftId: string }>;
}

export default async function GuestDraftSummaryPage({ params }: Props) {
  const { guestDraftId } = await params;
  const session = await auth();
  if (!session?.accessToken) redirect('/');

  const detail = await fetchGuestDraftDetails(session.accessToken, guestDraftId);

  // Only a genuinely completed draft has a summary — anything else sends
  // the visitor to wherever that status actually belongs, mirroring /live
  // and /setup's own redirect guards.
  if (detail.status === 'Created') {
    redirect(`/guest-drafts/${guestDraftId}/setup`);
  }
  if (detail.status === 'InProgress') {
    redirect(`/guest-drafts/${guestDraftId}/live`);
  }

  return <GuestDraftSummaryView detail={detail} />;
}