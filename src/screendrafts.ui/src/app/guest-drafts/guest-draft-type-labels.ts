// app/guest-drafts/guest-draft-type-labels.ts
//
// GuestDraftType.cs's 5 SmartEnum values, already exposed as strings by the
// backend (unlike canonical, which uses raw ints + draftTypeFromNumber) — so
// this is just a display-label lookup, not a number-to-name mapping.

export const GUEST_DRAFT_TYPE_LABELS: Record<string, string> = {
  Standard: 'Standard',
  MiniMega: 'Mini-Mega',
  Mega: 'Mega',
  Super: 'Super',
  MiniSuper: 'Mini-Super',
};

export function guestDraftTypeLabel(type: string): string {
  return GUEST_DRAFT_TYPE_LABELS[type] ?? type;
}