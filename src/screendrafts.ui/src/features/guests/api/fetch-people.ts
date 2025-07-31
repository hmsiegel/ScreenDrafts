import { getServerSession } from "next-auth";
import { DrafterResponse, DraftResponse, PagedResultOfPersonResponse, PersonResponse } from "./dto";
import { env } from "./env";
import { PagedResult, toPagedPeopleResult } from "./types/paged-result";
import { authOptions } from "../api/auth/[...nextauth]/route";

const apiBase = env.apiUrl;

export async function listGuests(params: {
   sort?: string | undefined,
   q?: string | undefined,
   dir?: "asc" | "desc",
   page?: number,
   pageSize?: number
} = {}): Promise<PagedResult<PersonResponse>> {
   const url = new URL(`${apiBase}/people`);
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
   const apiPaged = data.value as PagedResultOfPersonResponse;
   return toPagedPeopleResult(apiPaged);
}

export async function getDrafterDetails(id: string): Promise<DrafterResponse[]> {
   const url = `${apiBase}/drafters/${id}`;

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

   return response.json() as Promise<DrafterResponse[]>;
}
