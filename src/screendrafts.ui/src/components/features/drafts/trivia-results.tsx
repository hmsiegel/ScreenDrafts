import type { TriviaResultResponse } from "@/lib/dto";

interface TriviaResultsProps {
  results: TriviaResultResponse[];
}

export function TriviaResults({ results }: TriviaResultsProps) {
  if (results.length === 0) return null;

  return (
    <section>
      <h2
        className="font-['Oswald'] text-[10px] font-semibold tracking-[0.15em] uppercase text-sd-dim mb-2"
      >
        Trivia
      </h2>
      <ol className="space-y-1">
        {results.map((r) => (
          <li
            key={r.position}
            className="flex items-baseline gap-2 font-['JetBrains_Mono'] text-xs"
          >
            <span className="text-sd-red font-bold tabular-nums w-4 shrink-0">
              {r.position}.
            </span>
            <span className="text-sd-ink font-medium">{r.participantDisplayName}</span>
            <span className="text-sd-dim ml-auto tabular-nums whitespace-nowrap">
              {r.questionsWon}
            </span>
          </li>
        ))}
      </ol>
    </section>
  );
}