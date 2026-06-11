// app/admin/spotlight/spotlight-fetchers.ts

import type {
  PagedResultOfListSpotlightDraftsResponse,
  SearchSpotlightCandidatesResponse,
  SpotlightCandidateItem,
} from '@/lib/dto';

const API_BASE = process.env.NEXT_PUBLIC_API_URL;

// ── Fetchers ──────────────────────────────────────────────────────────────

function authHeaders(accessToken: string) {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  };
}

function authHeadersGet(accessToken: string) {
  return {
    Authorization: `Bearer ${accessToken}`,
  };
}

export async function fetchSpotlights(
  accessToken: string,
  page = 1,
  pageSize = 20,
  excludeActive = false,
  query?: string,
  draftType?: string
): Promise<PagedResultOfListSpotlightDraftsResponse> {
  const params = new URLSearchParams({
    page: String(page),
    pageSize: String(pageSize),
    excludeActive: String(excludeActive),
  });
  if (query) params.set('query', query);
  if (draftType) params.set('draftType', draftType);
  const res = await fetch(
    `${API_BASE}/reporting/spotlights?${params}`,
    { headers: authHeadersGet(accessToken), cache: 'no-store' }
  );
  if (!res.ok) throw new Error(`fetchSpotlights failed: ${res.status}`);
  return res.json();
}

export async function searchSpotlightCandidates(
  accessToken: string,
  query: string,
  page = 1,
  pageSize = 20
): Promise<SearchSpotlightCandidatesResponse> {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
  if (query.trim()) params.set('query', query.trim());
  const res = await fetch(
    `${API_BASE}/reporting/spotlights/candidates?${params}`,
    { headers: authHeadersGet(accessToken), cache: 'no-store' }
  );
  if (!res.ok) throw new Error(`searchSpotlightCandidates failed: ${res.status}`);
  return res.json();
}

export async function createSpotlight(
  accessToken: string,
  draftPublicId: string,
  spotlightDescription: string,
  spotifyUrl: string | null
): Promise<{ spotlightId: string }> { // spotlightId matches CreateSpotlightResponse DTO field name — leave as-is until NSwag regen
  const res = await fetch(`${API_BASE}/reporting/spotlights`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ draftPublicId, spotlightDescription, spotifyUrl: spotifyUrl || null }),
  });
  if (!res.ok) throw new Error(`createSpotlight failed: ${res.status}`);
  return res.json();
}

export async function activateSpotlight(
  accessToken: string,
  publicId: string
): Promise<void> {
  const res = await fetch(`${API_BASE}/reporting/spotlights/${publicId}/activate`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify({}),
  });
  if (!res.ok) throw new Error(`activateSpotlight failed: ${res.status}`);
}

export async function deactivateSpotlight(
  accessToken: string,
  publicId: string
): Promise<void> {
  const res = await fetch(`${API_BASE}/reporting/spotlights/${publicId}/deactivate`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify({}),
  });
  if (!res.ok) throw new Error(`deactivateSpotlight failed: ${res.status}`);
}

export async function deleteSpotlight(
  accessToken: string,
  publicId: string
): Promise<void> {
  const res = await fetch(`${API_BASE}/reporting/spotlights/${publicId}`, {
    method: 'DELETE',
    headers: authHeaders(accessToken),
    body: JSON.stringify({}),
  });
  if (!res.ok) throw new Error(`deleteSpotlight failed: ${res.status}`);
}

export async function rotateSpotlight(accessToken: string): Promise<void> {
  const res = await fetch(`${API_BASE}/reporting/spotlights/rotate`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({}),
  });
  if (!res.ok) throw new Error(`rotateSpotlight failed: ${res.status}`);
}