// services/drafts/fetch-drafts.ts
import { auth } from "@/auth";
import { DraftPartPredictionResponse, GetDraftResponse, GetTriviaResultsResponse, ListDraftsResponse, ListPredictionSeasonsResponse, PagedResultOfListDraftsResponse, PredictionStandingsResponse } from "@/lib/dto";
import { env } from "@/lib/env";
import { PagedResult, toPagedDraftResult } from "@/types/paged-result";

const apiBase = env.apiUrl;

// ── Fetchers ──────────────────────────────────────────────────────────────

async function authHeaders(): Promise<HeadersInit> {
  const session = await auth();
  if (session?.accessToken) {
    return { Authorization: `Bearer ${session.accessToken}` };
  }
  return {};
}

export async function listDrafts(params: {
  fromDate?: string;
  toDate?: string;
  minDrafters?: number;
  maxDrafters?: number;
  minPicks?: number;
  maxPicks?: number;
  draftType?: number;
  sort?: string;
  dir?: "asc" | "desc";
  q?: string;
  campaignPublicId?: string;
  categoryPublicIds?: string[];
  page?: number;
  pageSize?: number;
} = {}): Promise<PagedResult<ListDraftsResponse>> {
  const url = new URL(`${apiBase}/drafts`);

  const paramMap: Record<string, string> = {
    campaignPublicId: "campaignPublicId",
    sort: "sortBy",
    dir: "dir",
  }

  const { categoryPublicIds, ...scalarParams } = params;

  // scalarParams, not params: iterating params also wrote categoryPublicIds as one
  // comma-joined value ("a,b") ahead of the repeated entries appended below.
  Object.entries(scalarParams).forEach(([key, value]) => {
    if (value === undefined || value === null || value === "") return;
    const backendKey = paramMap[key] ?? key;
    url.searchParams.set(backendKey, String(value));
  });

  // Repeated param: ?categoryPublicIds=cat_abc&categoryPublicIds=cat_xyz
  if (categoryPublicIds && categoryPublicIds.length > 0) {
    categoryPublicIds
      .filter(id => id !== "")
      .forEach(id => url.searchParams.append("categoryPublicIds", id));
  }

  const response = await fetch(url, {
    method: "GET",
    headers: await authHeaders(),
    credentials: "include",
    next: { revalidate: 0 },
  });

  // Throws rather than returning an empty page: the drafts page renders app/error.tsx,
  // and the mobile infinite list shows TRY AGAIN instead of silently ending the list.
  if (!response.ok) {
    throw new Error(`listDrafts failed: ${response.status} ${response.statusText} (${url})`);
  }
  return toPagedDraftResult((await response.json()) as PagedResultOfListDraftsResponse);
}

/**
 * Null when the draft doesn't exist — 404, or 400 for a malformed public id — so the
 * page can show notFound(). Any other failure throws and reaches app/error.tsx.
 */
export async function getDraftDetails(id: string): Promise<GetDraftResponse | null> {
  const url = `${apiBase}/drafts/${encodeURIComponent(id)}`;

  const response = await fetch(url, {
    method: "GET",
    headers: await authHeaders(),
    credentials: "include",
    next: { revalidate: 0 },
  });

  if (response.status === 404 || response.status === 400) return null;
  if (!response.ok) {
    throw new Error(`getDraftDetails failed: ${response.status} ${response.statusText} (${url})`);
  }
  return (await response.json()) as GetDraftResponse;
}

export async function getDraftPartTriviaResults(
  draftPartPublicId: string
): Promise<GetTriviaResultsResponse> {
  const empty: GetTriviaResultsResponse = { draftPartId: "", results: [] };
  try {
    const response = await fetch(
      `${apiBase}/draft-parts/${draftPartPublicId}/trivia-results`,
      { next: { revalidate: 0 } }
    );
    if (!response.ok) return empty;
    return (await response.json()) as GetTriviaResultsResponse;
  } catch {
    return empty;
  }
}

export async function getDraftPartPredictions(
  draftPartPublicId: string
): Promise<DraftPartPredictionResponse[]> {
  try {
    const response = await fetch(
      `${apiBase}/draft-parts/${draftPartPublicId}/predictions`,
      { next: { revalidate: 0 } }
    );
    if (!response.ok) return [];
    return (await response.json()) as DraftPartPredictionResponse[];
  } catch {
    return [];
  }
}

export async function getPredictionStandings(
  seasonPublicId: string,
  asOfDraftPartId: string
): Promise<PredictionStandingsResponse | null> {
  try {
    const url = `${apiBase}/prediction-seasons/${seasonPublicId}/standings?asOfDraftPartId=${encodeURIComponent(asOfDraftPartId)}`;
    const response = await fetch(
      url,
      { next: { revalidate: 0 } }
    );
    if (!response.ok) return null;
    return (await response.json()) as PredictionStandingsResponse;
  } catch {
    return null;
  }
}

export async function listPredictionSeasons(): Promise<ListPredictionSeasonsResponse> {
  const empty: ListPredictionSeasonsResponse = { seasons: [] };
  try {
    const response = await fetch(
      `${apiBase}/prediction-seasons`,
      { next: { revalidate: 0 } }
    );
    if (!response.ok) return empty;
    return (await response.json()) as ListPredictionSeasonsResponse;
  } catch {
    return empty;
  }
}