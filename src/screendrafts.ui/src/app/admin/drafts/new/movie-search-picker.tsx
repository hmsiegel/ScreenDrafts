// app/admin/drafts/new/movie-search-picker.tsx
"use client";

import { useState } from "react";
import { useMovieSearch } from "@/lib/use-movie-search";
import type { ResolvedMovie } from "@/lib/movie-resolve";

const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";

interface Props {
  accessToken: string;
  onSelect: (movie: ResolvedMovie) => void;
  disabled?: boolean;
  placeholder?: string;
}

/**
 * Debounced movie search + click-to-select results, backed by useMovieSearch
 * (the same hook the prediction-seeding wizard already uses). Anywhere a
 * person needs to attach a specific film by TMDb ID, this replaces typing
 * that ID by hand — same "search, click a result" interaction as
 * DrafterPicker, just for films instead of people. Mirrors the inline
 * pattern CommunityFilmRule's film search already uses in
 * community-section.tsx, generalized into a standalone component so
 * Booster's Champion (and anything else that needs it) doesn't have to
 * re-implement its own search box.
 */
export function MovieSearchPicker({
  accessToken,
  onSelect,
  disabled,
  placeholder = "Search movies…",
}: Props) {
  const [query, setQuery] = useState("");
  const { results, searching } = useMovieSearch(query, accessToken);

  function handleSelect(movie: ResolvedMovie) {
    onSelect(movie);
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
          {results.map((m) => (
            <button
              key={m.tmdbId}
              type="button"
              onClick={() => handleSelect(m)}
              disabled={disabled}
              className="w-full text-left px-3 py-2 text-sm text-sd-ink hover:bg-sd-ink/5 border-b border-sd-ink/5 last:border-0 disabled:opacity-40"
            >
              {m.title}
              {m.year && (
                <span className="text-sd-ink/50 ml-1.5 text-[11px] font-mono">
                  ({m.year})
                </span>
              )}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}