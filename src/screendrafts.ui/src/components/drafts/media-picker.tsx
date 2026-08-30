"use client";

import { useState } from "react";
import MovieSearchInput from "@/components/drafts/movie-search-input";
import { EpisodeSeasonPicker } from "@/components/drafts/episode-season-picker";
import { MEDIA_TYPE_MOVIE, MEDIA_TYPE_TV_EPISODE } from "@/lib/tv-episode-resolve";
import type { SeasonEpisode } from "@/lib/tv-episode-resolve";
import type { MovieSearchResult } from "@/services/movies/fetch-tmdb";

/**
 * Unified shape both pickers below resolve to, so the three "add a
 * movie/episode" surfaces (CandidateListEditor, DraftBoardEditor,
 * DraftPoolManager) only need to handle one onSelect contract instead of
 * three near-duplicate ones. mediaType matches the backend MediaType
 * SmartEnum value (0 = Movie, 2 = TvEpisode) — required on every add-call
 * now, not defaulted, so it always travels with the selection from here on.
 */
export interface SelectedMedia {
  tmdbId: number;
  title: string;
  year: string | null;
  mediaType: number;
  tvSeriesTmdbId?: number;
  seasonNumber?: number;
  episodeNumber?: number;
  tvSeriesTitle?: string | null;
}

interface Props {
  accessToken: string;
  onSelect: (media: SelectedMedia) => void;
  disabled?: boolean;
  /**
   * When set, this picker only ever offers TV Episode mode, pre-locked to
   * this series — pass this once the draft's RestrictedTvSeriesTmdbId is
   * available to the caller (requires an NSwag regen to surface on the
   * Draft response; until then, omit this and use the manual toggle below).
   */
  fixedSeriesTmdbId?: number;
}

/**
 * Defaults to Movie mode (matches every existing caller's prior behavior
 * exactly — MovieSearchInput, unchanged). TV Episode mode is opt-in via the
 * toggle, unless fixedSeriesTmdbId locks it on. Once the Draft response
 * exposes RestrictedTvSeriesTmdbId (post NSwag-regen), callers can pass
 * fixedSeriesTmdbId and hide the toggle entirely for drafts that are
 * already restricted to one series — not wired up yet since that field
 * doesn't exist in dto.ts as of this delivery.
 */
export function MediaPicker({ accessToken, onSelect, disabled, fixedSeriesTmdbId }: Props) {
  const [mode, setMode] = useState<"movie" | "episode">(
    fixedSeriesTmdbId !== undefined ? "episode" : "movie"
  );

  function handleMovieSelect(movie: MovieSearchResult) {
    onSelect({
      tmdbId: movie.tmdbId,
      title: movie.title,
      year: movie.year,
      mediaType: MEDIA_TYPE_MOVIE,
    });
  }

  function handleEpisodeSelect(episode: SeasonEpisode, seriesTitle: string | null) {
    onSelect({
      tmdbId: episode.tmdbId,
      title: episode.name,
      year: episode.airDate?.slice(0, 4) ?? null,
      mediaType: MEDIA_TYPE_TV_EPISODE,
      tvSeriesTmdbId: fixedSeriesTmdbId,
      seasonNumber: episode.seasonNumber,
      episodeNumber: episode.episodeNumber,
      tvSeriesTitle: seriesTitle,
    });
  }

  return (
    <div className="space-y-2">
      {fixedSeriesTmdbId === undefined && (
        <div className="flex gap-1 text-[11px] font-mono uppercase tracking-wide">
          <button
            type="button"
            onClick={() => setMode("movie")}
            className={`px-2 py-1 border ${
              mode === "movie"
                ? "border-sd-blue text-sd-blue bg-sd-blue/5"
                : "border-sd-ink/20 text-sd-ink/50 hover:text-sd-ink"
            }`}
          >
            Movie
          </button>
          <button
            type="button"
            onClick={() => setMode("episode")}
            className={`px-2 py-1 border ${
              mode === "episode"
                ? "border-sd-blue text-sd-blue bg-sd-blue/5"
                : "border-sd-ink/20 text-sd-ink/50 hover:text-sd-ink"
            }`}
          >
            TV Episode
          </button>
        </div>
      )}

      {mode === "movie" ? (
        <MovieSearchInput
          onSelect={handleMovieSelect}
          accessToken={accessToken}
          placeholder="Search movies…"
        />
      ) : (
        <EpisodeSeasonPicker
          accessToken={accessToken}
          onSelect={handleEpisodeSelect}
          disabled={disabled}
          fixedSeriesTmdbId={fixedSeriesTmdbId}
        />
      )}
    </div>
  );
}