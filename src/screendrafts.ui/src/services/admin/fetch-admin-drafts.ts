import { env } from "@/lib/env";
import {
  CampaignResponse,
  CategoryResponse,
  CreatedResponse,
  GetDraftCategoryResponse,
  GetMediaByTmdbIdsResponse,
  SearchDraftsResponse,
  SearchHostResponse,
  SeriesResponse,
  SmartEnumResponse,
} from "@/lib/dto";
import { PositionConfig } from "@/app/admin/drafts/new/positions-editor";

// TODO: regenerate dto.ts after backend adds these response shapes if they differ
export interface AdminDraftListItem extends SearchDraftsResponse {}

export interface AdminSeriesOption {
  publicId: string;
  name: string;
  allowedDraftTypes: SmartEnumResponse[];
  defaultDraftType?: SmartEnumResponse;
}

export interface AdminHostOption {
  publicId: string;
  displayName: string;
}

export interface AdminDrafterOption {
  publicId: string;
  personPublicId: string;
  displayName: string;
  isRetired: boolean;
}

export interface AdminDrafterTeamOption {
  publicId: string;
  name: string;
  numberOfDrafters: number;
}

export interface DraftPartParticipant {
  participantIdValue: string; // Guid as string
  participantKindValue: SmartEnumResponse;
  participantPublicId: string | null;
  displayName: string | null;
  personPublicId: string | null;
}

export interface DraftPartHost {
  hostPublicId: string;
  displayName: string;
  personPublicId: string | null;
}

export interface DraftPartCommunityFilmRule {
  publicId: string;
  ruleKind: SmartEnumResponse;
  targetSlot: number | null;
  tmdbId: number | null;
  title: string | null;
}

export interface DraftPart {
  publicId: string;
  partIndex: number;
  draftType: SmartEnumResponse;
  status: SmartEnumResponse;
  primaryHost: DraftPartHost | null;
  coHosts: DraftPartHost[];
  participants: DraftPartParticipant[];
  maxCommunityPicks: number;
  maxCommunityVetoes: number;
  communityFilmRules: DraftPartCommunityFilmRule[];
}

export interface AdminDraftDetail {
  publicId: string;
  title: string;
  description: string | null;
  draftType: SmartEnumResponse;
  draftStatus: SmartEnumResponse;
  seriesPublicId: string | null;
  seriesName: string | null;
  episodeNumber: number | null;
  campaignPublicId: string | null;
  campaignName: string | null;
  imagePath: string | null;
  categories: GetDraftCategoryResponse[];
  parts: DraftPart[];
}

const apiBase = env.apiUrl;

function authHeaders(accessToken: string | undefined): HeadersInit {
  return accessToken ? { Authorization: `Bearer ${accessToken}` } : {};
}

export async function listAdminDrafts(
  accessToken: string | undefined,
  draftStatus?: number,
  page = 1,
  pageSize = 50
): Promise<AdminDraftListItem[]> {
  try {
    const url = new URL(`${apiBase}/drafts/search`);
    url.searchParams.set("page", String(page));
    url.searchParams.set("pageSize", String(pageSize));
    if (draftStatus !== undefined){
      url.searchParams.set("status", String(draftStatus));
    }
    const response = await fetch(url.toString(), {
      headers: authHeaders(accessToken),
      cache: "no-store",
    });
    if (!response.ok) return [];
    const data = await response.json() as { items?: AdminDraftListItem[] };
    return data.items ?? [];
  } catch (err) {
    console.error("[listAdminDrafts]", err);
    return [];
  }
}

export async function listAdminActiveDrafts(accessToken: string | undefined) {
  const [created, paused] = await Promise.all([
    listAdminDrafts(accessToken, 0),
    listAdminDrafts(accessToken, 3),
  ]);
  return [...created, ...paused];
}

