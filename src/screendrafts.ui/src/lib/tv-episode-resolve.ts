// New. Sibling to movie-resolve.ts, but for TV episodes. Two structural
// differences from the movie flow, both forced by what TMDb actually offers:
//
// 1. There's no keyword search over episode titles — searchMovies() has no
//    episode equivalent. browseSeasonEpisodes() lists a season instead; the
//    picker UI is "pick a season, see what's in it" rather than "type and
//    get results".
// 2. /media/by-tmdb-ids (used by resolveTmdbIds, unchanged) doesn't return
//    series/season/episode metadata — that endpoint's response shape wasn't
//    touched as part of this work. So importAndResolveEpisode does NOT rely
//    on the resolve step for series/season/episode info; callers already
//    have it from whichever SeasonEpisode the person clicked in the browse
//    step, and should carry it forward themselves (see SelectedMedia in
//    media-picker.tsx) rather than expect it back from this function.

const API = process.env.NEXT_PUBLIC_API_URL;

/** MediaType SmartEnum values, mirrored from the backend (Common.Domain.MediaType). */
export const MEDIA_TYPE_MOVIE = 0;
export const MEDIA_TYPE_TV_EPISODE = 2;

export interface SeasonEpisode {
  tmdbId: number;
  name: string;
  seasonNumber: number;
  episodeNumber: number;
  airDate?: string | null;
  overview?: string | null;
  stillUrl?: string | null;
}

interface BrowseSeasonEpisodesApiResponse {
  seriesTmdbId?: number;
  seriesTitle?: string | null;
  seasonNumber?: number;
  episodes?: {
    tmdbId: number;
    name: string;
    seasonNumber: number;
    episodeNumber: number;
    airDate?: string | null;
    overview?: string | null;
    stillUrl?: string | null;
  }[];
}

export interface SeasonBrowseResult {
  seriesTitle: string | null;
  episodes: SeasonEpisode[];
}

/**
 * Lists every episode in a season. Not a search — TMDb has no working
 * keyword search over episode titles, so this is the only way to populate a
 * picklist of episode candidates.
 */
export async function browseSeasonEpisodes(
  seriesTmdbId: number,
  seasonNumber: number,
  accessToken: string,
  signal?: AbortSignal
): Promise<SeasonBrowseResult> {
  const empty: SeasonBrowseResult = { seriesTitle: null, episodes: [] };

  if (!seriesTmdbId || !seasonNumber) return empty;

  try {
    const url = new URL(`${API}/integrations/movies/tv/season-episodes`);
    url.searchParams.set("seriesTmdbId", String(seriesTmdbId));
    url.searchParams.set("seasonNumber", String(seasonNumber));

    const res = await fetch(url.toString(), {
      headers: { Authorization: `Bearer ${accessToken}` },
      cache: "no-store",
      signal,
    });

    if (!res.ok) return empty;

    const data = (await res.json()) as BrowseSeasonEpisodesApiResponse;

    return {
      seriesTitle: data.seriesTitle ?? null,
      episodes: (data.episodes ?? []).map((e) => ({
        tmdbId: e.tmdbId,
        name: e.name,
        seasonNumber: e.seasonNumber,
        episodeNumber: e.episodeNumber,
        airDate: e.airDate,
        overview: e.overview,
        stillUrl: e.stillUrl,
      })),
    };
  } catch (err) {
    if (!(err instanceof DOMException && err.name === "AbortError")) {
      console.error("[browseSeasonEpisodes]", err);
    }
    return empty;
  }
}

/**
 * Same shape/behavior as movie-resolve.ts's importAndResolve, but for a TV
 * episode: triggers the fetch (POST .../import with the full episode
 * identity, not just tmdbId) and polls /media/by-tmdb-ids until the Media
 * record exists. Returns the resolved mediaPublicId, or null on timeout —
 * same contract as importAndResolve.
 */
export async function importAndResolveEpisode(
  tmdbId: number,
  tvSeriesTmdbId: number,
  seasonNumber: number,
  episodeNumber: number,
  accessToken: string,
  timeoutMs = 10000
): Promise<string | null> {
  await fetch(`${API}/integrations/movies/import`, {
    method: "POST",
    headers: { Authorization: `Bearer ${accessToken}`, "Content-Type": "application/json" },
    body: JSON.stringify({
      tmdbId,
      mediaType: MEDIA_TYPE_TV_EPISODE,
      tvSeriesTmdbId,
      seasonNumber,
      episodeNumber,
    }),
  });

  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    await new Promise((r) => setTimeout(r, 600));
    const res = await fetch(`${API}/media/by-tmdb-ids?tmdbIds=${tmdbId}`, {
      headers: { Authorization: `Bearer ${accessToken}` },
    });
    if (res.ok) {
      const data = await res.json();
      const items: { publicId: string; tmdbId: number }[] = data.items ?? data ?? [];
      const match = items.find((i) => i.tmdbId === tmdbId);
      if (match?.publicId) return match.publicId;
    }
  }
  return null;
}