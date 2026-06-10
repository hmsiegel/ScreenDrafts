'use client';

import { useRef, useState } from "react";
import MovieSearchInput from "@/components/drafts/movie-search-input";
import {
  addCandidateListEntry,
  bulkAddCandidateListEntries,
  removeCandidateListEntry,
} from "@/services/drafts/fetch-candidate-list";
import { type MovieSearchResult } from "@/services/movies/fetch-movies";
import type { CandidateListEntryResponse } from "@/lib/dto";

interface CandidateListEditorProps {
  draftPartId: string;
  accessToken: string;
  initialEntries: CandidateListEntryResponse[];
  readonly?: boolean;
}

interface PendingMovie {
  movie: MovieSearchResult;
  notes: string;
}

export default function CandidateListEditor({
  draftPartId,
  accessToken,
  initialEntries,
  readonly = false,
}: CandidateListEditorProps) {
  const [entries, setEntries] = useState<CandidateListEntryResponse[]>(initialEntries);
  const [pending, setPending] = useState<PendingMovie | null>(null);
  const [uploading, setUploading] = useState(false);
  const fileRef = useRef<HTMLInputElement>(null);

  async function handleSelect(movie: MovieSearchResult) {
    setPending({ movie, notes: "" });
  }

  async function confirmAdd() {
    if (!pending) return;
    await addCandidateListEntry(
      accessToken,
      draftPartId,
      pending.movie.tmdbId,
      pending.notes || undefined
    );
    setEntries((prev) => [
      ...prev,
      {
        entryId: crypto.randomUUID(),
        tmdbId: pending.movie.tmdbId,
        movieTitle: pending.movie.title,
        movieImdbId: undefined,
        addedByPublicId: "",
        notes: pending.notes || undefined,
        createdOnUtc: new Date(),
        isPending: false,
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
                <p className="text-sm font-medium text-sd-ink">{entry.movieTitle}</p>
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