export async function listAllSeries(
  accessToken: string | undefined
): Promise<AdminSeriesOption[]> {
  try {
    const response = await fetch(`${apiBase}/series`, {
      headers: authHeaders(accessToken),
      cache: "no-store",
    });
    if (!response.ok) return [];
    const data = await response.json() as { items?: SeriesResponse[] };
    return (data.items ?? []).map((s) => ({
      publicId: s.publicId ?? "",
      name: s.name ?? "",
      allowedDraftTypes: s.allowedDraftTypes ?? [],
      defaultDraftType: s.defaultDraftType,
    }));
  } catch (err) {
    console.error("[listAllSeries]", err);
    return [];
  }
}

export async function searchAllHosts(
  accessToken: string | undefined,
  name?: string
): Promise<AdminHostOption[]> {
  try {
    const url = new URL(`${apiBase}/hosts/search`);
    url.searchParams.set("page", "1");
    url.searchParams.set("pageSize", "200");
    if (name) url.searchParams.set("name", name);

    const response = await fetch(url.toString(), {
      headers: authHeaders(accessToken),
      cache: "no-store",
    });
    if (!response.ok) return [];

    const data = await response.json() as { items?: SearchHostResponse[] };
    return (data.items ?? []).map((h) => ({
      publicId: h.publicId,
      displayName: h.displayName ?? `${h.firstName} ${h.lastName}`.trim(),
    }));
  } catch (err) {
    console.error("[searchAllHosts]", err);
    return [];
  }
}

export async function searchDrafters(
  accessToken: string | undefined,
  name?: string,
  retired = false,
  page = 1,
  pageSize = 500
): Promise<AdminDrafterOption[]> {
  try {
    const url = new URL(`${apiBase}/drafters/search`);
    url.searchParams.set("page", String(page));
    url.searchParams.set("pageSize", String(pageSize));
    url.searchParams.set("retired", String(retired));
    if (name) url.searchParams.set("name", name);
    const response = await fetch(url.toString(), {
      headers: authHeaders(accessToken),
      cache: "no-store",
    });
    if (!response.ok) return [];
    const data = (await response.json()) as {
      items?: {
        publicId: string;
        personPublicId: string;
        displayName: string;
        isRetired: boolean;
      }[];
    };
    return data.items ?? [];
  } catch (err) {
    console.error("[searchDrafters]", err);
    return [];
  }
}

export async function searchDrafterTeams(
  accessToken: string | undefined,
  name?: string,
  page = 1,
  pageSize = 200
): Promise<AdminDrafterTeamOption[]> {
  try {
    const url = new URL(`${apiBase}/drafter-teams/search`);
    url.searchParams.set("page", String(page));
    url.searchParams.set("pageSize", String(pageSize));
    if (name) url.searchParams.set("name", name);
    const response = await fetch(url.toString(), {
      headers: authHeaders(accessToken),
      cache: "no-store",
    });
    if (!response.ok) return [];
    const data = (await response.json()) as {
      items?: AdminDrafterTeamOption[];
    };
    return data.items ?? [];
  } catch (err) {
    console.error("[searchDrafterTeams]", err);
    return [];
  }
}

export async function listAllCategories(
  accessToken: string | undefined
): Promise<CategoryResponse[]> {
  try {
    const response = await fetch(`${apiBase}/categories`, {
      headers: authHeaders(accessToken),
      cache: "no-store",
    });
    if (!response.ok) return [];
    const data = await response.json() as { items?: CategoryResponse[] };
    return (data.items ?? []).filter((c) => !c.isDeleted);
  } catch (err) {
    console.error("[listAllCategories]", err);
    return [];
  }
}

export async function listAllCampaigns(
  accessToken: string | undefined
): Promise<CampaignResponse[]> {
  try {
    const response = await fetch(`${apiBase}/campaigns`, {
      headers: authHeaders(accessToken),
      cache: "no-store",
    });
    if (!response.ok) return [];
    const data = await response.json() as { items?: CampaignResponse[] };
    return (data.items ?? []).filter((c) => !c.isDeleted);
  } catch (err) {
    console.error("[listAllCampaigns]", err);
    return [];
  }
}

