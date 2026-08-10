'use client';

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
    <div className="flex items-center gap-3 justify-end">
      <Link
        href={`/admin/drafts/${draft.publicId}/attendances`}
        className="text-sd-blue text-xs font-mono uppercase tracking-wide hover:underline"
      >
        Attendance
      </Link>
      <Link
        href={`/admin/drafts/${draft.publicId}/seed`}
        className="bg-sd-blue text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-blue/90"
      >
        Resume Seeding
      </Link>
      <Link
        href={`/admin/drafts/${draft.publicId}/edit`}
        className="text-sd-blue text-sm font-medium hover:underline"
      >
        Edit
      </Link>
    </div>
  );
}