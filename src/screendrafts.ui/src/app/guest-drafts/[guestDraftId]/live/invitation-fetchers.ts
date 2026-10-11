// app/guest-drafts/[guestDraftId]/live/invitation-fetchers.ts
// Types below come from the NSwag-generated @/lib/dto — they exist after you
// regenerate the client against the new endpoints.
import type {
  GuestDraftInvitationResponse,
  InviteGuestDrafterResponse,
  ListGuestDraftInvitationsResponse,
} from '@/lib/dto';

const API_BASE = process.env.NEXT_PUBLIC_API_URL;

/** Mirrors Invitation.MaxPerDraft on the backend. */
export const MAX_INVITATIONS = 10;

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  };
}

function authHeadersGet(accessToken: string): HeadersInit {
  return { Authorization: `Bearer ${accessToken}` };
}

// Backend errors arrive as ProblemDetails-style JSON or plain text; surface the
// most readable part rather than "failed: 409 - {...}".
async function readError(res: Response, fallback: string): Promise<Error> {
  const body = await res.text();
  try {
    const parsed = JSON.parse(body) as { detail?: string; title?: string };
    return new Error(parsed.detail ?? parsed.title ?? fallback);
  } catch {
    return new Error(body || fallback);
  }
}

export async function inviteGuestDrafter(
  accessToken: string,
  guestDraftId: string,
  email: string,
  name: string,
): Promise<InviteGuestDrafterResponse> {
  const res = await fetch(`${API_BASE}/guest-drafts/${guestDraftId}/invitations`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ email, name }),
  });
  if (!res.ok) throw await readError(res, `Invite failed (${res.status}).`);
  return res.json();
}

export async function listGuestDraftInvitations(
  accessToken: string,
  guestDraftId: string,
): Promise<GuestDraftInvitationResponse[]> {
  const res = await fetch(`${API_BASE}/guest-drafts/${guestDraftId}/invitations`, {
    headers: authHeadersGet(accessToken),
    cache: 'no-store',
  });
  if (!res.ok) throw await readError(res, `Could not load invitations (${res.status}).`);
  const data: ListGuestDraftInvitationsResponse = await res.json();
  return data.invitations ?? [];
}

export async function resendGuestDraftInvitation(
  accessToken: string,
  guestDraftId: string,
  invitationPublicId: string,
): Promise<void> {
  const res = await fetch(
    `${API_BASE}/guest-drafts/${guestDraftId}/invitations/${invitationPublicId}/resend`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify({}),
    },
  );
  if (!res.ok) throw await readError(res, `Resend failed (${res.status}).`);
}
