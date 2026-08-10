'use client';

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
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
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
        <tbody>
          {inProgress.map((d) => (
            <tr key={d.publicId} className="border-b border-sd-ink/5 hover:bg-sd-paper/60 transition-colors">
              <td className="py-3 pr-4 font-medium text-sd-ink">{d.title}</td>
              <td className="py-3 pr-4 text-sd-ink/70">
                <DraftTypeBadge type={draftTypeFromNumber(d.draftType)} />
              </td>
              <td className="py-3 pr-4 text-sd-ink/70">{d.seriesName ?? "—"}</td>
              <td className="py-3">
                <InProgressDraftActions draft={d} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}