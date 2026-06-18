// app/draft-parts/[draftPartId]/live/components/pick-source-panel.tsx
'use client';

import { useState, useEffect, useRef } from 'react';
import { useLiveDraft } from '../live-draft-context';

// ── Types ─────────────────────────────────────────────────────────────────────
// Replace with dto.ts imports after NSwag regen.

interface ResolvedMovie {
  mediaPublicId: string;
  tmdbId: number;
  title: string;
  year?: string | null;
  posterUrl?: string | null;
}

// Shape returned by GET /media/by-tmdb-ids
interface ByTmdbIdsItem {
  publicId: string;
  tmdbId: number;
  title: string;
  year?: string | null;
  posterUrl?: string | null;
  image?: string | null; // fallback field name
}

// ── Constants ─────────────────────────────────────────────────────────────────

const API = process.env.NEXT_PUBLIC_API_URL;

// ── Helpers ───────────────────────────────────────────────────────────────────

async function resolveTmdbIds(
  tmdbIds: number[],
  accessToken: string,
): Promise<ResolvedMovie[]> {
  if (tmdbIds.length === 0) return [];
  const params = tmdbIds.map((id) => `tmdbIds=${id}`).join('&');
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

// ── Import movie then wait for it to appear in DB ────────────────────────────

async function importAndResolve(
  tmdbId: number,
  accessToken: string,
  timeoutMs = 10000,
): Promise<ResolvedMovie | null> {
  // Trigger import
  await fetch(`${API}/integrations/movies/import`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${accessToken}`, 'Content-Type': 'application/json' },
    body: JSON.stringify({ tmdbId, mediaType: 0 }),
  });

  // Poll by-tmdb-ids until publicId appears
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    await new Promise((r) => setTimeout(r, 600));
    const resolved = await resolveTmdbIds([tmdbId], accessToken);
    if (resolved.length > 0 && resolved[0].mediaPublicId) return resolved[0];
  }
  return null;
}

// ── Props ─────────────────────────────────────────────────────────────────────

interface Props {
  accessToken: string;
  draftPartId: string;
  draftId: string;
  activeSlot: number;
  callerParticipantId: string;
  callerParticipantKind: number;
  onPickSubmitted: (playOrder: number, movieTitle: string) => void;
}

// ── Component ─────────────────────────────────────────────────────────────────

type SourceTab = 'pool' | 'board' | 'candidateList' | 'search';

export function PickSourcePanel({
  accessToken,
  draftPartId,
  draftId,
  activeSlot,
  callerParticipantId,
  callerParticipantKind,
  onPickSubmitted,
}: Props) {
  const { gameplay, picks } = useLiveDraft();

  const availableTabs: { key: SourceTab; label: string }[] = [
    ...(gameplay.hasDraftPool ? [{ key: 'pool' as SourceTab, label: 'POOL' }] : []),
    ...(gameplay.hasDraftBoard ? [{ key: 'board' as SourceTab, label: 'BOARD' }] : []),
    ...(gameplay.hasCandidateList
      ? [{ key: 'candidateList' as SourceTab, label: 'CANDIDATES' }]
      : []),
    { key: 'search' as SourceTab, label: 'SEARCH' },
  ];

  const [activeTab, setActiveTab] = useState<SourceTab>(availableTabs[0]?.key ?? 'search');
  const [submitting, setSubmitting] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  // Prevent picking if the slot is already filled (e.g. from a race between
  // two source tabs both submitting, or a SignalR update arriving mid-submit).
  const slotAlreadyPicked = picks.some(
    (p) => p.boardPosition === activeSlot && (!p.wasVetoed || p.wasVetoOverridden),
  );

  async function handlePick(movie: ResolvedMovie) {
    if (slotAlreadyPicked || submitting !== null) return;
    setSubmitting(movie.mediaPublicId || `importing-${movie.tmdbId}`);
    setError(null);
    try {
      let resolvedMovie = movie;

      // Movie not in DB yet — import from TMDb then wait for it to land.
      if (!movie.mediaPublicId) {
        const imported = await importAndResolve(movie.tmdbId, accessToken);
        if (!imported) {
          setError('Movie could not be imported in time. Try again in a moment.');
          return;
        }
        resolvedMovie = imported;
      }

      const playOrder = (picks.length ?? 0) + 1;
      const res = await fetch(`${API}/draft-parts/${draftPartId}/picks`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${accessToken}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          draftPartId,
          position: activeSlot,
          playOrder,
          participantPublicId: callerParticipantId,
          participantKind: callerParticipantKind,
          moviePublicId: resolvedMovie.mediaPublicId,
          movieVersionName: null,
        }),
      });
      if (!res.ok) {
        const body = await res.json().catch(() => null);
        throw new Error(body?.detail ?? `Pick failed: ${res.status}`);
      }
      onPickSubmitted(playOrder, resolvedMovie.title);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to submit pick.');
    } finally {
      setSubmitting(null);
    }
  }

  return (
    <div className="mt-6 border border-white/10">
      {/* Source tabs */}
      <div className="flex border-b border-white/10">
        {availableTabs.map((tab) => (
          <button
            key={tab.key}
            onClick={() => setActiveTab(tab.key)}
            className={`px-4 py-2 font-oswald text-xs tracking-widest transition-colors ${
              activeTab === tab.key
                ? 'text-sd-paper border-b-2 border-sd-red'
                : 'text-white/40 hover:text-white/60'
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {error && <p className="px-4 py-2 text-sd-red text-xs font-mono">{error}</p>}

      <div className="max-h-72 overflow-y-auto">
        {activeTab === 'pool' && (
          <PoolSource
            accessToken={accessToken}
            draftId={draftId}
            submitting={submitting}
            onPick={handlePick}
            disabled={slotAlreadyPicked}
          />
        )}
        {activeTab === 'board' && (
          <BoardSource
            accessToken={accessToken}
            draftId={draftId}
            submitting={submitting}
            onPick={handlePick}
            disabled={slotAlreadyPicked}
          />
        )}
        {activeTab === 'candidateList' && (
          <CandidateListSource
            accessToken={accessToken}
            draftPartId={draftPartId}
            submitting={submitting}
            onPick={handlePick}
            disabled={slotAlreadyPicked}
          />
        )}
        {activeTab === 'search' && (
          <SearchSource
            accessToken={accessToken}
            submitting={submitting}
            onPick={handlePick}
            disabled={slotAlreadyPicked}
          />
        )}
      </div>
    </div>
  );
}

// ── Pool ──────────────────────────────────────────────────────────────────────
// Response: { publicId, draftId, isLocked, tmdbIds: number[] }
// Resolve titles via /media/by-tmdb-ids.

function PoolSource({
  accessToken,
  draftId,
  submitting,
  onPick,
  disabled,
}: {
  accessToken: string;
  draftId: string;
  submitting: string | null;
  onPick: (movie: ResolvedMovie) => void;
  disabled: boolean;
}) {
  const [movies, setMovies] = useState<ResolvedMovie[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      try {
        const res = await fetch(`${API}/drafts/${draftId}/pool`, {
          headers: { Authorization: `Bearer ${accessToken}` },
        });
        if (!res.ok) return;
        const data = await res.json();
        const tmdbIds: number[] = data.tmdbIds ?? [];
        const resolved = await resolveTmdbIds(tmdbIds, accessToken);
        setMovies(resolved);
      } finally {
        setLoading(false);
      }
    })();
  }, [accessToken, draftId]);

  if (loading) return <LoadingRows />;
  if (movies.length === 0) return <EmptyMessage text="Pool is empty." />;
  return <MovieList movies={movies} submitting={submitting} onPick={onPick} disabled={disabled} />;
}

