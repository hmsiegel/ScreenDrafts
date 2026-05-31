import { env } from "@/lib/env";
import {
  CampaignResponse,
  CategoryResponse,
  CreatedResponse,
  SearchDraftsResponse,
  SearchHostResponse,
  SeriesResponse,
  SmartEnumResponse,
} from "@/lib/dto";

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
  pageSize = 200
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
  const raw = await response.text();
  try { return JSON.parse(raw) as string; } catch { return raw; }
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
