import { GetMyDraftDetailResponse, GetMyDraftsResponse } from "@/lib/dto";
import { env } from "@/lib/env";

const apiBase = env.apiUrl;

export async function getMyDrafts(
  accessToken: string | undefined
): Promise<GetMyDraftsResponse> {
  // TODO: backend endpoint not yet implemented — GET /my-drafts
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
  // TODO: backend endpoint not yet implemented — GET /my-drafts/{draftId}
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
  personPublicId: string,
): Promise<void> {
  const res = await fetch(
    `${apiBase}/draft-parts/${draftPartId}/attendances/${personPublicId}/join`,
    {
      method: 'PUT',
      headers: { Authorization: `Bearer ${accessToken}`},
    },
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`PUT /draft-parts/${draftPartId}/attendances/${personPublicId}/join failed (${res.status}): ${text}`);
  }
}