// ── Board ─────────────────────────────────────────────────────────────────────
// Response: { publicId, draftId, items: [{ tmdbId, title, year, notes, priority }] }
// Items have title/year but no publicId — resolve via /media/by-tmdb-ids.

function BoardSource({
  accessToken,
  draftId,
  submitting,
  onPick,
  disabled,
}: {
  accessToken: string;
  draftId: string;
  submitting: string | null;
  onPick: (movie: ResolvedMovie) => void;
  disabled: boolean;
}) {
  const [movies, setMovies] = useState<ResolvedMovie[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      try {
        const res = await fetch(`${API}/drafts/${draftId}/board`, {
          headers: { Authorization: `Bearer ${accessToken}` },
        });
        if (!res.ok) return;
        const data = await res.json();
        const items: { tmdbId: number; title?: string; year?: string }[] =
          data.items ?? [];
        const tmdbIds = items.map((i) => i.tmdbId);
        const resolved = await resolveTmdbIds(tmdbIds, accessToken);
        // Merge board title/year as fallback for items not yet in the DB.
        const byTmdbId = new Map(resolved.map((r) => [r.tmdbId, r]));
        const merged: ResolvedMovie[] = items.map((item) => {
          const r = byTmdbId.get(item.tmdbId);
          return r ?? {
            // Not in DB yet — no publicId, show greyed out
            mediaPublicId: '',
            tmdbId: item.tmdbId,
            title: item.title ?? `TMDb #${item.tmdbId}`,
            year: item.year,
            posterUrl: null,
          };
        });
        setMovies(merged);
      } finally {
        setLoading(false);
      }
    })();
  }, [accessToken, draftId]);

  if (loading) return <LoadingRows />;
  if (movies.length === 0) return <EmptyMessage text="Board is empty." />;
  return <MovieList movies={movies} submitting={submitting} onPick={onPick} disabled={disabled} />;
}

