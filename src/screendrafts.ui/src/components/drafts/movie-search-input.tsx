// components/drafts/movie-search-input.tsx
'use client';

import { useRef, useState } from "react";
import { searchMovies, type MovieSearchResult } from "@/services/movies/fetch-tmdb";
import { LIGHT_THEME, MediaPickerTheme } from "./media-picker-theme";

interface MovieSearchInputProps {
  onSelect: (movie: MovieSearchResult) => void;
  accessToken?: string;
  placeholder?: string;
  theme?: MediaPickerTheme;
}

export default function MovieSearchInput({
  onSelect,
  accessToken,
  placeholder = "Search movies…",
  theme = LIGHT_THEME,
}: MovieSearchInputProps) {
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<MovieSearchResult[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [loadingMore, setLoadingMore] = useState(false);
  const [open, setOpen] = useState(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const currentQuery = useRef("");

  function handleChange(value: string) {
    setQuery(value);
    if (debounceRef.current) clearTimeout(debounceRef.current);
    if (!value.trim()) {
      setResults([]);
      setPage(1);
      setTotalPages(1);
      setOpen(false);
      return;
    }
    debounceRef.current = setTimeout(async () => {
      currentQuery.current = value.trim();
      const data = await searchMovies(value.trim(), 1, accessToken);
      if (currentQuery.current !== value.trim()) return;
      setResults(data.results);
      setPage(data.page);
      setTotalPages(data.totalPages);
      setOpen(data.results.length > 0);
    }, 300);
  }

    async function handleLoadMore() {
    const nextPage = page + 1;
    setLoadingMore(true);
    try {
      const data = await searchMovies(currentQuery.current, nextPage, accessToken);
      setResults((prev) => [...prev, ...data.results]);
      setPage(data.page);
      setTotalPages(data.totalPages);
    } finally {
      setLoadingMore(false);
    }
  }

  function handleSelect(movie: MovieSearchResult) {
    onSelect(movie);
    setQuery("");
    setResults([]);
    setPage(1);
    setTotalPages(1);
    setOpen(false);
  }

  const hasMore = page < totalPages;
  const panelClass = theme.overlayPanel
    ? `absolute z-50 left-0 right-0 top-full ${theme.panelWrapper}`
    : `${theme.panelWrapper}`;

  return (
    <div className="relative">
      <input
        type="text"
        value={query}
        onChange={(e) => handleChange(e.target.value)}
        onBlur={() => setTimeout(() => setOpen(false), 150)}
        onFocus={() => results.length > 0 && setOpen(true)}
        placeholder={placeholder}
        className={`w-full ${theme.input}`}
      />
      {open && (
        <ul className={panelClass}>
          {results.map((movie) => (
            <li key={movie.tmdbId}>
              <button
                type="button"
                onMouseDown={() => handleSelect(movie)}
                className={`flex items-center gap-3 w-full px-3 py-2 text-left text-sm ${theme.row}`}
              >
                <span className="font-medium">{movie.title}</span>
                <span className={theme.rowMuted}>{movie.year ?? ""}</span>
              </button>
            </li>
          ))}
          {hasMore && (
            <li>
              <button
                type="button"
                onMouseDown={handleLoadMore}
                disabled={loadingMore}
                className={`w-full px-3 py-2 text-center font-mono text-xs ${theme.accentText} hover:opacity-80 disabled:opacity-50 border-t ${theme.border}`}
              >
                {loadingMore ? "Loading…" : `Load more (page ${page + 1} of ${totalPages})`}
              </button>
            </li>
          )}
        </ul>
      )}
    </div>
  );
}