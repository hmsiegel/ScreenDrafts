// app/media/page.tsx
import MediaFilterStrip from "@/components/features/media/media-filter-strip";
import { fetchMedia } from "@/services/media/fetch-media";
import { Metadata } from "next";
import { Suspense } from "react";
import { listCacheKey, mediaQuery, type QueryRecord } from "@/lib/list-queries";
import { MediaInfiniteList } from "./media-infinite-list";

export const metadata: Metadata = {
  title: "The Vault",
  description: "Every film, series, and game that has ever touched a ScreenDrafts board.",
};

export const dynamic = "force-dynamic";

type SearchParams = Promise<QueryRecord>;

export default async function MediaPage(props: { searchParams: SearchParams }) {
  const qp = await props.searchParams;

  const { page, sort, search, mediaType, year, args } = mediaQuery(qp);

  const result = await fetchMedia({ ...args, page });
  const totalPages = result.totalPages ?? 0;
  const cacheKey = listCacheKey("media", qp);

  return (
    <div className="min-h-screen bg-light-blue">
      {/* Banner — stacks below lg; title and blurb sit side by side from lg up. */}
      <div className="bg-sd-ink text-white page-x pt-10 pb-8 lg:pt-14 lg:pb-11">
        <p className="font-mono text-[11px] tracking-widest text-light-blue mb-3">/ MEDIA</p>
        <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between lg:gap-8">
          <h1 className="font-oswald font-bold text-[44px] sm:text-[56px] lg:text-[72px] leading-[0.95] text-white">THE VAULT</h1>
          <p className="font-serif italic text-[15px] lg:text-[17px] leading-relaxed text-white/70 max-w-[480px] lg:text-right">
            Every film, series, and game that has ever touched a ScreenDrafts board.
          </p>
        </div>
      </div>

      {/* Filter strip */}
      <Suspense>
        <MediaFilterStrip
          mediaType={mediaType !== undefined ? String(mediaType) : ""}
          sort={sort}
          q={search}
          year={year}
        />
      </Suspense>

      {/* List — pt-6 separates it from the filter strip */}
      <div className="page-x pt-6 pb-16">
        {/* Remounts (key) whenever filters, sort or page change, so infinite scroll restarts cleanly. */}
        <MediaInfiniteList
          key={cacheKey}
          cacheKey={cacheKey}
          query={qp}
          sort={sort}
          initialItems={result.items ?? []}
          initialPage={page}
          total={result.totalCount ?? 0}
          totalPages={totalPages}
          paginator={totalPages > 1 ? <Paginator page={page} totalPages={totalPages} searchParams={qp} /> : null}
        />
      </div>

    </div>
  );
}

// ── Pagination — desktop only; below lg the list scrolls infinitely ──────────

function Paginator({
  page,
  totalPages,
  searchParams,
}: {
  page: number;
  totalPages: number;
  searchParams: QueryRecord;
}) {
  function pageHref(p: number): string {
    const qs = new URLSearchParams();
    Object.entries(searchParams).forEach(([k, v]) => {
      if (k === "page") return;
      if (Array.isArray(v)) v.forEach((s) => qs.append(k, s));
      else if (v) qs.set(k, v);
    });
    qs.set("page", String(p));
    return `?${qs.toString()}`;
  }

  const pages = buildPageRange(page, totalPages);

  return (
    <nav aria-label="Pagination" className="flex items-center gap-1 font-mono text-[11px]">
      {page > 1 && (
        <a href={pageHref(page - 1)} aria-label="Previous page" className="px-2.5 py-1 border border-sd-ink text-sd-ink hover:bg-sd-ink hover:text-white transition-colors">‹</a>
      )}
      {pages.map((p, i) =>
        p === "…" ? (
          <span key={`ellipsis-${i}`} className="px-2 text-sd-ink/40">…</span>
        ) : (
          <a
            key={p}
            href={pageHref(p as number)}
            aria-current={p === page ? "page" : undefined}
            className={`px-2.5 py-1 border transition-colors ${
              p === page
                ? "bg-sd-ink text-white border-sd-ink"
                : "border-sd-ink text-sd-ink hover:bg-sd-ink hover:text-white"
            }`}
          >
            {p}
          </a>
        )
      )}
      {page < totalPages && (
        <a href={pageHref(page + 1)} aria-label="Next page" className="px-2.5 py-1 border border-sd-ink text-sd-ink hover:bg-sd-ink hover:text-white transition-colors">›</a>
      )}
    </nav>
  );
}

function buildPageRange(current: number, total: number): (number | "…")[] {
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
  const pages: (number | "…")[] = [1];
  if (current > 3) pages.push("…");
  for (let p = Math.max(2, current - 1); p <= Math.min(total - 1, current + 1); p++) pages.push(p);
  if (current < total - 2) pages.push("…");
  pages.push(total);
  return pages;
}