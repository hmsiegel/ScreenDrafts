"use client";

import { useState } from "react";
import { useTvShowSearch } from "@/lib/use-tv-show-search";
import type { TvShowSearchResult } from "@/lib/tv-show-resolve";

const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";

interface Props {
  accessToken: string;
  onSelect: (show: TvShowSearchResult) => void;
  disabled?: boolean;
  placeholder?: string;
}

/**
 * Debounced TV show search + click-to-select, mirroring MovieSearchPicker
 * exactly (same underlying pattern as useMovieSearch, just against
 * useTvShowSearch/searchTvShows instead). Used to pick which series a
 * draft's TvSeriesRestriction is set to — "search and pick" rather than
 * hand-typing a TMDb ID, same "never hand-type an external ID" convention
 * every other picker in this codebase follows (MovieSearchInput,
 * PersonSubjectPicker, DrafterPicker).
 */
export function TvSeriesSearchPicker({
  accessToken,
  onSelect,
  disabled,
  placeholder = "Search TV shows…",
}: Props) {
  const [query, setQuery] = useState("");
  const { results, searching } = useTvShowSearch(query, accessToken);

  function handleSelect(show: TvShowSearchResult) {
    onSelect(show);
    setQuery("");
  }

  return (
    <div className="w-full">
      <input
        type="text"
        className={INPUT}
        placeholder={placeholder}
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        disabled={disabled}
      />
      {searching && (
        <p className="text-[11px] font-mono text-sd-ink/40 mt-1 px-1">Searching…</p>
      )}
      {!searching && results.length > 0 && (
        <div className="border border-sd-ink/10 rounded mt-2 max-h-48 overflow-y-auto">
          {results.map((s) => (
            <button
              key={s.tmdbId}
              type="button"
              onClick={() => handleSelect(s)}
              disabled={disabled}
              className="w-full text-left px-3 py-2 text-sm text-sd-ink hover:bg-sd-ink/5 border-b border-sd-ink/5 last:border-0 disabled:opacity-40"
            >
              {s.title}
              {s.year && (
                <span className="text-sd-ink/50 ml-1.5 text-[11px] font-mono">
                  ({s.year})
                </span>
              )}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
