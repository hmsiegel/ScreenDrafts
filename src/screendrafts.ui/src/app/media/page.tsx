import MediaFilterStrip from "@/components/features/media/media-filter-strip";
import SiteFooter from "@/components/layout/footer/site-footer";
import SiteHeader from "@/components/layout/header/site-header";
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
      <SiteHeader activePath="/media" />

      {/* Banner */}
      <div className="bg-sd-ink text-white" style={{ padding: "56px 40px 44px" }}>
        <p className="font-mono text-[11px] tracking-widest text-light-blue mb-3">/ MEDIA</p>
        <div className="flex items-end justify-between gap-8">
          <h1 className="font-oswald font-bold text-[72px] leading-[0.95] text-white">THE VAULT</h1>
          <p className="font-serif italic text-[17px] leading-relaxed text-white/70 max-w-[480px] text-right">
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
      <div className="px-10 pt-0 pb-16">
        {result.totalCount === 0 ? (
          <div className="text-center font-mono text-sm text-sd-ink/50 py-16">
            Nothing in the Vault yet.
          </div>
        ) : (
          <MediaTable items={result.items} />
        )}

        <div className="flex items-center justify-between mt-4">
          <span className="font-mono text-[11px] text-sd-ink/60">
            SHOWING {result.items.length} OF {result.totalCount.toLocaleString("en-US")} TITLES
          </span>
          {totalPages > 1 && (
            <Paginator page={page} totalPages={totalPages} searchParams={qp} />
          )}
        </div>
      </div>

      <SiteFooter />
    </div>
  );
}

// ── Table ─────────────────────────────────────────────────────────────────────

function MediaTable({ items }: { items: MediaListItemResponse[] }) {
  return (
    <div className="bg-white border-2 border-sd-ink border-t-0">
      {/* Header */}
      <div className="grid bg-sd-ink text-white font-mono text-[10px] tracking-wide"
        style={{ gridTemplateColumns: "1fr 80px 120px 24px" }}>
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
      className="group grid border-t border-sd-ink/10 hover:bg-sd-paper transition-colors duration-100 cursor-pointer"
      style={{ gridTemplateColumns: "1fr 80px 120px 24px" }}
    >
      <div className="px-4 py-4 self-center overflow-hidden">
        <span className="block font-oswald font-semibold text-[17px] text-sd-ink group-hover:text-sd-red transition-colors truncate">
          {item.title}
        </span>
      </div>

      <div className="px-4 py-4 self-center font-mono text-[12px] text-sd-ink/60">
        {item.year ?? "—"}
      </div>

      <div className="px-4 py-4 self-center">
        <span className={`inline-block font-mono text-[9px] tracking-widest px-2 py-0.5 rounded-sm ${
          isMovie ? "bg-sd-blue/10 text-sd-blue" : "bg-sd-ink/10 text-sd-ink/60"
        }`}>
          {typeLabel}
        </span>
      </div>

      <div className="px-2 py-4 self-center text-sd-ink/30 group-hover:text-sd-red transition-colors text-center">
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

  return (
    <nav className="flex items-center gap-1 font-mono text-[11px]">
      {page > 1 && (
        <a href={pageHref(page - 1)} className="px-2.5 py-1 border border-sd-ink text-sd-ink hover:bg-sd-ink hover:text-white transition-colors">‹</a>
      )}
      {pages.map((p, i) =>
        p === "…" ? (
          <span key={`ellipsis-${i}`} className="px-2 text-sd-ink/40">…</span>
        ) : (
          <a
            key={p}
            href={pageHref(p as number)}
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
        <a href={pageHref(page + 1)} className="px-2.5 py-1 border border-sd-ink text-sd-ink hover:bg-sd-ink hover:text-white transition-colors">›</a>
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