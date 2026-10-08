// components/features/stats/title-level-tabs.tsx
import Link from "next/link";
import { TITLE_LEVELS } from "@/services/stats/title-levels";
import type { TitleHonorificCountView } from "@/services/stats/stats-types";
import { formatStatValue } from "./format-stat-value";
import { titleListHref } from "./title-links";
import { act } from "react";

/** The four honorific levels as sub-tabs, each with the number of titles at that level. Plain links. */
export function TitleLevelTabs({
  activeSlug,
  counts,
  includeAll,
}: {
  activeSlug: string;
  counts: TitleHonorificCountView[];
  includeAll: boolean;
}) {
  return (
    <nav aria-label="Honorific level" className="flex gap-2 overflow-x-auto pb-1">
      {TITLE_LEVELS.map((level) => {
        const active = level.slug === activeSlug;
        const count = counts.find((c) => c.code === level.slug)?.count;

        if (!level.named && !active && count === 0) return null;

        return (
          <Link
            key={level.slug}
            href={titleListHref(level.slug, { includeAll })}
            aria-current={active ? "page" : undefined}
            className={`shrink-0 min-h-10 px-4 flex items-center gap-2 border-2 border-sd-ink font-oswald font-medium text-sm tracking-[0.1em] transition-colors ${
              active ? "bg-sd-ink text-white" : "bg-white text-sd-ink hover:bg-sd-paper"
            }`}
          >
            {level.label.toUpperCase()}
            {count !== undefined && (
              <span className={`font-mono text-[11px] ${active ? "text-light-blue" : "text-[#5a6075]"}`}>
                {formatStatValue(count, "count")}
              </span>
            )}
          </Link>
        );
      })}
    </nav>
  );
}
