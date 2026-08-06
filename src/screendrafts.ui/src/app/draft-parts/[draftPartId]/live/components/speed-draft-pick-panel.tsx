// app/draft-parts/[draftPartId]/live/components/speed-draft-pick-panel.tsx
'use client';

import { useState, useEffect } from 'react';

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

// ── Resolve helpers — unchanged from before, still needed for import/poll ──

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
          const res = await fetch(
            `${API}/media/search?query=${encodeURIComponent(subjectName)}`,
            { headers: { Authorization: `Bearer ${accessToken}` } },
          );
          if (!res.ok) throw new Error(`Failed to search: ${res.status}`);
          const data = await res.json();
          if (cancelled) return;
          const items: TitleSearchItem[] = data.results?.items ?? data.items ?? [];
          setTitleResults(items);
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
  }, [isPersonSubject, subjectImdbId, subjectName, accessToken]);

  async function submitPick(mediaPublicId: string | null, item: {
    tmdbId?: number | null;
    imdbId?: string | null;
    mediaType: number;
  }, title: string, submittingKey: string) {
    if (slotAlreadyPicked || submitting !== null) return;
    setSubmitting(submittingKey);
    setError(null);
    try {
      let resolvedPublicId = mediaPublicId;

      if (!resolvedPublicId) {
        resolvedPublicId = await importAndResolve(item, accessToken);
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

      <div className="max-h-80 overflow-y-auto">
        {loading && (
          <div className="px-4 py-6 text-center text-white/30 text-xs font-mono animate-pulse">
            Loading…
          </div>
        )}

        {!loading && isPersonSubject && credits.length === 0 && (
          <div className="px-4 py-6 text-center text-white/30 text-xs font-mono italic">
            No filmography found.
          </div>
        )}

        {!loading &&
          isPersonSubject &&
          credits.map((c) => {
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

        {!loading && !isPersonSubject && titleResults.length === 0 && (
          <div className="px-4 py-6 text-center text-white/30 text-xs font-mono italic">
            No results for &ldquo;{subjectName}&rdquo;.
          </div>
        )}

        {!loading &&
          !isPersonSubject &&
          titleResults.map((item) => {
            const key =
              item.mediaPublicId || `${item.tmdbId != null ? 'tmdb' : 'imdb'}-${item.tmdbId ?? item.imdbId}`;
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
      </div>
    </div>
  );
}