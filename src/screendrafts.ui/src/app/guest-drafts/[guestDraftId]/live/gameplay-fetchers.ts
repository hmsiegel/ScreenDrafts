// app/guest-drafts/[guestDraftId]/live/gameplay-fetchers.ts
import {
  CreatedResponse,
  GetGuestDraftGameplayResponse,
  GuestDraftDetailResponse,
  GuestDrafterSummaryResponse,
  GuestDraftSummaryResponse,
  MediaResponse,
  GuestDraftPositionInput,
  SetGuestDraftStatusResponse,
} from '@/lib/dto';
import { PagedResult } from '@/types/paged-result';

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
  // Content-Type + an empty JSON body, not authHeadersGet — a POST/DELETE
  // with no Content-Type at all can 415 in FastEndpoints' model binding
  // before the request ever reaches the handler, even when the request DTO
  // is entirely route-bound. This bit us on revealGuestDraftPick below; same
  // fix applied to every other call in this file that previously sent
  // neither a Content-Type nor a body.
  const res = await fetch(
    `${API_BASE}/guest-drafts/${guestDraftId}/picks/${playOrder}/commissioner-override`,
    { method: 'POST', headers: authHeaders(accessToken), body: JSON.stringify({}) },
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
    { method: 'POST', headers: authHeaders(accessToken), body: JSON.stringify({}) },
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
    { method: 'POST', headers: authHeaders(accessToken), body: JSON.stringify({}) },
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
  // DELETE requests with a body are unusual but harmless here — this is
  // purely to dodge the same 415, not because the endpoint needs any data;
  // everything it needs is already in the route.
  const res = await fetch(
    `${API_BASE}/guest-drafts/${guestDraftId}/picks/${playOrder}`,
    { method: 'DELETE', headers: authHeaders(accessToken), body: JSON.stringify({}) },
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

// ── Creation ──────────────────────────────────────────────────────────────────
// CreateGuestDraftCommandHandler now builds the board INSIDE the create call —
// UseFixedBoardLayout/SetCustomPositions both run synchronously in the handler
// off request.Positions/request.NumberOfPicks. There is no separate
// "set board" step anymore; the two calls that used to do that
// (setFixedGuestDraftBoardLayout, setCustomGuestDraftPositions) are gone.
// Fixed types (Standard/MiniSuper) ignore `positions`/`numberOfPicks` entirely
// server-side (GuestDraftBoardTemplates supplies the real layout) — send an
// empty array and 0 for those rather than omitting the fields, since
// CreateGuestDraftRequest.NumberOfPicks is `required`.

export async function createGuestDraft(
  accessToken: string,
  args: {
    title: string;
    type: string;
    draftDate?: string | null; // yyyy-MM-dd, matches DateOnly? on the wire
    numberOfPicks: number;
    positions: GuestDraftPositionInput[];
  },
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

// ── Participants ──────────────────────────────────────────────────────────────
// Both endpoints take a GuestDrafterPublicId — the registered GuestDrafter's
// own public id, the same value that shows up as
// GuestDraftGameplayParticipantResponse.participantPublicId once someone's been added.
// Teams aren't supported by either command yet (GuestDrafterPublicId only,
// per AddParticipantCommand.cs/AssignParticipantToPositionCommand.cs's own
// comments) — no kind field to pass.

export async function addGuestDraftParticipant(
  accessToken: string,
  guestDraftId: string,
  guestDrafterPublicId: string,
): Promise<void> {
  const res = await fetch(`${API_BASE}/guest-drafts/${guestDraftId}/participants`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ guestDrafterPublicId }),
  });
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`addGuestDraftParticipant failed: ${res.status} - ${body}`);
  }
}

export async function assignGuestDraftParticipantToPosition(
  accessToken: string,
  guestDraftId: string,
  positionPublicId: string,
  guestDrafterPublicId: string,
): Promise<void> {
  const res = await fetch(
    `${API_BASE}/guest-drafts/${guestDraftId}/positions/${positionPublicId}/assign`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify({ guestDrafterPublicId }),
    },
  );
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`assignGuestDraftParticipantToPosition failed: ${res.status} - ${body}`);
  }
}

// Replaces the paste-a-user-id stopgap now that SearchGuestDraftersQuery
// exists. GET-only, one query param (`search`), returns { publicId,
// displayName }[] — no retired flag, no team results (GuestDrafters.Search
// is GuestDrafter-only, matching AddParticipant's scope).
//
// ASSUMPTION: GuestDrafterRoutes.Search resolves to "/guest-drafters/search" —
// inferred from the module's route-naming convention (GuestDraftsRoutes.
// Participants -> "/guest-drafts/{publicId}/participants", etc.), not
// confirmed against the actual route constant. Fix this one line if it
// resolves differently.
export async function searchGuestDrafters(
  accessToken: string,
  search?: string,
): Promise<GuestDrafterSummaryResponse[]> {
  const params = search ? `?search=${encodeURIComponent(search)}` : '';
  const res = await fetch(`${API_BASE}/guest-drafters/search${params}`, {
    headers: authHeadersGet(accessToken),
  });
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`searchGuestDrafters failed: ${res.status} - ${body}`);
  }
  return res.json();
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

// ── Standalone details (completed-draft summary page) ────────────────────────
// Separate from fetchGuestDraftGameplay above — that one is the live-session
// endpoint (CallerContext, veto token counts, reveal-authorization state).
// This is GetGuestDraftDetailsQuery, a new lighter read with none of that,
// meant for /summary, not /live.
//
// ASSUMPTION: route is GuestDraftsRoutes.Summary → "/guest-drafts/{publicId}/summary",
// not yet confirmed against the real route constant.
export async function fetchGuestDraftDetails(
  accessToken: string,
  guestDraftId: string,
): Promise<GuestDraftDetailResponse> {
  const res = await fetch(`${API_BASE}/guest-drafts/${guestDraftId}/summary`, {
    headers: authHeadersGet(accessToken),
    cache: 'no-store',
  });
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`fetchGuestDraftDetails failed: ${res.status} - ${body}`);
  }
  return res.json();
}

// ── My guest drafts (landing list) ───────────────────────────────────────────
// GET-only, caller-scoped server-side (owner-or-participant) — no arbitrary
// owner/user filter exists or should exist here. `status` is optional
// (GuestDraftStatus's SmartEnum name); omitted here since the landing page
// fetches everything once and buckets client-side into Upcoming/In Progress/
// Completed, same pattern as listAdminActiveDrafts.
//
// ASSUMPTION: route resolves to "/guest-drafts/search" — same flagged-guess
// pattern as searchGuestDrafters above, not confirmed against the real route
// constant.
export async function searchMyGuestDrafts(
  accessToken: string,
  page = 1,
  pageSize = 100,
): Promise<PagedResult<GuestDraftSummaryResponse>> {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
  const res = await fetch(`${API_BASE}/guest-drafts/search?${params}`, {
    headers: authHeadersGet(accessToken),
  });
  if (!res.ok) {
    const body = await res.text();
    throw new Error(`searchMyGuestDrafts failed: ${res.status} - ${body}`);
  }
  return res.json();
}