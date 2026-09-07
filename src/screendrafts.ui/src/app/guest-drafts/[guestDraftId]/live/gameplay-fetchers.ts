// app/guest-drafts/[guestDraftId]/live/gameplay-fetchers.ts
// Regenerate NSwag after backend changes — types below come from dto.ts.
//
// The generated NSwag client is not used here. Its guestDrafts_UndoVeto /
// guestDrafts_UndoPick / guestDrafts_RevealPick / guestDrafts_ApplyCommissionerOverride
// methods build a URL from the route template ("/guest-drafts/{publicId}/picks/{playOrder}/...")
// but never substitute publicId/playOrder into it — the literal "{publicId}" and
// "{playOrder}" placeholders go out on the wire. Same pattern gameplay-fetchers.ts
// already uses for DraftParts: raw fetch, types borrowed from dto.ts for shape only.

import {
  CreatedResponse,
  GetGuestDraftGameplayResponse,
  MediaResponse,
  PositionInput,
  SetGuestDraftStatusResponse,
} from '@/lib/dto';

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

export async function fetchGuestDraftGameplay(
  accessToken: string,
  guestDraftId: string,
): Promise<GetGuestDraftGameplayResponse> {
  const res = await fetch(
    `${API_BASE}/guest-drafts/${guestDraftId}`,
    { headers: authHeadersGet(accessToken), cache: 'no-store' },
  );
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`fetchGuestDraftGameplay failed: ${res.status} - ${body}`);
  }
  return res.json();
}

export async function playGuestDraftPick(
  accessToken: string,
  guestDraftId: string,
  args: { moviePublicId: string; position: number; playOrder: number },
): Promise<void> {
  const res = await fetch(`${API_BASE}/guest-drafts/${guestDraftId}/picks`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(args),
  });
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`playGuestDraftPick failed: ${res.status} - ${body}`);
  }
}

export async function applyGuestDraftVeto(
  accessToken: string,
  guestDraftId: string,
  playOrder: number,
  note?: string,
): Promise<void> {
  const res = await fetch(
    `${API_BASE}/guest-drafts/${guestDraftId}/picks/${playOrder}/veto`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify({ note }),
    },
  );
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`applyGuestDraftVeto failed: ${res.status} - ${body}`);
  }
}

export async function applyGuestDraftVetoOverride(
  accessToken: string,
  guestDraftId: string,
  playOrder: number,
  note?: string,
): Promise<void> {
  const res = await fetch(
    `${API_BASE}/guest-drafts/${guestDraftId}/picks/${playOrder}/veto-override`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify({ note }),
    },
  );
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`applyGuestDraftVetoOverride failed: ${res.status} - ${body}`);
  }
}

export async function applyGuestDraftCommissionerOverride(
  accessToken: string,
  guestDraftId: string,
  playOrder: number,
): Promise<void> {
  const res = await fetch(
    `${API_BASE}/guest-drafts/${guestDraftId}/picks/${playOrder}/commissioner-override`,
    { method: 'POST', headers: authHeadersGet(accessToken) },
  );
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`applyGuestDraftCommissionerOverride failed: ${res.status} - ${body}`);
  }
}

export async function revealGuestDraftPick(
  accessToken: string,
  guestDraftId: string,
  playOrder: number,
): Promise<void> {
  const res = await fetch(
    `${API_BASE}/guest-drafts/${guestDraftId}/picks/${playOrder}/reveal`,
    { method: 'POST', headers: authHeadersGet(accessToken) },
  );
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`revealGuestDraftPick failed: ${res.status} - ${body}`);
  }
}

export async function undoGuestDraftVeto(
  accessToken: string,
  guestDraftId: string,
  playOrder: number,
): Promise<void> {
  const res = await fetch(
    `${API_BASE}/guest-drafts/${guestDraftId}/picks/${playOrder}/undo-veto`,
    { method: 'POST', headers: authHeadersGet(accessToken) },
  );
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`undoGuestDraftVeto failed: ${res.status} - ${body}`);
  }
}

