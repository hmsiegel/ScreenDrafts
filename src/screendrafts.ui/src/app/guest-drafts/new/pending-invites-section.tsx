// app/guest-drafts/new/pending-invites-section.tsx
'use client';

import { useState } from 'react';
import { MAX_INVITATIONS } from '../[guestDraftId]/live/invitation-fetchers';

export interface PendingInvite {
  email: string;
  name: string;
}

const INPUT =
  'border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full';
const LABEL = 'block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1';
const SECTION_HEADING =
  'font-oswald font-bold text-[18px] tracking-wide uppercase text-sd-ink mb-4 pb-2 border-b border-sd-ink/10';

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

interface Props {
  invites: PendingInvite[];
  onChange: (next: PendingInvite[]) => void;
}

// Collects invites locally. The draft doesn't exist yet, so handleCreate sends
// them after creation, one call each.
export function PendingInvitesSection({ invites, onChange }: Props) {
  const [email, setEmail] = useState('');
  const [name, setName] = useState('');

  const normalized = email.trim().toLowerCase();
  const duplicate = invites.some((i) => i.email === normalized);
  const atCap = invites.length >= MAX_INVITATIONS;
  const canAdd = EMAIL_PATTERN.test(normalized) && name.trim() !== '' && !duplicate && !atCap;

  function handleAdd() {
    if (!canAdd) return;
    onChange([...invites, { email: normalized, name: name.trim() }]);
    setEmail('');
    setName('');
  }

  return (
    <section>
      <h2 className={SECTION_HEADING}>Invite By Email (Optional)</h2>
      <p className="text-xs text-sd-ink/60 font-mono mb-4">
        For people without an account. They get a sign-up link after you create the draft.
        Already registered? Use search above.
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
          />
        </div>
      </div>

      <button
        type="button"
        onClick={handleAdd}
        disabled={!canAdd}
        className="w-full sm:w-auto min-h-11 sm:min-h-0 px-4 py-2 border border-sd-ink text-sd-ink font-oswald text-sm tracking-widest uppercase hover:bg-sd-ink/5 disabled:opacity-40 transition-colors"
      >
        Add Invite
      </button>

      {duplicate && (
        <p className="text-sd-red text-xs font-mono mt-3">That email is already on the list.</p>
      )}
      {atCap && (
        <p className="text-sd-ink/60 text-xs font-mono mt-3">
          Invitation limit reached ({MAX_INVITATIONS} per draft).
        </p>
      )}

      {invites.length > 0 && (
        <div className="flex flex-wrap gap-1.5 mt-4">
          {invites.map((i) => (
            <span
              key={i.email}
              className="inline-flex items-center gap-1 px-2 py-0.5 bg-sd-ink text-white text-[11px] font-mono rounded"
            >
              {i.name} · {i.email}
              <button
                type="button"
                aria-label={`Remove invite for ${i.email}`}
                onClick={() => onChange(invites.filter((x) => x.email !== i.email))}
                className="ml-1 hover:text-sd-red"
              >
                ×
              </button>
            </span>
          ))}
        </div>
      )}
    </section>
  );
}
