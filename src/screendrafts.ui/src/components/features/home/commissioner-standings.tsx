interface Standing {
  rank: number;
  name: string;
  score: number;
}

const rankColors = ['bg-sd-blue', 'bg-sd-red', 'bg-sd-ink', 'bg-gray-400'];

export default function CommissionerStandings({
  standings,
  maxScore,
}: {
  standings: Standing[];
  maxScore: number;
}) {
  return (
    <div className="bg-white border-2 border-sd-ink rounded-sm">
      <div className="bg-sd-red text-white px-5 py-3.5">
        <div className="font-oswald font-bold text-[22px] tracking-[0.06em]">COMMISSIONER PREDICTIONS</div>
        <div className="text-[11px] tracking-[0.18em] opacity-85 mt-0.5">SEASON 4 · EPS 290–310</div>
      </div>

      <div className="px-5 py-5 space-y-0 divide-y divide-gray-100">
        {standings.map((s, i) => (
          <div key={s.name} className="flex items-center gap-3 py-3">
            <div
              className={`w-7 h-7 rounded ${rankColors[i] ?? 'bg-gray-400'} text-white font-oswald font-bold text-sm flex items-center justify-center flex-shrink-0`}
            >
              {s.rank}
            </div>
            <div className="font-oswald font-semibold text-lg tracking-[0.04em] w-16">{s.name}</div>
            <div className="flex-1 h-2 bg-gray-100 rounded overflow-hidden">
              <div
                className={`h-full ${i === 0 ? 'bg-sd-red' : 'bg-sd-blue'}`}
                style={{ width: `${(s.score / maxScore) * 100}%` }}
              />
            </div>
            <div className="font-mono text-lg font-bold w-9 text-right">{s.score}</div>
          </div>
        ))}
      </div>

      <div className="mx-5 mb-5 p-3.5 bg-sd-paper border border-dashed border-sd-ink text-sm leading-relaxed">
        <div className="text-[10px] tracking-[0.22em] text-sd-red mb-1.5 font-bold">★ HOW TO PLAY</div>
        Predict each draft&apos;s #1 pick before recording. Correct prediction = +5 pts. Top-3 finish = +1.
      </div>
    </div>
  );
}
