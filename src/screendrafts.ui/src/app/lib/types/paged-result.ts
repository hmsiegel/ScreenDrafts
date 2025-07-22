import { DraftResponse, PagedResultOfDraftResponse, PagedResultOfPersonResponse, PersonResponse } from "../dto";

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

export function toPagedPeopleResult(
   api: PagedResultOfPersonResponse
) : PagedResult<PersonResponse> {
   return api;
}