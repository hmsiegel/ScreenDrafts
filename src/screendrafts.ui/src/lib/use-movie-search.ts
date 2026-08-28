"use client";

import { useEffect, useRef, useState } from "react";
import { searchMovies, type ResolvedMovie } from "@/lib/movie-resolve";

/**
 * Debounced movie search that also cancels a still-in-flight request when a
 * newer one starts, rather than just delaying when new requests get sent.
 * Fast typing-with-corrections (type, pause, backspace, retype) can outrun a
 * plain setTimeout debounce — a request from an earlier, already-stale
 * fragment can still be in flight when a new one fires, so both hit the
 * backend. Each one is an independent shot at any per-query backend issue
 * (e.g. incomplete titles more often having zero OMDb matches), so this cuts
 * the actual request volume, not just the visible flicker.
 */
export function useMovieSearch(query: string, accessToken: string) {
  const [results, setResults] = useState<ResolvedMovie[]>([]);
  const [searching, setSearching] = useState(false);
  const abortRef = useRef<AbortController | null>(null);

  useEffect(() => {
    // >= 1 rather than >= 2 — a handful of real titles are a single
    // character (e.g. "$", TMDb 31644), and those would never be
    // searchable otherwise. Debounce + abort-on-new-query below already
    // guard against request spam, so there's no cost to allowing it.
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
        const found = await searchMovies(query, accessToken, controller.signal);
        if (!controller.signal.aborted) {
          setResults(found);
        }
      } catch (err) {
        if (!(err instanceof DOMException && err.name === "AbortError")) {
          console.error("[useMovieSearch]", err);
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