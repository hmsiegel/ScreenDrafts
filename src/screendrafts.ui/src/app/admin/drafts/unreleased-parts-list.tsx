'use client';
// app/admin/drafts/unreleased-parts-list.tsx

import Link from "next/link";
import DraftTypeBadge from "@/components/ui/draft-type-badge";
import { draftTypeFromNumber } from "@/lib/draft-type-display";
import { UnreleasedDraftPart } from "@/services/admin/fetch-admin-drafts";

interface UnreleasedPartsListProps {
  parts: UnreleasedDraftPart[];
}

export default function UnreleasedPartsList({ parts }: UnreleasedPartsListProps) {
  if (parts.length === 0) {
    return <p className="text-sd-ink/50 text-sm font-mono">Nothing completed is missing a release.</p>;
  }

  return (
    <div className="sm:overflow-x-auto">
      <table className="block sm:table w-full text-sm">
        <thead className="hidden sm:table-header-group">
          <tr className="border-b border-sd-ink/10">
            {["Draft", "Part", "Series", "Type", ""].map((col) => (
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
          {parts.map((p) => (
            <tr
              key={p.draftPartPublicId}
              className="flex flex-wrap items-center gap-x-3 gap-y-1 py-3 sm:table-row sm:py-0 border-b border-sd-ink/5 hover:bg-sd-paper/60 transition-colors"
            >
              <td className="w-full sm:w-auto sm:table-cell sm:py-3 sm:pr-4 font-medium text-sd-ink [overflow-wrap:anywhere]">{p.draftTitle}</td>
              <td className="sm:table-cell sm:py-3 sm:pr-4 text-xs sm:text-sm text-sd-ink/70">Part {p.partIndex}</td>
              <td className="sm:table-cell sm:py-3 sm:pr-4 text-xs sm:text-sm text-sd-ink/70">{p.seriesName ?? "—"}</td>
              <td className="sm:table-cell sm:py-3 sm:pr-4 text-xs sm:text-sm text-sd-ink/70">
                <DraftTypeBadge type={draftTypeFromNumber(p.draftType)} />
              </td>
              <td className="w-full sm:w-auto sm:table-cell sm:py-3 pt-2">
                <Link
                  href={`/admin/drafts/${p.draftPublicId}/edit-meta`}
                  className="inline-flex items-center min-h-9 sm:min-h-0 font-mono text-[11px] tracking-widest uppercase text-sd-blue hover:underline"
                >
                  Set Release →
                </Link>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}