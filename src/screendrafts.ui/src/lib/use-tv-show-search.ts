"use client";

import { useEffect, useRef, useState } from "react";
import { searchTvShows, TvShowSearchResult } from "./tv-show-resolve";

/** Sibling to use-movie-search.ts — same debounce-plus-abort reasoning, just
 * against /integrations/movies/tv/search instead of /media/search. */
export function useTvShowSearch(query: string, accessToken: string) {
  const [results, setResults] = useState<TvShowSearchResult[]>([]);
  const [searching, setSearching] = useState(false);
  const abortRef = useRef<AbortController | null>(null);

  useEffect(() => {
    if (query.trim().length < 1) {
      abortRef.current?.abort();
      setResults([]);
      setSearching(false);
      return;
    }

    const handle = setTimeout(async () => {
      abortRef.current?.abort();
      const controller = new AbortController();
      abortRef.current = controller;
      setSearching(true);
      try {
        const found = await searchTvShows(query, accessToken, controller.signal);
        if (!controller.signal.aborted) {
          setResults(found);
        }
      } catch (err) {
        if (!(err instanceof DOMException && err.name === "AbortError")) {
          console.error("[useTvShowSearch]", err);
        }
      } finally {
        if (!controller.signal.aborted) {
          setSearching(false);
        }
      }
    }, 350);

    return () => {
      clearTimeout(handle);
      abortRef.current?.abort();
    };
  }, [query, accessToken]);

  return { results, searching };
}