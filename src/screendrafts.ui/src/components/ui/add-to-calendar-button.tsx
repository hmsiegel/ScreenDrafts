// components/ui/add-to-calendar-button.tsx
'use client';

import { useEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import {
  downloadIcs,
  draftDurationMinutes,
  googleCalendarUrl,
  microsoft365Url,
  outlookComUrl,
  parseScheduledUtc,
  type CalendarEvent,
} from '@/lib/calendar-links';

interface Props {
  /** Stable id for the event (draft part or guest draft public id). */
  uid: string;
  title: string;
  scheduledForUtc: string | Date | null | undefined;
  /** Canonical's raw int draftType, or GuestDrafts' type name string. */
  draftType: string | number | null | undefined;
  /** Site-relative path the event links back to, e.g. "/my-drafts/d_abc123". */
  path: string;
}

const TRIGGER =
  'inline-flex items-center min-h-9 sm:min-h-0 border border-sd-ink/20 text-sd-ink font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-ink/5 shrink-0';
const ITEM =
  'block w-full text-left px-4 py-3 sm:py-2 font-mono text-[11px] tracking-widest uppercase text-sd-ink hover:bg-sd-ink/5';

// The menu renders in a portal with fixed positioning because the guest
// draft lists sit inside an `overflow-x-auto` wrapper, which would clip or
// scroll an absolutely-positioned dropdown on the last rows.
export default function AddToCalendarButton({
  uid,
  title,
  scheduledForUtc,
  draftType,
  path,
}: Props) {
  const [anchor, setAnchor] = useState<{ top: number; right: number } | null>(null);
  const buttonRef = useRef<HTMLButtonElement>(null);
  const menuRef = useRef<HTMLDivElement>(null);
  const [now] = useState(() => Date.now());

  useEffect(() => {
    if (!anchor) return;

    const close = () => setAnchor(null);
    function onPointerDown(e: PointerEvent) {
      const target = e.target as Node;
      if (buttonRef.current?.contains(target) || menuRef.current?.contains(target)) return;
      close();
    }
    function onKeyDown(e: KeyboardEvent) {
      if (e.key === 'Escape') close();
    }

    document.addEventListener('pointerdown', onPointerDown);
    document.addEventListener('keydown', onKeyDown);
    window.addEventListener('resize', close);
    window.addEventListener('scroll', close, true);
    return () => {
      document.removeEventListener('pointerdown', onPointerDown);
      document.removeEventListener('keydown', onKeyDown);
      window.removeEventListener('resize', close);
      window.removeEventListener('scroll', close, true);
    };
  }, [anchor]);

  const startUtc = parseScheduledUtc(scheduledForUtc);

  // Nothing to add if there's no schedule, or it's already in the past.
  if (!startUtc || startUtc.getTime() < now) return null;

  function toggle() {
    if (anchor) {
      setAnchor(null);
      return;
    }
    const rect = buttonRef.current?.getBoundingClientRect();
    if (rect) setAnchor({ top: rect.bottom + 4, right: window.innerWidth - rect.right });
  }

  // Only built while open, i.e. after a click — window is always defined then.
  const event: CalendarEvent | null =
    anchor && startUtc
      ? {
          uid,
          title,
          startUtc,
          durationMinutes: draftDurationMinutes(draftType),
          url: `${window.location.origin}${path}`,
        }
      : null;

  return (
    <>
      <button
        ref={buttonRef}
        type="button"
        aria-haspopup="menu"
        aria-expanded={anchor !== null}
        onClick={toggle}
        className={TRIGGER}
      >
        + Calendar
      </button>

      {anchor &&
        event &&
        createPortal(
          <div
            ref={menuRef}
            role="menu"
            style={{ position: 'fixed', top: anchor.top, right: anchor.right }}
            className="z-50 min-w-[12rem] bg-white border border-sd-ink/20 shadow-md"
          >
            <a
              role="menuitem"
              href={googleCalendarUrl(event)}
              target="_blank"
              rel="noopener noreferrer"
              onClick={() => setAnchor(null)}
              className={ITEM}
            >
              Google Calendar
            </a>
            <a
              role="menuitem"
              href={outlookComUrl(event)}
              target="_blank"
              rel="noopener noreferrer"
              onClick={() => setAnchor(null)}
              className={ITEM}
            >
              Outlook.com
            </a>
            <a
              role="menuitem"
              href={microsoft365Url(event)}
              target="_blank"
              rel="noopener noreferrer"
              onClick={() => setAnchor(null)}
              className={ITEM}
            >
              Microsoft 365
            </a>
            <button
              role="menuitem"
              type="button"
              onClick={() => {
                downloadIcs(event);
                setAnchor(null);
              }}
              className={ITEM}
            >
              Download .ics
            </button>
          </div>,
          document.body,
        )}
    </>
  );
}