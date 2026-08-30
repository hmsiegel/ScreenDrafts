'use client';

import { useRef, useState } from "react";
import { MediaPicker, type SelectedMedia } from "@/components/drafts/media-picker";
import {
  addCandidateListEntry,
  bulkAddCandidateListEntries,
  removeCandidateListEntry,
} from "@/services/drafts/fetch-candidate-list";
import type { CandidateListEntryResponse } from "@/lib/dto";
import { MEDIA_TYPE_TV_EPISODE } from "@/lib/tv-episode-resolve";

interface CandidateListEditorProps {
  draftPartId: string;
  accessToken: string;
  initialEntries: CandidateListEntryResponse[];
  readonly?: boolean;
  /**
   * When the draft this part belongs to is restricted to one TV series,
   * pass its TMDb ID here to lock the picker into TV Episode mode with that
   * series pre-filled. Not wired up by any current caller — the Draft
   * response doesn't expose RestrictedTvSeriesTmdbId yet (needs an NSwag
   * regen after the backend delivery lands). Omit it and the picker falls
   * back to the manual Movie/TV Episode toggle, which works today.
   */
  fixedSeriesTmdbId?: number;
}

interface PendingEntry {
  media: SelectedMedia;
  notes: string;
}

export default function CandidateListEditor({
  draftPartId,
  accessToken,
  initialEntries,
  readonly = false,
  fixedSeriesTmdbId,
}: CandidateListEditorProps) {
  const [entries, setEntries] = useState<CandidateListEntryResponse[]>(initialEntries);
  const [pending, setPending] = useState<PendingEntry | null>(null);
  const [uploading, setUploading] = useState(false);
  const fileRef = useRef<HTMLInputElement>(null);

  async function handleSelect(media: SelectedMedia) {
    setPending({ media, notes: "" });
  }

  async function confirmAdd() {
    if (!pending) return;
    const { media, notes } = pending;
    await addCandidateListEntry(
      accessToken,
      draftPartId,
      media.tmdbId,
      media.mediaType,
      notes || undefined,
      media.tvSeriesTmdbId,
      media.seasonNumber,
      media.episodeNumber
    );
    setEntries((prev) => [
      ...prev,
      {
        entryId: crypto.randomUUID(),
        tmdbId: media.tmdbId,
        movieTitle: media.title,
        movieImdbId: undefined,
        addedByPublicId: "",
        notes: notes || undefined,
        createdOnUtc: new Date(),
        isPending: false,
        mediaType: { name: undefined, value: media.mediaType },
        tvSeriesTmdbId: media.tvSeriesTmdbId,
        seasonNumber: media.seasonNumber,
        episodeNumber: media.episodeNumber,
        tvSeriesTitle: media.tvSeriesTitle ?? undefined,
      } as CandidateListEntryResponse,
    ]);
    setPending(null);
  }

  async function handleRemove(tmdbId: number) {
    await removeCandidateListEntry(accessToken, draftPartId, tmdbId);
    setEntries((prev) => prev.filter((e) => e.tmdbId !== tmdbId));
  }

  async function handleBulkUpload(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    setUploading(true);
    try {
      await bulkAddCandidateListEntries(accessToken, draftPartId, file);
      window.location.reload();
    } finally {
      setUploading(false);
      if (fileRef.current) fileRef.current.value = "";
    }
  }

  return (
    <div className="space-y-4">
      {!readonly && (
        <div className="space-y-2">
          <MediaPicker
            onSelect={handleSelect}
            accessToken={accessToken}
            fixedSeriesTmdbId={fixedSeriesTmdbId}
          />
          {pending && (
            <div className="border border-sd-ink/20 bg-sd-paper p-3 space-y-2">
              <p className="text-sm font-medium text-sd-ink">
                <EntryLabel
                  title={pending.media.title}
                  year={pending.media.year}
                  mediaType={pending.media.mediaType}
                  tvSeriesTitle={pending.media.tvSeriesTitle}
                  seasonNumber={pending.media.seasonNumber}
                  episodeNumber={pending.media.episodeNumber}
                />
              </p>
              <input
                type="text"
                value={pending.notes}
                onChange={(e) => setPending((p) => p && { ...p, notes: e.target.value })}
                placeholder="Notes (optional)"
                className="w-full border border-sd-ink/20 bg-white px-3 py-1.5 text-sm font-mono text-sd-ink placeholder:text-sd-ink/40 focus:outline-none focus:border-sd-blue"
              />
              <div className="flex gap-2">
                <button
                  type="button"
                  onClick={confirmAdd}
                  className="bg-sd-blue text-white font-oswald font-medium uppercase tracking-wide px-4 py-1.5 text-sm hover:bg-sd-blue/90"
                >
                  Add
                </button>
                <button
                  type="button"
                  onClick={() => setPending(null)}
                  className="border border-sd-ink/20 text-sd-ink font-mono text-sm px-4 py-1.5 hover:bg-sd-ink/5"
                >
                  Cancel
                </button>
              </div>
            </div>
          )}
          <div>
            <button
              type="button"
              onClick={() => fileRef.current?.click()}
              disabled={uploading}
              className="border border-sd-ink/20 text-sd-ink/70 font-mono text-xs uppercase tracking-wide px-3 py-1.5 hover:bg-sd-ink/5 disabled:opacity-50"
            >
              {uploading ? "Uploading…" : "Bulk Upload (CSV)"}
            </button>
            <input
              ref={fileRef}
              type="file"
              accept=".csv"
              className="hidden"
              onChange={handleBulkUpload}
            />
          </div>
        </div>
      )}

      {entries.length === 0 ? (
        <p className="text-sm font-mono text-sd-ink/40">No entries yet.</p>
      ) : (
        <ul className="divide-y divide-sd-ink/10 border border-sd-ink/10">
          {entries.map((entry) => (
            <li key={entry.tmdbId} className="flex items-center gap-3 px-3 py-2">
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-sd-ink">
                  <EntryLabel
                    title={entry.movieTitle ?? `TMDb #${entry.tmdbId}`}
                    year={undefined}
                    mediaType={entry.mediaType?.value}
                    tvSeriesTitle={entry.tvSeriesTitle}
                    seasonNumber={entry.seasonNumber}
                    episodeNumber={entry.episodeNumber}
                  />
                </p>
                {entry.notes && (
                  <p className="text-xs text-sd-ink/60 mt-0.5 italic">{entry.notes}</p>
                )}
              </div>
              {!readonly && (
                <button
                  type="button"
                  onClick={() => handleRemove(entry.tmdbId ?? 0)}
                  className="text-sd-ink/40 hover:text-sd-red text-lg leading-none shrink-0"
                  aria-label="Remove"
                >
                  ×
                </button>
              )}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

/**
 * Shared display logic for both the pending-confirmation card and the
 * committed entry list — a TV episode entry shows "Series — SxxExx — Name"
 * instead of just the bare episode name, since the episode title alone
 * (e.g. "The City on the Edge of Forever") gives no clue which show it's
 * from. Falls back to the plain title/year rendering for movies, unchanged
 * from before this component supported episodes at all.
 */
function EntryLabel({
  title,
  year,
  mediaType,
  tvSeriesTitle,
  seasonNumber,
  episodeNumber,
}: {
  title: string;
  year?: string | null;
  mediaType?: number;
  tvSeriesTitle?: string | null;
  seasonNumber?: number;
  episodeNumber?: number;
}) {
  if (mediaType === MEDIA_TYPE_TV_EPISODE) {
    const code =
      seasonNumber != null && episodeNumber != null
        ? `S${String(seasonNumber).padStart(2, "0")}E${String(episodeNumber).padStart(2, "0")}`
        : null;
    return (
      <>
        {tvSeriesTitle && <span>{tvSeriesTitle} — </span>}
        {code && <span className="font-mono text-xs text-sd-ink/50">{code} — </span>}
        <span>{title}</span>
      </>
    );
  }

  return (
    <>
      {title}
      {year && <span className="font-mono text-xs text-sd-ink/50"> ({year})</span>}
    </>
  );
}