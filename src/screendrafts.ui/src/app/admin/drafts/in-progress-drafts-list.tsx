'use client';
// app/admin/drafts/in-progress-drafts-list.tsx

import DraftTypeBadge from "@/components/ui/draft-type-badge";
import { draftTypeFromNumber } from "@/lib/draft-type-display";
import type { AdminDraftListItem } from "@/services/admin/fetch-admin-drafts";
import InProgressDraftActions from "./in-progress-draft-actions";

interface InProgressDraftsListProps {
  drafts: AdminDraftListItem[];
}

// Deliberately not independently fetched — draws from the same
// listAdminActiveDrafts() call the page already makes for
// UpcomingDraftsList, filtered to draftStatus === 2 (InProgress, confirmed
// against DraftStatus.cs — value 1 is unused in this enum). Both the fetch
// and this filter were wrong before: listAdminActiveDrafts only requested
// statuses 0 and 3, and this filter checked for 1 instead of 2.
export default function InProgressDraftsList({ drafts }: InProgressDraftsListProps) {
  const inProgress = drafts.filter((d) => d.draftStatus === 2 && !d.isDeleted);

  if (inProgress.length === 0) {
    return <p className="text-sd-ink/50 text-sm font-mono">Nothing in progress right now.</p>;
  }

  return (
    <div className="sm:overflow-x-auto">
      <table className="block sm:table w-full text-sm">
        <thead className="hidden sm:table-header-group">
          <tr className="border-b border-sd-ink/10">
            {["Title", "Type", "Series", ""].map((col) => (
              <th
                key={col}
                className="text-left font-mono text-[11px] tracking-widest uppercase text-sd-ink/50 pb-3 pr-4"
              >
                {col}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="block sm:table-row-group">
          {inProgress.map((d) => (
            <tr key={d.publicId} className="flex flex-wrap items-center gap-x-3 gap-y-1 py-3 sm:table-row sm:py-0 border-b border-sd-ink/5 hover:bg-sd-paper/60 transition-colors">
              <td className="w-full sm:w-auto sm:table-cell sm:py-3 sm:pr-4 font-medium text-sd-ink [overflow-wrap:anywhere]">{d.title}</td>
              <td className="sm:table-cell sm:py-3 sm:pr-4 text-xs sm:text-sm text-sd-ink/70">
                <DraftTypeBadge type={draftTypeFromNumber(d.draftType)} />
              </td>
              <td className="sm:table-cell sm:py-3 sm:pr-4 text-xs sm:text-sm text-sd-ink/70">{d.seriesName ?? "—"}</td>
              <td className="w-full sm:w-auto sm:table-cell sm:py-3 pt-2">
                <InProgressDraftActions draft={d} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}