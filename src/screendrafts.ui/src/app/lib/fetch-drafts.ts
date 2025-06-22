import { getServerSession } from "next-auth";
import { authOptions } from "../api/auth/[...nextauth]/route";
import { DraftResponse, PagedResultOfDraftResponse, UpcomingDraftDto } from "./dto";
import { env } from "./env";
import { PagedResult, toPagedDraftResult } from "./types/paged-result";

const apiBase = env.apiUrl;

export async function getLatestDrafts(): Promise<DraftResponse[]> {
   const url = `${apiBase}/drafts/latest`;

   const session = await getServerSession(authOptions);
   const headers: HeadersInit = {};

   if (session?.accessToken) {
      headers["Authorization"] = `Bearer ${session.accessToken}`;
   }

   const response = await fetch(url, {
      method: "GET",
      headers,
      credentials: "include",
      next: { revalidate: 0 }, // Disable caching
   });

   if (!response.ok) {
      const body = await response.text();
      throw new Error(
         `Request failed with status ${response.status}: ${response.statusText} - ${body}`
      );
   }

   return response.json() as Promise<DraftResponse[]>;
}

export async function getUpcomingDrafts(): Promise<UpcomingDraftDto[]> {
   const url = `${apiBase}/drafts/upcoming`;

   const session = await getServerSession(authOptions);
   const headers: HeadersInit = {};

   if (session?.accessToken) {
      headers["Authorization"] = `Bearer ${session.accessToken}`;
   }

   try {
      const response = await fetch(url, {
         method: "GET",
         headers,
         credentials: "include",
         next: { revalidate: 0 }, // Disable caching
      });

      if (!response.ok) {
         const body = await response.text();
         throw new Error(
            `Request failed with status ${response.status}: ${response.statusText} - ${body}`
         );
      }

      return response.json() as Promise<UpcomingDraftDto[]>;
   } catch (error) {
      console.error("Error fetching upcoming drafts:", error);
      throw error; // Re-throw the error for further handling
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
   dir?: "asc" | "desc",
   page?: number,
   pageSize?: number,
} = {}): Promise<PagedResult<DraftResponse>> {
   const url = new URL(`${apiBase}/drafts`);
   Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== "") {
         url.searchParams.set(key, String(value));
      }
   });

   const session = await getServerSession(authOptions);
   const headers: HeadersInit = {};

   if (session?.accessToken) {
      headers["Authorization"] = `Bearer ${session.accessToken}`;
   }

   const response = await fetch(url, {
      method: "GET",
      headers,
      credentials: "include",
      next: { revalidate: 0 }, // Disable caching
   });

   if (!response.ok) {
      const body = await response.text();
      throw new Error(
         `Request failed with status ${response.status}: ${response.statusText} - ${body}`
      );
   }

   //return response.json() as Promise<DraftResponse[]>;
   const data = await response.json();
   const apiPaged = data.value as PagedResultOfDraftResponse;
   return toPagedDraftResult(apiPaged);
}