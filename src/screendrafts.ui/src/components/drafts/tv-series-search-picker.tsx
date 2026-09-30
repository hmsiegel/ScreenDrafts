// components/drafts/tv-series-search-picker.tsx
"use client";

import { useState } from "react";
import { useTvShowSearch } from "@/lib/use-tv-show-search";
import type { TvShowSearchResult } from "@/lib/tv-show-resolve";
import type { MediaPickerTheme } from "@/components/drafts/media-picker-theme";

const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";

/** Original light-form styling, used when no theme is passed. */
const DEFAULT_STYLES = {
  input: INPUT,
  panel: "border border-sd-ink/10 rounded mt-2 max-h-48 overflow-y-auto",
  row: "text-sd-ink hover:bg-sd-ink/5",
  border: "border-sd-ink/5",
  year: "text-sd-ink/50 ml-1.5 text-[11px] font-mono",
  searching: "text-sd-ink/40",
};

interface Props {
  accessToken: string;
  onSelect: (show: TvShowSearchResult) => void;
  disabled?: boolean;
  placeholder?: string;
  /**
   * Optional. Omit to keep the original light styling (create/edit draft
   * forms render identically to before). Pass DARK_THEME for dark surfaces
   * like the live draft picker.
   */
  theme?: MediaPickerTheme;
}

/**
 * Debounced TV show search + click-to-select, mirroring MovieSearchPicker
 * exactly (same underlying pattern as useMovieSearch, just against
 * useTvShowSearch/searchTvShows instead). Used to pick which series a
 * draft's TvSeriesRestriction is set to, and as the first step of
 * EpisodeSeasonPicker — "search and pick" rather than hand-typing a TMDb ID,
 * same "never hand-type an external ID" convention every other picker in this
 * codebase follows (MovieSearchInput, PersonSubjectPicker, DrafterPicker).
 */
export function TvSeriesSearchPicker({
  accessToken,
  onSelect,
  disabled,
  placeholder = "Search TV shows…",
  theme,
}: Props) {
  const [query, setQuery] = useState("");
  const { results, searching } = useTvShowSearch(query, accessToken);

  const s = theme
    ? {
        input: `${theme.input} w-full`,
        panel: theme.panelWrapper,
        row: theme.row,
        border: theme.border,
        year: `ml-1.5 ${theme.rowMuted}`,
        searching: theme.mutedText,
      }
    : DEFAULT_STYLES;

  function handleSelect(show: TvShowSearchResult) {
    onSelect(show);
    setQuery("");
  }

  return (
    <div className="w-full">
      <input
        type="text"
        className={s.input}
        placeholder={placeholder}
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        disabled={disabled}
      />
      {searching && (
        <p className={`text-[11px] font-mono ${s.searching} mt-1 px-1`}>Searching…</p>
      )}
      {!searching && results.length > 0 && (
        <div className={s.panel}>
          {results.map((show) => (
            <button
              key={show.tmdbId}
              type="button"
              onClick={() => handleSelect(show)}
              disabled={disabled}
              className={`w-full text-left px-3 py-2 text-sm ${s.row} border-b ${s.border} last:border-0 disabled:opacity-40`}
            >
              {show.title}
              {show.year && <span className={s.year}>({show.year})</span>}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}