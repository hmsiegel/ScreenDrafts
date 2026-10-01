// components/drafts/episode-season-picker.tsx
"use client";

import { SeasonEpisode } from "@/lib/tv-episode-resolve";
import { useEpisodeBrowse } from "@/lib/use-episode-browse";
import type { TvShowSearchResult } from "@/lib/tv-show-resolve";
import { TvSeriesSearchPicker } from "@/components/drafts/tv-series-search-picker";
import { useState } from "react";
import { LIGHT_THEME, type MediaPickerTheme } from "./media-picker-theme";
import { useTvSeasons } from "@/lib/use-tv-seasons";

interface Props {
  accessToken: string;
  /**
   * seriesTmdbId is the third argument so existing callers that only read
   * (episode, seriesTitle) keep compiling.
   */
  onSelect: (episode: SeasonEpisode, seriesTitle: string | null, seriesTmdbId: number) => void;
  disabled?: boolean;
  /** Pre-fills and locks the series TMDb ID when the draft is restricted to one series. */
  fixedSeriesTmdbId?: number;
  theme?: MediaPickerTheme;
}

/**
 * Search a series, pick a season from a dropdown, click an episode. TMDb has
 * no working keyword search over episode titles, so the episode step is a
 * browse of GET /tv/{series}/season/{n}, not a search box.
 *
 * When fixedSeriesTmdbId is provided (draft is restricted to one series —
 * see Draft.RestrictedTvSeriesTmdbId on the backend), the series search is
 * skipped and the season dropdown loads immediately, so nobody accidentally
 * browses the wrong show mid-draft.
 */
export function EpisodeSeasonPicker({
  accessToken,
  onSelect,
  disabled,
  fixedSeriesTmdbId,
  theme = LIGHT_THEME,
}: Props) {
  const [picked, setPicked] = useState<{ tmdbId: number; title: string } | null>(null);
  const [seasonNumber, setSeasonNumber] = useState<number | null>(null);

  const seriesTmdbId = fixedSeriesTmdbId ?? picked?.tmdbId ?? null;

  const {
    seasons,
    seriesTitle: seasonsSeriesTitle,
    loading: seasonsLoading,
  } = useTvSeasons(seriesTmdbId, accessToken);
  const { episodes, loading } = useEpisodeBrowse(seriesTmdbId, seasonNumber, accessToken);

  const seriesTitle = picked?.title ?? seasonsSeriesTitle;

  function handleSeriesPick(show: TvShowSearchResult) {
    setPicked({ tmdbId: show.tmdbId, title: show.title });
    setSeasonNumber(null);
  }

  function handleChangeSeries() {
    setPicked(null);
    setSeasonNumber(null);
  }

  function handleSelect(episode: SeasonEpisode) {
    if (seriesTmdbId === null) return;
    onSelect(episode, seriesTitle, seriesTmdbId);
  }

  return (
    <div className="w-full space-y-2">
      {seriesTmdbId === null && (
        <TvSeriesSearchPicker
          accessToken={accessToken}
          onSelect={handleSeriesPick}
          disabled={disabled}
          theme={theme}
        />
      )}

      {seriesTmdbId !== null && (
        <>
          <div className="flex items-center gap-2">
            <p className={`text-[11px] ${theme.rowMuted} flex-1 min-w-0 truncate`}>
              {seriesTitle ?? "Loading series…"}
            </p>
            {fixedSeriesTmdbId === undefined && (
              <button
                type="button"
                onClick={handleChangeSeries}
                disabled={disabled}
                className={`text-[11px] font-mono ${theme.accentText} disabled:opacity-40`}
              >
                Change
              </button>
            )}
          </div>

          {seasonsLoading && (
            <p className={`text-[11px] font-mono ${theme.mutedText}`}>Loading seasons…</p>
          )}

          {!seasonsLoading && seasons.length === 0 && (
            <p className={`text-[11px] font-mono ${theme.mutedText}`}>
              No seasons found for that series.
            </p>
          )}

          {seasons.length > 0 && (
            <select
              className={`${theme.input} w-full [&>option]:text-black`}
              value={seasonNumber ?? ""}
              onChange={(e) => setSeasonNumber(e.target.value ? Number(e.target.value) : null)}
              disabled={disabled}
            >
              <option value="">Select a season…</option>
              {seasons.map((s) => (
                <option key={s.seasonNumber} value={s.seasonNumber}>
                  {s.name ?? `Season ${s.seasonNumber}`} · {s.episodeCount ?? 0} eps
                </option>
              ))}
            </select>
          )}
        </>
      )}

      {loading && <p className={`text-[11px] font-mono ${theme.mutedText}`}>Loading season…</p>}

      {!loading && seasonNumber && episodes.length === 0 && (
        <p className={`text-[11px] font-mono ${theme.mutedText}`}>
          No episodes found for that series/season.
        </p>
      )}

      {!loading && episodes.length > 0 && (
        <div className={theme.panelWrapper}>
          {episodes.map((ep) => (
            <button
              key={ep.tmdbId}
              type="button"
              onClick={() => handleSelect(ep)}
              disabled={disabled}
              className={`flex items-center gap-3 w-full text-left px-3 py-2 text-sm ${theme.row} border-b ${theme.border} last:border-0 disabled:opacity-40`}
            >
              <span className={`font-mono text-xs ${theme.mutedText} shrink-0 w-14`}>
                S{String(ep.seasonNumber).padStart(2, "0")}E
                {String(ep.episodeNumber).padStart(2, "0")}
              </span>
              <span className="flex-1 min-w-0 truncate">{ep.name}</span>
              {ep.airDate && (
                <span className={`${theme.rowMuted} shrink-0`}>{ep.airDate}</span>
              )}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}