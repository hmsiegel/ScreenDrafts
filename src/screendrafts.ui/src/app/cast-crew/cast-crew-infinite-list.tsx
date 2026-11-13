// app/cast-crew/cast-crew-infinite-list.tsx
'use client';

import { useCallback } from "react";
import type { ReactNode } from "react";
import Link from "next/link";
import { InfiniteListFooter, useInfiniteList } from "@/components/ui/infinite-list";
import { loadCastCrewPage } from "./actions";
import type { MediaPersonListItemResponse } from "@/lib/dto";
import type { QueryRecord } from "@/lib/list-queries";

const ROLE_LABELS: Record<string, string> = {
  actor: "ACTOR",
  director: "DIRECTOR",
  writer: "WRITER",
  producer: "PRODUCER",
};

interface CastCrewInfiniteListProps {
  /** Must also be passed as this component's React key. */
  cacheKey: string;
  query: QueryRecord;
  sort: string;
  initialItems: MediaPersonListItemResponse[];
  initialPage: number;
  total: number;
  totalPages: number;
  paginator: ReactNode;
}

const personKey = (p: MediaPersonListItemResponse) => p.id;

export function CastCrewInfiniteList({
  cacheKey,
  query,
  sort,
  initialItems,
  initialPage,
  total,
  totalPages,
  paginator,
}: CastCrewInfiniteListProps) {
  const fetchPage = useCallback((page: number) => loadCastCrewPage(query, page), [query]);
  const list = useInfiniteList({
    cacheKey,
    initialItems,
    initialPage,
    total,
    totalPages,
    fetchPage,
    getKey: personKey,
  });

  return (
    <>
      {list.total === 0 ? (
        <div className="text-center font-mono text-sm text-sd-ink/50 py-16">
          No one matches.
        </div>
      ) : (
        <PeopleTable items={list.items} sort={sort} query={query} />
      )}
      <InfiniteListFooter
        shown={list.items.length}
        total={list.total}
        noun="PEOPLE"
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

type SortField = "name" | "titles";
const DEFAULT_DIRECTION: Record<SortField, "asc" | "desc"> = { name: "asc", titles: "desc" };

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

// From sm up: name, roles, titles, arrow. Below sm each row stacks: name on top,
// roles and title count beneath, arrow on the right.
const ROW_COLS = "grid-cols-[minmax(0,1fr)_24px] sm:grid-cols-[minmax(0,1fr)_minmax(0,260px)_90px_24px]";

function PeopleTable({
  items,
  sort,
  query,
}: {
  items: MediaPersonListItemResponse[];
  sort: string;
  query: QueryRecord;
}) {
  return (
    <div className="bg-white border-2 border-sd-ink">
      {/* Phones: sort strip in place of the column headers */}
      <div className="sm:hidden sticky top-0 z-20 flex items-center gap-x-5 bg-sd-ink text-white font-mono text-[10px] tracking-wide px-4 py-1">
        <span className="text-white/40">SORT</span>
        <SortLink field="name" label="NAME" sort={sort} query={query} className="py-2" />
        <SortLink field="titles" label="TITLES" sort={sort} query={query} className="py-2" />
      </div>

      {/* sm+: column headers, NAME and TITLES sortable */}
      <div className={`hidden sm:grid ${ROW_COLS} sticky top-0 z-20 bg-sd-ink text-white font-mono text-[10px] tracking-wide`}>
        <div className="px-4 py-3">
          <SortLink field="name" label="NAME" sort={sort} query={query} />
        </div>
        <div className="px-4 py-3 text-white/60">ROLES</div>
        <div className="px-4 py-3">
          <SortLink field="titles" label="TITLES" sort={sort} query={query} />
        </div>
        <div className="px-2 py-3" />
      </div>

      {items.map((person) => (
        <PersonRow key={person.id} person={person} />
      ))}
    </div>
  );
}

function PersonRow({ person }: { person: MediaPersonListItemResponse }) {
  return (
    <Link
      href={`/cast-crew/${encodeURIComponent(person.id)}`}
      className={`group grid ${ROW_COLS} border-t border-sd-ink/10 hover:bg-sd-paper transition-colors duration-100 cursor-pointer`}
    >
      <div className="px-4 pt-4 pb-1.5 sm:py-4 self-center overflow-hidden flex items-center gap-3">
        <span
          aria-hidden="true"
          className="hidden sm:flex shrink-0 w-8 h-8 rounded-full bg-sd-ink/10 text-sd-ink/60 items-center justify-center font-oswald text-[12px]"
        >
          {initials(person.name)}
        </span>
        <span className="font-oswald font-semibold text-[17px] text-sd-ink group-hover:text-sd-red transition-colors line-clamp-2 wrap-anywhere sm:line-clamp-none sm:truncate">
          {person.name}
        </span>
      </div>

      {/* Below sm: a meta line under the name. From sm `contents` returns roles and
          titles to their own columns. */}
      <div className="col-start-1 row-start-2 flex flex-wrap items-center gap-x-3 gap-y-1.5 px-4 pb-4 sm:contents">
        <div className="sm:px-4 sm:py-4 sm:self-center flex flex-wrap gap-1">
          {person.roles.map((role) => (
            <span
              key={role}
              className="inline-block font-mono text-[9px] tracking-widest px-2 py-0.5 rounded-xs bg-sd-blue/10 text-sd-blue"
            >
              {ROLE_LABELS[role] ?? role.toUpperCase()}
            </span>
          ))}
        </div>

        <div className="sm:px-4 sm:py-4 sm:self-center font-mono text-[12px] text-sd-ink/60">
          {person.titleCount} {person.titleCount === 1 ? "title" : "titles"}
        </div>
      </div>

      <div className="col-start-2 row-span-2 row-start-1 sm:col-start-auto sm:row-span-1 sm:row-start-auto px-2 py-4 self-center text-sd-ink/30 group-hover:text-sd-red transition-colors text-center">
        ›
      </div>
    </Link>
  );
}

function initials(name: string): string {
  const parts = name.trim().split(/\s+/).filter(Boolean);
  if (parts.length === 0) return "?";
  const first = parts[0][0];
  const last = parts.length > 1 ? parts[parts.length - 1][0] : "";
  return (first + last).toUpperCase();
}
