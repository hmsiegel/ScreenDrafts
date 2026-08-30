"use client";

import { useEffect, useRef, useState } from "react";
import { browseSeasonEpisodes, type SeasonEpisode } from "@/lib/tv-episode-resolve";

/**
 * Sibling to use-movie-search.ts. Triggered by seriesTmdbId + seasonNumber
 * both being present and valid, rather than debounced text input — there's
 * no text to debounce, the person is picking a season number, not typing a
 * query. Still cancels a still-in-flight request when season changes fast
 * (e.g. someone clicking through season 1, 2, 3 quickly), same reasoning as
 * the movie search hook.
 */
export function useEpisodeBrowse(
  seriesTmdbId: number | null,
  seasonNumber: number | null,
  accessToken: string
) {
  const [episodes, setEpisodes] = useState<SeasonEpisode[]>([]);
  const [seriesTitle, setSeriesTitle] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const abortRef = useRef<AbortController | null>(null);

  useEffect(() => {
    if (!seriesTmdbId || !seasonNumber || seasonNumber < 1) {
      abortRef.current?.abort();
      setEpisodes([]);
      setSeriesTitle(null);
      setLoading(false);
      return;
    }

    const controller = new AbortController();
    abortRef.current?.abort();
    abortRef.current = controller;
    setLoading(true);

    browseSeasonEpisodes(seriesTmdbId, seasonNumber, accessToken, controller.signal)
      .then((result) => {
        if (controller.signal.aborted) return;
        setEpisodes(result.episodes);
        setSeriesTitle(result.seriesTitle);
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false);
      });

    return () => controller.abort();
  }, [seriesTmdbId, seasonNumber, accessToken]);

  return { episodes, seriesTitle, loading };
}