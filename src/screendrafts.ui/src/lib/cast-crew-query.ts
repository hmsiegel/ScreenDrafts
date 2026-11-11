// lib/cast-crew-query.ts
//
// Turns the cast & crew page's search params into API arguments. Shared by the page
// (first page, rendered on the server) and app/cast-crew/actions.ts (later pages for
// infinite scroll), so both ask the API the same question.

import { asNumber, asString, type QueryRecord } from "@/lib/list-queries";

const MAX_PAGE_SIZE = 100;

export const CAST_CREW_ROLES = ["actor", "director", "writer", "producer"] as const;

export function castCrewQuery(qp: QueryRecord) {
  const page = Math.max(1, Math.floor(asNumber(qp.page) ?? 1));
  const pageSize = Math.min(MAX_PAGE_SIZE, Math.max(1, Math.floor(asNumber(qp.pageSize) ?? 48)));
  const sort = asString(qp.sort) ?? "name_asc";
  const search = asString(qp.q)?.trim() || undefined;
  const requestedRole = asString(qp.role) ?? "";
  const role = (CAST_CREW_ROLES as readonly string[]).includes(requestedRole) ? requestedRole : "";

  return {
    page,
    pageSize,
    sort,
    search,
    role,
    args: { pageSize, sort, search, role: role || undefined },
  };
}
