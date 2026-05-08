const DRAFT_TYPE_DISPLAY: Record<string, string> = {
   Standard:      'Standard',
   MiniMega:      'Mini-Mega',
   Mega:          'Mega' ,
   Super:         'Super',
   MiniSuper:     'Mini-Super',
   SpeedDraft:    'Speed Draft'
}

export function formatDraftType(raw: string | null | undefined): string {
   if (!raw) return '';
   return DRAFT_TYPE_DISPLAY[raw] ?? raw;
}