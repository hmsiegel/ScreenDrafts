// app/guest-drafts/[guestDraftId]/setup/invite-by-email-section.tsx
'use client';

import { useCallback, useEffect, useState } from 'react';
import { useGuestDraftLive } from '../live/guest-draft-context';
import {
  MAX_INVITATIONS,
  inviteGuestDrafter,
  listGuestDraftInvitations,
  resendGuestDraftInvitation,
} from '../live/invitation-fetchers';
import type { GuestDraftInvitationResponse } from '@/lib/dto';

const INPUT =
  'border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full';
const LABEL = 'block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1';
const SECTION_HEADING =
  'font-oswald font-bold text-[18px] tracking-wide uppercase text-sd-ink mb-4 pb-2 border-b border-sd-ink/10';

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

interface Props {
  accessToken: string;
  guestDraftId: string;
}

// For people without an account: store an invite, email a sign-up link, and the
// backend adds them as a participant when they register with that address.
export function InviteByEmailSection({ accessToken, guestDraftId }: Props) {
  const { isOwner } = useGuestDraftLive();

  const [invitations, setInvitations] = useState<GuestDraftInvitationResponse[]>([]);
  const [email, setEmail] = useState('');
  const [name, setName] = useState('');
  const [sending, setSending] = useState(false);
  const [resending, setResending] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setInvitations(await listGuestDraftInvitations(accessToken, guestDraftId));
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Could not load invitations.');
    }
  }, [accessToken, guestDraftId]);

  useEffect(() => {
    if (isOwner) void load();
  }, [isOwner, load]);

  if (!isOwner) return null;

  const atCap = invitations.length >= MAX_INVITATIONS;
  const canSend = EMAIL_PATTERN.test(email.trim()) && name.trim() !== '' && !atCap && !sending;

  async function handleInvite() {
    if (!canSend) return;
    setSending(true);
    setError(null);
    setNotice(null);
    try {
      await inviteGuestDrafter(accessToken, guestDraftId, email.trim(), name.trim());
      setNotice(`Invitation sent to ${email.trim()}.`);
      setEmail('');
      setName('');
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to send invitation.');
    } finally {
      setSending(false);
    }
  }

  async function handleResend(invitationPublicId: string, to: string) {
    setResending(invitationPublicId);
    setError(null);
    setNotice(null);
    try {
      await resendGuestDraftInvitation(accessToken, guestDraftId, invitationPublicId);
      setNotice(`Invitation resent to ${to}.`);
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to resend invitation.');
    } finally {
      setResending(null);
    }
  }

  return (
    <section>
      <h2 className={SECTION_HEADING}>Invite By Email</h2>
      <p className="text-xs text-sd-ink/60 font-mono mb-4">
        For people without an account. They get a sign-up link and join this draft when they
        register with the same email. Already registered? Use search above.
      </p>

      <div className="grid gap-3 sm:grid-cols-2 mb-3">
        <div>
          <label className={LABEL}>Name</label>
          <input
            type="text"
            className={INPUT}
            value={name}
            maxLength={100}
            onChange={(e) => setName(e.target.value)}
            placeholder="e.g. Clay Smith"
          />
        </div>
        <div>
          <label className={LABEL}>Email</label>
          <input
            type="email"
            className={INPUT}
            value={email}
            maxLength={254}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="name@example.com"
          />
        </div>
      </div>

      <button
        type="button"
        onClick={handleInvite}
        disabled={!canSend}
        className="w-full sm:w-auto min-h-11 sm:min-h-0 px-6 py-2.5 bg-sd-ink text-white font-oswald text-sm tracking-widest uppercase hover:bg-sd-ink/80 disabled:opacity-40 transition-colors"
      >
        {sending ? 'SENDING…' : 'SEND INVITE'}
      </button>

      {atCap && (
        <p className="text-sd-ink/60 text-xs font-mono mt-3">
          Invitation limit reached ({MAX_INVITATIONS} per draft).
        </p>
      )}
      {error && <p className="text-sd-red text-xs font-mono mt-3">{error}</p>}
      {notice && <p className="text-green-700 text-xs font-mono mt-3">{notice}</p>}

      {invitations.length > 0 && (
        <ul className="mt-5 space-y-2">
          {invitations.map((inv) => (
            <li
              key={inv.publicId}
              className="flex flex-wrap items-center gap-x-3 gap-y-2 border border-sd-ink/10 p-3"
            >
              <div className="min-w-0 flex-1">
                <p className="text-sm text-sd-ink [overflow-wrap:anywhere]">{inv.name}</p>
                <p className="text-[11px] text-sd-ink/50 font-mono [overflow-wrap:anywhere]">
                  {inv.email}
                </p>
              </div>
              <span
                className={`text-[11px] font-mono uppercase tracking-wide ${
                  inv.status === 'Accepted' ? 'text-green-700' : 'text-sd-ink/60'
                }`}
              >
                {inv.status}
              </span>
              {inv.status === 'Pending' && (
                <button
                  type="button"
                  onClick={() => handleResend(inv.publicId, inv.email)}
                  disabled={!inv.canResend || resending === inv.publicId}
                  title={
                    inv.canResend || !inv.resendAvailableOnUtc
                      ? undefined
                      : `Available ${new Date(inv.resendAvailableOnUtc).toLocaleString()}`
                  }
                  className="text-sd-blue text-xs font-mono uppercase tracking-wide hover:underline disabled:opacity-40 disabled:no-underline"
                >
                  {resending === inv.publicId ? 'Resending…' : 'Resend'}
                </button>
              )}
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
