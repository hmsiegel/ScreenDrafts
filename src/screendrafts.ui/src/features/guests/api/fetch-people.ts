import { auth } from "@/auth";
import { GetDrafterProfileResponse, PagedResultOfPersonResponse, PersonResponse } from "@/lib/dto";
import { env } from "@/lib/env";
import { PagedResult, toPagedPeopleResult } from "@/types/paged-result";

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
   const apiPaged = data.value as PagedResultOfPersonResponse;
   return toPagedPeopleResult(apiPaged);
}

export async function getDrafterProfile(id: string): Promise<GetDrafterProfileResponse> {
   const url = `${apiBase}/drafters/${id}/profile`;

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

   return response.json() as Promise<GetDrafterProfileResponse>;
}
