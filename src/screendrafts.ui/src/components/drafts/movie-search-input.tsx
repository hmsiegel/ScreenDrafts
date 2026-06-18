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
  const [open, setOpen] = useState(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  function handleChange(value: string) {
    setQuery(value);
    if (debounceRef.current) clearTimeout(debounceRef.current);
    if (!value.trim()) {
      setResults([]);
      setOpen(false);
      return;
    }
    debounceRef.current = setTimeout(async () => {
      const items = await searchMovies(value.trim(), accessToken);
      setResults(items);
      setOpen(items.length > 0);
    }, 300);
  }

  function handleSelect(movie: MovieSearchResult) {
    onSelect(movie);
    setQuery("");
    setResults([]);
    setOpen(false);
  }

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
        </ul>
      )}
    </div>
  );
}