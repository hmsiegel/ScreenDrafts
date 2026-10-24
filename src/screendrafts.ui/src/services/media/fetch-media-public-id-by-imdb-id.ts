// src/services/media/fetch-media-public-id-by-imdb-id.ts

import { GetMediaPublicIdByImdbIdResponse } from "@/lib/dto";
import { env } from "@/lib/env";

const apiBase = env.apiUrl;

// Resolves an IMDb ID to a media public ID. Returns null when no title matches.
// Anonymous endpoint, so no auth header. Other failures throw so crawlers see a 5xx, not a 404.
export async function fetchMediaPublicIdByImdbId(imdbId: string): Promise<string | null> {
  const response = await fetch(`${apiBase}/media/by-imdb-id/${encodeURIComponent(imdbId)}`, {
    method: "GET",
    next: { revalidate: 3600 },
  });

  if (response.status === 404) return null;

  if (!response.ok) {
    throw new Error(`Request failed with status ${response.status}: ${response.statusText}`);
  }

  const body = (await response.json()) as GetMediaPublicIdByImdbIdResponse;
  return body.publicId ?? null;
}
