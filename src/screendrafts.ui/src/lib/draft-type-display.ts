const DRAFT_TYPE_DISPLAY: Record<string, string> = {
   Standard: 'Standard',
   MiniMega: 'Mini-Mega',
   Mega: 'Mega',
   Super: 'Super',
   MiniSuper: 'Mini-Super',
   SpeedDraft: 'Speed Draft'
}

// Numeric enum values from the API
const DRAFT_TYPE_BY_NUMBER: Record<number, string> = {
   0: 'Standard',
   1: 'Mini-Mega',
   2: 'Mega',
   3: 'Super',
   4: 'Mini-Super',
   5: 'Speed Draft',
}

export function formatDraftType(raw: string | null | undefined): string {
   if (!raw) return '';
   return DRAFT_TYPE_DISPLAY[raw] ?? raw;
}

export function draftTypeFromNumber(n: number | null | undefined): string {
   if (n == null) return '';
   return DRAFT_TYPE_BY_NUMBER[n] ?? String(n);
}