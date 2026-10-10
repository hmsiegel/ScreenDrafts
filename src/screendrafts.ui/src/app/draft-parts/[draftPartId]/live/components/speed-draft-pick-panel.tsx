// app/draft-parts/[draftPartId]/live/components/speed-draft-pick-panel.tsx
'use client';

import { useState, useEffect, useRef } from 'react';
import { EpisodeSeasonPicker } from '@/components/drafts/episode-season-picker';
import { DARK_THEME } from '@/components/drafts/media-picker-theme';
import { searchTvShows, type TvShowSearchResult } from '@/lib/tv-show-resolve';
import {
  importAndResolveEpisode,
  MEDIA_TYPE_TV_EPISODE,
  type SeasonEpisode,
} from '@/lib/tv-episode-resolve';

// ── Types ─────────────────────────────────────────────────────────────────────
// Replace with dto.ts imports after NSwag regen.

interface FilmographyCredit {
  tmdbId: number;
  title: string;
  year?: string | null;
  posterUrl?: string | null;
  mediaType: number; // 0 Movie, 1 TvShow
  creditRole?: string | null;
  isInMediaDatabase: boolean;
  mediaPublicId?: string | null;
}

interface TitleSearchItem {
  imdbId?: string | null;
  tmdbId?: number | null;
  title: string;
  year?: string | null;
  posterUrl?: string | null;
  mediaType: number;
  isInMediaDatabase: boolean;
  mediaPublicId?: string | null;
}

const API = process.env.NEXT_PUBLIC_API_URL;

const titleKey = (item: TitleSearchItem) =>
  item.mediaPublicId || `${item.tmdbId != null ? 'tmdb' : 'imdb'}-${item.tmdbId ?? item.imdbId}`;

async function fetchTitlePage(
  query: string,
  page: number,
  loadedBefore: number,
  accessToken: string,
): Promise<{ items: TitleSearchItem[]; hasMore: boolean }> {
  const res = await fetch(
    `${API}/media/search?query=${encodeURIComponent(query)}&page=${page}`,
    { headers: { Authorization: `Bearer ${accessToken}` } },
  );
  if (!res.ok) throw new Error(`Failed to search: ${res.status}`);
  const data = await res.json();
  const paged = data.results ?? data;
  // /media/search returns mediaType as a SmartEnum object ({ name, value }),
  // not a number. Flatten it here, or "mediaType=${mediaType}" in the resolve
  // helpers becomes "[object Object]" and /media/by-tmdb-ids returns 400.
  const items: TitleSearchItem[] = (paged.items ?? []).map(
    (i: Omit<TitleSearchItem, 'mediaType'> & { mediaType?: number | { value: number } | null }) => ({
      ...i,
      mediaType: typeof i.mediaType === 'number' ? i.mediaType : (i.mediaType?.value ?? 0),
    }),
  );
  // Raw count vs TMDb's total. Do not trust hasNextPage/totalPages: the
  // handler computes them from request.PageSize, but TMDb pages are fixed at 20.
  const hasMore = items.length > 0 && loadedBefore + items.length < (paged.totalCount ?? 0);
  return { items, hasMore };
}

// ── Resolve helpers — unchanged from before, still needed for import/poll ──

const MEDIA_TYPE_TV_SHOW = 1;

async function resolveByTmdbIds(
  tmdbIds: number[],
  mediaType: number,
  accessToken: string,
): Promise<Map<number, string>> {
  if (tmdbIds.length === 0) return new Map();
  const params = tmdbIds.map((id) => `tmdbIds=${id}`).join('&') + `&mediaType=${mediaType}`;
  const res = await fetch(`${API}/media/by-tmdb-ids?${params}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });
  if (!res.ok) return new Map();
  const data = await res.json();
  const items: { publicId: string; tmdbId: number }[] = data.items ?? data ?? [];
  return new Map(items.map((i) => [i.tmdbId, i.publicId]));
}

async function resolveByImdbIds(imdbIds: string[], accessToken: string): Promise<Map<string, string>> {
  if (imdbIds.length === 0) return new Map();
  const params = imdbIds.map((id) => `imdbIds=${id}`).join('&');
  const res = await fetch(`${API}/media/by-imdb-ids?${params}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });
  if (!res.ok) return new Map();
  const data = await res.json();
  const items: { publicId: string; imdbId: string }[] = data.items ?? data ?? [];
  return new Map(items.map((i) => [i.imdbId, i.publicId]));
}

