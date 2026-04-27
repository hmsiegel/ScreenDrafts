interface SpotlightData {
  episodeNumber: number;
  type: string;
  parts: number;
  subject: string;
  description: string;
  topFive: { position: number; title: string; year: number }[];
  totalPicks: number;
}

export default function SpotlightHero({ spotlight }: { spotlight: SpotlightData }) {
  return (
    <div className="grid grid-cols-[1.45fr_1fr]" style={{ height: 540 }}>
      {/* Left — featured draft */}
      <div className="bg-sd-blue text-white px-14 py-12 relative overflow-hidden flex flex-col justify-between">
        <div>
          <div className="flex items-center gap-3.5 text-[11px] tracking-[0.32em] opacity-85">
            <span className="bg-sd-red text-white px-2.5 py-1 tracking-[0.24em] font-bold text-[11px]">
              ★ DRAFT SPOTLIGHT
            </span>
            <span>EPISODE {spotlight.episodeNumber} · {spotlight.type.toUpperCase()} · {spotlight.parts} PARTS</span>
          </div>

          <div className="font-oswald font-bold leading-[0.9] tracking-[-0.01em] mt-5" style={{ fontSize: 92 }}>
            {spotlight.subject.toUpperCase().split(' ').map((word, i) => (
              <span key={i}>{word}<br /></span>
            ))}
          </div>

          <p className="text-base opacity-90 mt-6 max-w-xl leading-[1.55]">
            {spotlight.description}
          </p>

          <div className="flex gap-3 mt-7">
            <button className="bg-sd-red text-white px-5 py-3.5 font-oswald font-bold tracking-[0.18em] text-sm">
              ▶ LISTEN TO THE EPISODE
            </button>
            <button className="border-2 border-white px-5 py-3 font-oswald font-semibold tracking-[0.18em] text-sm">
              VIEW THE LIST →
            </button>
          </div>
        </div>

        <div className="text-[10px] tracking-[0.26em] opacity-55">
          SPOTLIGHT ROTATES WEEKLY · CHOSEN BY THE COMMISSIONERS
        </div>

        {/* Decorative circle */}
        <div className="absolute bottom-0 right-0 translate-x-1/4 translate-y-1/4 w-72 h-72 rounded-full bg-sd-red opacity-20 pointer-events-none" />
      </div>

      {/* Right — top 5 */}
      <div className="bg-sd-paper text-sd-ink px-12 py-12 flex flex-col">
        <div className="text-[11px] tracking-[0.28em] text-sd-red font-bold">
          THE FINAL LIST · TOP 5 OF {spotlight.totalPicks}
        </div>
        <div className="font-oswald font-bold text-2xl tracking-[0.04em] mt-1.5 text-sd-blue">
          HOW IT SHOOK OUT
        </div>

        <div className="mt-6 flex-1 divide-y divide-sd-ink/10">
          {spotlight.topFive.map((pick) => (
            <div key={pick.position} className="flex items-baseline gap-3.5 py-3.5">
              <span
                className={`font-oswald font-bold text-[38px] leading-none w-11 ${pick.position === 1 ? 'text-sd-red' : 'text-sd-blue'}`}
              >
                #{pick.position}
              </span>
              <span className="flex-1 font-oswald font-semibold text-[22px] tracking-[0.02em]">
                {pick.title}
              </span>
              <span className="font-mono text-[13px] text-gray-500">{pick.year}</span>
            </div>
          ))}
        </div>

        <div className="mt-3.5 text-xs tracking-[0.18em] text-sd-blue font-bold">
          SEE ALL {spotlight.totalPicks} PICKS →
        </div>
      </div>
    </div>
  );
}
