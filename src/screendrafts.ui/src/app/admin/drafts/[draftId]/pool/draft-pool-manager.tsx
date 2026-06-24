'use client';

import { useRef, useState } from "react";
import MovieSearchInput from "@/components/drafts/movie-search-input";
import {
  createDraftPool,
  addMovieToDraftPool,
  removeMovieFromDraftPool,
  bulkAddMoviesToDraftPool,
  type DraftPoolData,
} from "@/services/admin/fetch-admin-drafts";
import { type MovieSearchResult } from "@/services/movies/fetch-tmdb";
import Link from "next/link";

interface PoolMovie {
  tmdbId: number;
  title: string;
  year: string | null;
}

interface DraftPoolManagerProps {
  draftId: string;
  draftName: string;
  accessToken: string;
  initialPool: DraftPoolData | null;
  initialMovies: PoolMovie[];
}

export default function DraftPoolManager({
  draftId,
  draftName,
  accessToken,
  initialPool,
  initialMovies,
}: DraftPoolManagerProps) {
  const [pool, setPool] = useState<DraftPoolData | null>(initialPool);
  const [movies, setMovies] = useState<PoolMovie[]>(initialMovies);
  const [creating, setCreating] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [showCsvInfo, setShowCsvInfo] = useState(false);
  const fileRef = useRef<HTMLInputElement>(null);

  async function handleCreatePool() {
    setCreating(true);
    try {
      await createDraftPool(accessToken, draftId);
      setPool({ publicId: "", draftId, isLocked: false, tmdbIds: [] });
    } finally {
      setCreating(false);
    }
  }

  async function handleAddMovie(movie: MovieSearchResult) {
    await addMovieToDraftPool(accessToken, draftId, movie.tmdbId);
    setMovies((prev) => [...prev, { tmdbId: movie.tmdbId, title: movie.title, year: movie.year }]);
    setPool((prev) => prev ? { ...prev, tmdbIds: [...(prev.tmdbIds ?? []), movie.tmdbId] } : prev);
  }

  async function handleRemoveMovie(tmdbId: number) {
    await removeMovieFromDraftPool(accessToken, draftId, tmdbId);
    setMovies((prev) => prev.filter((m) => m.tmdbId !== tmdbId));
    setPool((prev) => prev ? { ...prev, tmdbIds: (prev.tmdbIds ?? []).filter((id) => id !== tmdbId) } : prev);
  }

  async function handleBulkUpload(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    setUploading(true);
    try {
      await bulkAddMoviesToDraftPool(accessToken, draftId, file);
      window.location.reload();
    } finally {
      setUploading(false);
      if (fileRef.current) fileRef.current.value = "";
    }
  }

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="px-6 md:px-10 py-10 max-w-[900px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">
          <Link href="/admin/drafts" className="hover:text-sd-ink transition-colors">
            / ADMIN / DRAFTS
          </Link>
          {" / "}{draftName.toUpperCase()} / POOL
        </p>

        <div className="flex items-end justify-between mb-10">
          <h1 className="font-oswald font-bold text-[48px] leading-none text-sd-ink">
            {draftName} — DRAFT POOL
          </h1>
        </div>

        <div className="bg-white border border-sd-ink/10">
          <div className="flex items-center gap-3 px-6 py-4 border-b border-sd-ink/10 bg-sd-ink">
            <div className="w-1 h-5 bg-sd-red shrink-0" />
            <h2 className="font-oswald font-bold text-[16px] tracking-wide uppercase text-white">
              Pool
            </h2>
          </div>
          <div className="p-6">
            {!pool ? (
              <div className="text-center py-8 space-y-4">
                <p className="text-sd-ink/60 font-mono text-sm">No pool created yet for this draft.</p>
                <button
                  type="button"
                  onClick={handleCreatePool}
                  disabled={creating}
                  className="bg-sd-blue text-white font-oswald font-medium uppercase tracking-wide px-6 py-3 hover:bg-sd-blue/90 disabled:opacity-50"
                >
                  {creating ? "Creating…" : "Create Pool"}
                </button>
              </div>
            ) : pool.isLocked ? (
              <div className="space-y-4">
                <div className="flex items-center gap-2 px-4 py-3 bg-yellow-50 border border-yellow-200 text-yellow-800 font-mono text-sm">
                  <span>🔒</span>
                  <span>This pool is locked. No changes can be made.</span>
                </div>
                <PoolList movies={movies} />
              </div>
            ) : (
              <div className="space-y-4">
                <MovieSearchInput
                  onSelect={handleAddMovie}
                  accessToken={accessToken}
                  placeholder="Search to add a film to pool…"
                />

                {/* Bulk upload row */}
                <div className="flex items-center gap-3">
                  <button
                    type="button"
                    onClick={() => fileRef.current?.click()}
                    disabled={uploading}
                    className="border border-sd-ink/20 text-sd-ink/70 font-mono text-xs uppercase tracking-wide px-3 py-1.5 hover:bg-sd-ink/5 disabled:opacity-50"
                  >
                    {uploading ? "Uploading…" : "Bulk Upload (CSV)"}
                  </button>
                  <button
                    type="button"
                    onClick={() => setShowCsvInfo((v) => !v)}
                    className="text-sd-ink/40 hover:text-sd-ink/70 font-mono text-xs underline underline-offset-2 transition-colors"
                  >
                    {showCsvInfo ? "Hide format" : "CSV format"}
                  </button>
                  <input
                    ref={fileRef}
                    type="file"
                    accept=".csv"
                    className="hidden"
                    onChange={handleBulkUpload}
                  />
                </div>

                {/* CSV format info card */}
                {showCsvInfo && (
                  <div className="border border-sd-ink/10 bg-sd-paper px-4 py-3 space-y-2">
                    <p className="font-oswald font-bold text-xs tracking-widest uppercase text-sd-ink/60">
                      CSV Format
                    </p>
                    <p className="font-mono text-xs text-sd-ink/70 leading-relaxed">
                      No header row. Each line should be:
                    </p>
                    <pre className="font-mono text-xs text-sd-ink bg-white border border-sd-ink/10 px-3 py-2 leading-relaxed">
{`603,The Matrix
680,Pulp Fiction
424,Schindler's List`}
                    </pre>
                    <p className="font-mono text-xs text-sd-ink/50 leading-relaxed">
                      Column 1: TMDb ID (required) · Column 2: Title (optional, for your reference only)
                    </p>
                    <p className="font-mono text-xs text-sd-ink/40">
                      Rows missing a TMDb ID will be skipped. Duplicates are ignored.
                    </p>
                  </div>
                )}

                <PoolList movies={movies} onRemove={handleRemoveMovie} />
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

function PoolList({
  movies,
  onRemove,
}: {
  movies: PoolMovie[];
  onRemove?: (tmdbId: number) => void;
}) {
  if (movies.length === 0) {
    return <p className="text-sm font-mono text-sd-ink/40">No movies in pool yet.</p>;
  }
  return (
    <table className="w-full text-sm">
      <thead>
        <tr className="border-b border-sd-ink/10">
          <th className="text-left font-mono text-[11px] tracking-widest uppercase text-sd-ink/50 pb-3 pr-4">Title</th>
          <th className="text-left font-mono text-[11px] tracking-widest uppercase text-sd-ink/50 pb-3 pr-4">Year</th>
          <th />
        </tr>
      </thead>
      <tbody>
        {movies.map((movie) => (
          <tr key={movie.tmdbId} className="border-b border-sd-ink/5 hover:bg-sd-paper/60">
            <td className="py-2 pr-4 font-medium text-sd-ink">{movie.title}</td>
            <td className="py-2 pr-4 font-mono text-sd-ink/60">{movie.year ?? ""}</td>
            <td className="py-2 text-right">
              {onRemove && (
                <button
                  type="button"
                  onClick={() => onRemove(movie.tmdbId)}
                  className="text-sd-ink/40 hover:text-sd-red text-xl leading-none"
                  aria-label="Remove"
                >
                  ×
                </button>
              )}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}