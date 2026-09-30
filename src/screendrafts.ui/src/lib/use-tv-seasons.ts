// lib/use-tv-seasons.ts
"use client";

import { useEffect, useRef, useState } from "react";
import type { TvSeasonResult } from "@/lib/dto";
import { browseTvSeasons } from "@/lib/tv-episode-resolve";

/**
 * Sibling to use-episode-browse.ts. Loads a series' seasons whenever the
 * series changes, aborting a still-in-flight request if the person switches
 * series quickly.
 */
export function useTvSeasons(seriesTmdbId: number | null, accessToken: string) {
  const [seasons, setSeasons] = useState<TvSeasonResult[]>([]);
  const [seriesTitle, setSeriesTitle] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const abortRef = useRef<AbortController | null>(null);

  useEffect(() => {
    if (!seriesTmdbId) {
      abortRef.current?.abort();
      setSeasons([]);
      setSeriesTitle(null);
      setLoading(false);
      return;
    }

    const controller = new AbortController();
    abortRef.current?.abort();
    abortRef.current = controller;
    setLoading(true);

    browseTvSeasons(seriesTmdbId, accessToken, controller.signal)
      .then((result) => {
        if (controller.signal.aborted) return;
        setSeasons(result.seasons);
        setSeriesTitle(result.seriesTitle);
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false);
      });

    return () => controller.abort();
  }, [seriesTmdbId, accessToken]);

  return { seasons, seriesTitle, loading };
}