'use client';

import { useCallback, useEffect, useRef, useState } from 'react';
import {
  activateSpotlight,
  createSpotlight,
  deactivateSpotlight,
  deleteSpotlight,
  fetchSpotlights,
  rotateSpotlight,
  searchSpotlightCandidates,
} from './spotlight-fetcher';
import { ListSpotlightDraftsResponse, PagedResultOfListSpotlightDraftsResponse, SpotlightCandidateItem } from '@/lib/dto';

// ── Constants ─────────────────────────────────────────────────────────────

const PAGE_SIZE = 5;

const DRAFT_TYPES: {value: string; label: string}[] = [
  { value: 'Mega', label: 'Mega'},
  { value: 'MiniMega', label: 'mini-Mega'},
  { value: 'Standard', label: 'Standard'},
  { value: 'Super', label: 'Super'},
]

// ── Helpers ───────────────────────────────────────────────────────────────

function formatDraftLabel(item: { title: string; episodeNumber: number | null | undefined; draftType: string }) {
  const ep = item.episodeNumber != null ? `Ep. ${item.episodeNumber} · ` : '';
  return `${ep}${item.title} (${item.draftType})`;
}

// ── Sub-components ────────────────────────────────────────────────────────

function LiveBadge() {
  return (
    <span className="bg-sd-red text-white text-[10px] font-bold tracking-[0.2em] px-2 py-0.5">
      LIVE
    </span>
  );
}

function PinnedBadge() {
  return (
    <span className="border border-sd-blue/40 text-sd-blue text-[10px] font-bold tracking-[0.2em] px-2 py-0.5">
      PINNED
    </span>
  );
}

// ── Add to library form ───────────────────────────────────────────────────

interface AddSpotlightFormProps {
  accessToken: string;
  onCreated: () => void;
}