// ── Import a not-yet-in-database result, then wait for it to land ───────────
// Filmography credits are TMDb-sourced (tmdbId) now; title-search results
// can be either tmdb or imdb depending on which source matched.

async function importAndResolve(
  item: { tmdbId?: number | null; imdbId?: string | null; mediaType: number },
  accessToken: string,
  timeoutMs = 25000,
): Promise<string | null> {
  const source: 'tmdb' | 'imdb' = item.tmdbId != null ? 'tmdb' : 'imdb';

  await fetch(`${API}/integrations/movies/import`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${accessToken}`, 'Content-Type': 'application/json' },
    body: JSON.stringify({
      mediaType: item.mediaType,
      tmdbId: item.tmdbId ?? null,
      imdbId: item.imdbId ?? null,
    }),
  });

  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    await new Promise((r) => setTimeout(r, 600));

    if (source === 'tmdb' && item.tmdbId != null) {
      const resolved = await resolveByTmdbIds([item.tmdbId], item.mediaType, accessToken);
      const publicId = resolved.get(item.tmdbId);
      if (publicId) return publicId;
    } else if (item.imdbId) {
      const resolved = await resolveByImdbIds([item.imdbId], accessToken);
      const publicId = resolved.get(item.imdbId);
      if (publicId) return publicId;
    }
  }
  return null;
}

// ── Props ─────────────────────────────────────────────────────────────────────

interface ExistingPick {
  playOrder: number;
  boardPosition: number;
  wasVetoed: boolean;
  wasVetoOverridden: boolean;
}

interface Props {
  accessToken: string;
  draftPartId: string;
  subDraftId: string;
  activeSlot: number;
  callerParticipantId: string;
  callerParticipantKind: number;
  subjectKind: number; // 0 Actor, 1 Director, 2 Word
  subjectName: string;
  subjectImdbId: string | null; // set for Actor/Director, null for Word
  // This sub-draft's own picks — was previously read from useLiveDraft()'s
  // main-context `picks`, which structurally excludes every sub-draft pick
  // (the pk.sub_draft_id IS NULL filter in the main gameplay query, there
  // on purpose). That meant playOrder was always computed as 1 for every
  // single pick in every sub-draft, and slotAlreadyPicked never actually
  // caught anything. Passed down from SubDraftPanel's own `detail.picks`.
  existingPicks: ExistingPick[];
  onPickSubmitted: (playOrder: number, title: string) => void;
}

// ── Component ─────────────────────────────────────────────────────────────────

export function SpeedDraftPickPanel({
  accessToken,
  draftPartId,
  subDraftId,
  activeSlot,
  callerParticipantId,
  callerParticipantKind,
  subjectKind,
  subjectName,
  subjectImdbId,
  existingPicks,
  onPickSubmitted,
}: Props) {
  const isPersonSubject = subjectKind === 0 || subjectKind === 1;

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState<string | null>(null);

  // Person subject state
  const [personPhotoUrl, setPersonPhotoUrl] = useState<string | null>(null);
  const [credits, setCredits] = useState<FilmographyCredit[]>([]);

  // Word subject state
  const [titleResults, setTitleResults] = useState<TitleSearchItem[]>([]);
  const [titlePage, setTitlePage] = useState(1);
  const [titleLoaded, setTitleLoaded] = useState(0);
  const [titleHasMore, setTitleHasMore] = useState(false);
  const [loadingMore, setLoadingMore] = useState(false);

  const [refine, setRefine] = useState('');
  const [debouncedRefine, setDebouncedRefine] = useState('');

  useEffect(() => {
    const t = setTimeout(() => setDebouncedRefine(refine.trim()), 350);
    return () => clearTimeout(t);
  }, [refine]);

  // New sub-draft subject: drop any leftover refine text.
  useEffect(() => {
    setRefine('');
    setDebouncedRefine('');
  }, [subjectName]);

  // Person subjects filter the loaded filmography client-side, so refine text
  // stays out of the query (and out of the filmography load effect).
  const searchQuery = isPersonSubject
    ? subjectName
    : `${subjectName} ${debouncedRefine}`.trim();
  const searchQueryRef = useRef(searchQuery);
  searchQueryRef.current = searchQuery;

  // 'subject' = the subject's own list; 'episode' = browse a series' seasons
  // and episodes. drillSeries pre-locks the picker to one series (EPISODES
  // button on a TV row); null means search for any series.
  const [view, setView] = useState<'subject' | 'episode'>('subject');
  const [drillSeries, setDrillSeries] = useState<{ tmdbId: number; title: string } | null>(null);

  // Word subjects: /media/search is movies-only, so TV shows come from the
  // TMDb TV search (/integrations/movies/tv/search) behind a Movies / TV Shows
  // switch. Same query as the movie list, including the refine text.
  const [kind, setKind] = useState<'all' | 'movie' | 'tv'>(isPersonSubject ? 'all' : 'movie');
  const [tvResults, setTvResults] = useState<TvShowSearchResult[]>([]);
  const [tvLoading, setTvLoading] = useState(false);

  const visibleCredits = credits
    .filter((c) => kind === 'all' || (kind === 'tv' ? c.mediaType === 1 : c.mediaType === 0))
    .filter((c) => c.title.toLowerCase().includes(refine.trim().toLowerCase()));

  useEffect(() => {
    setKind(isPersonSubject ? 'all' : 'movie');
  }, [subjectName, isPersonSubject]);

  useEffect(() => {
    if (isPersonSubject || kind !== 'tv') return;
    const controller = new AbortController();
    setTvLoading(true);
    searchTvShows(searchQuery, accessToken, controller.signal)
      .then((r) => {
        if (!controller.signal.aborted) setTvResults(r);
      })
      .finally(() => {
        if (!controller.signal.aborted) setTvLoading(false);
      });
    return () => controller.abort();
  }, [isPersonSubject, kind, searchQuery, accessToken]);

  // Sub-draft picks have no commissioner-override concept (blocked at the
  // domain level), unlike the main-context shape this used to read from —
  // no need to check for it here.
  const slotAlreadyPicked = existingPicks.some(
    (p) => p.boardPosition === activeSlot && (!p.wasVetoed || p.wasVetoOverridden),
  );

  // Load once per subject — filmography for Actor/Director, auto-run title
  // search for Word. No typing required either way, matching the "subject
  // already picked at setup, credits just appear" design.
  useEffect(() => {
    let cancelled = false;

    async function load() {
      setLoading(true);
      setError(null);
      try {
        if (isPersonSubject && subjectImdbId) {
          const res = await fetch(
            `${API}/media/person-filmography?imdbId=${encodeURIComponent(subjectImdbId)}`,
            { headers: { Authorization: `Bearer ${accessToken}` } },
          );
          if (!res.ok) throw new Error(`Failed to load filmography: ${res.status}`);
          const data = await res.json();
          if (cancelled) return;
          setPersonPhotoUrl(data.personPhotoUrl ?? null);
          setCredits(data.credits ?? []);
        } else {
          const { items, hasMore } = await fetchTitlePage(searchQuery, 1, 0, accessToken);
          if (cancelled) return;
          setTitleResults(items);
          setTitlePage(1);
          setTitleLoaded(items.length);
          setTitleHasMore(hasMore);
        }
      } catch (e) {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Failed to load.');
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    void load();
    return () => {
      cancelled = true;
    };
  }, [isPersonSubject, subjectImdbId, subjectName, searchQuery, accessToken]);

  async function loadMoreTitles() {
    if (loadingMore || !titleHasMore) return;
    setLoadingMore(true);
    try {
      const next = titlePage + 1;
      const q = searchQuery;
      const { items, hasMore } = await fetchTitlePage(q, next, titleLoaded, accessToken);
      if (q !== searchQueryRef.current) return; // query changed mid-flight
      setTitleResults((prev) => {
        const seen = new Set(prev.map(titleKey));
        return [...prev, ...items.filter((i) => !seen.has(titleKey(i)))];
      });
      setTitlePage(next);
      setTitleLoaded(titleLoaded + items.length);
      setTitleHasMore(hasMore);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to load more titles.');
    } finally {
      setLoadingMore(false);
    }
  }

  async function submitPick(mediaPublicId: string | null, item: {
    tmdbId?: number | null;
    imdbId?: string | null;
    mediaType: number;
    tvSeriesTmdbId?: number;
    seasonNumber?: number;
    episodeNumber?: number;
  }, title: string, submittingKey: string) {
    if (slotAlreadyPicked || submitting !== null) return;
    setSubmitting(submittingKey);
    setError(null);
    try {
      let resolvedPublicId = mediaPublicId;

      if (!resolvedPublicId) {
        resolvedPublicId =
          item.mediaType === MEDIA_TYPE_TV_EPISODE &&
          item.tmdbId != null &&
          item.tvSeriesTmdbId &&
          item.seasonNumber &&
          item.episodeNumber
            ? await importAndResolveEpisode(
                item.tmdbId,
                item.tvSeriesTmdbId,
                item.seasonNumber,
                item.episodeNumber,
                accessToken,
                25000,
              )
            : await importAndResolve(item, accessToken);
        if (!resolvedPublicId) {
          setError('Title could not be imported in time. Try again in a moment.');
          return;
        }
      }

      const playOrder = existingPicks.length + 1;

      // The resolve poll above only confirms the title exists in the
      // Movies module's own table — a second, separate async hop syncs it
      // into the Drafts module's copy, which is what this call actually
      // reads from. That hop can still be in flight even after resolve
      // succeeds, especially at a fast poll interval — retrying here
      // treats "not found yet" as transient instead of failing immediately.
      const maxAttempts = 5;
      let lastErrorBody: { detail?: string } | null = null;

      for (let attempt = 1; attempt <= maxAttempts; attempt++) {
        const res = await fetch(
          `${API}/draft-parts/${draftPartId}/sub-drafts/${subDraftId}/picks`,
          {
            method: 'POST',
            headers: {
              Authorization: `Bearer ${accessToken}`,
              'Content-Type': 'application/json',
            },
            body: JSON.stringify({
              draftPartPublicId: draftPartId,
              subDraftPublicId: subDraftId,
              position: activeSlot,
              playOrder,
              participantPublicId: callerParticipantId,
              participantKind: callerParticipantKind,
              moviePublicId: resolvedPublicId,
            }),
          },
        );

        if (res.ok) {
          onPickSubmitted(playOrder, title);
          return;
        }

        lastErrorBody = await res.json().catch(() => null);

        // Only worth retrying a "not found" — the 500 is a known mismapping
        // (MovieErrors.NotFound comes back as 500, not 404, on the backend
        // today), so status code alone isn't a safe signal; checking the
        // message text avoids silently retrying an unrelated server error
        // for 5 seconds before surfacing it.
        const isTransientNotFound =
          (res.status === 404 || res.status === 500) &&
          (lastErrorBody?.detail ?? '').toLowerCase().includes('not found');
        if (!isTransientNotFound || attempt === maxAttempts) {
          throw new Error(lastErrorBody?.detail ?? `Pick failed: ${res.status}`);
        }

        await new Promise((r) => setTimeout(r, 1000));
      }
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to submit pick.');
    } finally {
      setSubmitting(null);
    }
  }

  function openEpisodes(tmdbId: number, title: string) {
    setDrillSeries({ tmdbId, title });
    setView('episode');
  }

  function pickShow(show: TvShowSearchResult) {
    void submitPick(
      null,
      { tmdbId: show.tmdbId, imdbId: null, mediaType: MEDIA_TYPE_TV_SHOW },
      show.title,
      `importing-tv-${show.tmdbId}`,
    );
  }

  function handleEpisodeSelect(ep: SeasonEpisode, seriesTitle: string | null, seriesTmdbId: number) {
    const code = `S${String(ep.seasonNumber).padStart(2, '0')}E${String(ep.episodeNumber).padStart(2, '0')}`;
    void submitPick(
      null,
      {
        tmdbId: ep.tmdbId,
        imdbId: null,
        mediaType: MEDIA_TYPE_TV_EPISODE,
        tvSeriesTmdbId: seriesTmdbId,
        seasonNumber: ep.seasonNumber,
        episodeNumber: ep.episodeNumber,
      },
      seriesTitle ? `${seriesTitle} — ${code} — ${ep.name}` : `${code} — ${ep.name}`,
      `episode-${ep.tmdbId}`,
    );
  }

  return (
    <div className="mt-6 border border-white/10">
      {isPersonSubject && (
        <div className="flex items-center gap-3 px-3 py-3 border-b border-white/10">
          {personPhotoUrl ? (
            <img
              src={personPhotoUrl}
              alt=""
              className="w-12 h-12 rounded-full object-cover shrink-0"
            />
          ) : (
            <div className="w-12 h-12 rounded-full bg-white/10 shrink-0" />
          )}
          <div>
            <p className="font-oswald text-lg text-sd-paper leading-tight">{subjectName}</p>
            <p className="text-[11px] text-white/40 font-mono uppercase">
              {subjectKind === 0 ? 'Actor' : 'Director'} — full filmography
            </p>
          </div>
        </div>
      )}

      {error && <p className="px-4 py-2 text-sd-red text-xs font-mono">{error}</p>}

      <div className="flex gap-1 px-3 py-2 border-b border-white/10 text-[11px] font-mono uppercase tracking-wide">
        <button
          type="button"
          onClick={() => {
            setView('subject');
            setDrillSeries(null);
          }}
          className={`px-2 py-1 border ${
            view === 'subject' ? DARK_THEME.toggleActive : DARK_THEME.toggleInactive
          }`}
        >
          Titles
        </button>
        <button
          type="button"
          onClick={() => {
            setView('episode');
            setDrillSeries(null);
          }}
          className={`px-2 py-1 border ${
            view === 'episode' ? DARK_THEME.toggleActive : DARK_THEME.toggleInactive
          }`}
        >
          TV Episode
        </button>
      </div>

      {view === 'subject' && (
        <div className="flex gap-1 px-3 py-2 border-b border-white/10 text-[11px] font-mono uppercase tracking-wide">
          {isPersonSubject && (
            <button
              type="button"
              onClick={() => setKind('all')}
              className={`px-2 py-1 border ${
                kind === 'all' ? DARK_THEME.toggleActive : DARK_THEME.toggleInactive
              }`}
            >
              All
            </button>
          )}
          <button
            type="button"
            onClick={() => setKind('movie')}
            className={`px-2 py-1 border ${
              kind === 'movie' ? DARK_THEME.toggleActive : DARK_THEME.toggleInactive
            }`}
          >
            Movies
          </button>
          <button
            type="button"
            onClick={() => setKind('tv')}
            className={`px-2 py-1 border ${
              kind === 'tv' ? DARK_THEME.toggleActive : DARK_THEME.toggleInactive
            }`}
          >
            TV Shows
          </button>
        </div>
      )}

      {view === 'subject' && (
        <div className="px-3 py-2 border-b border-white/10">
          <input
            type="text"
            value={refine}
            onChange={(e) => setRefine(e.target.value)}
            placeholder={
              isPersonSubject
                ? `Filter "${subjectName}" filmography…`
                : `Refine within "${subjectName}"… (e.g. "of soul")`
            }
            className="w-full bg-transparent border border-white/20 px-3 py-2 text-sm text-sd-paper placeholder:text-white/30 font-mono focus:outline-none focus:border-white/50"
          />
        </div>
      )}

      {view === 'subject' && (
        <div className="max-h-80 overflow-y-auto">
          {loading && (
            <div className="px-4 py-6 text-center text-white/30 text-xs font-mono animate-pulse">
              Loading…
            </div>
          )}

          {!loading && isPersonSubject && visibleCredits.length === 0 && (
            <div className="px-4 py-6 text-center text-white/30 text-xs font-mono italic">
              {credits.length === 0 ? 'No filmography found.' : 'No credits match your filter.'}
            </div>
          )}

          {!loading &&
            isPersonSubject &&
            visibleCredits.map((c) => {
              const submittingKey = `importing-tmdb-${c.tmdbId}-${c.mediaType}`;
              return (
                <div
                  key={`${c.tmdbId}-${c.mediaType}`}
                  className="flex items-center gap-4 px-4 py-3 border-b border-white/5 hover:bg-white/5 transition-colors"
                >
                  {c.posterUrl ? (
                    <img
                      src={c.posterUrl}
                      alt=""
                      className="w-12 h-[72px] object-cover shrink-0 bg-white/10"
                    />
                  ) : (
                    <div className="w-12 h-[72px] bg-white/10 shrink-0" />
                  )}
                  <div className="flex-1 min-w-0">
                    <p className="font-oswald text-lg truncate leading-tight text-sd-paper">
                      {c.title}
                    </p>
                    <p className="text-sm text-white/40 font-mono">
                      {[c.year, c.creditRole].filter(Boolean).join(' · ')}
                    </p>
                  </div>
                  {c.mediaType === 1 && (
                    <button
                      type="button"
                      onClick={() => openEpisodes(c.tmdbId, c.title)}
                      disabled={submitting !== null}
                      className="shrink-0 px-3 py-2 font-oswald text-xs tracking-widest border border-white/20 text-white/60 hover:text-white hover:border-white/50 disabled:opacity-30 transition-colors"
                    >
                      EPISODES
                    </button>
                  )}
                  <button
                    onClick={() =>
                      submitPick(
                        c.mediaPublicId ?? null,
                        { tmdbId: c.tmdbId, imdbId: null, mediaType: c.mediaType },
                        c.title,
                        submittingKey,
                      )
                    }
                    disabled={submitting !== null || slotAlreadyPicked}
                    className={`shrink-0 px-4 py-2 font-oswald text-sm tracking-widest transition-colors ${
                      submitting === submittingKey
                        ? 'bg-sd-red/50 text-white cursor-wait'
                        : 'border border-sd-red/50 text-sd-red hover:border-sd-red hover:bg-sd-red hover:text-white disabled:opacity-30 disabled:cursor-not-allowed'
                    }`}
                  >
                    {submitting === submittingKey ? '…' : 'PICK'}
                  </button>
                </div>
              );
            })}

          {!isPersonSubject && kind === 'tv' && (
          <>
            {tvLoading && (
              <div className="px-4 py-6 text-center text-white/30 text-xs font-mono animate-pulse">
                Loading…
              </div>
            )}
            {!tvLoading && tvResults.length === 0 && (
              <div className="px-4 py-6 text-center text-white/30 text-xs font-mono italic">
                No shows for &ldquo;{searchQuery}&rdquo;.
              </div>
            )}
            {!tvLoading &&
              tvResults.map((show) => {
                const submittingKey = `importing-tv-${show.tmdbId}`;
                return (
                  <div
                    key={`tv-${show.tmdbId}`}
                    className="flex items-center gap-3 px-3 py-2 border-b border-white/5 hover:bg-white/5 transition-colors"
                  >
                    {show.posterUrl ? (
                      <img
                        src={show.posterUrl}
                        alt=""
                        className="w-8 h-12 object-cover shrink-0 bg-white/10"
                      />
                    ) : (
                      <div className="w-8 h-12 bg-white/10 shrink-0" />
                    )}
                    <div className="flex-1 min-w-0">
                      <p className="font-oswald text-sm truncate leading-tight text-sd-paper">
                        {show.title}
                      </p>
                      <p className="text-[11px] text-white/40 font-mono">
                        {[show.year, 'TV'].filter(Boolean).join(' · ')}
                      </p>
                    </div>
                    <button
                      type="button"
                      onClick={() => openEpisodes(show.tmdbId, show.title)}
                      disabled={submitting !== null}
                      className="shrink-0 px-3 py-1.5 font-oswald text-xs tracking-widest border border-white/20 text-white/60 hover:text-white hover:border-white/50 disabled:opacity-30 transition-colors"
                    >
                      EPISODES
                    </button>
                    <button
                      onClick={() => pickShow(show)}
                      disabled={submitting !== null || slotAlreadyPicked}
                      className={`shrink-0 px-3 py-1.5 font-oswald text-xs tracking-widest transition-colors ${
                        submitting === submittingKey
                          ? 'bg-sd-red/50 text-white cursor-wait'
                          : 'border border-sd-red/50 text-sd-red hover:border-sd-red hover:bg-sd-red hover:text-white disabled:opacity-30 disabled:cursor-not-allowed'
                      }`}
                    >
                      {submitting === submittingKey ? '…' : 'PICK'}
                    </button>
                  </div>
                );
              })}
          </>
        )}

        {!loading && !isPersonSubject && kind === 'movie' && titleResults.length === 0 && (
            <div className="px-4 py-6 text-center text-white/30 text-xs font-mono italic">
              No results for &ldquo;{searchQuery}&rdquo;.
            </div>
          )}

          {!loading &&
            !isPersonSubject && kind === 'movie' &&
            titleResults.map((item) => {
              const key = titleKey(item);
              const submittingKey =
                item.mediaPublicId || `importing-${item.tmdbId ?? item.imdbId}`;
              return (
                <div
                  key={key}
                  className="flex items-center gap-3 px-3 py-2 border-b border-white/5 hover:bg-white/5 transition-colors"
                >
                  {item.posterUrl ? (
                    <img
                      src={item.posterUrl}
                      alt=""
                      className="w-8 h-12 object-cover shrink-0 bg-white/10"
                    />
                  ) : (
                    <div className="w-8 h-12 bg-white/10 shrink-0" />
                  )}
                  <div className="flex-1 min-w-0">
                    <p className="font-oswald text-sm truncate leading-tight text-sd-paper">
                      {item.title}
                    </p>
                    {item.year && (
                      <p className="text-[11px] text-white/40 font-mono">{item.year}</p>
                    )}
                  </div>
                  {item.mediaType === 1 && item.tmdbId != null && (
                    <button
                      type="button"
                      onClick={() => openEpisodes((item.tmdbId as number), item.title)}
                      disabled={submitting !== null}
                      className="shrink-0 px-3 py-2 font-oswald text-xs tracking-widest border border-white/20 text-white/60 hover:text-white hover:border-white/50 disabled:opacity-30 transition-colors"
                    >
                      EPISODES
                    </button>
                  )}
                  <button
                    onClick={() =>
                      submitPick(
                        item.mediaPublicId ?? null,
                        { tmdbId: item.tmdbId, imdbId: item.imdbId, mediaType: item.mediaType },
                        item.title,
                        submittingKey,
                      )
                    }
                    disabled={submitting !== null || slotAlreadyPicked}
                    className={`shrink-0 px-3 py-1.5 font-oswald text-xs tracking-widest transition-colors ${
                      submitting === submittingKey
                        ? 'bg-sd-red/50 text-white cursor-wait'
                        : 'border border-sd-red/50 text-sd-red hover:border-sd-red hover:bg-sd-red hover:text-white disabled:opacity-30 disabled:cursor-not-allowed'
                    }`}
                  >
                    {submitting === submittingKey ? '…' : 'PICK'}
                  </button>
                </div>
              );
            })}

          {!loading && !isPersonSubject && kind === 'movie' && titleHasMore && (
            <button
              onClick={loadMoreTitles}
              disabled={loadingMore}
              className="w-full px-4 py-3 font-oswald text-xs tracking-widest text-white/60 hover:text-white hover:bg-white/5 disabled:opacity-40 transition-colors"
            >
              {loadingMore ? 'LOADING…' : 'LOAD MORE'}
            </button>
          )}
        </div>
      )}

      {view === 'episode' && (
        <div className="px-3 py-3 space-y-2">
          {drillSeries && (
            <div className="flex items-center justify-between gap-2">
              <p className="font-oswald text-sm text-sd-paper truncate">
                Episodes of {drillSeries.title}
              </p>
              <button
                type="button"
                onClick={() => {
                  setView('subject');
                  setDrillSeries(null);
                }}
                className="text-[11px] font-mono text-sd-red uppercase tracking-widest hover:underline"
              >
                Back
              </button>
            </div>
          )}
          <EpisodeSeasonPicker
            key={drillSeries?.tmdbId ?? 'search'}
            accessToken={accessToken}
            fixedSeriesTmdbId={drillSeries?.tmdbId}
            onSelect={handleEpisodeSelect}
            disabled={submitting !== null || slotAlreadyPicked}
            theme={DARK_THEME}
          />
          {submitting?.startsWith('episode-') && (
            <p className="text-[11px] font-mono text-white/40">Importing episode…</p>
          )}
        </div>
      )}
    </div>
  );
}