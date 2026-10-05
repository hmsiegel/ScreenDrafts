// components/features/stats/query-results.tsx
import Link from "next/link";
import { formatDraftType } from "@/lib/draft-type-display";
import type { HolderKind, StatsQueryResultView } from "@/services/stats/stats-types";
import { formatStatValue, statHref } from "./format-stat-value";

function kindFor(groupBy: string): HolderKind | null {
  switch (groupBy) {
    case "drafter":
      return "drafter";
    case "draft":
      return "draft";
    case "title":
      return "title";
    default:
      return null;
  }
}

export function QueryResults({ result }: { result: StatsQueryResultView }) {
  const kind = kindFor(result.groupBy);

  return (
    <div className="bg-white border-2 border-sd-ink p-5 sm:p-6">
      <div className="flex flex-wrap items-baseline justify-between gap-2 mb-4">
        <h2 className="font-oswald font-bold text-[13px] tracking-widest text-sd-red uppercase">
          {result.metricLabel}
        </h2>
        <span className="font-mono text-[10px] tracking-widest text-[#5a6075]">
          {result.includesNonCanonical ? "ALL DRAFTS" : "CANONICAL DRAFTS"}
        </span>
      </div>

      {result.rows.length === 0 ? (
        <p className="font-mono text-sm text-sd-ink/50 py-6 text-center">
          No results for these filters.
        </p>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-left">
            <thead>
              <tr className="font-mono text-[10px] tracking-widest text-[#5a6075] border-b border-sd-ink/15">
                <th scope="col" className="py-2 pr-3 w-12">RANK</th>
                <th scope="col" className="py-2 pr-3">NAME</th>
                <th scope="col" className="py-2 pr-3 text-right">VALUE</th>
                <th scope="col" className="py-2 hidden sm:table-cell">DETAIL</th>
              </tr>
            </thead>
            <tbody>
              {result.rows.map((row, index) => {
                const href = kind ? statHref(kind, row.publicId) : null;

                return (
                  <tr key={`${row.rank}:${row.publicId ?? row.name}:${index}`} className="border-b border-sd-ink/10 last:border-b-0">
                    <td className="py-2.5 pr-3 font-mono text-[12px] text-[#5a6075]">{row.rank}</td>
                    <td className="py-2.5 pr-3 font-oswald font-bold text-[15px] [overflow-wrap:anywhere]">
                      {href ? (
                        <Link href={href} className="text-sd-blue hover:text-sd-red transition-colors">
                          {row.name}
                        </Link>
                      ) : (
                        <span className="text-sd-ink">
                          {result.groupBy === "draftType" ? formatDraftType(row.name) : row.name}
                        </span>
                      )}
                    </td>
                    <td className="py-2.5 pr-3 text-right font-oswald font-bold text-[18px] text-sd-ink">
                      {formatStatValue(row.value, result.format)}
                    </td>
                    <td className="py-2.5 hidden sm:table-cell font-mono text-[11px] text-[#5a6075]">
                      {row.context}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      <p className="font-mono text-[10px] tracking-widest text-[#5a6075] mt-4">
        SHOWING {result.rows.length} OF {result.totalGroups}
        {result.truncated ? " · NARROW THE FILTERS OR RAISE THE LIMIT TO SEE MORE" : ""}
      </p>
    </div>
  );
}