import { env } from '@/lib/env';

const apiBaseUrl = env.apiUrl;

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