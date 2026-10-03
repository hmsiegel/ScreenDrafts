// lib/list-queries.ts
//
// Turns a list page's search params into API arguments. Shared by the pages, which
// render the first page on the server, and app/list-actions.ts, which fetches later
// pages for infinite scroll, so both ask the API exactly the same question.
//
// Plain module: no 'use client', no 'use server'. Server pages and client components
// can both call it.

export type QueryRecord = Record<string, string | string[] | undefined>;

/** One page of a list, as the infinite-scroll actions return it. */
export interface ListPage<T> {
  items: T[];
  total: number;
  totalPages: number;
}

// The actions are public endpoints, so a caller can send any pageSize. Clamp it.
const MAX_PAGE_SIZE = 100;

export function asNumber(v: string | string[] | undefined): number | undefined {
  if (!v) return undefined;
  const n = Number(Array.isArray(v) ? v[0] : v);
  return isNaN(n) ? undefined : n;
}

export function asString(v: string | string[] | undefined): string | undefined {
  if (!v) return undefined;
  return Array.isArray(v) ? v[0] : v;
}

export function asStringArray(v: string | string[] | undefined): string[] {
  if (!v) return [];
  return Array.isArray(v) ? v : [v];
}

function pageOf(qp: QueryRecord): number {
  return Math.max(1, Math.floor(asNumber(qp.page) ?? 1));
}

function pageSizeOf(qp: QueryRecord, fallback: number): number {
  return Math.min(MAX_PAGE_SIZE, Math.max(1, Math.floor(asNumber(qp.pageSize) ?? fallback)));
}

/**
 * Identifies one list view: scope plus every param, page included, in a stable order.
 * Used as the React key that remounts the infinite list when filters, sort or page
 * change, and as its sessionStorage key.
 */
export function listCacheKey(scope: string, qp: QueryRecord): string {
  const qs = new URLSearchParams();
  Object.keys(qp)
    .sort()
    .forEach((k) => asStringArray(qp[k]).forEach((v) => qs.append(k, v)));
  return `${scope}?${qs.toString()}`;
}

// ── Drafts ────────────────────────────────────────────────────────────────

export function draftsQuery(qp: QueryRecord) {
  const pageSize = pageSizeOf(qp, 25);
  return {
    page: pageOf(qp),
    pageSize,
    args: {
      q: asString(qp.q),
      fromDate: asString(qp.fromDate),
      toDate: asString(qp.toDate),
      draftType: asNumber(qp.draftType),
      minDrafters: asNumber(qp.minDrafters),
      maxDrafters: asNumber(qp.maxDrafters),
      pageSize,
      campaignPublicId: asString(qp.campaignPublicId),
      categoryPublicIds: asStringArray(qp.categoryPublicIds),
      sort: asString(qp.sort) ?? "date",
      dir: (asString(qp.dir) ?? "desc") as "asc" | "desc",
    },
  };
}

// ── Media ─────────────────────────────────────────────────────────────────

export function mediaQuery(qp: QueryRecord) {
  const pageSize = pageSizeOf(qp, 50);
  const sort = asString(qp.sort) ?? "title_asc";
  const search = asString(qp.q);
  const mediaType = asNumber(qp.mediaType);
  const year = asString(qp.year);
  return {
    page: pageOf(qp),
    pageSize,
    sort,
    search,
    mediaType,
    year,
    args: { pageSize, sort, search, mediaType, year },
  };
}

// ── Drafters ──────────────────────────────────────────────────────────────

export function draftersQuery(qp: QueryRecord) {
  const pageSize = pageSizeOf(qp, 24);
  const sort = asString(qp.sort) ?? "name";
  const q = asString(qp.q);
  const honorific = asString(qp.honorific) ?? "";
  // An honorific filter overrides the role tabs — honorifics are GM-only.
  const filter = honorific ? "all" : (asString(qp.filter) ?? "all");

  // Map UI filter tab → API role param
  const role =
    honorific ? undefined
      : filter === "commissioners" ? "commissioner"
      : filter === "gms" ? "gm"
      : filter === "hosts" ? "host"
      : undefined;

  return {
    page: pageOf(qp),
    pageSize,
    sort,
    q,
    honorific,
    filter,
    args: { q, role, sort, pageSize, honorific: honorific || undefined },
  };
}
