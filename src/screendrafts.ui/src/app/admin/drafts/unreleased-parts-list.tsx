'use client';

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
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
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
        <tbody>
          {parts.map((p) => (
            <tr
              key={p.draftPartPublicId}
              className="border-b border-sd-ink/5 hover:bg-sd-paper/60 transition-colors"
            >
              <td className="py-3 pr-4 font-medium text-sd-ink">{p.draftTitle}</td>
              <td className="py-3 pr-4 text-sd-ink/70">Part {p.partIndex}</td>
              <td className="py-3 pr-4 text-sd-ink/70">{p.seriesName ?? "—"}</td>
              <td className="py-3 pr-4 text-sd-ink/70">
                <DraftTypeBadge type={draftTypeFromNumber(p.draftType)} />
              </td>
              <td className="py-3">
                <Link
                  href={`/admin/drafts/${p.draftPublicId}/edit-meta`}
                  className="font-mono text-[11px] tracking-widest uppercase text-sd-blue hover:underline"
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