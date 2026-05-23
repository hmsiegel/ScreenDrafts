'use client';

import { useState } from "react";
import { GetUserResponse } from "@/lib/dto";
import AvatarUpload from "./avatar-upload";

type Tab = 'personal' | 'password' | 'social' | 'avatar';

interface ProfileFormProps {
  profile: GetUserResponse | null;
  accessToken: string | undefined;
  apiBase: string;
}

const INPUT = "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";
const LABEL = "block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1";
const BTN_PRIMARY = "bg-sd-red text-white font-oswald tracking-wide uppercase px-4 py-2 hover:bg-sd-red/90 disabled:opacity-50 transition-colors";

type Status = { type: 'success' | 'error'; message: string } | null;

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div>
      <label className={LABEL}>{label}</label>
      {children}
    </div>
  );
}

function Feedback({ status }: { status: Status }) {
  if (!status) return null;
  return (
    <p className={`text-[13px] ${status.type === 'success' ? 'text-green-700' : 'text-sd-red'}`}>
      {status.message}
    </p>
  );
}

function PersonalTab({ profile, accessToken, apiBase }: ProfileFormProps) {
  const [firstName, setFirstName] = useState(profile?.firstName ?? '');
  const [lastName, setLastName] = useState(profile?.lastName ?? '');
  const [status, setStatus] = useState<Status>(null);
  const [saving, setSaving] = useState(false);

  async function handleSave() {
    setSaving(true);
    setStatus(null);
    try {
      const res = await fetch(`${apiBase}/users/profile`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
        },
        body: JSON.stringify({ firstName, lastName }),
      });
      if (!res.ok) throw new Error(`${res.status}`);
      setStatus({ type: 'success', message: 'Profile updated.' });
    } catch {
      setStatus({ type: 'error', message: 'Failed to save. Please try again.' });
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="space-y-5">
      <Field label="First Name">
        <input className={INPUT} value={firstName} onChange={e => setFirstName(e.target.value)} />
      </Field>
      <Field label="Last Name">
        <input className={INPUT} value={lastName} onChange={e => setLastName(e.target.value)} />
      </Field>
      <div className="pt-1 flex items-center gap-4">
        <button className={BTN_PRIMARY} onClick={handleSave} disabled={saving}>
          {saving ? 'SAVING…' : 'SAVE CHANGES'}
        </button>
        <Feedback status={status} />
      </div>
    </div>
  );
}

function PasswordTab({ accessToken, apiBase }: { accessToken: string | undefined; apiBase: string }) {
  const [current, setCurrent] = useState('');
  const [next, setNext] = useState('');
  const [confirm, setConfirm] = useState('');
  const [status, setStatus] = useState<Status>(null);
  const [saving, setSaving] = useState(false);

  async function handleSave() {
    if (next !== confirm) {
      setStatus({ type: 'error', message: 'New passwords do not match.' });
      return;
    }
    if (next.length < 8) {
      setStatus({ type: 'error', message: 'New password must be at least 8 characters.' });
      return;
    }
    setSaving(true);
    setStatus(null);
    try {
      const res = await fetch(`${apiBase}/users/profile/password`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
        },
        body: JSON.stringify({ currentPassword: current, newPassword: next, confirmNewPassword: confirm }),
      });
      if (!res.ok) throw new Error(`${res.status}`);
      setStatus({ type: 'success', message: 'Password changed.' });
      setCurrent(''); setNext(''); setConfirm('');
    } catch {
      setStatus({ type: 'error', message: 'Failed to change password. Check your current password and try again.' });
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="space-y-5">
      <Field label="Current Password">
        <input type="password" className={INPUT} value={current} onChange={e => setCurrent(e.target.value)} autoComplete="current-password" />
      </Field>
      <Field label="New Password">
        <input type="password" className={INPUT} value={next} onChange={e => setNext(e.target.value)} autoComplete="new-password" />
      </Field>
      <Field label="Confirm New Password">
        <input type="password" className={INPUT} value={confirm} onChange={e => setConfirm(e.target.value)} autoComplete="new-password" />
      </Field>
      <div className="pt-1 flex items-center gap-4">
        <button className={BTN_PRIMARY} onClick={handleSave} disabled={saving}>
          {saving ? 'SAVING…' : 'CHANGE PASSWORD'}
        </button>
        <Feedback status={status} />
      </div>
    </div>
  );
}

