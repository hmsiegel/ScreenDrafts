// app/media/page.tsx
import MediaFilterStrip from "@/components/features/media/media-filter-strip";
import { fetchMedia } from "@/services/media/fetch-media";
import { MediaListItemResponse } from "@/lib/dto";
import { Metadata } from "next";
import { Suspense } from "react";
import Link from "next/link";

export const metadata: Metadata = {
  title: "The Vault",
  description: "Every film, series, and game that has ever touched a ScreenDrafts board.",
};

export const dynamic = "force-dynamic";

type SearchParams = Promise<{ [key: string]: string | string[] | undefined }>;

function asNumber(v: string | string[] | undefined): number | undefined {
  if (!v) return undefined;
  const n = Number(Array.isArray(v) ? v[0] : v);
  return isNaN(n) ? undefined : n;
}

function asString(v: string | string[] | undefined): string | undefined {
  if (!v) return undefined;
  return Array.isArray(v) ? v[0] : v;
}

const MEDIA_TYPE_LABELS: Record<number, string> = {
  0: "Movie",
  1: "TV Show",
  2: "TV Episode",
  3: "Video Game",
  4: "Music Video",
};

export default async function MediaPage(props: { searchParams: SearchParams }) {
  const qp = await props.searchParams;

  const page     = asNumber(qp.page)    ?? 1;
  const pageSize = asNumber(qp.pageSize) ?? 50;
  const sort     = asString(qp.sort)    ?? "title_asc";
  const search   = asString(qp.q);
  const mediaType = asNumber(qp.mediaType);
  const year     = asString(qp.year);

  const result = await fetchMedia({ page, pageSize, sort, search, mediaType, year });
  const totalPages = result.totalPages ?? 0;

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

      {/* Table */}
      <div className="page-x pt-0 pb-16">
        {result.totalCount === 0 ? (
          <div className="text-center font-mono text-sm text-sd-ink/50 py-16">
            Nothing in the Vault yet.
          </div>
        ) : (
          <MediaTable items={result.items} />
        )}

        {/* Pagination — pager above the count on phones so it sits under the thumb. */}
        <div className="flex flex-col-reverse items-start gap-3 sm:flex-row sm:items-center sm:justify-between mt-4">
          <span className="font-mono text-[11px] text-sd-ink/60">
            SHOWING {result.items.length} OF {result.totalCount.toLocaleString("en-US")} TITLES
          </span>
          {totalPages > 1 && (
            <Paginator page={page} totalPages={totalPages} searchParams={qp} />
          )}
        </div>
      </div>

    </div>
  );
}

// ── Table ─────────────────────────────────────────────────────────────────────

// From sm up: the original four-column table. Below sm each row stacks into a card —
// title on top (up to two lines), year and type beneath, arrow on the right. The
// table's fixed columns take 224px, which on a phone left the title ~115px.
const ROW_COLS = "grid-cols-[minmax(0,1fr)_24px] sm:grid-cols-[minmax(0,1fr)_80px_120px_24px]";

function MediaTable({ items }: { items: MediaListItemResponse[] }) {
  return (
    <div className="bg-white border-2 border-sd-ink border-t-0">
      {/* Header — the cards need none. Sorting lives in the filter strip. */}
      <div className={`hidden sm:grid ${ROW_COLS} bg-sd-ink text-white font-mono text-[10px] tracking-wide`}>
        <div className="px-4 py-3 text-white/60">TITLE</div>
        <div className="px-4 py-3 text-white/60">YEAR</div>
        <div className="px-4 py-3 text-white/60">TYPE</div>
        <div className="px-2 py-3" />
      </div>

      {items.map((item) => (
        <MediaRow key={item.publicId} item={item} />
      ))}
    </div>
  );
}

function MediaRow({ item }: { item: MediaListItemResponse }) {
  const typeLabel = MEDIA_TYPE_LABELS[item.mediaTypeValue] ?? item.mediaTypeName;
  const isMovie   = item.mediaTypeValue === 0;

  return (
    <Link
      href={`/media/${item.publicId}`}
      className={`group grid ${ROW_COLS} border-t border-sd-ink/10 hover:bg-sd-paper transition-colors duration-100 cursor-pointer`}
    >
      {/* No `block` here: Tailwind emits it after line-clamp's -webkit-box and would cancel the clamp. */}
      <div className="px-4 pt-4 pb-1.5 sm:py-4 self-center overflow-hidden">
        <span className="font-oswald font-semibold text-[17px] text-sd-ink group-hover:text-sd-red transition-colors line-clamp-2 [overflow-wrap:anywhere] sm:line-clamp-none sm:truncate">
          {item.title}
        </span>
      </div>

      {/* Below sm this wrapper is a meta line under the title; from sm `contents`
          dissolves it so year and type fall back into their own table columns. */}
      <div className="col-start-1 row-start-2 flex items-center gap-3 px-4 pb-4 sm:contents">
        <div className="sm:px-4 sm:py-4 sm:self-center font-mono text-[12px] text-sd-ink/60">
          {item.year ?? "—"}
        </div>

        <div className="sm:px-4 sm:py-4 sm:self-center">
          <span className={`inline-block font-mono text-[9px] tracking-widest px-2 py-0.5 rounded-sm ${
            isMovie ? "bg-sd-blue/10 text-sd-blue" : "bg-sd-ink/10 text-sd-ink/60"
          }`}>
            {typeLabel}
          </span>
        </div>
      </div>

      <div className="col-start-2 row-span-2 row-start-1 sm:col-start-auto sm:row-span-1 sm:row-start-auto px-2 py-4 self-center text-sd-ink/30 group-hover:text-sd-red transition-colors text-center">
        ›
      </div>
    </Link>
  );
}

// ── Pagination ────────────────────────────────────────────────────────────────

function Paginator({
  page,
  totalPages,
  searchParams,
}: {
  page: number;
  totalPages: number;
  searchParams: { [key: string]: string | string[] | undefined };
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

  // 36px square cells below sm (touch); original compact cells from sm up.
  const cell =
    "inline-flex items-center justify-center min-w-9 h-9 sm:min-w-0 sm:h-auto px-2.5 sm:py-1 border transition-colors";

  return (
    <nav aria-label="Pagination" className="flex flex-wrap items-center gap-1 font-mono text-[11px]">
      {page > 1 && (
        <a href={pageHref(page - 1)} aria-label="Previous page" className={`${cell} border-sd-ink text-sd-ink hover:bg-sd-ink hover:text-white`}>‹</a>
      )}
      {pages.map((p, i) =>
        p === "…" ? (
          <span key={`ellipsis-${i}`} className="px-2 text-sd-ink/40">…</span>
        ) : (
          <a
            key={p}
            href={pageHref(p as number)}
            aria-current={p === page ? "page" : undefined}
            className={`${cell} ${
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
        <a href={pageHref(page + 1)} aria-label="Next page" className={`${cell} border-sd-ink text-sd-ink hover:bg-sd-ink hover:text-white`}>›</a>
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