export async function getDraft(
  accessToken: string | undefined,
  draftPublicId: string
): Promise<AdminDraftDetail | null> {
  try {
    const response = await fetch(
      `${apiBase}/drafts/${encodeURIComponent(draftPublicId)}`,
      {
        headers: authHeaders(accessToken),
        cache: "no-store",
      }
    );
    if (!response.ok) return null;
    return (await response.json()) as AdminDraftDetail;
  } catch (err) {
    console.error("[getDraft]", err);
    return null;
  }
}

// ── Mutation helpers (used in 'use client' form via server action or direct fetch) ──

export async function createDraft(
  accessToken: string,
  body: { title: string; draftType: number; seriesId: string }
): Promise<CreatedResponse> {
  const response = await fetch(`${apiBase}/drafts`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${accessToken}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
  });
  if (!response.ok) {
    const text = await response.text().catch(() => response.statusText);
    throw new Error(`POST /drafts failed (${response.status}): ${text}`);
  }
  return response.json() as Promise<CreatedResponse>;
}

export async function createDraftPart(
  accessToken: string,
  draftPublicId: string,
  body: { publicId: string; partIndex: number; minimumPosition: number; maximumPosition: number }
): Promise<string> {
  const response = await fetch(`${apiBase}/drafts/${encodeURIComponent(draftPublicId)}/parts`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${accessToken}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
  });
  if (!response.ok) {
    const text = await response.text().catch(() => response.statusText);
    throw new Error(`POST /drafts/${draftPublicId}/parts failed (${response.status}): ${text}`);
  }
  // Returns the new part publicId as a JSON string
  const data = (await response.json()) as { publicId: string };
  return data.publicId;
}

export async function addHostToDraftPart(
  accessToken: string,
  draftPartId: string,
  body: { draftPartId: string; hostPublicId: string; hostRole: number }
): Promise<void> {
  const response = await fetch(
    `${apiBase}/draft-parts/${encodeURIComponent(draftPartId)}/hosts`,
    {
      method: "POST",
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify(body),
    }
  );
  if (!response.ok) {
    const text = await response.text().catch(() => response.statusText);
    throw new Error(`POST /draft-parts/${draftPartId}/hosts failed (${response.status}): ${text}`);
  }
}

export async function addParticipantToDraftPart(
  accessToken: string,
  draftPartId: string,
  body: { draftPartId: string; participantPublicId: string; participantKind: number }
): Promise<void> {
  const response = await fetch(
    `${apiBase}/draft-parts/${encodeURIComponent(draftPartId)}/participants`,
    {
      method: "POST",
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify(body),
    }
  );
  if (!response.ok) {
    const text = await response.text().catch(() => response.statusText);
    throw new Error(
      `POST /draft-parts/${draftPartId}/participants failed (${response.status}): ${text}`
    );
  }
}

export async function setDraftCategories(
  accessToken: string,
  draftPublicId: string,
  categoryIds: string[]
): Promise<void> {
  const response = await fetch(
    `${apiBase}/drafts/${encodeURIComponent(draftPublicId)}/categories`,
    {
      method: "PUT",
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ publicId: draftPublicId, categoryIds }),
    }
  );
  if (!response.ok) {
    const text = await response.text().catch(() => response.statusText);
    throw new Error(
      `PUT /drafts/${draftPublicId}/categories failed (${response.status}): ${text}`
    );
  }
}

export async function setDraftCampaign(
  accessToken: string,
  draftPublicId: string,
  campaignId: string
): Promise<void> {
  const response = await fetch(
    `${apiBase}/drafts/${encodeURIComponent(draftPublicId)}/campaign`,
    {
      method: "POST",
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ publicId: draftPublicId, campaignId }),
    }
  );
  if (!response.ok) {
    const text = await response.text().catch(() => response.statusText);
    throw new Error(
      `POST /drafts/${draftPublicId}/campaign failed (${response.status}): ${text}`
    );
  }
}

