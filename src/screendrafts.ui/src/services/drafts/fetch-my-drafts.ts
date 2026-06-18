import { GetMyDraftDetailResponse, GetMyDraftsResponse } from "@/lib/dto";
import { env } from "@/lib/env";

const apiBase = env.apiUrl;

export async function getMyDrafts(
  accessToken: string | undefined
): Promise<GetMyDraftsResponse> {
  try {
    const res = await fetch(`${apiBase}/my-drafts`, {
      headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
      cache: "no-store",
    });
    if (!res.ok) return { upcoming: [], inProgress: [], completed: [] };
    return (await res.json()) as GetMyDraftsResponse;
  } catch (err) {
    console.error("[getMyDrafts]", err);
    return { upcoming: [], inProgress: [], completed: [] };
  }
}

export async function getMyDraftDetail(
  accessToken: string | undefined,
  draftId: string
): Promise<GetMyDraftDetailResponse | null> {
  try {
    const res = await fetch(`${apiBase}/my-drafts/${encodeURIComponent(draftId)}`, {
      headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
      cache: "no-store",
    });
    if (!res.ok) return null;
    return (await res.json()) as GetMyDraftDetailResponse;
  } catch (err) {
    console.error("[getMyDraftDetail]", err);
    return null;
  }
}

export async function joinDraftPart(
  accessToken: string,
  draftPartId: string,
): Promise<void> {
  const res = await fetch(
    `${apiBase}/draft-parts/${draftPartId}/attendances/join`,
    {
      method: "PUT",
      headers: { Authorization: `Bearer ${accessToken}` },
    },
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`joinDraftPart failed (${res.status}): ${text}`);
  }
}

export async function startDraftPart(
  accessToken: string,
  draftPublicId: string,
  partIndex: number,
): Promise<void> {
  const res = await fetch(
    `${apiBase}/drafts/${draftPublicId}/parts/${partIndex}/status`,
    {
      method: "PUT",
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ action: 1 }), // DraftPartStatusAction.Start = 1
    },
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`startDraftPart failed (${res.status}): ${text}`);
  }
}

export async function completeDraftPart(
  accessToken: string,
  draftPublicId: string,
  partIndex: number,
): Promise<void> {
  const res = await fetch(
    `${apiBase}/drafts/${draftPublicId}/parts/${partIndex}/status`,
    {
      method: "PUT",
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ action: 2 }), // DraftPartStatusAction.Complete = 2
    },
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`completeDraftPart failed (${res.status}): ${text}`);
  }
}