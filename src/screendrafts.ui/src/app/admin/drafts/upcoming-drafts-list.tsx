'use client';

import { useState } from "react";
import DraftTypeBadge from "@/components/ui/draft-type-badge";
import { draftTypeFromNumber } from "@/lib/draft-type-display";
import { type AdminDraftListItem } from "@/services/admin/fetch-admin-drafts";
import UpcomingDraftActions from "./upcoming-draft-actions";

const STATUS_LABELS: Record<number, string> = {
  0: "Created",
  1: "In Progress",
  3: "Paused",
  4: "Completed",
  5: "Cancelled",
};

interface UpcomingDraftsListProps {
  initialDrafts: AdminDraftListItem[];
  accessToken: string;
}

export default function UpcomingDraftsList({ initialDrafts, accessToken }: UpcomingDraftsListProps) {
  const [drafts, setDrafts] = useState<AdminDraftListItem[]>(initialDrafts);

  function removeDraft(publicId: string) {
    setDrafts((prev) => prev.filter((d) => d.publicId !== publicId));
  }

  if (drafts.length === 0) {
    return <p className="text-sd-ink/50 text-sm font-mono">No drafts found.</p>;
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-sd-ink/10">
            {["Title", "Type", "Series", "Status", ""].map((col) => (
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
          {drafts.map((d) => (
            <tr
              key={d.publicId}
              className="border-b border-sd-ink/5 hover:bg-sd-paper/60 transition-colors"
            >
              <td className="py-3 pr-4 font-medium text-sd-ink">{d.title}</td>
              <td className="py-3 pr-4 text-sd-ink/70">
                <DraftTypeBadge type={draftTypeFromNumber(d.draftType)} />
              </td>
              <td className="py-3 pr-4 text-sd-ink/70">{d.seriesName ?? "—"}</td>
              <td className="py-3 pr-4">
                <span
                  className={`inline-block px-2 py-0.5 text-[11px] font-mono tracking-wide uppercase rounded ${
                    d.draftStatus === 2
                      ? "bg-green-100 text-green-800"
                      : d.draftStatus === 3
                        ? "bg-yellow-100 text-yellow-800"
                        : "bg-gray-100 text-gray-600"
                  }`}
                >
                  {STATUS_LABELS[d.draftStatus ?? -1] ?? "Unknown"}
                </span>
              </td>
              <td className="py-3">
                <UpcomingDraftActions
                  draft={d}
                  accessToken={accessToken}
                  onRemove={removeDraft}
                />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
