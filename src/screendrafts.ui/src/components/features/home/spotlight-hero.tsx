import type { MappedSpotlight } from "@/services/home/fetch-home-data";
import Link from "next/link";

export default function SpotlightHero({ spotlight }: { spotlight: MappedSpotlight }) {
  return (
    <div className="grid grid-cols-[1.45fr_1fr]" style={{ height: 540 }}>
      {/* Left — featured draft */}
      <div className="bg-sd-blue text-white px-14 py-12 relative overflow-hidden flex flex-col justify-between">
        <div>
          <div className="flex items-center gap-3.5 text-[11px] tracking-[0.32em] opacity-85">
            <span className="bg-sd-red text-white px-2.5 py-1 tracking-[0.24em] font-bold text-[11px]">
              ★ DRAFT SPOTLIGHT
            </span>
            <span>
              EPISODE {spotlight.episodeNumber} · {(spotlight.draftType ?? '').toUpperCase()}{spotlight.totalParts > 1 ? ` · ${spotlight.totalParts} PARTS` : ''}
            </span>
          </div>

          <div
            className="font-oswald font-bold mt-5 leading-[1] tracking-[-0.01em] line-clamp-2"
            style={{fontSize: 'clamp(48px, 6vw, 92px'}}
            >
              {spotlight.title.toUpperCase()}
          </div>

          <p className="text-base opacity-90 mt-6 max-w-xl leading-[1.55]">
            {spotlight.spotlightDescription}
          </p>

          <div className="flex gap-3 mt-7">
            {spotlight.spotifyUrl && (
              <a href={spotlight.spotifyUrl}
                target="_blank"
                rel="noopener noreferrer"
                className="bg-sd-red text-white px-5 py-3.5 font-oswald font-bold tracking-[0.18em] text-sm">
                ▶ LISTEN TO THE EPISODE
              </a>
            )}
            <Link
              href={`/drafts/${spotlight.draftPublicId}`}
              className="border-2 border-white px-5 py-3 font-oswald font-semibold tracking-[0.18em] text-sm">
              VIEW THE LIST →
            </Link>
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
          THE FINAL LIST · TOP {spotlight.topPicks.length} OF {spotlight.totalPicks} PICKS
        </div>
        <div className="font-oswald font-bold text-2xl tracking-[0.04em] mt-1.5 text-sd-blue">
          HOW IT SHOOK OUT
        </div>

        <div className="mt-6 flex-1 divide-y divide-sd-ink/10">
          {spotlight.topPicks.map((pick) => (
            <div key={pick.position} className="flex items-baseline gap-3.5 py-3.5">
              <span
                className={`font-oswald font-bold text-[38px] leading-none w-11 ${pick.position === 1 ? 'text-sd-red' : 'text-sd-blue'}`}
              >
                #{pick.position}
              </span>
              <span className="flex-1 font-oswald font-semibold text-[22px] tracking-[0.02em]">
                {pick.title}
              </span>
            </div>
          ))}
        </div>

        <Link
          href={`/drafts/${spotlight.draftPublicId}`}
          className="mt-3.5 text-xs tracking-[0.18em] text-sd-blue font-bold">
          SEE ALL {spotlight.totalPicks} PICKS →
        </Link>
      </div>
    </div>
  );
}
