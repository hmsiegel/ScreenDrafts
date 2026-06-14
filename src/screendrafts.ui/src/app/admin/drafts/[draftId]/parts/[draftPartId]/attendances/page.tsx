// src/app/admin/drafts/[draftId]/parts/[draftPartId]/attendances/page.tsx
import { auth } from '@/auth';
import { redirect, notFound } from 'next/navigation';
import Link from 'next/link';
import { fetchAttendances } from '@/services/admin/fetch-attendances';
import { getDraft } from '@/services/admin/fetch-admin-drafts';
import { AttendanceManager } from './attendance-manager';
import type { Metadata } from 'next';

export const metadata: Metadata = { title: 'Attendance — ScreenDrafts Admin' };

const ADMIN_ROLES = ['Administrator', 'SuperAdministrator'];

interface Props {
  params: Promise<{ draftId: string; draftPartId: string }>;
}

export default async function AttendancePage({ params }: Props) {
  const { draftId, draftPartId } = await params;

  const session = await auth();
  if (!session?.accessToken || !ADMIN_ROLES.some((r) => session.roles?.includes(r))) {
    redirect('/');
  }

  const [items, draft] = await Promise.all([
    fetchAttendances(session.accessToken, draftPartId),
    getDraft(session.accessToken, draftId),
  ]);

  if (!draft) notFound();

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="px-6 md:px-10 py-10 max-w-3xl mx-auto space-y-8">
        {/* Breadcrumb */}
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50">
          <Link href="/admin/drafts" className="hover:text-sd-ink/70">
            ADMIN / DRAFTS
          </Link>
          {' / '}
          {draft.title.toUpperCase()}
          {' / ATTENDANCE'}
        </p>

        <div>
          <h1 className="font-oswald font-bold text-[48px] leading-none text-sd-ink">
            ATTENDANCE
          </h1>
          <p className="text-sd-ink/60 font-oswald text-lg mt-1">{draft.title}</p>
          <p className="font-mono text-xs text-sd-ink/40 mt-1">
            Confirm participants before the draft begins.
          </p>
        </div>

        <AttendanceManager
          initialItems={items}
          accessToken={session.accessToken}
          draftPartId={draftPartId}
        />
      </div>
    </div>
  );
}