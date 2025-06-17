import { DraftResponse, PagedResultOfDraftResponse } from "../dto";

export interface PagedResult<T> {
   items: T[];
   total: number;
   page: number;
   pageSize: number;
}

export function toPagedDraftResult(
   api: PagedResultOfDraftResponse
) : PagedResult<DraftResponse> {
   return api;
}