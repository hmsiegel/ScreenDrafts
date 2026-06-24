'use client';

import { useRef, useState } from "react";
import { searchMovies, type MovieSearchResult } from "@/services/movies/fetch-tmdb";

interface MovieSearchInputProps {
  onSelect: (movie: MovieSearchResult) => void;
  accessToken?: string;
  placeholder?: string;
}

export default function MovieSearchInput({
  onSelect,
  accessToken,
  placeholder = "Search movies…",
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

  return (
    <div className="relative">
      <input
        type="text"
        value={query}
        onChange={(e) => handleChange(e.target.value)}
        onBlur={() => setTimeout(() => setOpen(false), 150)}
        onFocus={() => results.length > 0 && setOpen(true)}
        placeholder={placeholder}
        className="w-full border border-sd-ink/20 bg-white px-3 py-2 text-sm font-mono text-sd-ink placeholder:text-sd-ink/40 focus:outline-none focus:border-sd-blue"
      />
      {open && (
        <ul className="absolute z-50 left-0 right-0 top-full border border-sd-ink/20 bg-white shadow-lg max-h-64 overflow-y-auto">
          {results.map((movie) => (
            <li key={movie.tmdbId}>
              <button
                type="button"
                onMouseDown={() => handleSelect(movie)}
                className="flex items-center gap-3 w-full px-3 py-2 text-left hover:bg-sd-paper text-sm text-sd-ink"
              >
                <span className="font-medium">{movie.title}</span>
                <span className="font-mono text-sd-ink/50 text-xs">{movie.year ?? ""}</span>
              </button>
            </li>
          ))}
          {hasMore && (
            <li>
              <button
                type="button"
                onMouseDown={handleLoadMore}
                disabled={loadingMore}
                className="w-full px-3 py-2 text-center font-mono text-xs text-sd-blue hover:bg-sd-paper disabled:opacity-50 border-t border-sd-ink/10"
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