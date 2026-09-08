// app/guest-drafts/[guestDraftId]/live/components/pick-source-panel.tsx
'use client';

import { useState } from 'react';
import { useGuestDraftLive } from '../guest-draft-context';
import { playGuestDraftPick } from '../gameplay-fetchers';
import { importAndResolve, ResolvedMovie } from '@/lib/movie-resolve';
import { importAndResolveEpisode, MEDIA_TYPE_TV_EPISODE } from '@/lib/tv-episode-resolve';
import { MediaPicker, type SelectedMedia } from '@/components/drafts/media-picker';
import { DARK_THEME } from '@/components/drafts/media-picker-theme';

// ── Props ─────────────────────────────────────────────────────────────────────

interface Props {
  accessToken: string;
  guestDraftId: string;
  activeSlot: number;
  onPickSubmitted: (playOrder: number, movieTitle: string) => void;
}

// ── Component ─────────────────────────────────────────────────────────────────
//
// Search-only for now — GuestDrafts has no pool/board/candidate-list concept
// yet. If one gets added later, this is the same fork point pick-source-panel.tsx
// uses for DraftParts: add a SourceTab union, a tab bar, and a sibling source
// component next to SearchSource below.

export function PickSourcePanel({
  accessToken,
  guestDraftId,
  activeSlot,
  onPickSubmitted,
}: Props) {
  const { picks } = useGuestDraftLive();

  const [submitting, setSubmitting] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  // Prevent picking if the slot is already filled (e.g. a SignalR update
  // landing mid-submit). isActiveOnFinalBoard already applies the Landed
  // formula server-side (see GuestDraftGameplayPickResponse's remarks).
  const slotAlreadyPicked = picks.some(
    (p) => p.position === activeSlot && p.isActiveOnFinalBoard,
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
      await playGuestDraftPick(accessToken, guestDraftId, {
        moviePublicId: resolvedMovie.mediaPublicId,
        position: activeSlot,
        playOrder,
      });
      onPickSubmitted(playOrder, resolvedMovie.title);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to submit pick.');
    } finally {
      setSubmitting(null);
    }
  }

  return (
    <div className="mt-6 border border-white/10">
      {error && <p className="px-4 py-2 text-sd-red text-xs font-mono">{error}</p>}
      <div className="max-h-72 overflow-y-auto">
        <SearchSource
          accessToken={accessToken}
          submitting={submitting}
          onPick={handlePick}
          disabled={slotAlreadyPicked}
          // GetGuestDraftGameplayResponse has no restrictedTvSeriesTmdbId field
          // yet (canonical's gameplay response does) — TV-restricted guest
          // drafts aren't wired through to the read model, so this is left
          // unset rather than guessed. Flag if that field needs adding.
          fixedSeriesTmdbId={undefined}
        />
      </div>
    </div>
  );
}

// ── Search ────────────────────────────────────────────────────────────────────
// MediaPicker handles both the free-text movie search and the TV episode
// browse-a-season flow. Every selection is resolved/imported here before
// being handed to onPick, unchanged from the DraftParts version.

function SearchSource({
  accessToken,
  submitting,
  onPick,
  disabled,
  fixedSeriesTmdbId,
}: {
  accessToken: string;
  submitting: string | null;
  onPick: (movie: ResolvedMovie) => void;
  disabled: boolean;
  fixedSeriesTmdbId?: number;
}) {
  const [resolving, setResolving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleMediaSelect(media: SelectedMedia) {
    setError(null);

    if (media.mediaType === MEDIA_TYPE_TV_EPISODE) {
      if (!media.tvSeriesTmdbId || !media.seasonNumber || !media.episodeNumber) {
        setError('Missing episode identity — try selecting again.');
        return;
      }
      setResolving(true);
      try {
        const publicId = await importAndResolveEpisode(
          media.tmdbId,
          media.tvSeriesTmdbId,
          media.seasonNumber,
          media.episodeNumber,
          accessToken,
        );
        if (!publicId) {
          setError('Episode could not be imported in time. Try again in a moment.');
          return;
        }
        onPick({
          mediaPublicId: publicId,
          tmdbId: media.tmdbId,
          title: media.title,
          year: media.year,
          posterUrl: null,
        });
      } finally {
        setResolving(false);
      }
      return;
    }

    // Movie — leave mediaPublicId blank. handlePick (the parent's onPick)
    // already knows how to import-then-resolve a movie by tmdbId via its
    // own `!movie.mediaPublicId` branch.
    onPick({
      mediaPublicId: '',
      tmdbId: media.tmdbId,
      title: media.title,
      year: media.year,
      posterUrl: null,
    });
  }

  return (
    <div>
      <div className="px-3 py-2 border-b border-white/10">
        <MediaPicker
          accessToken={accessToken}
          onSelect={handleMediaSelect}
          disabled={disabled || submitting !== null || resolving}
          fixedSeriesTmdbId={fixedSeriesTmdbId}
          theme={DARK_THEME}
        />
      </div>
      {resolving && (
        <div className="px-4 py-6 text-center text-white/30 text-xs font-mono animate-pulse">
          Loading…
        </div>
      )}
      {error && <p className="px-4 py-2 text-sd-red text-xs font-mono">{error}</p>}
    </div>
  );
}