export async function updateDraft(
  accessToken: string,
  draftPublicId: string,
  body: {
    title?: string;
    description?: string;
    seriesPublicId?: string;
    campaignPublicId?: string;
    publicCategoryIds?: string[];
    draftTypeValue: number;
  }
): Promise<void> {
  const response = await fetch(
    `${apiBase}/drafts/${encodeURIComponent(draftPublicId)}`,
    {
      method: "PUT",
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify(body),
    }
  );
  if (!response.ok) {
    const text = await response.text().catch(() => response.statusText);
    throw new Error(`PUT /drafts/${draftPublicId} failed (${response.status}): ${text}`);
  }
}

export async function removeHostFromDraftPart(
  accessToken: string,
  draftPartId: string,
  hostPublicId: string
): Promise<void> {
  const response = await fetch(
    `${apiBase}/draft-parts/${encodeURIComponent(draftPartId)}/hosts/${encodeURIComponent(hostPublicId)}`,
    {
      method: "DELETE",
      headers: { Authorization: `Bearer ${accessToken}` },
    }
  );
  if (!response.ok) {
    const text = await response.text().catch(() => response.statusText);
    throw new Error(
      `DELETE /draft-parts/${draftPartId}/hosts/${hostPublicId} failed (${response.status}): ${text}`
    );
  }
}

export async function removeParticipantFromDraftPart(
  accessToken: string,
  draftPartId: string,
  body: { participantPublicId: string; participantKind: number }
): Promise<void> {
  const response = await fetch(
    `${apiBase}/draft-parts/${encodeURIComponent(draftPartId)}/participants`,
    {
      method: "DELETE",
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ draftPartId, ...body }),
    }
  );
  if (!response.ok) {
    const text = await response.text().catch(() => response.statusText);
    throw new Error(
      `DELETE /draft-parts/${draftPartId}/participants failed (${response.status}): ${text}`
    );
  }
}

export async function clearDraftCampaign(
  accessToken: string,
  draftPublicId: string
): Promise<void> {
  const response = await fetch(
    `${apiBase}/drafts/${encodeURIComponent(draftPublicId)}/campaign`,
    {
      method: "DELETE",
      headers: { Authorization: `Bearer ${accessToken}` },
    }
  );
  if (!response.ok) {
    const text = await response.text().catch(() => response.statusText);
    throw new Error(
      `DELETE /drafts/${draftPublicId}/campaign failed (${response.status}): ${text}`
    );
  }
}

export async function setDraftPartCommunityParticipant(
  accessToken: string,
  draftPartId: string
): Promise<void> {
  const res = await fetch(
    `${apiBase}/draft-parts/${encodeURIComponent(draftPartId)}/participants`,
    {
      method: "POST",
      headers: { Authorization: `Bearer ${accessToken}`, "Content-Type": "application/json" },
      body: JSON.stringify({
        participantPublicId: null,
        participantKind: 2, // Community
      }),
    }
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`POST /draft-parts/${draftPartId}/participants (community) failed (${res.status}): ${text}`);
  }
}

export async function removeDraftPartCommunityParticipant(
  accessToken: string,
  draftPartId: string
): Promise<void> {
  const res = await fetch(
    `${apiBase}/draft-parts/${encodeURIComponent(draftPartId)}/participants/community/aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee`,
    {
      method: "DELETE",
      headers: { Authorization: `Bearer ${accessToken}` },
    }
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`DELETE /draft-parts/${draftPartId}/participants/community failed (${res.status}): ${text}`);
  }
}

