import { ListDraftsResponse, PagedResultOfListDraftsResponse } from "../lib/dto";

export interface PagedResult<T> {
   items: T[];
   total: number;
   page: number;
   pageSize: number;
}

export function toPagedDraftResult(
   api: PagedResultOfListDraftsResponse
): PagedResult<ListDraftsResponse> {
   return {
      items: api.items ?? [],
      total: api.totalCount ?? 0,
      page: api.page ?? 1,
      pageSize: api.pageSize ?? 10,
   };
}
