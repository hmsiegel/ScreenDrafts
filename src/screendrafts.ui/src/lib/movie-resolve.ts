// Shared with app/draft-parts/[draftPartId]/live/components/pick-source-panel.tsx —
// resolveTmdbIds and importAndResolve are extracted verbatim from there rather
// than reimplemented, so both the live panel and the seed picks step stay on
// one source of truth for movie resolution. searchMovies is new — the live
// panel currently inlines the same fetch inside SearchSource's effect; worth
// pointing that at this too when convenient, though it's not required for
// seeding to work.

export interface ResolvedMovie {
  mediaPublicId: string;
  tmdbId: number;
  title: string;
  year?: string | null;
  posterUrl?: string | null;
}

interface ByTmdbIdsItem {
  publicId: string;
  tmdbId: number;
  title: string;
  year?: string | null;
  posterUrl?: string | null;
  image?: string | null;
}

const API = process.env.NEXT_PUBLIC_API_URL;

export async function resolveTmdbIds(
  tmdbIds: number[],
  accessToken: string
): Promise<ResolvedMovie[]> {
  if (tmdbIds.length === 0) return [];
  const params = tmdbIds.map((id) => `tmdbIds=${id}`).join("&");
  const res = await fetch(`${API}/media/by-tmdb-ids?${params}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });
  if (!res.ok) return [];
  const data = await res.json();
  const items: ByTmdbIdsItem[] = data.items ?? data ?? [];
  return items.map((item) => ({
    mediaPublicId: item.publicId,
    tmdbId: item.tmdbId,
    title: item.title,
    year: item.year,
    posterUrl: item.posterUrl ?? item.image ?? null,
  }));
}

export async function importAndResolve(
  tmdbId: number,
  accessToken: string,
  timeoutMs = 10000
): Promise<ResolvedMovie | null> {
  await fetch(`${API}/integrations/movies/import`, {
    method: "POST",
    headers: { Authorization: `Bearer ${accessToken}`, "Content-Type": "application/json" },
    body: JSON.stringify({ tmdbId, mediaType: 0 }),
  });

  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    await new Promise((r) => setTimeout(r, 600));
    const resolved = await resolveTmdbIds([tmdbId], accessToken);
    if (resolved.length > 0 && resolved[0].mediaPublicId) return resolved[0];
  }
  return null;
}

export async function searchMovies(
  query: string,
  accessToken: string,
  signal?: AbortSignal
): Promise<ResolvedMovie[]> {
  const trimmed = query.trim();
  if (trimmed.length < 2) return [];

  const res = await fetch(`${API}/media/search?query=${encodeURIComponent(trimmed)}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
    signal,
  });
  if (!res.ok) return [];
  const data = await res.json();
  const items: {
    tmdbId?: number;
    title: string;
    year?: string;
    posterUrl?: string;
    mediaPublicId?: string;
    isInMediaDatabase: boolean;
  }[] = data.results?.items ?? data.items ?? [];

  return items
    .filter((i) => i.tmdbId != null)
    .map((i) => ({
      mediaPublicId: i.mediaPublicId ?? "",
      tmdbId: i.tmdbId!,
      title: i.title,
      year: i.year,
      posterUrl: i.posterUrl ?? null,
    }));
}