// ── Candidate list ────────────────────────────────────────────────────────────
// Response: { response: { items: [{ entryId, tmdbId, movieTitle, isPending, ... }] } }
// Items have movieTitle but no publicId — resolve non-pending via /media/by-tmdb-ids.

function CandidateListSource({
  accessToken,
  draftPartId,
  submitting,
  onPick,
  disabled,
}: {
  accessToken: string;
  draftPartId: string;
  submitting: string | null;
  onPick: (movie: ResolvedMovie) => void;
  disabled: boolean;
}) {
  const [movies, setMovies] = useState<ResolvedMovie[]>([]);
  const [pendingCount, setPendingCount] = useState(0);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      try {
        const res = await fetch(`${API}/draft-parts/${draftPartId}/candidate-list`, {
          headers: { Authorization: `Bearer ${accessToken}` },
        });
        if (!res.ok) return;
        const data = await res.json();
        const items: {
          entryId: string;
          tmdbId: number;
          movieTitle?: string;
          isPending: boolean;
        }[] = data.response?.items ?? data.items ?? [];

        const ready = items.filter((i) => !i.isPending);
        const pending = items.filter((i) => i.isPending);
        setPendingCount(pending.length);

        const tmdbIds = ready.map((i) => i.tmdbId);
        const resolved = await resolveTmdbIds(tmdbIds, accessToken);
        const byTmdbId = new Map(resolved.map((r) => [r.tmdbId, r]));

        const merged: ResolvedMovie[] = ready.map((item) => {
          const r = byTmdbId.get(item.tmdbId);
          return r ?? {
            mediaPublicId: '',
            tmdbId: item.tmdbId,
            title: item.movieTitle ?? `TMDb #${item.tmdbId}`,
            year: null,
            posterUrl: null,
          };
        });
        setMovies(merged);
      } finally {
        setLoading(false);
      }
    })();
  }, [accessToken, draftPartId]);

  if (loading) return <LoadingRows />;
  if (movies.length === 0 && pendingCount === 0)
    return <EmptyMessage text="Candidate list is empty." />;
  return (
    <MovieList
      movies={movies}
      submitting={submitting}
      onPick={onPick}
      pendingCount={pendingCount}
      disabled={disabled}
    />
  );
}

// ── Search ────────────────────────────────────────────────────────────────────
// Response: { results: { items: [{ tmdbId, title, year, posterUrl, mediaPublicId, isInMediaDatabase }] } }
// Items that are in the database already have mediaPublicId — use directly.
// Items not in the database have no mediaPublicId — disable PICK (must be in DB first).

