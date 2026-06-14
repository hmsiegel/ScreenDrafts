// src/app/admin/drafts/[draftId]/attendances/page.tsx
import { auth } from '@/auth';
import { redirect, notFound } from 'next/navigation';
import Link from 'next/link';
import { getDraft } from '@/services/admin/fetch-admin-drafts';
import type { Metadata } from 'next';

export const metadata: Metadata = { title: 'Attendance — ScreenDrafts Admin' };

const ADMIN_ROLES = ['Administrator', 'SuperAdministrator'];

interface Props {
  params: Promise<{ draftId: string }>;
}

export default async function DraftAttendancesPage({ params }: Props) {
  const { draftId } = await params;

  const session = await auth();
  if (!session?.accessToken || !ADMIN_ROLES.some((r) => session.roles?.includes(r))) {
    redirect('/');
  }

  const draft = await getDraft(session.accessToken, draftId);
  if (!draft) notFound();

  const parts = draft.parts ?? [];

  // Single-part draft: skip the selector and go straight to the part's attendance page.
  if (parts.length === 1 && parts[0].publicId) {
    redirect(`/admin/drafts/${draftId}/parts/${parts[0].publicId}/attendances`);
  }

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="px-6 md:px-10 py-10 max-w-2xl mx-auto space-y-8">
        {/* Breadcrumb */}
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50">
          <Link href="/admin" className="hover:text-sd-ink/70">ADMIN</Link>
          {' / '}
          <Link href="/admin/drafts" className="hover:text-sd-ink/70">DRAFTS</Link>
          {' / ATTENDANCE'}
        </p>

        <div>
          <h1 className="font-oswald font-bold text-[48px] leading-none text-sd-ink">
            ATTENDANCE
          </h1>
          <p className="text-sd-ink/60 font-oswald text-lg mt-1">{draft.title}</p>
          <p className="font-mono text-xs text-sd-ink/40 mt-1">Select a part to manage attendance.</p>
        </div>

        {parts.length === 0 ? (
          <p className="text-sd-ink/40 font-mono text-sm">No parts found for this draft.</p>
        ) : (
          <div className="space-y-2">
            {parts.map((part) => (
              <Link
                key={part.publicId}
                href={`/admin/drafts/${draftId}/parts/${part.publicId}/attendances`}
                className="flex items-center justify-between bg-white border border-sd-ink/10 px-5 py-4 hover:border-sd-ink/30 transition-colors group"
              >
                <p className="font-oswald font-bold text-sd-ink uppercase tracking-wide">
                  Part {part.partIndex}
                </p>
                <span className="font-mono text-xs text-sd-blue group-hover:underline uppercase tracking-wider">
                  Manage Attendance →
                </span>
              </Link>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}