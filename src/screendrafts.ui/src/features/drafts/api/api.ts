import { authOptions } from '@/app/api/auth/[...nextauth]/route';
import { ContinueDraftRequest, DeleteDraftRequest, DraftsClient, EditDraftRequest } from '@/lib/dto';
import { env } from '@/lib/env';
import { getServerSession } from 'next-auth';

const apiBaseUrl = env.apiUrl;


/* ------------------------generic signed fetch------------------------ */
async function signedFetch(url: string , init: RequestInit = {}) {
   const session = await getServerSession(authOptions);
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
      next: { revalidate: 0 }, // Disable caching
   });

   if (!res.ok) {
      const body = await res.text();
      throw new Error(
         `Request failed with status ${res.status}: ${res.statusText} - ${body}`);
   }
   return res.json();
}

const draftsClient = new DraftsClient(
   apiBaseUrl,
   {
      fetch: (url, init) => signedFetch(url.toString(), init),
   }
);

export const startDraft = (draftId: string) => draftsClient.startDraft({ draftId });
export const playDraft = (draftId: string) => draftsClient.continueDraft({ draftId } as ContinueDraftRequest);
export const editDraft = (draftId: string) => draftsClient.editDraft({ draftId } as EditDraftRequest);
export const deleteDraft = (draftId: string) => draftsClient.deleteDraft({draftId} as DeleteDraftRequest);

export async function apiRequest<T = unknown>(
   path: string,
   init: RequestInit = {}
): Promise<T> {
   return signedFetch(`${apiBaseUrl}${path}`, init);
}