import { env } from "@/lib/env";

// NOTE: awaiting NSwag regen — MovieSearchResult will be generated once backend adds movie search endpoint
export interface MovieSearchResult {
  tmdbId: number;
  title: string;
  year: number;
  posterUrl?: string;
}

const apiBase = env.apiUrl;

export async function searchMovies(query: string): Promise<MovieSearchResult[]> {
  // TODO: confirm endpoint and response shape
  try {
    const url = new URL(`${apiBase}/movies`);
    url.searchParams.set("search", query);
    const res = await fetch(url.toString(), { cache: "no-store" });
    if (!res.ok) return [];
    const data = (await res.json()) as { items?: MovieSearchResult[] };
    return data.items ?? [];
  } catch (err) {
    console.error("[searchMovies]", err);
    return [];
  }
}

export const TMDB_IMAGE_BASE = "https://image.tmdb.org/t/p/w92";