function AddSpotlightForm({ accessToken, onCreated }: AddSpotlightFormProps) {
  const [query, setQuery] = useState('');
  const [candidates, setCandidates] = useState<SpotlightCandidateItem[]>([]);
  const [selected, setSelected] = useState<SpotlightCandidateItem | null>(null);
  const [description, setDescription] = useState('');
  const [spotifyUrl, setSpotifyUrl] = useState('');
  const [searching, setSearching] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const search = useCallback(
    (q: string) => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
      debounceRef.current = setTimeout(async () => {
        if (!q.trim()) { setCandidates([]); return; }
        setSearching(true);
        try {
          const res = await searchSpotlightCandidates(accessToken, q);
          setCandidates(res.items ?? []);
        } finally {
          setSearching(false);
        }
      }, 300);
    },
    [accessToken]
  );

  useEffect(() => { search(query); }, [query, search]);

  async function handleSubmit() {
    if (!selected || !description.trim()) return;
    setSaving(true);
    setError(null);
    try {
      await createSpotlight(accessToken, selected.draftPublicId, description.trim(), spotifyUrl.trim() || null);
      setSelected(null);
      setQuery('');
      setDescription('');
      setSpotifyUrl('');
      setCandidates([]);
      onCreated();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to create spotlight.');
    } finally {
      setSaving(false);
    }
  }

  return (
    <section>
      <div className="bg-white border border-sd-ink/10 p-6">
        <h2 className="font-oswald font-bold text-base tracking-[0.1em] text-sd-ink mb-5">
          ADD TO LIBRARY
        </h2>

        <div className="mb-4">
          <label className="block text-[10px] tracking-[0.18em] font-bold text-sd-ink/50 mb-1.5">
            SEARCH DRAFTS
          </label>
          <input
            type="text"
            value={query}
            onChange={e => { setQuery(e.target.value); setSelected(null); }}
            placeholder="Title or episode number…"
            className="w-full border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sm text-sd-ink placeholder:text-sd-ink/30 focus:outline-none focus:border-sd-blue"
          />
          {searching && <p className="text-[11px] text-sd-ink/40 mt-1.5">Searching…</p>}
          {!searching && candidates.length > 0 && !selected && (
            <ul className="border border-sd-ink/10 bg-white mt-0.5 divide-y divide-sd-ink/5 max-h-52 overflow-y-auto shadow-sm">
              {candidates.map(c => (
                <li key={c.draftPublicId}>
                  <button
                    type="button"
                    onClick={() => { setSelected(c); setQuery(formatDraftLabel(c)); setCandidates([]); }}
                    className="w-full text-left px-3 py-2.5 text-sm text-sd-ink hover:bg-sd-blue/5 transition-colors"
                  >
                    <span className="font-semibold">{c.title}</span>
                    <span className="text-sd-ink/50 ml-2 text-xs">
                      {c.episodeNumber != null ? `Ep. ${c.episodeNumber} · ` : ''}{c.draftType} · {c.totalPicks} picks
                    </span>
                  </button>
                </li>
              ))}
            </ul>
          )}
          {!searching && query.trim() && candidates.length === 0 && !selected && (
            <p className="text-[11px] text-sd-ink/40 mt-1.5">No eligible drafts found.</p>
          )}
        </div>

        <div className="mb-4">
          <label className="block text-[10px] tracking-[0.18em] font-bold text-sd-ink/50 mb-1.5">
            SPOTLIGHT DESCRIPTION
          </label>
          <textarea
            value={description}
            onChange={e => setDescription(e.target.value)}
            rows={3}
            placeholder="Write a short description for the home page hero…"
            className="w-full border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sm text-sd-ink placeholder:text-sd-ink/30 focus:outline-none focus:border-sd-blue resize-none"
          />
        </div>

        <div className="mb-5">
          <label className="block text-[10px] tracking-[0.18em] font-bold text-sd-ink/50 mb-1.5">
            SPOTIFY EPISODE URL <span className="font-normal opacity-60">(optional)</span>
          </label>
          <input
            type="url"
            value={spotifyUrl}
            onChange={e => setSpotifyUrl(e.target.value)}
            placeholder="https://open.spotify.com/episode/…"
            className="w-full border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sm text-sd-ink placeholder:text-sd-ink/30 focus:outline-none focus:border-sd-blue"
          />
        </div>

        {error && <p className="text-sd-red text-sm mb-4">{error}</p>}

        <button
          type="button"
          onClick={handleSubmit}
          disabled={!selected || !description.trim() || saving}
          className="bg-sd-ink text-white font-oswald font-bold tracking-[0.14em] text-sm px-6 py-2.5 hover:bg-sd-ink/80 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
        >
          {saving ? 'SAVING…' : 'ADD TO LIBRARY'}
        </button>
      </div>
    </section>
  );
}

// ── Main component ────────────────────────────────────────────────────────

interface SpotlightManagerProps {
  accessToken: string;
}

