// app/guest-drafts/guest-draft-schedule-editor.tsx
'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { setGuestDraftSchedule } from './[guestDraftId]/live/gameplay-fetchers';
import { parseScheduledUtc } from '@/lib/calendar-links';

interface Props {
  accessToken: string;
  guestDraftId: string;
  /** Current schedule from the server (ISO string at runtime, typed Date by NSwag). */
  scheduledForUtc: string | Date | null | undefined;
  /** Owners get the editor; everyone else gets read-only text. */
  isOwner: boolean;
  /**
   * Runs after a successful save. Pages with the live context pass its
   * `refetch`. Server-rendered pages (summary) omit it and get
   * `router.refresh()`.
   */
  onSaved?: () => void | Promise<unknown>;
  /** 'dark' for the live page (bg-sd-ink), 'light' everywhere else. */
  tone?: 'light' | 'dark';
}

const TONES = {
  light: {
    label: 'text-sd-ink/60',
    input: 'border-sd-ink/20 bg-sd-paper text-sd-ink focus:ring-sd-blue',
    button: 'border-sd-red text-sd-red hover:bg-sd-red hover:text-white',
    saved: 'text-green-700',
    readOnly: 'text-sd-ink',
  },
  dark: {
    label: 'text-white/50',
    input: 'border-white/20 bg-white/5 text-white focus:ring-light-blue [color-scheme:dark]',
    button: 'border-light-blue text-light-blue hover:bg-light-blue hover:text-sd-ink',
    saved: 'text-green-400',
    readOnly: 'text-white/60',
  },
} as const;

/** datetime-local wants local time as "YYYY-MM-DDTHH:mm". */
function toDatetimeLocal(date: Date | null): string {
  if (!date) return '';
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(
    date.getHours(),
  )}:${pad(date.getMinutes())}`;
}

// The backend allows the owner to change the schedule at any draft status
// (including after start/completion, e.g. to correct a finished draft's
// date), so this has no status gating of its own. There is no "clear"
// action: the command takes a non-nullable DateTime.
export function GuestDraftScheduleEditor({
  accessToken,
  guestDraftId,
  scheduledForUtc,
  isOwner,
  onSaved,
  tone = 'light',
}: Props) {
  const router = useRouter();
  const t = TONES[tone];

  const currentStart = parseScheduledUtc(scheduledForUtc);
  const currentLocal = toDatetimeLocal(currentStart);

  const [value, setValue] = useState(currentLocal);
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOwner) {
    return (
      <p className={`text-sm font-mono ${t.readOnly}`}>
        {currentStart ? currentStart.toLocaleString() : 'Not scheduled'}
      </p>
    );
  }

  async function handleSave() {
    if (saving || !value) return;
    setSaving(true);
    setSaved(false);
    setError(null);
    try {
      // datetime-local is zoneless local time; new Date() reads it as local
      // and toISOString() converts to UTC with a Z.
      await setGuestDraftSchedule(accessToken, guestDraftId, new Date(value).toISOString());
      if (onSaved) await onSaved();
      else router.refresh();
      setSaved(true);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to update schedule.');
    } finally {
      setSaving(false);
    }
  }

  return (
    <div>
      <div className="flex items-end gap-3 flex-wrap">
        <div>
          <label
            className={`block text-[11px] font-mono tracking-widest uppercase mb-1 ${t.label}`}
          >
            Scheduled For (your local time)
          </label>
          <input
            type="datetime-local"
            className={`border px-3 py-2 font-sans text-sm rounded max-w-[240px] focus:outline-none focus:ring-2 ${t.input}`}
            value={value}
            onChange={(e) => {
              setValue(e.target.value);
              setSaved(false);
            }}
          />
        </div>
        <button
          type="button"
          onClick={handleSave}
          disabled={saving || !value || value === currentLocal}
          className={`shrink-0 px-3 py-2 border font-oswald text-xs tracking-widest disabled:opacity-40 transition-colors ${t.button}`}
        >
          {saving ? '…' : 'SAVE'}
        </button>
        {saved && <span className={`text-[11px] font-mono ${t.saved}`}>saved</span>}
      </div>
      {error && <p className="text-sd-red text-xs font-mono mt-2">{error}</p>}
    </div>
  );
}