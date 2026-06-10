'use client';

import { useRef, useState } from "react";
import MovieSearchInput from "@/components/drafts/movie-search-input";
import {
  addMovieToDraftBoard,
  removeMovieFromDraftBoard,
  updateDraftBoardItem,
  updateDraftBoardOrder,
} from "@/services/drafts/fetch-draft-board";
import { type MovieSearchResult } from "@/services/movies/fetch-movies";
import type { DraftBoardItemResponse } from "@/lib/dto";

interface DraftBoardEditorProps {
  draftId: string;
  accessToken: string;
  initialBoard: DraftBoardItemResponse[];
}

interface PendingMovie {
  movie: MovieSearchResult;
  notes: string;
  priority: string;
}

export default function DraftBoardEditor({ draftId, accessToken, initialBoard }: DraftBoardEditorProps) {
  const [board, setBoard] = useState<DraftBoardItemResponse[]>(initialBoard);
  const [pending, setPending] = useState<PendingMovie | null>(null);
  const dragIdx = useRef<number | null>(null);

  async function handleSelect(movie: MovieSearchResult) {
    setPending({ movie, notes: "", priority: "" });
  }

  async function confirmAdd() {
    if (!pending) return;
    const priority = pending.priority ? parseInt(pending.priority, 10) : undefined;
    await addMovieToDraftBoard(
      accessToken,
      draftId,
      pending.movie.tmdbId,
      pending.notes || undefined,
      priority
    );
    setBoard((prev) => [
      ...prev,
      {
        tmdbId: pending.movie.tmdbId,
        title: pending.movie.title,
        year: pending.movie.year ?? undefined,
        notes: pending.notes || undefined,
        priority,
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
        <MovieSearchInput
          onSelect={handleSelect}
          accessToken={accessToken}
          placeholder="Search to add a film…"
        />
        {pending && (
          <div className="border border-sd-ink/20 bg-sd-paper p-3 space-y-2">
            <p className="text-sm font-medium text-sd-ink">
              {pending.movie.title}{" "}
              <span className="font-mono text-xs text-sd-ink/50">{pending.movie.year ?? ""}</span>
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
                  {movie.title ?? `TMDb #${movie.tmdbId}`}
                </p>
                {movie.year && (
                  <p className="font-mono text-xs text-sd-ink/50">{movie.year}</p>
                )}
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