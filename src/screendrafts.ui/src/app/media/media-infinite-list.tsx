// app/media/media-infinite-list.tsx
'use client';

import { useCallback } from "react";
import type { ReactNode } from "react";
import Link from "next/link";
import { InfiniteListFooter, useInfiniteList } from "@/components/ui/infinite-list";
import { loadMediaPage } from "@/app/list-actions";
import type { MediaListItemResponse } from "@/lib/dto";
import type { QueryRecord } from "@/lib/list-queries";

const MEDIA_TYPE_LABELS: Record<number, string> = {
  0: "Movie",
  1: "TV Show",
  2: "TV Episode",
  3: "Video Game",
  4: "Music Video",
};

interface MediaInfiniteListProps {
  /** Must also be passed as this component's React key. */
  cacheKey: string;
  query: QueryRecord;
  sort: string;
  initialItems: MediaListItemResponse[];
  initialPage: number;
  total: number;
  totalPages: number;
  paginator: ReactNode;
}

const mediaKey = (m: MediaListItemResponse) => m.publicId ?? "";

export function MediaInfiniteList({
  cacheKey,
  query,
  sort,
  initialItems,
  initialPage,
  total,
  totalPages,
  paginator,
}: MediaInfiniteListProps) {
  const fetchPage = useCallback((page: number) => loadMediaPage(query, page), [query]);
  const list = useInfiniteList({
    cacheKey,
    initialItems,
    initialPage,
    total,
    totalPages,
    fetchPage,
    getKey: mediaKey,
  });

  return (
    <>
      {list.total === 0 ? (
        <div className="text-center font-mono text-sm text-sd-ink/50 py-16">
          Nothing in the Vault yet.
        </div>
      ) : (
        <MediaTable items={list.items} sort={sort} query={query} />
      )}
      <InfiniteListFooter
        shown={list.items.length}
        total={list.total}
        noun="TITLES"
        status={list.status}
        onRetry={list.loadMore}
        sentinelRef={list.sentinelRef}
        paginator={paginator}
        desktopShown={initialItems.length}
      />
    </>
  );
}

// ── Sorting ───────────────────────────────────────────────────────────────

// Sort values the API accepts. Each sortable column flips between its two.
type SortField = "title" | "year";
const DEFAULT_DIRECTION: Record<SortField, "asc" | "desc"> = { title: "asc", year: "desc" };

function sortHref(field: SortField, currentSort: string, query: QueryRecord): string {
  const [currentField, currentDir] = currentSort.split("_");
  const nextDir =
    currentField === field ? (currentDir === "asc" ? "desc" : "asc") : DEFAULT_DIRECTION[field];

  const qs = new URLSearchParams();
  for (const [key, value] of Object.entries(query)) {
    if (key === "sort" || key === "page" || value === undefined) continue;
    if (Array.isArray(value)) value.forEach((v) => qs.append(key, v));
    else qs.set(key, value);
  }
  qs.set("sort", `${field}_${nextDir}`);
  qs.set("page", "1");
  return `?${qs.toString()}`;
}

function SortLink({
  field,
  label,
  sort,
  query,
  className = "",
}: {
  field: SortField;
  label: string;
  sort: string;
  query: QueryRecord;
  className?: string;
}) {
  const [currentField, currentDir] = sort.split("_");
  const isActive = currentField === field;
  const indicator = isActive ? (currentDir === "asc" ? " ↑" : " ↓") : " ↕";

  return (
    <Link
      href={sortHref(field, sort, query)}
      className={`flex items-center gap-1 hover:text-light-blue transition-colors ${
        isActive ? "text-white" : "text-white/60"
      } ${className}`}
    >
      {label}
      <span className={`text-[9px] ${isActive ? "text-sd-red" : "text-white/30"}`}>{indicator}</span>
    </Link>
  );
}

// ── Table ─────────────────────────────────────────────────────────────────

// From sm up: the four-column table. Below sm each row stacks into a card —
// title on top (up to two lines), year and type beneath, arrow on the right.
const ROW_COLS = "grid-cols-[minmax(0,1fr)_24px] sm:grid-cols-[minmax(0,1fr)_80px_120px_24px]";

function MediaTable({
  items,
  sort,
  query,
}: {
  items: MediaListItemResponse[];
  sort: string;
  query: QueryRecord;
}) {
  return (
    <div className="bg-white border-2 border-sd-ink">
      {/* Phones: sort strip in place of the column headers. Both pin to the top of the
          screen while the rows scroll under them. */}
      <div className="sm:hidden sticky top-0 z-20 flex items-center gap-x-5 bg-sd-ink text-white font-mono text-[10px] tracking-wide px-4 py-1">
        <span className="text-white/40">SORT</span>
        <SortLink field="title" label="TITLE" sort={sort} query={query} className="py-2" />
        <SortLink field="year" label="YEAR" sort={sort} query={query} className="py-2" />
      </div>

      {/* sm+: column headers, TITLE and YEAR sortable */}
      <div className={`hidden sm:grid ${ROW_COLS} sticky top-0 z-20 bg-sd-ink text-white font-mono text-[10px] tracking-wide`}>
        <div className="px-4 py-3">
          <SortLink field="title" label="TITLE" sort={sort} query={query} />
        </div>
        <div className="px-4 py-3">
          <SortLink field="year" label="YEAR" sort={sort} query={query} />
        </div>
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
  const isMovie = item.mediaTypeValue === 0;

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
