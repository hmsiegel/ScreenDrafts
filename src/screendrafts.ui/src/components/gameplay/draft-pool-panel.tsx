import Image from "next/image";
import { TMDB_IMAGE_BASE } from "@/services/movies/fetch-tmdb";

interface PoolMovie {
  tmdbId: number;
  title: string;
  year: number;
  posterUrl?: string;
}

interface DraftPoolPanelProps {
  movies: PoolMovie[];
}

export default function DraftPoolPanel({ movies }: DraftPoolPanelProps) {
  return (
    <div className="border border-sd-ink/20 bg-white">
      <div className="flex items-center gap-3 px-4 py-3 border-b border-sd-ink/10 bg-sd-ink">
        <div className="w-1 h-5 bg-sd-red shrink-0" />
        <h2 className="font-oswald font-bold text-sm uppercase tracking-wide text-white">
          Draft Pool
        </h2>
      </div>
      {/* TODO: filter picked movies once live pick data is wired */}
      {movies.length === 0 ? (
        <p className="px-4 py-6 text-sm font-mono text-sd-ink/40">No movies in pool.</p>
      ) : (
        <ul className="divide-y divide-sd-ink/5 max-h-[60vh] overflow-y-auto">
          {movies.map((movie) => (
            <li key={movie.tmdbId} className="flex items-center gap-3 px-3 py-2">
              {movie.posterUrl ? (
                <Image
                  src={`${TMDB_IMAGE_BASE}${movie.posterUrl}`}
                  alt={movie.title}
                  width={28}
                  height={42}
                  className="shrink-0 object-cover"
                />
              ) : (
                <div className="w-7 h-10 bg-sd-ink/10 shrink-0" />
              )}
              <div>
                <p className="text-sm font-medium text-sd-ink">{movie.title}</p>
                <p className="font-mono text-xs text-sd-ink/50">{movie.year}</p>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}