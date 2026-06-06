import { env } from "@/lib/env";

// NOTE: awaiting NSwag regen — backend endpoints not yet implemented

export interface MyDraftPartSummary {
  draftPartId: string;
  partNumber: number;
  status: { name: string; value: number };
  isJoined: boolean;
  releaseDate?: string;
}

export interface MyDraftSummary {
  draftId: string;
  name: string;
  draftType: { name: string; value: number };
  episodeNumber?: number;
  parts: MyDraftPartSummary[];
}

export interface MyDraftsResponse {
  upcoming: MyDraftSummary[];
  inProgress: MyDraftSummary[];
  completed: MyDraftSummary[];
}

export interface MyDraftPartDetail {
  draftPartId: string;
  partNumber: number;
  status: { name: string; value: number };
  isJoined: boolean;
  hostRole?: { name: string; value: number };
  drafterPosition?: number;
  candidateList: CandidateListEntryBrief[];
  draftBoard: DraftBoardMovieBrief[];
}

export interface CandidateListEntryBrief {
  tmdbId: number;
  title: string;
  year: number;
  posterUrl?: string;
  notes?: string;
}

export interface DraftBoardMovieBrief {
  tmdbId: number;
  title: string;
  year: number;
  posterUrl?: string;
  notes?: string;
  priority?: number;
}

export interface DraftDetail {
  publicId: string;
  title: string;
  draftType: { name: string; value: number };
  draftStatus: { name: string; value: number };
  episodeNumber?: number;
  seriesName?: string;
}

export interface MyDraftDetailResponse {
  draft: DraftDetail;
  myRoles: ("Host" | "Drafter")[];
  isSurrogate: boolean;
  parts: MyDraftPartDetail[];
}

const apiBase = env.apiUrl;

export async function getMyDrafts(
  accessToken: string | undefined
): Promise<MyDraftsResponse> {
  // TODO: backend endpoint not yet implemented — GET /my-drafts
  try {
    const res = await fetch(`${apiBase}/my-drafts`, {
      headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
      cache: "no-store",
    });
    if (!res.ok) return { upcoming: [], inProgress: [], completed: [] };
    return (await res.json()) as MyDraftsResponse;
  } catch (err) {
    console.error("[getMyDrafts]", err);
    return { upcoming: [], inProgress: [], completed: [] };
  }
}

export async function getMyDraftDetail(
  accessToken: string | undefined,
  draftId: string
): Promise<MyDraftDetailResponse | null> {
  // TODO: backend endpoint not yet implemented — GET /my-drafts/{draftId}
  try {
    const res = await fetch(`${apiBase}/my-drafts/${encodeURIComponent(draftId)}`, {
      headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
      cache: "no-store",
    });
    if (!res.ok) return null;
    return (await res.json()) as MyDraftDetailResponse;
  } catch (err) {
    console.error("[getMyDraftDetail]", err);
    return null;
  }
}

export async function joinDraftPart(
  accessToken: string,
  draftPartId: string
): Promise<void> {
  // TODO: confirm endpoint — does not exist yet in backend
  // TODO: confirm whether this creates a backend attendance record
  const res = await fetch(
    `${apiBase}/draft-parts/${encodeURIComponent(draftPartId)}/join`,
    {
      method: "POST",
      headers: { Authorization: `Bearer ${accessToken}` },
    }
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`POST /draft-parts/${draftPartId}/join failed (${res.status}): ${text}`);
  }
}
