// app/guest-drafts/[guestDraftId]/live/page.tsx
import { auth } from '@/auth';
import { redirect } from 'next/navigation';
import { Metadata } from 'next';
import { fetchGuestDraftGameplay } from './gameplay-fetchers';
import { GuestDraftLiveProvider } from './guest-draft-context';
import { LiveGuestDraftView } from './live-guest-draft-view';

export const metadata: Metadata = { title: 'Guest Draft' };
export const dynamic = 'force-dynamic';

interface Props {
  params: Promise<{ guestDraftId: string }>;
}

export default async function GuestDraftLivePage({ params }: Props) {
  const { guestDraftId } = await params;
  const session = await auth();
  if (!session?.accessToken) redirect('/');

  const gameplay = await fetchGuestDraftGameplay(session.accessToken, guestDraftId);

  // Nothing to play yet — no participants/board assignment happens on this
  // screen (see CreateGuestDraftCommandHandler's remarks: creation no longer
  // adds any participants, not even the owner). Send the owner back to setup
  // rather than rendering an empty board.
  if (gameplay.status === 'Created') {
    redirect(`/guest-drafts/${guestDraftId}/setup`);
  }

  return (
    <GuestDraftLiveProvider
      guestDraftId={guestDraftId}
      accessToken={session.accessToken}
      initialGameplay={gameplay}
    >
      <LiveGuestDraftView accessToken={session.accessToken} guestDraftId={guestDraftId} />
    </GuestDraftLiveProvider>
  );
}