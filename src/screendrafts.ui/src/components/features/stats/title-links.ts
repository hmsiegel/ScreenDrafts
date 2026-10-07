// components/features/stats/title-links.ts
import type { TitleSort } from "@/services/stats/stats-types";

export interface TitleListParams {
  q?: string;
  sort?: TitleSort;
  page?: number;
  includeAll?: boolean;
}

/** Builds a /stats/titles/{level} link that keeps the search, sort and scope. Defaults are left out of the URL. */
export function titleListHref(level: string, params: TitleListParams = {}): string {
  const search = new URLSearchParams();
  if (params.q) search.set("q", params.q);
  if (params.sort && params.sort !== "newest") search.set("sort", params.sort);
  if (params.page && params.page > 1) search.set("page", String(params.page));
  if (params.includeAll) search.set("scope", "all");

  const qs = search.toString();
  return `/stats/titles/${level}${qs ? `?${qs}` : ""}`;
}
