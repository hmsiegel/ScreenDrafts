import { auth } from "@/auth";
import {
  GetDraftResponse,
  LatestDraftResponse,
  ListLatestDraftsResponse,
  ListUpcomingDraftsResponse,
  PagedResultOfListDraftsResponse,
  UpcomingDraftResponse,
} from "@/lib/dto";
import { env } from "@/lib/env";
import { PagedResult, toPagedDraftResult } from "@/types/paged-result";

const apiBase = env.apiUrl;

export async function getLatestDrafts(): Promise<LatestDraftResponse[]> {
   const url = `${apiBase}/drafts/latest`;

   const session = await auth();
   const headers: HeadersInit = {};

   if (session?.accessToken) {
      headers["Authorization"] = `Bearer ${session.accessToken}`;
   }

   const response = await fetch(url, {
      method: "GET",
      headers,
      credentials: "include",
      next: { revalidate: 0 },
   });

   if (!response.ok) {
      const body = await response.text();
      throw new Error(
         `Request failed with status ${response.status}: ${response.statusText} - ${body}`
      );
   }

   const data = await response.json() as ListLatestDraftsResponse;
   return data.drafts ?? [];
}

export async function getUpcomingDrafts(): Promise<UpcomingDraftResponse[]> {
   const url = `${apiBase}/drafts/upcoming`;

   const session = await auth();
   const headers: HeadersInit = {};

   if (session?.accessToken) {
      headers["Authorization"] = `Bearer ${session.accessToken}`;
   }

   try {
      const response = await fetch(url, {
         method: "GET",
         headers,
         credentials: "include",
         next: { revalidate: 0 },
      });

      if (!response.ok) {
         const body = await response.text();
         throw new Error(
            `Request failed with status ${response.status}: ${response.statusText} - ${body}`
         );
      }

      const data = await response.json() as ListUpcomingDraftsResponse;
      return data.drafts ?? [];
   } catch (error) {
      console.error("Error fetching upcoming drafts:", error);
      throw error;
   }
}

export async function listDrafts(params: {
   fromDate?: string,
   toDate?: string,
   minDrafters?: number,
   maxDrafters?: number,
   minPicks?: number,
   maxPicks?: number,
   draftType?: number[],
   sort?: string | undefined,
   q?: string | undefined,
   dir?: "asc" | "desc",
   page?: number,
   pageSize?: number,
} = {}): Promise<PagedResult<LatestDraftResponse>> {
   const url = new URL(`${apiBase}/drafts`);
   Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== "") {
         url.searchParams.set(key, String(value));
      }
   });

   const session = await auth();
   const headers: HeadersInit = {};

   if (session?.accessToken) {
      headers["Authorization"] = `Bearer ${session.accessToken}`;
   }

   const response = await fetch(url, {
      method: "GET",
      headers,
      credentials: "include",
      next: { revalidate: 0 },
   });

   if (!response.ok) {
      const body = await response.text();
      throw new Error(
         `Request failed with status ${response.status}: ${response.statusText} - ${body}`
      );
   }

   const data = await response.json();
   const apiPaged = data as PagedResultOfListDraftsResponse;
   return toPagedDraftResult(apiPaged) as PagedResult<LatestDraftResponse>;
}

export async function getDraftDetails(id: string): Promise<GetDraftResponse> {
   const url = `${apiBase}/drafts/${id}`;

   const session = await auth();
   const headers: HeadersInit = {};

   if (session?.accessToken) {
      headers["Authorization"] = `Bearer ${session.accessToken}`;
   }

   const response = await fetch(url, {
      method: "GET",
      headers,
      credentials: "include",
      next: { revalidate: 0 },
   });

   if (!response.ok) {
      const body = await response.text();
      throw new Error(
         `Request failed with status ${response.status}: ${response.statusText} - ${body}`
      );
   }

   return response.json() as Promise<GetDraftResponse>;
}
