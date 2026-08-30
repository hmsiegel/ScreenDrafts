// components/drafts/media-picker-theme.ts
// Shared style tokens for MediaPicker, MovieSearchInput, and
// EpisodeSeasonPicker. Exists because those three components were built for
// the light admin surfaces (CandidateListEditor, DraftBoardEditor,
// DraftPoolManager, the seeding wizard) with sd-ink/sd-paper classes baked
// directly into their JSX, but PickSourcePanel (the live in-draft picker)
// uses a dark theme. Rather than fork three near-duplicate components — the
// exact duplication MediaPicker itself was built to avoid — each component
// takes an optional `theme` prop and falls back to LIGHT_THEME, so every
// existing caller is unaffected by this change.
//
// LIGHT_THEME's values are extracted verbatim from the pre-existing
// hardcoded classes in MovieSearchInput/EpisodeSeasonPicker/MediaPicker, so
// light-theme callers render identically to before this file existed.
//
// DARK_THEME's values are pulled from confirmed classes already present in
// pick-source-panel.tsx itself (text-sd-paper, text-white/40, text-white/30,
// border-white/10, border-white/5, hover:bg-white/5, bg-sd-red/text-sd-red
// for accents) — not invented. The one exception: `toggleActive`'s
// `bg-sd-red/5` and `toggleInactive`'s `hover:text-white/60` are new
// combinations built by mirroring LIGHT_THEME's existing pattern
// (accent-color/5 for the active fill, an existing hover-text opacity step
// for inactive) rather than confirmed from an existing dark toggle control —
// worth a quick visual check.
export interface MediaPickerTheme {
  /** Text input styling. Callers append their own width class (w-full, flex-1, w-28, …). */
  input: string;
  /** True for a floating/overlay results panel (needs its own solid background + shadow to sit above page content). False renders the panel inline in normal document flow — which is how every other tab in PickSourcePanel already renders its result lists, so no dark background color needs to be invented. */
  overlayPanel: boolean;
  /** Class string for the results panel/list wrapper itself. */
  panelWrapper: string;
  /** Hairline border color used for row dividers. */
  border: string;
  /** Row text color + hover background, applied to the row's clickable container. */
  row: string;
  /** Secondary text within a row (year, air date). */
  rowMuted: string;
  /** Standalone helper text — loading state, empty state. */
  mutedText: string;
  /** Links / accents (e.g. "load more"). */
  accentText: string;
  toggleActive: string;
  toggleInactive: string;
}

export const LIGHT_THEME: MediaPickerTheme = {
  input:
    "border border-sd-ink/20 bg-white px-3 py-2 text-sm font-mono text-sd-ink placeholder:text-sd-ink/40 focus:outline-none focus:border-sd-blue",
  overlayPanel: true,
  panelWrapper: "border border-sd-ink/20 bg-white shadow-lg max-h-64 overflow-y-auto",
  border: "border-sd-ink/10",
  row: "text-sd-ink hover:bg-sd-paper",
  rowMuted: "font-mono text-sd-ink/50 text-xs",
  mutedText: "text-sd-ink/40",
  accentText: "text-sd-blue",
  toggleActive: "border-sd-blue text-sd-blue bg-sd-blue/5",
  toggleInactive: "border-sd-ink/20 text-sd-ink/50 hover:text-sd-ink",
};

export const DARK_THEME: MediaPickerTheme = {
  input: "bg-transparent text-sd-paper text-sm font-mono placeholder:text-white/30 outline-none",
  overlayPanel: false,
  panelWrapper: "border border-white/10 rounded mt-2 max-h-64 overflow-y-auto",
  border: "border-white/5",
  row: "text-sd-paper hover:bg-white/5",
  rowMuted: "font-mono text-white/40 text-xs",
  mutedText: "text-white/30",
  accentText: "text-sd-red",
  toggleActive: "border-sd-red text-sd-red bg-sd-red/5",
  toggleInactive: "border-white/10 text-white/40 hover:text-white/60",
};