import { env } from "@/lib/env";

// NOTE: awaiting NSwag regen
export interface DraftBoardMovie {
  tmdbId: number;
  title: string;
  year: number;
  posterUrl?: string;
  notes?: string;
  priority?: number;
}

const apiBase = env.apiUrl;

export async function getDraftBoard(
  accessToken: string | undefined,
  draftId: string
): Promise<DraftBoardMovie[]> {
  try {
    const res = await fetch(`${apiBase}/drafts/${encodeURIComponent(draftId)}/board`, {
      headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
      cache: "no-store",
    });
    if (!res.ok) return [];
    const data = (await res.json()) as { items?: DraftBoardMovie[] };
    return data.items ?? [];
  } catch (err) {
    console.error("[getDraftBoard]", err);
    return [];
  }
}

export async function addMovieToDraftBoard(
  accessToken: string,
  draftId: string,
  tmdbId: number,
  notes?: string,
  priority?: number
): Promise<void> {
  const res = await fetch(`${apiBase}/drafts/${encodeURIComponent(draftId)}/board`, {
    method: "POST",
    headers: { Authorization: `Bearer ${accessToken}`, "Content-Type": "application/json" },
    body: JSON.stringify({ tmdbId, notes, priority }),
  });
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`POST /drafts/${draftId}/board failed (${res.status}): ${text}`);
  }
}

export async function removeMovieFromDraftBoard(
  accessToken: string,
  draftId: string,
  tmdbId: number
): Promise<void> {
  const res = await fetch(
    `${apiBase}/drafts/${encodeURIComponent(draftId)}/board/${tmdbId}`,
    {
      method: "DELETE",
      headers: { Authorization: `Bearer ${accessToken}` },
    }
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`DELETE /drafts/${draftId}/board/${tmdbId} failed (${res.status}): ${text}`);
  }
}

export async function updateDraftBoardItem(
  accessToken: string,
  draftId: string,
  tmdbId: number,
  notes?: string,
  priority?: number
): Promise<void> {
  // TODO: confirm request shape for UpdateItem
  const res = await fetch(
    `${apiBase}/drafts/${encodeURIComponent(draftId)}/board/${tmdbId}`,
    {
      method: "PUT",
      headers: { Authorization: `Bearer ${accessToken}`, "Content-Type": "application/json" },
      body: JSON.stringify({ notes, priority }),
    }
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`PUT /drafts/${draftId}/board/${tmdbId} failed (${res.status}): ${text}`);
  }
}

export async function updateDraftBoardOrder(
  accessToken: string,
  draftId: string,
  orderedTmdbIds: number[]
): Promise<void> {
  // TODO: confirm whether a reorder endpoint exists
  // Fallback: call updateDraftBoardItem per item with updated priority values
  for (let i = 0; i < orderedTmdbIds.length; i++) {
    await updateDraftBoardItem(accessToken, draftId, orderedTmdbIds[i], undefined, i + 1);
  }
}
