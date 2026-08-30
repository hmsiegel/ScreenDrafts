// Sibling to movie-resolve.ts's searchMovies, but for TV shows — hits TMDb's
// /search/tv (via the new /integrations/movies/tv/search endpoint), not
// /search/movie. This is genuinely a keyword search (unlike episodes —
// TMDb has no working title search over individual episodes, but it does
// have one over shows, same as movies). Used to pick which series a draft
// as a whole is restricted to (create/edit draft forms) — a distinct
// concern from tv-episode-resolve.ts's browseSeasonEpisodes, which lists
// episodes once a series is already known.

const API = process.env.NEXT_PUBLIC_API_URL;

export interface TvShowSearchResult {
  tmdbId: number;
  title: string;
  year: string | null;
  posterUrl?: string | null;
  overview?: string | null;
}

export async function searchTvShows(
  query: string,
  accessToken: string,
  signal?: AbortSignal
): Promise<TvShowSearchResult[]> {
  const trimmed = query.trim();
  if (trimmed.length < 1) return [];

  try {
    const url = new URL(`${API}/integrations/movies/tv/search`);
    url.searchParams.set("query", trimmed);

    const res = await fetch(url.toString(), {
      headers: { Authorization: `Bearer ${accessToken}` },
      cache: "no-store",
      signal,
    });

    if (!res.ok) return [];

    const data = (await res.json()) as {
      results?: {
        tmdbId?: number;
        title: string;
        year?: string | null;
        posterUrl?: string | null;
        overview?: string | null;
      }[];
    };

    return (data.results ?? [])
      .filter((r) => r.tmdbId != null)
      .map((r) => ({
        tmdbId: r.tmdbId!,
        title: r.title,
        year: r.year ?? null,
        posterUrl: r.posterUrl ?? null,
        overview: r.overview ?? null,
      }));
  } catch (err) {
    if (!(err instanceof DOMException && err.name === "AbortError")) {
      console.error("[searchTvShows]", err);
    }
    return [];
  }
}