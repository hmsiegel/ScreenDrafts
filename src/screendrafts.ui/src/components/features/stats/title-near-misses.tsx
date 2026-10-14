// components/features/stats/title-near-misses.tsx
import Link from "next/link";
import type { TitleNearMissView } from "@/services/stats/stats-types";

const CYCLE_PLACES = [1, 2, 3, 4] as const;

/** Titles one place short of The Cycle: the three places they hold, filled, and the one they lack, dashed. */
export function TitleNearMisses({ titles }: { titles: TitleNearMissView[] }) {
  if (titles.length === 0) return null;

  return (
    <section aria-labelledby="near-cycle-heading" className="flex flex-col gap-3">
      <div>
        <h2
          id="near-cycle-heading"
          className="font-oswald font-bold text-[13px] tracking-widest text-sd-red"
        >
          CLOSE TO THE CYCLE
        </h2>
        <p className="text-sm text-sd-ink/70 mt-1">
          One place short. Each title has finished at three of the four places and needs the last.
        </p>
      </div>

      <ul className="flex flex-col gap-3">
        {titles.map((entry) => (
          <li
            key={entry.mediaPublicId}
            className="bg-white border-2 border-sd-ink px-4 py-3 sm:px-5 flex items-center gap-3 sm:gap-4"
          >
            <span className="flex-1 min-w-0">
              <Link
                href={`/media/${entry.mediaPublicId}`}
                className="font-oswald font-bold text-[18px] sm:text-[20px] leading-tight text-sd-blue hover:text-sd-red transition-colors wrap-anywhere"
              >
                {entry.title}
              </Link>
              <span className="block font-mono text-[11px] text-[#5a6075] mt-1">
                Needs No. {entry.missingPosition} · {entry.appearanceCount} drafts
              </span>
            </span>

            <span className="shrink-0 flex gap-1.5">
              {CYCLE_PLACES.map((place) => {
                const held = entry.positionsHeld.includes(place);

                return (
                  <span
                    key={place}
                    className={`min-w-9 px-1.5 py-1 text-center font-mono text-[11px] ${
                      held
                        ? "bg-sd-ink text-white border-2 border-sd-ink"
                        : "border-2 border-dashed border-sd-red text-sd-red"
                    }`}
                  >
                    <span className="sr-only">{held ? "Finished at " : "Needs "}</span>
                    No. {place}
                  </span>
                );
              })}
            </span>
          </li>
        ))}
      </ul>
    </section>
  );
}
