// app/guest-drafts/new/page.tsx
import { auth } from '@/auth';
import { redirect } from 'next/navigation';
import { Metadata } from 'next';
import { CreateGuestDraftForm } from './create-guest-draft-form';

export const metadata: Metadata = { title: 'Create Guest Draft' };
export const dynamic = 'force-dynamic';

export default async function CreateGuestDraftPage() {
  const session = await auth();
  if (!session?.accessToken) redirect('/');

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="page-x py-8 lg:py-10 max-w-[900px] mx-auto">
        <h1 className="font-oswald font-bold text-[32px] sm:text-[48px] leading-none text-sd-ink mb-6 sm:mb-10">
          CREATE GUEST DRAFT
        </h1>

        {/* p-4 on phones: 32px of padding each side left the form about 260px wide on a 375px screen. */}
        <div className="bg-white border border-sd-ink/10 p-4 sm:p-8">
          <CreateGuestDraftForm accessToken={session.accessToken} />
        </div>
      </div>
    </div>
  );
}