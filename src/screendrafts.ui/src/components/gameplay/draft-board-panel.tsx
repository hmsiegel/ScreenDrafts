import CandidateListEditor from "@/components/drafts/candidate-list-editor";
import Image from "next/image";
import { TMDB_IMAGE_BASE } from "@/services/movies/fetch-tmdb";
import { CandidateListEntryResponse, DraftBoardItemResponse } from "@/lib/dto";

interface DraftBoardPanelProps {
  isHost: boolean;
  accessToken: string;
  draftPartId: string;
  board: DraftBoardItemResponse[];
  candidateList: CandidateListEntryResponse[];
}

export default function DraftBoardPanel({
  isHost,
  accessToken,
  draftPartId,
  board,
  candidateList,
}: DraftBoardPanelProps) {
  return (
    <div className="border border-sd-ink/20 bg-white">
      <div className="flex items-center gap-3 px-4 py-3 border-b border-sd-ink/10 bg-sd-ink">
        <div className="w-1 h-5 bg-sd-red shrink-0" />
        <h2 className="font-oswald font-bold text-sm uppercase tracking-wide text-white">
          {isHost ? "Hosting" : "My Board"}
        </h2>
      </div>
      <div className="p-4 space-y-4">
        {isHost ? (
          <p className="text-sm font-mono text-sd-ink/60">You are hosting this draft.</p>
        ) : (
          <ul className="divide-y divide-sd-ink/5 border border-sd-ink/10">
            {board.length === 0 ? (
              <li className="px-3 py-4 text-sm font-mono text-sd-ink/40">No movies on board yet.</li>
            ) : (
              board
                .slice()
                .sort((a, b) => (a.priority ?? 999) - (b.priority ?? 999))
                .map((movie) => (
                  <li key={movie.tmdbId} className="flex items-center gap-3 px-3 py-2">
                    {movie.posterUrl ? (
                      <Image
                        src={`${TMDB_IMAGE_BASE}${movie.posterUrl}`}
                        alt={movie.title || ""}
                        width={28}
                        height={42}
                        className="shrink-0 object-cover"
                      />
                    ) : (
                      <div className="w-7 h-10 bg-sd-ink/10 shrink-0" />
                    )}
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium text-sd-ink">{movie.title}</p>
                      <p className="font-mono text-xs text-sd-ink/50">{movie.year}</p>
                    </div>
                    {movie.priority != null && (
                      <span className="font-mono text-xs text-sd-ink/40">#{movie.priority}</span>
                    )}
                  </li>
                ))
            )}
          </ul>
        )}

        {candidateList.length > 0 && (
          <details className="border border-sd-ink/10">
            <summary className="px-3 py-2 font-oswald font-bold text-sm uppercase tracking-wide text-sd-ink cursor-pointer hover:bg-sd-paper/60 select-none">
              Candidate List
            </summary>
            <div className="p-3">
              <CandidateListEditor
                draftPartId={draftPartId}
                accessToken={accessToken}
                initialEntries={candidateList}
                readonly={true}
              />
            </div>
          </details>
        )}
      </div>
    </div>
  );
}