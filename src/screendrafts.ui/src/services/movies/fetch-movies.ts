import { env } from "@/lib/env";

export interface MovieSearchResult {
  tmdbId: number;
  title: string;
  year: string | null;
  posterUrl?: string;
  overview?: string;
}

const apiBase = env.apiUrl;

export async function searchMovies(
  query: string,
  accessToken?: string
): Promise<MovieSearchResult[]> {
  if (!query.trim()) return [];

  try {
    const url = new URL(`${apiBase}/integrations/movies/search`);
    url.searchParams.set("query", query.trim());
    const res = await fetch(url.toString(), {
      headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
      cache: "no-store",
    });
    if (!res.ok) return [];
    const data = await res.json() as {
      results?: {
        tmdbId: number;
        title: string;
        year?: string | null;
        posterUrl?: string | null;
        overview?: string | null;
      }[];
    };
    return (data.results ?? []).map((r) => ({
      tmdbId: r.tmdbId,
      title: r.title,
      year: r.year ?? null,
      posterUrl: r.posterUrl ?? undefined,
      overview: r.overview ?? undefined,
    }));
  } catch (err) {
    console.error("[searchMovies]", err);
    return [];
  }
}