// components/features/stats/title-pagination.tsx
import Link from "next/link";
import type { TitleSort } from "@/services/stats/stats-types";
import { titleListHref } from "./title-links";

const WINDOW = 2;

/** Previous, next, and the pages around the current one. Plain links that keep the search, sort and scope. */
export function TitlePagination({
  level,
  page,
  totalPages,
  query,
  sort,
  includeAll,
}: {
  level: string;
  page: number;
  totalPages: number;
  query: string;
  sort: TitleSort;
  includeAll: boolean;
}) {
  if (totalPages <= 1) return null;

  const href = (target: number) => titleListHref(level, { q: query, sort, page: target, includeAll });

  const first = Math.max(1, page - WINDOW);
  const last = Math.min(totalPages, page + WINDOW);
  const pages = Array.from({ length: last - first + 1 }, (_, i) => first + i);

  const base = "min-h-10 min-w-10 px-3 flex items-center justify-center border-2 border-sd-ink font-mono text-[12px] transition-colors";

  return (
    <nav aria-label="Pages" className="flex flex-wrap items-center justify-center gap-2">
      {page > 1 && (
        <Link href={href(page - 1)} className={`${base} bg-white hover:bg-sd-paper`}>
          PREV
        </Link>
      )}

      {pages.map((p) => (
        <Link
          key={p}
          href={href(p)}
          aria-current={p === page ? "page" : undefined}
          className={`${base} ${p === page ? "bg-sd-ink text-white" : "bg-white hover:bg-sd-paper"}`}
        >
          {p}
        </Link>
      ))}

      {page < totalPages && (
        <Link href={href(page + 1)} className={`${base} bg-white hover:bg-sd-paper`}>
          NEXT
        </Link>
      )}
    </nav>
  );
}