export async function undoGuestDraftPick(
  accessToken: string,
  guestDraftId: string,
  playOrder: number,
): Promise<void> {
  const res = await fetch(
    `${API_BASE}/guest-drafts/${guestDraftId}/picks/${playOrder}`,
    { method: 'DELETE', headers: authHeadersGet(accessToken) },
  );
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`undoGuestDraftPick failed: ${res.status} - ${body}`);
  }
}

// PickSubmitted only carries moviePublicId, not a title (see guest-draft-context.tsx's
// pendingReveal wiring) — this resolves it for the reveal prompt. GET /media/{publicId}
// is a generic Movies-module endpoint, not GuestDrafts-specific; it lives here rather
// than in movie-resolve.ts only because I haven't seen that file's source to safely
// add to it — move it there if it's a more natural home.
export async function fetchMediaByPublicId(
  accessToken: string,
  moviePublicId: string,
): Promise<MediaResponse> {
  const res = await fetch(`${API_BASE}/media/${moviePublicId}`, {
    headers: authHeadersGet(accessToken),
  });
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`fetchMediaByPublicId failed: ${res.status} - ${body}`);
  }
  return res.json();
}

// ── Setup / creation ─────────────────────────────────────────────────────────
// GuestDraftType.cs has 5 values. Per GuestDraftBoardTemplates.cs, only
// Standard and MiniSuper are fixed (setFixedGuestDraftBoardLayout, no body —
// the backend already knows both templates: Standard is A[7,6,4,2]/B[5,3,1],
// MiniSuper is A[5,3,1]/B[4,2]). MiniMega, Super, and Mega are owner-defined
// custom layouts — setCustomGuestDraftPositions below, PositionInput[] body,
// which matches positions-editor.tsx's PositionConfig shape field-for-field.

export async function setCustomGuestDraftPositions(
  accessToken: string,
  guestDraftId: string,
  positions: PositionInput[],
): Promise<void> {
  const res = await fetch(`${API_BASE}/guest-drafts/${guestDraftId}/board/custom-layout`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ positions }),
  });
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`setCustomGuestDraftPositions failed: ${res.status} - ${body}`);
  }
}

export async function createGuestDraft(
  accessToken: string,
  args: { title: string; type: string },
): Promise<CreatedResponse> {
  const res = await fetch(`${API_BASE}/guest-drafts`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(args),
  });
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`createGuestDraft failed: ${res.status} - ${body}`);
  }
  return res.json();
}

export async function setFixedGuestDraftBoardLayout(
  accessToken: string,
  guestDraftId: string,
): Promise<void> {
  const res = await fetch(`${API_BASE}/guest-drafts/${guestDraftId}/board/fixed-layout`, {
    method: 'POST',
    headers: authHeadersGet(accessToken),
  });
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`setFixedGuestDraftBoardLayout failed: ${res.status} - ${body}`);
  }
}

// Takes a raw user public id, not a username — there's no backend search
// endpoint yet to back a proper typeahead (flagged previously; still open).
// This is the stopgap until that exists.
export async function inviteGuestDraftParticipant(
  accessToken: string,
  guestDraftId: string,
  inviteeUserPublicId: string,
): Promise<void> {
  const res = await fetch(`${API_BASE}/guest-drafts/${guestDraftId}/participants`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ inviteeUserPublicId }),
  });
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`inviteGuestDraftParticipant failed: ${res.status} - ${body}`);
  }
}

export async function assignGuestDraftParticipantToPosition(
  accessToken: string,
  guestDraftId: string,
  positionPublicId: string,
  participantPublicId: string,
): Promise<void> {
  const res = await fetch(
    `${API_BASE}/guest-drafts/${guestDraftId}/positions/${positionPublicId}/assign`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify({ participantPublicId }),
    },
  );
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`assignGuestDraftParticipantToPosition failed: ${res.status} - ${body}`);
  }
}

// `action` is GuestDraftStatusAction.cs's raw int: Start = 1, Complete = 2.
export async function setGuestDraftStatus(
  accessToken: string,
  guestDraftId: string,
  action: number,
): Promise<SetGuestDraftStatusResponse> {
  const res = await fetch(`${API_BASE}/guest-drafts/${guestDraftId}/status`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ action }),
  });
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`setGuestDraftStatus failed: ${res.status} - ${body}`);
  }
  return res.json();
}