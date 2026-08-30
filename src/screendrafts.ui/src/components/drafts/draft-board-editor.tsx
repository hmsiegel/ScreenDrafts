'use client';

import { useRef, useState } from "react";
import { MediaPicker, type SelectedMedia } from "@/components/drafts/media-picker";
import {
  addMovieToDraftBoard,
  removeMovieFromDraftBoard,
  updateDraftBoardItem,
  updateDraftBoardOrder,
} from "@/services/drafts/fetch-draft-board";
import type { DraftBoardItemResponse } from "@/lib/dto";
import { MEDIA_TYPE_TV_EPISODE } from "@/lib/tv-episode-resolve";

interface DraftBoardEditorProps {
  draftId: string;
  accessToken: string;
  initialBoard: DraftBoardItemResponse[];
  /** See the same prop on CandidateListEditor — not wired up by any caller yet. */
  fixedSeriesTmdbId?: number;
}

interface PendingEntry {
  media: SelectedMedia;
  notes: string;
  priority: string;
}

export default function DraftBoardEditor({
  draftId,
  accessToken,
  initialBoard,
  fixedSeriesTmdbId,
}: DraftBoardEditorProps) {
  const [board, setBoard] = useState<DraftBoardItemResponse[]>(initialBoard);
  const [pending, setPending] = useState<PendingEntry | null>(null);
  const dragIdx = useRef<number | null>(null);

  async function handleSelect(media: SelectedMedia) {
    setPending({ media, notes: "", priority: "" });
  }

  async function confirmAdd() {
    if (!pending) return;
    const { media, notes, priority: priorityStr } = pending;
    const priority = priorityStr ? parseInt(priorityStr, 10) : undefined;
    await addMovieToDraftBoard(
      accessToken,
      draftId,
      media.tmdbId,
      media.mediaType,
      notes || undefined,
      priority,
      media.tvSeriesTmdbId,
      media.seasonNumber,
      media.episodeNumber
    );
    setBoard((prev) => [
      ...prev,
      {
        tmdbId: media.tmdbId,
        title: media.title,
        year: media.year ?? undefined,
        notes: notes || undefined,
        priority,
        mediaType: { name: undefined, value: media.mediaType },
        tvSeriesTmdbId: media.tvSeriesTmdbId,
        seasonNumber: media.seasonNumber,
        episodeNumber: media.episodeNumber,
        tvSeriesTitle: media.tvSeriesTitle ?? undefined,
      } as DraftBoardItemResponse,
    ]);
    setPending(null);
  }

  async function handleRemove(tmdbId: number) {
    await removeMovieFromDraftBoard(accessToken, draftId, tmdbId);
    setBoard((prev) => prev.filter((m) => m.tmdbId !== tmdbId));
  }

  async function handleNotesChange(tmdbId: number, notes: string) {
    setBoard((prev) => prev.map((m) => m.tmdbId === tmdbId ? { ...m, notes } : m));
    await updateDraftBoardItem(accessToken, draftId, tmdbId, notes || undefined, undefined);
  }

  async function handlePriorityChange(tmdbId: number, priorityStr: string) {
    const priority = priorityStr ? parseInt(priorityStr, 10) : undefined;
    setBoard((prev) => prev.map((m) => m.tmdbId === tmdbId ? { ...m, priority } : m));
    await updateDraftBoardItem(accessToken, draftId, tmdbId, undefined, priority);
  }

  function handleDragStart(idx: number) { dragIdx.current = idx; }

  function handleDragOver(e: React.DragEvent, idx: number) {
    e.preventDefault();
    if (dragIdx.current === null || dragIdx.current === idx) return;
    const newBoard = [...board];
    const [moved] = newBoard.splice(dragIdx.current, 1);
    newBoard.splice(idx, 0, moved);
    dragIdx.current = idx;
    setBoard(newBoard);
  }

  async function handleDrop() {
    if (dragIdx.current === null) return;
    dragIdx.current = null;
    await updateDraftBoardOrder(accessToken, draftId, board.map((m) => m.tmdbId ?? 0));
  }

  return (
    <div className="space-y-4">
      <div className="space-y-2">
        <MediaPicker
          onSelect={handleSelect}
          accessToken={accessToken}
          fixedSeriesTmdbId={fixedSeriesTmdbId}
        />
        {pending && (
          <div className="border border-sd-ink/20 bg-sd-paper p-3 space-y-2">
            <p className="text-sm font-medium text-sd-ink">
              <BoardEntryLabel
                title={pending.media.title}
                year={pending.media.year}
                mediaType={pending.media.mediaType}
                tvSeriesTitle={pending.media.tvSeriesTitle}
                seasonNumber={pending.media.seasonNumber}
                episodeNumber={pending.media.episodeNumber}
              />
            </p>
            <div className="flex gap-2">
              <input
                type="text"
                value={pending.notes}
                onChange={(e) => setPending((p) => p && { ...p, notes: e.target.value })}
                placeholder="Notes (optional)"
                className="flex-1 border border-sd-ink/20 bg-white px-3 py-1.5 text-sm font-mono text-sd-ink placeholder:text-sd-ink/40 focus:outline-none focus:border-sd-blue"
              />
              <input
                type="number"
                value={pending.priority}
                onChange={(e) => setPending((p) => p && { ...p, priority: e.target.value })}
                placeholder="Priority"
                className="w-24 border border-sd-ink/20 bg-white px-3 py-1.5 text-sm font-mono text-sd-ink placeholder:text-sd-ink/40 focus:outline-none focus:border-sd-blue"
              />
            </div>
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
      </div>

      {board.length === 0 ? (
        <p className="text-sm font-mono text-sd-ink/40">No movies on board yet.</p>
      ) : (
        <ul className="divide-y divide-sd-ink/10 border border-sd-ink/10">
          {board.map((movie, idx) => (
            <li
              key={movie.tmdbId}
              draggable
              onDragStart={() => handleDragStart(idx)}
              onDragOver={(e) => handleDragOver(e, idx)}
              onDrop={handleDrop}
              className="flex items-center gap-3 px-3 py-2 bg-white hover:bg-sd-paper/40 cursor-grab active:cursor-grabbing"
            >
              <span className="text-sd-ink/30 font-mono text-xs select-none">⠿</span>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-sd-ink">
                  <BoardEntryLabel
                    title={movie.title ?? `TMDb #${movie.tmdbId}`}
                    year={movie.year}
                    mediaType={movie.mediaType?.value}
                    tvSeriesTitle={movie.tvSeriesTitle}
                    seasonNumber={movie.seasonNumber}
                    episodeNumber={movie.episodeNumber}
                  />
                </p>
                <input
                  type="text"
                  defaultValue={movie.notes ?? ""}
                  onBlur={(e) => handleNotesChange(movie.tmdbId ?? 0, e.target.value)}
                  placeholder="Notes…"
                  className="mt-1 w-full border-b border-sd-ink/10 bg-transparent text-xs font-mono text-sd-ink placeholder:text-sd-ink/30 focus:outline-none focus:border-sd-blue"
                />
              </div>
              <input
                type="number"
                defaultValue={movie.priority ?? ""}
                onBlur={(e) => handlePriorityChange(movie.tmdbId ?? 0, e.target.value)}
                placeholder="Pri"
                className="w-16 border border-sd-ink/20 bg-white px-2 py-1 text-xs font-mono text-center text-sd-ink focus:outline-none focus:border-sd-blue"
              />
              <button
                type="button"
                onClick={() => handleRemove(movie.tmdbId ?? 0)}
                className="text-sd-ink/40 hover:text-sd-red text-lg leading-none shrink-0"
                aria-label="Remove"
              >
                ×
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

/** Same rendering rule as CandidateListEditor's EntryLabel — kept as a
 * separate small component rather than shared/exported, since the two
 * callers' prop shapes (year required vs optional, etc.) diverge slightly
 * and it's a handful of lines either way. */
function BoardEntryLabel({
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