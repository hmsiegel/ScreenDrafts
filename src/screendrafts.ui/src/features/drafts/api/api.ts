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

// TODO: re-map to correct endpoints after DraftsClient was removed in dto regen.
// The new Client uses drafts_SetDraftPartStatus / drafts_UpdateDraft.
export const startDraft  = (draftId: string): Promise<void> =>
   apiRequest(`/drafts/${draftId}/start`,    { method: 'POST' });

export const playDraft   = (draftId: string): Promise<void> =>
   apiRequest(`/drafts/${draftId}/continue`, { method: 'POST' });

export const editDraft   = (draftId: string): Promise<void> =>
   apiRequest(`/drafts/${draftId}`,          { method: 'PUT'  });

export const deleteDraft = (draftId: string): Promise<void> =>
   apiRequest(`/drafts/${draftId}`,          { method: 'DELETE' });
