// components/drafts/episode-season-picker.tsx
"use client";

import { SeasonEpisode } from "@/lib/tv-episode-resolve";
import { useEpisodeBrowse } from "@/lib/use-episode-browse";
import { useState } from "react";
import { LIGHT_THEME, type MediaPickerTheme } from "./media-picker-theme";

interface Props {
  accessToken: string;
  onSelect: (episode: SeasonEpisode, seriesTitle: string | null) => void;
  disabled?: boolean;
  /** Pre-fills and locks the series TMDb ID when the draft is restricted to one series. */
  fixedSeriesTmdbId?: number;
  theme?: MediaPickerTheme;
}

/**
 * "Pick a season, see what's in it" — not a search box. TMDb doesn't offer a
 * working keyword search over episode titles the way it does movie titles,
 * so this is the only sane way to populate an episode candidate list: browse
 * GET /tv/{series}/season/{n} and click one, rather than type and hope.
 *
 * When fixedSeriesTmdbId is provided (draft is restricted to one series —
 * see Draft.RestrictedTvSeriesTmdbId on the backend), the series field is
 * locked so nobody accidentally browses the wrong show mid-draft.
 */
export function EpisodeSeasonPicker({
  accessToken,
  onSelect,
  disabled,
  fixedSeriesTmdbId,
  theme = LIGHT_THEME,
}: Props) {
  const [seriesTmdbId, setSeriesTmdbId] = useState<number | null>(
    fixedSeriesTmdbId ?? null
  );
  const [seasonNumber, setSeasonNumber] = useState<number | null>(null);
  const { episodes, seriesTitle, loading } = useEpisodeBrowse(
    seriesTmdbId,
    seasonNumber,
    accessToken
  );

  function handleSelect(episode: SeasonEpisode) {
    onSelect(episode, seriesTitle);
  }

  return (
    <div className="w-full space-y-2">
      <div className="flex gap-2">
        {fixedSeriesTmdbId === undefined && (
          <input
            type="number"
            className={`${theme.input} flex-1`}
            placeholder="Series TMDb ID"
            value={seriesTmdbId ?? ""}
            onChange={(e) => setSeriesTmdbId(e.target.value ? Number(e.target.value) : null)}
            disabled={disabled}
          />
        )}
        <input
          type="number"
          min={1}
          className={`${theme.input} w-28`}
          placeholder="Season #"
          value={seasonNumber ?? ""}
          onChange={(e) => setSeasonNumber(e.target.value ? Number(e.target.value) : null)}
          disabled={disabled}
        />
      </div>

      {seriesTitle && (
        <p className={`text-[11px] ${theme.rowMuted}`}>{seriesTitle}</p>
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