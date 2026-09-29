// src/lib/calendar-links.ts

export interface CalendarEvent {
  /** Stable per event (e.g. a draft part / guest draft public id) so re-adding updates instead of duplicating. */
  uid: string;
  title: string;
  startUtc: Date;
  durationMinutes: number;
  /** Absolute URL back to the draft page. */
  url: string;
  description?: string;
}

// ── Durations ────────────────────────────────────────────────────────────────
// Keyed by DraftType name. Standard/MiniSuper 2h, MiniMega 3h, Mega/Super 4h,
// SpeedDraft 45m (SpeedDraft exists in canonical only).
const DURATION_MINUTES_BY_TYPE_NAME: Record<string, number> = {
  Standard: 120,
  MiniMega: 180,
  Mega: 240,
  Super: 240,
  MiniSuper: 120,
  SpeedDraft: 45,
};

// Canonical's MyDraftSummary.draftType is a raw int. Mirrors DraftType.cs
// (Drafts module): Standard=0, MiniMega=1, Mega=2, Super=3, MiniSuper=4,
// SpeedDraft=5. If a value is ever added to that enum, add it here too.
const CANONICAL_TYPE_NAME_BY_VALUE: Record<number, string> = {
  0: 'Standard',
  1: 'MiniMega',
  2: 'Mega',
  3: 'Super',
  4: 'MiniSuper',
  5: 'SpeedDraft',
};

export const DEFAULT_DURATION_MINUTES = 120;

/** Accepts canonical's raw int or GuestDrafts' type name string. */
export function draftDurationMinutes(type: string | number | null | undefined): number {
  const name = typeof type === 'number' ? CANONICAL_TYPE_NAME_BY_VALUE[type] : type;
  return (name ? DURATION_MINUTES_BY_TYPE_NAME[name] : undefined) ?? DEFAULT_DURATION_MINUTES;
}

// ── Parsing ──────────────────────────────────────────────────────────────────

/**
 * Parses a scheduledForUtc value. If a string has no zone designator, it is
 * treated as UTC — without this, `new Date("2026-10-05T18:30:00")` would be
 * read as the viewer's local time and shift the event by their UTC offset.
 */
export function parseScheduledUtc(value: string | Date | null | undefined): Date | null {
  if (!value) return null;
  if (value instanceof Date) return isNaN(value.getTime()) ? null : value;
  const hasZone = /(Z|[+-]\d{2}:?\d{2})$/i.test(value);
  const parsed = new Date(hasZone ? value : `${value}Z`);
  return isNaN(parsed.getTime()) ? null : parsed;
}

// ── Formatting helpers ───────────────────────────────────────────────────────

function endOf(event: CalendarEvent): Date {
  return new Date(event.startUtc.getTime() + event.durationMinutes * 60_000);
}

/** 20261005T183000Z */
function toCompactUtc(date: Date): string {
  return date
    .toISOString()
    .replace(/[-:]/g, '')
    .replace(/\.\d{3}Z$/, 'Z');
}

/** 2026-10-05T18:30:00Z */
function toIsoNoMs(date: Date): string {
  return date.toISOString().replace(/\.\d{3}Z$/, 'Z');
}

function buildDescription(event: CalendarEvent): string {
  return event.description ? `${event.description}\n\n${event.url}` : event.url;
}

// ── Web calendar links ───────────────────────────────────────────────────────

export function googleCalendarUrl(event: CalendarEvent): string {
  const params = new URLSearchParams({
    action: 'TEMPLATE',
    text: event.title,
    dates: `${toCompactUtc(event.startUtc)}/${toCompactUtc(endOf(event))}`,
    details: buildDescription(event),
  });
  return `https://calendar.google.com/calendar/render?${params.toString()}`;
}

function outlookDeeplink(base: string, event: CalendarEvent): string {
  const params = new URLSearchParams({
    path: '/calendar/action/compose',
    rru: 'addevent',
    subject: event.title,
    startdt: toIsoNoMs(event.startUtc),
    enddt: toIsoNoMs(endOf(event)),
    body: buildDescription(event),
  });
  return `${base}/calendar/0/deeplink/compose?${params.toString()}`;
}

/** Personal Outlook.com / Hotmail / Live accounts. */
export function outlookComUrl(event: CalendarEvent): string {
  return outlookDeeplink('https://outlook.live.com', event);
}

/** Work / school Microsoft 365 accounts. */
export function microsoft365Url(event: CalendarEvent): string {
  return outlookDeeplink('https://outlook.office.com', event);
}

// ── ICS ──────────────────────────────────────────────────────────────────────

function escapeIcsText(text: string): string {
  return text
    .replace(/\\/g, '\\\\')
    .replace(/;/g, '\\;')
    .replace(/,/g, '\\,')
    .replace(/\r?\n/g, '\\n');
}

/** RFC 5545 line folding: max 75 octets per line, continuation lines start with a space. */
function foldLine(line: string): string {
  const encoder = new TextEncoder();
  const out: string[] = [];
  let current = '';
  let bytes = 0;
  for (const ch of line) {
    const len = encoder.encode(ch).length;
    if (bytes + len > 75) {
      out.push(current);
      current = ` ${ch}`;
      bytes = 1 + len;
    } else {
      current += ch;
      bytes += len;
    }
  }
  out.push(current);
  return out.join('\r\n');
}

export function buildIcs(event: CalendarEvent): string {
  const lines = [
    'BEGIN:VCALENDAR',
    'VERSION:2.0',
    'PRODID:-//Screen Drafts//Add to Calendar//EN',
    'CALSCALE:GREGORIAN',
    'METHOD:PUBLISH',
    'BEGIN:VEVENT',
    `UID:${event.uid}@screen-drafts.com`,
    `DTSTAMP:${toCompactUtc(new Date())}`,
    `DTSTART:${toCompactUtc(event.startUtc)}`,
    `DTEND:${toCompactUtc(endOf(event))}`,
    `SUMMARY:${escapeIcsText(event.title)}`,
    `DESCRIPTION:${escapeIcsText(buildDescription(event))}`,
    `URL:${event.url}`,
    'END:VEVENT',
    'END:VCALENDAR',
  ];
  return `${lines.map(foldLine).join('\r\n')}\r\n`;
}

function slugify(text: string): string {
  return (
    text
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '') || 'screen-drafts-event'
  );
}

/** Browser-only: triggers a .ics file download. */
export function downloadIcs(event: CalendarEvent): void {
  const blob = new Blob([buildIcs(event)], { type: 'text/calendar;charset=utf-8' });
  const href = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = href;
  anchor.download = `${slugify(event.title)}.ics`;
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();
  setTimeout(() => URL.revokeObjectURL(href), 1000);
}