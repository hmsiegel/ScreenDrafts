// app/draft-parts/[draftPartId]/live/gameplay-fetchers.ts
// Regenerate NSwag after backend changes — types below come from dto.ts.

import { GetDraftPartGameplayResponse } from "@/lib/dto";

const API_BASE = process.env.NEXT_PUBLIC_API_URL;

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

// ── Fetchers ──────────────────────────────────────────────────────────────────

export async function fetchGameplay(
  accessToken: string,
  draftPartId: string,
): Promise<GetDraftPartGameplayResponse> {
  const res = await fetch(
    `${API_BASE}/draft-parts/${draftPartId}/gameplay`,
    { headers: authHeadersGet(accessToken), cache: 'no-store' },
  );
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`fetchGameplay failed: ${res.status} - ${body}`);
  }
  return res.json();
}

export async function submitTriviaResults(
  accessToken: string,
  draftPartId: string,
  results: { participantPublicId: string; kind: number; position: number; questionsWon: number }[],
): Promise<void> {
  const res = await fetch(`${API_BASE}/draft-parts/${draftPartId}/trivia-results`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ results }),
  });
  if (!res.ok) throw new Error(`submitTriviaResults failed: ${res.status}`);
}

export async function assignPosition(
  accessToken: string,
  draftPartId: string,
  positionPublicId: string,
  participantPublicId: string,
  participantKind: number,
): Promise<void> {
  const res = await fetch(
    `${API_BASE}/draft-parts/${draftPartId}/positions/${positionPublicId}/assign`,
    {
      method: 'PUT',
      headers: authHeaders(accessToken),
      body: JSON.stringify({ participantPublicId, participantKind }),
    },
  );
  if (!res.ok) throw new Error(`assignPosition failed: ${res.status}`);
}

export async function clearPositionAssignment(
  accessToken: string,
  draftPartId: string,
  positionPublicId: string,
): Promise<void> {
  const res = await fetch(
    `${API_BASE}/draft-parts/${draftPartId}/positions/${positionPublicId}/assign`,
    { method: 'DELETE', headers: authHeadersGet(accessToken) },
  );
  if (!res.ok) throw new Error(`clearPositionAssignment failed: ${res.status}`);
}

export async function completeDraftPart(
  accessToken: string,
  draftPartId: string,
): Promise<void> {
  const res = await fetch(`${API_BASE}/draft-parts/${draftPartId}/status`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ status: 'Completed' }),
  });
  if (!res.ok) throw new Error(`completeDraftPart failed: ${res.status}`);
}

export async function undoVeto(
  accessToken: string,
  draftPartId: string,
  playOrder: number,
): Promise<void> {
  const res = await fetch(
    `${API_BASE}/draft-parts/${draftPartId}/picks/${playOrder}/undo-veto`,
    { method: 'POST', headers: authHeaders(accessToken) },
  );
  if (!res.ok) throw new Error(`undoVeto failed: ${res.status}`);
}

export async function undoPick(
  accessToken: string,
  draftPartId: string,
  playOrder: number,
): Promise<void> {
  const res = await fetch(
    `${API_BASE}/draft-parts/${draftPartId}/picks/${playOrder}/undo`,
    { method: 'DELETE', headers: authHeadersGet(accessToken) },
  );
  if (!res.ok) throw new Error(`undoPick failed: ${res.status}`);
}