function SearchSource({
  accessToken,
  submitting,
  onPick,
  disabled,
}: {
  accessToken: string;
  submitting: string | null;
  onPick: (movie: ResolvedMovie) => void;
  disabled: boolean;
}) {
  const [query, setQuery] = useState('');
  const [movies, setMovies] = useState<ResolvedMovie[]>([]);
  const [loading, setLoading] = useState(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current);
    if (query.trim().length < 2) {
      setMovies([]);
      return;
    }
    debounceRef.current = setTimeout(async () => {
      setLoading(true);
      try {
        const res = await fetch(
          `${API}/media/search?query=${encodeURIComponent(query.trim())}`,
          { headers: { Authorization: `Bearer ${accessToken}` } },
        );
        if (!res.ok) return;
        const data = await res.json();
        const items: {
          tmdbId?: number;
          title: string;
          year?: string;
          posterUrl?: string;
          mediaPublicId?: string;
          isInMediaDatabase: boolean;
        }[] = data.results?.items ?? data.items ?? [];

        setMovies(
          items
            .filter((i) => i.tmdbId != null)
            .map((i) => ({
              mediaPublicId: i.mediaPublicId ?? '',
              tmdbId: i.tmdbId!,
              title: i.title,
              year: i.year,
              posterUrl: i.posterUrl ?? null,
            })),
        );
      } finally {
        setLoading(false);
      }
    }, 350);

    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
    };
  }, [query, accessToken]);

  return (
    <div>
      <div className="px-3 py-2 border-b border-white/10">
        <input
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Search movies…"
          autoFocus
          className="w-full bg-transparent text-sd-paper text-sm font-mono placeholder:text-white/30 outline-none"
        />
      </div>
      {loading && <LoadingRows />}
      {!loading && query.trim().length >= 2 && movies.length === 0 && (
        <EmptyMessage text="No results." />
      )}
      {!loading && movies.length > 0 && (
        <MovieList movies={movies} submitting={submitting} onPick={onPick} disabled={disabled} />
      )}
    </div>
  );
}

// ── Shared movie list ─────────────────────────────────────────────────────────

function MovieList({
  movies,
  submitting,
  onPick,
  pendingCount,
  disabled = false,
}: {
  movies: ResolvedMovie[];
  submitting: string | null;
  onPick: (movie: ResolvedMovie) => void;
  pendingCount?: number;
  disabled?: boolean;
}) {
  return (
    <div>
      {(pendingCount ?? 0) > 0 && (
        <p className="px-4 py-1.5 text-[11px] text-white/30 font-mono border-b border-white/5">
          {pendingCount} pending entr{pendingCount === 1 ? 'y' : 'ies'} hidden
        </p>
      )}
      {movies.map((movie) => {
        return (
          <div
            key={`${movie.tmdbId}-${movie.mediaPublicId}`}
            className="flex items-center gap-3 px-3 py-2 border-b border-white/5 hover:bg-white/5 transition-colors"
          >
            {/* Poster */}
            {movie.posterUrl ? (
              <img
                src={movie.posterUrl}
                alt=""
                className="w-8 h-12 object-cover shrink-0 bg-white/10"
              />
            ) : (
              <div className="w-8 h-12 bg-white/10 shrink-0" />
            )}

            {/* Title + year */}
            <div className="flex-1 min-w-0">
              <p className="font-oswald text-sm truncate leading-tight text-sd-paper">
                {movie.title}
              </p>
              {movie.year && (
                <p className="text-[11px] text-white/40 font-mono">{movie.year}</p>
              )}

            </div>

            {/* Pick button */}
            <button
              onClick={() => onPick(movie)}
              disabled={submitting !== null || disabled}
              className={`shrink-0 px-3 py-1.5 font-oswald text-xs tracking-widest transition-colors ${
                submitting === movie.mediaPublicId || submitting === `importing-${movie.tmdbId}`
                  ? 'bg-sd-red/50 text-white cursor-wait'
                  : 'border border-sd-red/50 text-sd-red hover:border-sd-red hover:bg-sd-red hover:text-white disabled:opacity-30 disabled:cursor-not-allowed'
              }`}
            >
              {submitting === movie.mediaPublicId || submitting === `importing-${movie.tmdbId}` ? '…' : 'PICK'}
            </button>
          </div>
        );
      })}
    </div>
  );
}

// ── Micro components ──────────────────────────────────────────────────────────

function LoadingRows() {
  return (
    <div className="px-4 py-6 text-center text-white/30 text-xs font-mono animate-pulse">
      Loading…
    </div>
  );
}

function EmptyMessage({ text }: { text: string }) {
  return (
    <div className="px-4 py-6 text-center text-white/30 text-xs font-mono italic">
      {text}
    </div>
  );
}