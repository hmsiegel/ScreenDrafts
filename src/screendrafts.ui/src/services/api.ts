import { auth } from '@/auth';
import { env } from '@/lib/env';

const apiBaseUrl = env.apiUrl;

/* ------------------------generic signed fetch------------------------ */
async function signedFetch(url: string, init: RequestInit = {}) {
   const session = await auth();
   const headers = new Headers(init.headers);

   if (!session) {
      throw new Error("No session found");
   }

   if (session?.accessToken && !headers.has("Authorization")) {
      headers.set("Authorization", `Bearer ${session.accessToken}`);
   }

   const res = await fetch(url, {
      ...init,
      headers,
      credentials: "include",
      next: { revalidate: 0 },
   });

   if (!res.ok) {
      const body = await res.text();
      throw new Error(
         `Request failed with status ${res.status}: ${res.statusText} - ${body}`);
   }
   return res.json();
}

export async function apiRequest<T = unknown>(
   path: string,
   init: RequestInit = {}
): Promise<T> {
   return signedFetch(`${apiBaseUrl}${path}`, init);
}

/* ------------------------ unauthenticated fetch ------------------------ */

async function publicFetch(url: string, init: RequestInit = {}) {
   const headers = new Headers(init.headers);

   const res = await fetch(url, {
      ...init,
      headers,
      next: { revalidate: 0 },
   });

   if (!res.ok) {
      const body = await res.text();
      throw new Error(
         `Request failed with status ${res.status}: ${res.statusText} - ${body}`);
   }
   return res.json();
}

export async function publicApiRequest<T = unknown>(
   path: string,
   init: RequestInit = {}
): Promise<T> {
   return publicFetch(`${apiBaseUrl}${path}`, init);
}