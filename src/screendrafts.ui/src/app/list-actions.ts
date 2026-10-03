// app/list-actions.ts
'use server';

// Later pages for the mobile infinite-scroll lists. Each action runs the same
// query parsing as its page (lib/list-queries.ts), so page N here is exactly what
// ?page=N would have rendered.
//
// Server actions are public POST endpoints. These only wrap the public list
// endpoints the pages already call, and list-queries clamps pageSize.

import { listDrafts } from "@/services/drafts/fetch-drafts";
import { fetchMedia } from "@/services/media/fetch-media";
import { listParticipants } from "@/services/participants/fetch-participants";
import { draftersQuery, draftsQuery, mediaQuery, type ListPage, type QueryRecord } from "@/lib/list-queries";
import type { ListDraftsResponse, MediaListItemResponse, ParticipantListItem } from "@/lib/dto";

export async function loadDraftsPage(query: QueryRecord, page: number): Promise<ListPage<ListDraftsResponse>> {
  const { pageSize, args } = draftsQuery(query);
  const result = await listDrafts({ ...args, page });
  return {
    items: result.items,
    total: result.total,
    totalPages: Math.ceil(result.total / pageSize),
  };
}

export async function loadMediaPage(query: QueryRecord, page: number): Promise<ListPage<MediaListItemResponse>> {
  const { args } = mediaQuery(query);
  const result = await fetchMedia({ ...args, page });
  return {
    items: result.items ?? [],
    total: result.totalCount ?? 0,
    totalPages: result.totalPages ?? 0,
  };
}

export async function loadDraftersPage(query: QueryRecord, page: number): Promise<ListPage<ParticipantListItem>> {
  const { pageSize, args } = draftersQuery(query);
  const result = await listParticipants({ ...args, page });
  return {
    items: result.items,
    total: result.total,
    totalPages: Math.ceil(result.total / pageSize),
  };
}