function SocialTab({ profile, accessToken, apiBase }: ProfileFormProps) {
  const [twitter, setTwitter] = useState(profile?.twitterHandle ?? '');
  const [instagram, setInstagram] = useState(profile?.instagramHandle ?? '');
  const [letterboxd, setLetterboxd] = useState(profile?.letterboxdHandle ?? '');
  const [bluesky, setBluesky] = useState(profile?.blueskyHandle ?? '');
  const [status, setStatus] = useState<Status>(null);
  const [saving, setSaving] = useState(false);

  async function handleSave() {
    setSaving(true);
    setStatus(null);
    try {
      const res = await fetch(`${apiBase}/users/profile/social`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
        },
        body: JSON.stringify({ twitter, instagram, letterboxd, bluesky }),
      });
      if (!res.ok) throw new Error(`${res.status}`);
      setStatus({ type: 'success', message: 'Social profiles updated.' });
    } catch {
      setStatus({ type: 'error', message: 'Failed to save. Please try again.' });
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="space-y-5">
      <Field label="Twitter / X (without @)">
        <input className={INPUT} value={twitter} onChange={e => setTwitter(e.target.value)} placeholder="yourhandle" />
      </Field>
      <Field label="Instagram">
        <input className={INPUT} value={instagram} onChange={e => setInstagram(e.target.value)} placeholder="yourhandle" />
      </Field>
      <Field label="Letterboxd">
        <input className={INPUT} value={letterboxd} onChange={e => setLetterboxd(e.target.value)} placeholder="yourusername" />
      </Field>
      <Field label="Bluesky">
        <input className={INPUT} value={bluesky} onChange={e => setBluesky(e.target.value)} placeholder="yourhandle" />
      </Field>
      <div className="pt-1 flex items-center gap-4">
        <button className={BTN_PRIMARY} onClick={handleSave} disabled={saving}>
          {saving ? 'SAVING…' : 'SAVE SOCIAL PROFILES'}
        </button>
        <Feedback status={status} />
      </div>
    </div>
  );
}

const TABS: { id: Tab; label: string }[] = [
  { id: 'personal', label: 'Personal Info' },
  { id: 'password', label: 'Password' },
  { id: 'social', label: 'Social Profiles' },
  { id: 'avatar', label: 'Avatar' },
];

export default function ProfileForm({ profile, accessToken, apiBase }: ProfileFormProps) {
  const [activeTab, setActiveTab] = useState<Tab>('personal');

  return (
    <div className="bg-white border border-sd-ink/10">
      {/* Tab bar */}
      <div className="flex border-b border-sd-ink/10">
        {TABS.map(({ id, label }) => (
          <button
            key={id}
            onClick={() => setActiveTab(id)}
            className={`px-5 py-3.5 font-oswald text-[13px] tracking-wide uppercase transition-colors border-b-2 -mb-px ${
              activeTab === id
                ? 'border-sd-red text-sd-ink font-semibold'
                : 'border-transparent text-sd-ink/40 hover:text-sd-ink'
            }`}
          >
            {label}
          </button>
        ))}
      </div>

      <div className="p-7 border-b border-sd-ink/10 min-h-[320px]">
        {activeTab === 'personal' && (
          <PersonalTab profile={profile} accessToken={accessToken} apiBase={apiBase} />
        )}
        {activeTab === 'password' && (
          <PasswordTab accessToken={accessToken} apiBase={apiBase} />
        )}
        {activeTab === 'social' && (
          <SocialTab profile={profile} accessToken={accessToken} apiBase={apiBase} />
        )}
        {activeTab === 'avatar' && (
          <AvatarUpload
            currentAvatarUrl={profile?.profilePicturePath}
            displayName={[profile?.firstName, profile?.lastName].filter(Boolean).join(' ') || profile?.email || 'User'}
            accessToken={accessToken}
            apiBase={apiBase}
          />
        )}
      </div>
    </div>
  );
}
