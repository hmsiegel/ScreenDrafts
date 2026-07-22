import { GetMyDraftDetailResponse, GetMyDraftsResponse } from "@/lib/dto";
import { env } from "@/lib/env";

export interface PredictionSeasonSummary {
  publicId: string;
  number: number;
}

export interface DraftPartPredictionRulesDto {
  predictionMode: number;
  requiredCount: number;
  topN: number | null;
  deadlineUtc: string | null;
}

export interface SubmitPredictionEntry {
  tmdbId: number;
  mediaTitle: string;
  orderIndex: number | null;
  notes: string | null;
}

export interface PredictionEntryDto {
  tmdbId: number;
  mediaPublicId: string;
  mediaTitle: string;
  orderIndex: number;
  isCorrect: boolean | null;
  notes: string | null;
}

export interface PredictionResultDto {
  correctCount: number;
  shootsTheMoon: boolean;
  pointsAwarded: number;
  scoredAtUtc: string;
}

export interface DraftPartPredictionSetDto {
  publicId: string;
  contestantPublicId: string;
  contestantDisplayName: string;
  submittedAtUtc: string;
  sourceKind: string;
  isLocked: boolean;
  lockedAtUtc: string | null;
  entries: PredictionEntryDto[];
  result: PredictionResultDto | null;
}

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

export async function getCurrentPredictionSeason(
  accessToken: string | undefined
): Promise<PredictionSeasonSummary | null> {
  try {
    const res = await fetch(`${apiBase}/prediction-seasons/current`, {
      headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
      cache: "no-store",
    });
    if (!res.ok) return null;
    return (await res.json()) as PredictionSeasonSummary;
  } catch (err) {
    console.error("[getCurrentPredictionSeason]", err);
    return null;
  }
}

export async function getDraftPartPredictionRules(
  accessToken: string | undefined,
  draftPartId: string
): Promise<DraftPartPredictionRulesDto | null> {
  try {
    const res = await fetch(
      `${apiBase}/draft-parts/${encodeURIComponent(draftPartId)}/prediction-rules`,
      { headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {}, cache: "no-store" }
    );
    if (!res.ok) return null;
    return (await res.json()) as DraftPartPredictionRulesDto | null;
  } catch (err) {
    console.error("[getDraftPartPredictionRules]", err);
    return null;
  }
}

export async function submitPredictionSet(
  accessToken: string,
  draftPartId: string,
  body: {
    seasonPublicId: string;
    contestantPublicId: string;
    submittedByPersonPublicId: string | null;
    sourceKind: number;
    entries: SubmitPredictionEntry[];
  }
): Promise<void> {
  const res = await fetch(
    `${apiBase}/draft-parts/${encodeURIComponent(draftPartId)}/predictions`,
    {
      method: "POST",
      headers: { Authorization: `Bearer ${accessToken}`, "Content-Type": "application/json" },
      body: JSON.stringify({ draftPartId, ...body }),
    }
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(
      `POST /draft-parts/${draftPartId}/predictions failed (${res.status}): ${text}`
    );
  }
}

export async function getDraftPartPredictions(
  accessToken: string | undefined,
  draftPartId: string
): Promise<DraftPartPredictionSetDto[]> {
  try {
    const res = await fetch(
      `${apiBase}/draft-parts/${encodeURIComponent(draftPartId)}/predictions`,
      {
        headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
        cache: "no-store",
      }
    );
    if (!res.ok) return [];
    return (await res.json()) as DraftPartPredictionSetDto[];
  } catch (err) {
    console.error("[getDraftPartPredictions]", err);
    return [];
  }
}