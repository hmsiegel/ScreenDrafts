// app/cast-crew/actions.ts
'use server';

// Later pages for the mobile infinite-scroll cast & crew list. Runs the same query
// parsing as the page. A public POST endpoint wrapping a public list endpoint;
// castCrewQuery clamps pageSize.

import { castCrewQuery } from "@/lib/cast-crew-query";
import type { ListPage, QueryRecord } from "@/lib/list-queries";
import type { MediaPersonListItemResponse } from "@/lib/dto";
import { fetchMediaPeople } from "@/services/media/fetch-media-people";

export async function loadCastCrewPage(
  query: QueryRecord,
  page: number
): Promise<ListPage<MediaPersonListItemResponse>> {
  const { args } = castCrewQuery(query);
  const result = await fetchMediaPeople({ ...args, page });
  return {
    items: result.items ?? [],
    total: result.totalCount ?? 0,
    totalPages: result.totalPages ?? 0,
  };
}
