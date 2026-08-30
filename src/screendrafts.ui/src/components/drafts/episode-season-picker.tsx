"use client";

import { SeasonEpisode } from "@/lib/tv-episode-resolve";
import { useEpisodeBrowse } from "@/lib/use-episode-browse";
import { useState } from "react";

const INPUT =
  "border border-sd-ink/20 bg-white px-3 py-1.5 text-sm font-mono text-sd-ink placeholder:text-sd-ink/40 focus:outline-none focus:border-sd-blue";

interface Props {
  accessToken: string;
  onSelect: (episode: SeasonEpisode, seriesTitle: string | null) => void;
  disabled?: boolean;
  /** Pre-fills and locks the series TMDb ID when the draft is restricted to one series. */
  fixedSeriesTmdbId?: number;
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
            className={`${INPUT} flex-1`}
            placeholder="Series TMDb ID"
            value={seriesTmdbId ?? ""}
            onChange={(e) => setSeriesTmdbId(e.target.value ? Number(e.target.value) : null)}
            disabled={disabled}
          />
        )}
        <input
          type="number"
          min={1}
          className={`${INPUT} w-28`}
          placeholder="Season #"
          value={seasonNumber ?? ""}
          onChange={(e) => setSeasonNumber(e.target.value ? Number(e.target.value) : null)}
          disabled={disabled}
        />
      </div>

      {seriesTitle && (
        <p className="text-[11px] font-mono text-sd-ink/50">{seriesTitle}</p>
      )}

      {loading && <p className="text-[11px] font-mono text-sd-ink/40">Loading season…</p>}

      {!loading && seasonNumber && episodes.length === 0 && (
        <p className="text-[11px] font-mono text-sd-ink/40">
          No episodes found for that series/season.
        </p>
      )}

      {!loading && episodes.length > 0 && (
        <div className="border border-sd-ink/10 rounded max-h-64 overflow-y-auto">
          {episodes.map((ep) => (
            <button
              key={ep.tmdbId}
              type="button"
              onClick={() => handleSelect(ep)}
              disabled={disabled}
              className="flex items-center gap-3 w-full text-left px-3 py-2 text-sm text-sd-ink hover:bg-sd-ink/5 border-b border-sd-ink/5 last:border-0 disabled:opacity-40"
            >
              <span className="font-mono text-xs text-sd-ink/50 shrink-0 w-14">
                S{String(ep.seasonNumber).padStart(2, "0")}E
                {String(ep.episodeNumber).padStart(2, "0")}
              </span>
              <span className="flex-1 min-w-0 truncate">{ep.name}</span>
              {ep.airDate && (
                <span className="text-sd-ink/40 text-[11px] font-mono shrink-0">
                  {ep.airDate}
                </span>
              )}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}