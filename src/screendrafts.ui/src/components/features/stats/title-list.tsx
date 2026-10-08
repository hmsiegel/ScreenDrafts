// components/features/stats/title-list.tsx
import Link from "next/link";
import type { TitleHonorificEntryView } from "@/services/stats/stats-types";
import { formatReleaseDate, formatSpan } from "./format-stat-value";

/** "Part 4" for a multi-part draft. A single-part draft shows nothing. */
function partLabel(partIndex: number, totalParts: number): string | null {
  return totalParts > 1 ? `Part ${partIndex}` : null;
}

function joinedLine(joinText: string, entry: TitleHonorificEntryView): string {
  const where = [
    entry.joinedEpisode !== null ? `Ep. ${entry.joinedEpisode}` : null,
    partLabel(entry.joinedPartIndex, entry.joinedTotalParts),
    formatReleaseDate(entry.joinedOn),
  ]
    .filter((p): p is string => p !== null)
    .join(" · ");

  const base = `${joinText} on ${entry.joinedDraftTitle}`;
  return where ? `${base} · ${where}` : base;
}

function gapLine(entry: TitleHonorificEntryView): string | null {
  const span = formatSpan(entry.firstOn, entry.joinedOn);
  const episodes =
    entry.gapEpisodes !== null && entry.gapEpisodes > 0
      ? `${entry.gapEpisodes} ${entry.gapEpisodes === 1 ? "episode" : "episodes"}`
      : null;

  if (episodes && span) return `${episodes} after its first draft (${span})`;
  if (episodes) return `${episodes} after its first draft`;
  if (span) return `${span} after its first draft`;
  return null;
}

/** One row per title, in a collapsible card: the join line up front, every appearance when opened. */
export function TitleList({
  joinText,
  titles,
}: {
  joinText: string;
  titles: TitleHonorificEntryView[];
}) {
  if (titles.length === 0) {
    return (
      <p className="font-mono text-sm text-sd-ink/50 py-10 text-center bg-white border-2 border-sd-ink">
        No titles match.
      </p>
    );
  }

  return (
    <ul className="flex flex-col gap-3">
      {titles.map((entry) => {
        const gap = gapLine(entry);

        return (
          <li key={entry.mediaPublicId}>
            <details className="bg-white border-2 border-sd-ink">
              <summary className="cursor-pointer list-none px-4 py-3 sm:px-5 flex items-start gap-3 sm:gap-4">
                <span className="font-oswald font-bold text-[20px] sm:text-[24px] leading-none text-sd-red shrink-0 w-14 sm:w-16">
                  #{entry.number}
                </span>

                <span className="flex-1 min-w-0">
                  <Link
                    href={`/media/${entry.mediaPublicId}`}
                    className="font-oswald font-bold text-[18px] sm:text-[20px] leading-tight text-sd-blue hover:text-sd-red transition-colors [overflow-wrap:anywhere]"
                  >
                    {entry.title}
                  </Link>
                  <span className="block font-mono text-[11px] text-[#5a6075] mt-1 [overflow-wrap:anywhere]">
                    {joinedLine(joinText, entry)}
                  </span>
                  {gap && (
                    <span className="block font-mono text-[11px] text-[#5a6075] [overflow-wrap:anywhere]">
                      {gap}
                    </span>
                  )}
                </span>

                <span className="shrink-0 text-right">
                  <span className="block font-oswald font-bold text-[20px] leading-none text-sd-ink">
                    {entry.appearanceCount}
                  </span>
                  <span className="block font-mono text-[10px] tracking-widest text-[#5a6075]">DRAFTS</span>
                </span>
              </summary>

              <ol className="border-t border-sd-ink/15 px-4 py-3 sm:px-5 flex flex-col gap-1.5">
                {entry.appearances.map((appearance) => {
                  const episode = appearance.episodeNumber !== null ? `Ep. ${appearance.episodeNumber}` : null;
                  const date = formatReleaseDate(appearance.releasedOn);
                  const detail = [
                    episode,
                    partLabel(appearance.partIndex, appearance.totalParts),
                    date,
                    `No. ${appearance.position}`,
                  ]
                    .filter((p): p is string => p !== null)
                    .join(" · ");

                  return (
                    <li
                      key={`${appearance.appearanceNumber}:${appearance.draftPublicId}`}
                      className="flex gap-3 items-baseline"
                    >
                      <span className="font-mono text-[11px] text-[#5a6075] w-5 shrink-0">
                        {appearance.appearanceNumber}.
                      </span>
                      <span className="min-w-0">
                        <Link
                          href={`/drafts/${appearance.draftPublicId}`}
                          className="font-oswald font-bold text-[15px] text-sd-blue hover:text-sd-red transition-colors [overflow-wrap:anywhere]"
                        >
                          {appearance.draftTitle}
                        </Link>
                        <span className="font-mono text-[11px] text-[#5a6075] ml-2">{detail}</span>
                      </span>
                    </li>
                  );
                })}
              </ol>
            </details>
          </li>
        );
      })}
    </ul>
  );
}
