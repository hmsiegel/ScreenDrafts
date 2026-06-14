// src/app/admin/drafts/[draftId]/parts/[draftPartId]/attendances/page.tsx
import { auth } from '@/auth';
import { redirect } from 'next/navigation';
import { fetchAttendances } from '@/services/admin/fetch-attendances';
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

  const items = await fetchAttendances(session.accessToken, draftPartId);

  return (
    <div className="min-h-screen bg-sd-ink text-sd-paper">
      <div className="px-6 md:px-10 py-10 max-w-3xl mx-auto space-y-8">
        {/* Breadcrumb */}
        <p className="font-mono text-[11px] tracking-widest text-white/40 uppercase">
          / Admin / Drafts / {draftId} / Parts / {draftPartId} / Attendance
        </p>

        <div>
          <h1 className="font-oswald font-bold text-4xl uppercase tracking-wide text-sd-paper">
            Attendance
          </h1>
          <p className="text-white/40 font-mono text-sm mt-1">
            Confirm participants before the draft begins. A confirmed status is required to join.
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