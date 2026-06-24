import { env } from "@/lib/env";

export const TMDB_IMAGE_BASE = "https://image.tmdb.org/t/p/w500";

export interface MovieSearchResult {
  tmdbId: number;
  title: string;
  year: string | null;
  posterUrl?: string;
  overview?: string;
}

export interface MovieSearchPage {
  results: MovieSearchResult[];
  page: number;
  totalPages: number;
  totalResults: number;
}

const apiBase = env.apiUrl;

export async function searchMovies(
  query: string,
  page: number = 1,
  accessToken?: string
): Promise<MovieSearchPage> {
  const empty: MovieSearchPage = { results: [], page: 1, totalPages: 1, totalResults: 0 };

  if (!query.trim()) return empty;

  try {
    const url = new URL(`${apiBase}/integrations/movies/search`);
    url.searchParams.set("query", query.trim());
    url.searchParams.set("page", String(page));

    const res = await fetch(url.toString(), {
      headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
      cache: "no-store",
    });

    if (!res.ok) return empty;

    const data = await res.json() as {
      results?: {
        tmdbId: number;
        title: string;
        year?: string | null;
        posterUrl?: string | null;
        overview?: string | null;
      }[];
      page?: number;
      totalPages?: number;
      totalResults?: number;
    };

    return {
      results: (data.results ?? []).map((r) => ({
        tmdbId: r.tmdbId,
        title: r.title,
        year: r.year ?? null,
        posterUrl: r.posterUrl ?? undefined,
        overview: r.overview ?? undefined,
      })),
      page: data.page ?? 1,
      totalPages: data.totalPages ?? 1,
      totalResults: data.totalResults ?? 0,
    };
  } catch (err) {
    console.error("[searchMovies]", err);
    return empty;
  }
}