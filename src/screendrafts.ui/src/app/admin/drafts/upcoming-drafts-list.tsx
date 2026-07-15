'use client';

import { useCallback, useState } from "react";
import DraftTypeBadge from "@/components/ui/draft-type-badge";
import { draftTypeFromNumber } from "@/lib/draft-type-display";
import { listAdminActiveDrafts, type AdminDraftListItem } from "@/services/admin/fetch-admin-drafts";
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
  const [showDeleted, setShowDeleted] = useState(false);
  const [refreshing, setRefreshing] = useState(false);

  // Re-fetches from the server rather than patching local state — same
  // refresh() pattern as SpotlightManager and the other admin managers.
  // listAdminActiveDrafts already takes accessToken explicitly (it doesn't
  // call auth() internally), so it's safe to call directly from a client
  // component, unlike listAllCategories/listAllCampaigns/listAllSeries.
  const refresh = useCallback(async () => {
    setRefreshing(true);
    try {
      const fresh = await listAdminActiveDrafts(accessToken, true);
      setDrafts(fresh);
    } finally {
      setRefreshing(false);
    }
  }, [accessToken]);

  const visibleDrafts = showDeleted ? drafts : drafts.filter((d) => !d.isDeleted);

  return (
    <div>
      <div className="flex items-center justify-end mb-4 gap-3">
        {refreshing && (
          <span className="font-mono text-[11px] text-sd-ink/30">refreshing…</span>
        )}
        <label className="flex items-center gap-2 font-mono text-[11px] tracking-widest text-sd-ink/60 cursor-pointer select-none">
          <input
            type="checkbox"
            checked={showDeleted}
            onChange={(e) => setShowDeleted(e.target.checked)}
            className="accent-sd-red"
          />
          SHOW DELETED
        </label>
      </div>

      {visibleDrafts.length === 0 ? (
        <p className="text-sd-ink/50 text-sm font-mono">No drafts found.</p>
      ) : (
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
              {visibleDrafts.map((d) => (
                <tr
                  key={d.publicId}
                  className={`border-b border-sd-ink/5 transition-colors ${
                    d.isDeleted ? "opacity-50" : "hover:bg-sd-paper/60"
                  }`}
                >
                  <td className="py-3 pr-4 font-medium text-sd-ink">{d.title}</td>
                  <td className="py-3 pr-4 text-sd-ink/70">
                    <DraftTypeBadge type={draftTypeFromNumber(d.draftType)} />
                  </td>
                  <td className="py-3 pr-4 text-sd-ink/70">{d.seriesName ?? "—"}</td>
                  <td className="py-3 pr-4">
                    {d.isDeleted ? (
                      <span className="font-mono text-[10px] tracking-widest text-sd-red uppercase">
                        Deleted
                      </span>
                    ) : (
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
                    )}
                  </td>
                  <td className="py-3">
                    <UpcomingDraftActions
                      draft={d}
                      accessToken={accessToken}
                      onRefresh={refresh}
                    />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}