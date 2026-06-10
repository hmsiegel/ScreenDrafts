import { env } from "@/lib/env";
import { auth } from "@/auth";
import { CandidateListEntryResponse } from "@/lib/dto";

const apiBase = env.apiUrl;

async function accessToken(): Promise<string | undefined> {
  const session = await auth();
  return session?.accessToken;
}

export async function getCandidateList(draftPartId: string): Promise<CandidateListEntryResponse[]> {
  // TODO: confirm endpoint
  try {
    const token = await accessToken();
    const res = await fetch(
      `${apiBase}/draft-parts/${encodeURIComponent(draftPartId)}/candidate-list`,
      {
        headers: token ? { Authorization: `Bearer ${token}` } : {},
        cache: "no-store",
      }
    );
    if (!res.ok) return [];
    const data = (await res.json()) as { items?: CandidateListEntryResponse[] };
    return data.items ?? [];
  } catch (err) {
    console.error("[getCandidateList]", err);
    return [];
  }
}

export async function addCandidateListEntry(
  accessTokenValue: string,
  draftPartId: string,
  tmdbId: number,
  notes?: string
): Promise<void> {
  // TODO: confirm endpoint
  const res = await fetch(
    `${apiBase}/draft-parts/${encodeURIComponent(draftPartId)}/candidate-list`,
    {
      method: "POST",
      headers: { Authorization: `Bearer ${accessTokenValue}`, "Content-Type": "application/json" },
      body: JSON.stringify({ tmdbId, notes }),
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
