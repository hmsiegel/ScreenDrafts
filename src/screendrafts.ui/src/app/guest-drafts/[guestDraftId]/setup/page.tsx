// app/guest-drafts/[guestDraftId]/setup/page.tsx
import { auth } from '@/auth';
import { redirect } from 'next/navigation';
import { Metadata } from 'next';
import { fetchGuestDraftGameplay } from '../live/gameplay-fetchers';
import { GuestDraftLiveProvider } from '../live/guest-draft-context';
import { SetupPanel } from './setup-panel';
import Link from 'next/link';

export const metadata: Metadata = { title: 'Guest Draft Setup' };
export const dynamic = 'force-dynamic';

interface Props {
  params: Promise<{ guestDraftId: string }>;
}

export default async function GuestDraftSetupPage({ params }: Props) {
  const { guestDraftId } = await params;
  const session = await auth();
  if (!session?.accessToken) redirect('/');

  const gameplay = await fetchGuestDraftGameplay(session.accessToken, guestDraftId);

  // Already running or done — nothing to set up anymore, send the owner to
  // the live board instead of a setup screen that no longer applies.
  if (gameplay.status !== 'Created') {
    redirect(`/guest-drafts/${guestDraftId}/live`);
  }

  return (
    <GuestDraftLiveProvider
      guestDraftId={guestDraftId}
      accessToken={session.accessToken}
      initialGameplay={gameplay}
    >
      <div className="min-h-screen bg-light-blue">
        <div className="page-x py-8 lg:py-10 max-w-[900px] mx-auto">
          <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">
            <Link href="/guest-drafts" className="hover:text-sd-ink transition-colors">
              / GUEST DRAFTS
            </Link> / SETUP
          </p>
          <h1 className="font-oswald font-bold text-[30px] sm:text-[40px] leading-none text-sd-ink mb-6 sm:mb-8 [overflow-wrap:anywhere]">
            {gameplay.title}
          </h1>
          {/* p-4 on phones: 32px of padding each side left the panel about 260px wide on a 375px screen. */}
          <div className="bg-white border border-sd-ink/10 p-4 sm:p-8">
            <SetupPanel accessToken={session.accessToken} guestDraftId={guestDraftId} />
          </div>
        </div>
      </div>
    </GuestDraftLiveProvider>
  );
}