export default function SpotlightManager({ accessToken }: SpotlightManagerProps) {
  const [library, setLibrary] = useState<PagedResultOfListSpotlightDraftsResponse | null>(null);
  const [active, setActive] = useState<ListSpotlightDraftsResponse | null>(null);
  const [page, setPage] = useState(1);
  const [librarySearch, setLibrarySearch] = useState('');
  const [draftTypeFilter, setDraftTypeFilter] = useState('');
  const [loading, setLoading] = useState(true);
  const [pendingId, setPendingId] = useState<string | null>(null);
  const [deleteConfirmId, setDeleteConfirmId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [rotating, setRotating] = useState(false);

  const librarySearchRef = useRef(librarySearch);
  const draftTypeFilterRef = useRef(draftTypeFilter);
  librarySearchRef.current = librarySearch;
  draftTypeFilterRef.current = draftTypeFilter;

  // Debounce ref for library search
  const searchDebounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  // Initial load
  useEffect(() => { refresh(); }, []); // eslint-disable-line react-hooks/exhaustive-deps

  // Page changes (from pagination buttons)
  useEffect(() => { refresh(); }, [page]); // eslint-disable-line react-hooks/exhaustive-deps

  // Filter changes — debounce search text, immediate on type dropdown
  useEffect(() => {
    if (searchDebounceRef.current) clearTimeout(searchDebounceRef.current);
    searchDebounceRef.current = setTimeout(() => {
      setPage(1);
      refresh();
    }, 300);
    return () => {
      if (searchDebounceRef.current) clearTimeout(searchDebounceRef.current);
    };
  }, [librarySearch]); // eslint-disable-line react-hooks/exhaustive-deps

  useEffect(() => {
    setPage(1);
    refresh();
  }, [draftTypeFilter]); // eslint-disable-line react-hooks/exhaustive-deps

  async function refresh() {
    setLoading(true);
    try {
      const [libResult, activeResult] = await Promise.all([
        fetchSpotlights(
          accessToken, page, PAGE_SIZE, true,
          librarySearchRef.current || undefined,
          draftTypeFilterRef.current || undefined
        ),
        fetchSpotlights(accessToken, 1, 1, false),
      ]);
      setLibrary(libResult);
      const top = activeResult.items?.[0];
      setActive(top && top.isActive ? top : null);
      if (libResult.items?.length === 0 && page > 1) {
        setPage(p => p - 1);
      }
    } finally {
      setLoading(false);
    }
  }

  async function handleRotate() {
    setRotating(true);
    setError(null);
    try {
      await rotateSpotlight(accessToken);
      await new Promise(r => setTimeout(r, 1200));
      await refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to trigger rotation.');
    } finally {
      setRotating(false);
    }
  }

  async function handleActivate(publicId: string) {
    setPendingId(publicId);
    setError(null);
    try {
      await activateSpotlight(accessToken, publicId);
      await refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to activate spotlight.');
    } finally {
      setPendingId(null);
    }
  }

  async function handleDeactivate(publicId: string) {
    setPendingId(publicId);
    setError(null);
    try {
      await deactivateSpotlight(accessToken, publicId);
      await refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to deactivate spotlight.');
    } finally {
      setPendingId(null);
    }
  }

  async function handleDelete(publicId: string) {
    setPendingId(publicId);
    setError(null);
    try {
      await deleteSpotlight(accessToken, publicId);
      setDeleteConfirmId(null);
      await refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to delete spotlight.');
    } finally {
      setPendingId(null);
    }
  }

  const libraryItems = library?.items ?? [];
  const totalLibraryCount = library?.totalCount ?? 0;
  const totalPages = library?.totalPages ?? 1;
  const hasPrev = library?.hasPreviousPage ?? false;
  const hasNext = library?.hasNextPage ?? false;

  return (
    <div className="space-y-8">

      {/* ── Active spotlight ── */}
      <section>
        <h2 className="font-oswald font-bold text-base tracking-[0.1em] text-sd-ink/50 mb-3">
          ACTIVE SPOTLIGHT
        </h2>
        {active ? (
          <div className="bg-white border border-sd-ink/10 border-l-4 border-l-sd-red px-6 py-5 flex items-start justify-between gap-6">
            <div>
              <div className="flex items-center gap-2.5 mb-2">
                <LiveBadge />
                {active.isPinned && <PinnedBadge />}
              </div>
              <p className="font-oswald font-bold text-xl tracking-[0.02em] text-sd-ink">
                {active.title}
              </p>
              <p className="text-[11px] tracking-[0.18em] text-sd-ink/40 mt-0.5">
                {active.episodeNumber != null ? `EP. ${active.episodeNumber} · ` : ''}
                {active.draftType?.toUpperCase()}
              </p>
              <p className="text-sm text-sd-ink/65 mt-2.5 max-w-xl leading-relaxed">
                {active.spotlightDescription}
              </p>
              {active.spotifyUrl && (
                <a href={active.spotifyUrl} target="_blank" rel="noopener noreferrer"
                  className="text-[11px] tracking-[0.16em] text-sd-blue font-bold mt-2 block">
                  SPOTIFY EPISODE ↗
                </a>
              )}
            </div>
            <div className="shrink-0 flex flex-col gap-2">
              <button type="button" onClick={() => handleDeactivate(active.publicId!)}
                disabled={pendingId === active.publicId || rotating}
                className="border border-sd-ink/20 text-sd-ink font-oswald font-bold tracking-[0.14em] text-xs px-4 py-2 hover:border-sd-red hover:text-sd-red transition-colors disabled:opacity-40 disabled:cursor-not-allowed">
                {pendingId === active.publicId ? 'WORKING…' : 'DEACTIVATE'}
              </button>
              <button type="button" onClick={handleRotate}
                disabled={rotating || pendingId !== null}
                className="border border-sd-ink/20 text-sd-ink/60 font-oswald font-bold tracking-[0.14em] text-xs px-4 py-2 hover:border-sd-ink hover:text-sd-ink transition-colors disabled:opacity-40 disabled:cursor-not-allowed">
                {rotating ? 'ROTATING…' : 'ROTATE NOW'}
              </button>
            </div>
          </div>
        ) : (
          <div className="bg-white border border-dashed border-sd-ink/20 px-6 py-5 flex items-center justify-between gap-6">
            <p className="text-sd-ink/40 text-sm">
              No active spotlight — the home page is showing the fallback. Activate one from the
              library, or rotate to a random pick.
            </p>
            <button type="button" onClick={handleRotate}
              disabled={rotating || pendingId !== null}
              className="shrink-0 border border-sd-ink/20 text-sd-ink/60 font-oswald font-bold tracking-[0.14em] text-xs px-4 py-2 hover:border-sd-ink hover:text-sd-ink transition-colors disabled:opacity-40 disabled:cursor-not-allowed">
              {rotating ? 'ROTATING…' : 'ROTATE NOW'}
            </button>
          </div>
        )}
      </section>

      {error && <p className="text-sd-red text-sm -mt-4">{error}</p>}

      {/* ── Library ── */}
      <section>
        <div className="flex items-center justify-between mb-3">
          <h2 className="font-oswald font-bold text-base tracking-[0.1em] text-sd-ink/50">
            SPOTLIGHT LIBRARY
            {totalLibraryCount > 0 && (
              <span className="ml-2 text-sd-ink/30 font-normal">({totalLibraryCount})</span>
            )}
          </h2>
          {loading && <span className="text-[11px] tracking-[0.14em] text-sd-ink/30">Loading…</span>}
        </div>

        {/* Search + filter bar */}
        <div className="bg-white border border-sd-ink/10 border-b-0 px-4 py-3 flex items-center gap-3">
          <input
            type="text"
            value={librarySearch}
            onChange={e => setLibrarySearch(e.target.value)}
            placeholder="Search library…"
            className="flex-1 border border-sd-ink/15 bg-sd-paper px-3 py-1.5 text-sm text-sd-ink placeholder:text-sd-ink/30 focus:outline-none focus:border-sd-blue"
          />
          <select
            value={draftTypeFilter}
            onChange={e => setDraftTypeFilter(e.target.value)}
            className="border border-sd-ink/15 bg-sd-paper px-3 py-1.5 text-sm text-sd-ink focus:outline-none focus:border-sd-blue"
          >
            <option value="">All types</option>
            {DRAFT_TYPES.map(t => (
              <option key={t.value} value={t.value}>{t.label}</option>
            ))}
          </select>
          {(librarySearch || draftTypeFilter) && (
            <button
              type="button"
              onClick={() => { setLibrarySearch(''); setDraftTypeFilter(''); }}
              className="text-[11px] font-bold tracking-[0.12em] text-sd-ink/40 hover:text-sd-red transition-colors"
            >
              CLEAR
            </button>
          )}
        </div>

        <div className="bg-white border border-sd-ink/10">
          {libraryItems.length === 0 ? (
            <p className="px-5 py-6 text-sm text-sd-ink/40">
              {totalLibraryCount === 0 && !librarySearch && !draftTypeFilter
                ? 'No spotlights in the library yet. Add one below.'
                : 'No spotlights match your search.'}
            </p>
          ) : (
            <div className="divide-y divide-sd-ink/8">
              {libraryItems.map(item => (
                <div key={item.publicId} className="flex items-start gap-4 px-5 py-4 hover:bg-sd-paper/60 transition-colors">
                  <div className="flex-1 min-w-0">
                    <p className="font-oswald font-semibold text-sd-ink tracking-[0.02em] truncate">
                      {item.title}
                    </p>
                    <p className="text-[11px] tracking-[0.14em] text-sd-ink/40 mt-0.5">
                      {item.episodeNumber != null ? `EP. ${item.episodeNumber} · ` : ''}
                      {item.draftType?.toUpperCase()}
                    </p>
                    <p className="text-xs text-sd-ink/55 mt-1.5 line-clamp-1 leading-relaxed">
                      {item.spotlightDescription}
                    </p>
                  </div>
                  <div className="flex items-center gap-2 shrink-0 pt-0.5">
                    {deleteConfirmId === item.publicId ? (
                      <>
                        <span className="text-xs text-sd-red font-bold tracking-[0.1em]">DELETE?</span>
                        <button type="button" onClick={() => handleDelete(item.publicId!)}
                          disabled={pendingId === item.publicId}
                          className="border border-sd-red text-sd-red font-oswald font-bold tracking-[0.14em] text-xs px-3 py-1.5 disabled:opacity-40">
                          {pendingId === item.publicId ? '…' : 'CONFIRM'}
                        </button>
                        <button type="button" onClick={() => setDeleteConfirmId(null)}
                          className="text-sd-ink/40 text-xs font-bold tracking-[0.1em] hover:text-sd-ink transition-colors">
                          CANCEL
                        </button>
                      </>
                    ) : (
                      <>
                        <button type="button" onClick={() => handleActivate(item.publicId!)}
                          disabled={pendingId === item.publicId}
                          className="bg-sd-blue text-white font-oswald font-bold tracking-[0.14em] text-xs px-4 py-1.5 hover:bg-sd-blue/80 transition-colors disabled:opacity-40 disabled:cursor-not-allowed">
                          {pendingId === item.publicId ? 'WORKING…' : 'ACTIVATE'}
                        </button>
                        <button type="button" onClick={() => setDeleteConfirmId(item.publicId!)}
                          className="border border-sd-ink/15 text-sd-ink/40 font-oswald font-bold tracking-[0.14em] text-xs px-3 py-1.5 hover:border-sd-red hover:text-sd-red transition-colors">
                          DELETE
                        </button>
                      </>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}

          {/* Pagination — inside the white box, separated by a top border */}
          {totalLibraryCount > PAGE_SIZE && (
            <div className="flex items-center justify-between px-5 py-3 border-t border-sd-ink/8 bg-sd-paper/40">
              <p className="text-[11px] tracking-[0.14em] text-sd-ink/40 font-bold">
                PAGE {page} OF {totalPages}
              </p>
              <div className="flex items-center gap-2">
                <button type="button" onClick={() => setPage(p => p - 1)}
                  disabled={!hasPrev || loading}
                  className="border border-sd-ink/15 text-sd-ink/60 font-oswald font-bold tracking-[0.14em] text-xs px-3 py-1.5 hover:border-sd-ink hover:text-sd-ink transition-colors disabled:opacity-30 disabled:cursor-not-allowed">
                  ← PREV
                </button>
                <button type="button" onClick={() => setPage(p => p + 1)}
                  disabled={!hasNext || loading}
                  className="border border-sd-ink/15 text-sd-ink/60 font-oswald font-bold tracking-[0.14em] text-xs px-3 py-1.5 hover:border-sd-ink hover:text-sd-ink transition-colors disabled:opacity-30 disabled:cursor-not-allowed">
                  NEXT →
                </button>
              </div>
            </div>
          )}
        </div>
      </section>

      {/* ── Add to library ── */}
      <AddSpotlightForm accessToken={accessToken} onCreated={refresh} />
    </div>
  );
}