'use client';
// app/admin/drafts/in-progress-draft-actions.tsx

import Link from "next/link";
import type { AdminDraftListItem } from "@/services/admin/fetch-admin-drafts";

interface InProgressDraftActionsProps {
  draft: AdminDraftListItem;
}

// No Start (already started) and no Delete — Draft.SoftDelete() refuses once
// any part has left Created status, so offering it here would just fail.
// Resume Seeding is the primary action; falls through to the normal Edit
// flow for anything the seed wizard doesn't cover yet.
export default function InProgressDraftActions({ draft }: InProgressDraftActionsProps) {
  return (
    <div className="flex flex-wrap items-center gap-x-4 gap-y-1 sm:gap-3 sm:justify-end">
      <Link
        href={`/admin/drafts/${draft.publicId}/attendances`}
        className="inline-flex items-center min-h-9 sm:min-h-0 text-sd-blue text-xs font-mono uppercase tracking-wide hover:underline"
      >
        Attendance
      </Link>
      <Link
        href={`/admin/drafts/${draft.publicId}/seed`}
        className="inline-flex items-center min-h-9 sm:min-h-0 bg-sd-blue text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-blue/90"
      >
        Resume Seeding
      </Link>
      <Link
        href={`/admin/drafts/${draft.publicId}/edit`}
        className="inline-flex items-center min-h-9 sm:min-h-0 text-sd-blue text-sm font-medium hover:underline"
      >
        Edit
      </Link>
    </div>
  );
}