export async function setDraftPartCommunityLimits(
  accessToken: string,
  draftPartId: string,
  body: { maxCommunityPicks: number; maxCommunityVetoes: number }
): Promise<void> {
  const res = await fetch(
    `${apiBase}/draft-parts/${encodeURIComponent(draftPartId)}/community-limits`,
    {
      method: "PUT",
      headers: { Authorization: `Bearer ${accessToken}`, "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`PUT /draft-parts/${draftPartId}/community-limits failed (${res.status}): ${text}`);
  }
}

export async function addCommunityFilmRule(
  accessToken: string,
  draftPartId: string,
  body: { ruleKind: number; targetSlot: number | null }
): Promise<string> {
  const res = await fetch(
    `${apiBase}/draft-parts/${encodeURIComponent(draftPartId)}/community-film-rules`,
    {
      method: "POST",
      headers: { Authorization: `Bearer ${accessToken}`, "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`POST /draft-parts/${draftPartId}/community-film-rules failed (${res.status}): ${text}`);
  }
  const data = (await res.json()) as {publicId: string};
  return data.publicId;
}

export async function updateCommunityFilmRule(
  accessToken: string,
  draftPartId: string,
  ruleId: string,
  body: { ruleKind: number; targetSlot: number | null }
): Promise<void> {
  const res = await fetch(
    `${apiBase}/draft-parts/${encodeURIComponent(draftPartId)}/community-film-rules/${encodeURIComponent(ruleId)}`,
    {
      method: "PUT",
      headers: { Authorization: `Bearer ${accessToken}`, "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`PUT /draft-parts/${draftPartId}/community-film-rules/${ruleId} failed (${res.status}): ${text}`);
  }
}

export async function assignFilmToCommunityFilmRule(
  accessToken: string,
  draftPartId: string,
  ruleId: string,
  tmdbId: number
): Promise<void> {
  const res = await fetch(
    `${apiBase}/draft-parts/${encodeURIComponent(draftPartId)}/community-film-rules/${encodeURIComponent(ruleId)}/film`,
    {
      method: "PUT",
      headers: { Authorization: `Bearer ${accessToken}`, "Content-Type": "application/json" },
      body: JSON.stringify({ tmdbId }),
    }
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`PUT /draft-parts/${draftPartId}/community-film-rules/${ruleId}/film failed (${res.status}): ${text}`);
  }
}

export async function removeCommunityFilmRule(
  accessToken: string,
  draftPartId: string,
  ruleId: string
): Promise<void> {
  const res = await fetch(
    `${apiBase}/draft-parts/${encodeURIComponent(draftPartId)}/community-film-rules/${encodeURIComponent(ruleId)}`,
    {
      method: "DELETE",
      headers: { Authorization: `Bearer ${accessToken}` },
    }
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`DELETE /draft-parts/${draftPartId}/community-film-rules/${ruleId} failed (${res.status}): ${text}`);
  }
}

export async function setDraftPositions(
  accessToken: string | undefined,
  draftPartId: string,
  positions: PositionConfig[]
): Promise<void> {
  const res = await fetch(`${apiBase}/draft-parts/${draftPartId}/positions`, {
    method: "PUT",
    headers: { "Content-Type": "application/json", ...authHeaders(accessToken) },
    body: JSON.stringify({
      draftPartId,
      positions: positions.map((p) => ({
        name: p.name,
        picks: p.picks,
        hasBonusVeto: p.hasBonusVeto,
        hasBonusVetoOverride: p.hasBonusVetoOverride,
      })),
    }),
  });
  if (!res.ok) throw await res.json().catch(() => new Error("Failed to set positions"));
}

export async function startDraftPart(
  accessToken: string,
  draftId: string,
  partIndex: number
): Promise<void> {
  const res = await fetch(
    `${apiBase}/drafts/${encodeURIComponent(draftId)}/parts/${partIndex}/status`,
    {
      method: "PUT",
      headers: { Authorization: `Bearer ${accessToken}`, "Content-Type": "application/json" },
      body: JSON.stringify({ action: 1 }),
    }
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`PUT /drafts/${draftId}/parts/${partIndex}/status failed (${res.status}): ${text}`);
  }
}

export async function deleteDraft(
  accessToken: string,
  draftId: string
): Promise<void> {
  // TODO: confirm whether a dedicated DELETE /drafts/{draftId} exists
  const res = await fetch(`${apiBase}/drafts/${encodeURIComponent(draftId)}`, {
    method: "PUT",
    headers: { Authorization: `Bearer ${accessToken}`, "Content-Type": "application/json" },
    body: JSON.stringify({ isDeleted: true }),
  });
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`DELETE /drafts/${draftId} failed (${res.status}): ${text}`);
  }
}

export interface DraftPoolData {
  publicId: string;
  draftId: string;
  isLocked: boolean;
  tmdbIds: number[];
}

export async function getDraftPool(
  accessToken: string | undefined,
  draftId: string
): Promise<DraftPoolData | null> {
  try {
    const res = await fetch(`${apiBase}/drafts/${encodeURIComponent(draftId)}/pool`, {
      headers: authHeaders(accessToken),
      cache: "no-store",
    });
    if (res.status === 404) return null;
    if (!res.ok) return null;
    return (await res.json()) as DraftPoolData;
  } catch (err) {
    console.error("[getDraftPool]", err);
    return null;
  }
}

export async function createDraftPool(
  accessToken: string,
  draftId: string
): Promise<void> {
  const res = await fetch(`${apiBase}/drafts/${encodeURIComponent(draftId)}/pool`, {
    method: "POST",
    headers: { Authorization: `Bearer ${accessToken}` , "Content-Type": "application/json" },
    body: JSON.stringify({}),
  });
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`POST /drafts/${draftId}/pool failed (${res.status}): ${text}`);
  }
}

