import { env } from "@/lib/env";

const apiBase = env.apiUrl;

export async function addCandidateListEntry(
  accessTokenValue: string,
  draftPartId: string,
  tmdbId: number,
  mediaType: number,
  notes?: string,
  tvSeriesTmdbId?: number,
  seasonNumber?: number,
  episodeNumber?: number
): Promise<void> {
  const res = await fetch(
    `${apiBase}/draft-parts/${encodeURIComponent(draftPartId)}/candidate-list`,
    {
      method: "POST",
      headers: { Authorization: `Bearer ${accessTokenValue}`, "Content-Type": "application/json" },
      body: JSON.stringify({
        tmdbId,
        notes,
        mediaType,
        tvSeriesTmdbId,
        seasonNumber,
        episodeNumber,
      }),
    }
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`POST /draft-parts/${draftPartId}/candidate-list failed (${res.status}): ${text}`);
  }
}

export async function removeCandidateListEntry(
  accessTokenValue: string,
  draftPartId: string,
  tmdbId: number
): Promise<void> {
  // TODO: confirm endpoint
  const res = await fetch(
    `${apiBase}/draft-parts/${encodeURIComponent(draftPartId)}/candidate-list/${tmdbId}`,
    {
      method: "DELETE",
      headers: { Authorization: `Bearer ${accessTokenValue}` },
    }
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`DELETE /draft-parts/${draftPartId}/candidate-list/${tmdbId} failed (${res.status}): ${text}`);
  }
}

export async function bulkAddCandidateListEntries(
  accessTokenValue: string,
  draftPartId: string,
  file: File
): Promise<void> {
  // POST /draft-parts/{draftPartId}/candidate-list/bulk  (multipart/form-data, field: file)
  const form = new FormData();
  form.append("file", file);
  const res = await fetch(
    `${apiBase}/draft-parts/${encodeURIComponent(draftPartId)}/candidate-list/bulk`,
    {
      method: "POST",
      headers: { Authorization: `Bearer ${accessTokenValue}` },
      body: form,
    }
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`POST /draft-parts/${draftPartId}/candidate-list/bulk failed (${res.status}): ${text}`);
  }
}