export async function addMovieToDraftPool(
  accessToken: string,
  draftId: string,
  tmdbId: number
): Promise<void> {
  const res = await fetch(`${apiBase}/drafts/${encodeURIComponent(draftId)}/pool/items`, {
    method: "POST",
    headers: { Authorization: `Bearer ${accessToken}`, "Content-Type": "application/json" },
    body: JSON.stringify({ tmdbId }),
  });
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`POST /drafts/${draftId}/pool/items failed (${res.status}): ${text}`);
  }
}

export async function removeMovieFromDraftPool(
  accessToken: string,
  draftId: string,
  tmdbId: number
): Promise<void> {
  // TODO: confirm DELETE /drafts/{draftId}/pool/items/{tmdbId} endpoint
  const res = await fetch(
    `${apiBase}/drafts/${encodeURIComponent(draftId)}/pool/items/${tmdbId}`,
    {
      method: "DELETE",
      headers: { Authorization: `Bearer ${accessToken}` },
    }
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`DELETE /drafts/${draftId}/pool/items/${tmdbId} failed (${res.status}): ${text}`);
  }
}

export async function bulkAddMoviesToDraftPool(
  accessToken: string,
  draftId: string,
  file: File
): Promise<void> {
  const form = new FormData();
  form.append("file", file);
  const res = await fetch(`${apiBase}/drafts/${encodeURIComponent(draftId)}/pool/bulk`, {
    method: "POST",
    headers: { Authorization: `Bearer ${accessToken}` },
    body: form,
  });
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`POST /drafts/${draftId}/pool/bulk failed (${res.status}): ${text}`);
  }
}

export async function listDraftPositions(
  accessToken: string | undefined,
  draftPartId: string
): Promise<PositionConfig[]> {
  const res = await fetch(`${apiBase}/draft-parts/${draftPartId}/positions`, {
    method: "GET",
    headers: { ...authHeaders(accessToken) },
  });
  if (!res.ok) return [];
  const data = await res.json();
  return (data.positions ?? []).map(
    (p: { name?: string; picks?: number[]; hasBonusVeto?: boolean; hasBonusVetoOverride?: boolean }) => ({
      name: p.name ?? "",
      picks: p.picks ?? [],
      hasBonusVeto: p.hasBonusVeto ?? false,
      hasBonusVetoOverride: p.hasBonusVetoOverride ?? false,
    })
  );
}

export async function getMediaByTmdbIds(
  accessToken: string,
  tmdbIds: number[]
): Promise<GetMediaByTmdbIdsResponse> {
  const url = new URL(`${apiBase}/media/by-tmdb-ids`);
  tmdbIds.forEach((id) => url.searchParams.append("tmdbIds", String(id)));
  const res = await fetch(url.toString(), {
    headers: { Authorization: `Bearer ${accessToken}` },
    cache: "no-store",
  });
  if (!res.ok) return { items: [] };
  return res.json() as Promise<GetMediaByTmdbIdsResponse>;
}