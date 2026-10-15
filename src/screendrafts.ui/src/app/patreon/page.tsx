// app/patreon/page.tsx
import type { Metadata } from 'next';
import Link from 'next/link';

export const metadata: Metadata = { title: 'Link Patreon' };

// Clapperboard sticks: diagonal black/white bars, the fixed one in opposite phase.
const STRIPES_ARM =
  'repeating-linear-gradient(115deg, #ffffff 0 26px, #0d1430 26px 52px)';
const STRIPES_BAR =
  'repeating-linear-gradient(115deg, #0d1430 0 26px, #ffffff 26px 52px)';

function Field({ label, value, className = '' }: { label: string; value: string; className?: string }) {
  return (
    <div className={`border border-white/25 px-3 py-2 min-w-0 ${className}`}>
      <div className="text-[9px] tracking-[0.28em] text-white/50">{label}</div>
      <div className="font-oswald font-bold text-lg sm:text-xl tracking-[0.06em] truncate">{value}</div>
    </div>
  );
}

export default function PatreonPage() {
  return (
    <div className="min-h-screen bg-light-blue">
      {/*
        The hinged stick starts raised, snaps shut once, and stays closed. With reduced motion
        there is no animation, so it simply renders closed.
      */}
      <style>{`
        @keyframes sd-clap {
          0%, 55% { transform: rotate(-9deg); }
          70%, 100% { transform: rotate(0deg); }
        }
        @media (prefers-reduced-motion: no-preference) {
          .sd-clapper-arm { animation: sd-clap 1.4s ease-out 0.3s both; }
        }
      `}</style>

      <main className="max-w-xl mx-auto page-x pt-12 pb-16 sm:pt-20 sm:pb-24">
        {/* Clapperboard */}
        <div>
          <div
            aria-hidden
            className="sd-clapper-arm h-9 border-2 border-sd-ink rounded-t-xs"
            style={{ backgroundImage: STRIPES_ARM, transformOrigin: 'left bottom' }}
          />
          <div
            aria-hidden
            className="h-9 border-x-2 border-b-2 border-sd-ink"
            style={{ backgroundImage: STRIPES_BAR }}
          />

          <div className="bg-sd-ink text-white border-2 border-t-0 border-sd-ink p-3 sm:p-5 rounded-b-xs">
            <div aria-hidden className="grid grid-cols-3 gap-2">
              <Field label="PRODUCTION" value="SCREEN DRAFTS" className="col-span-2" />
              <Field label="TAKE" value="1" />
              <Field label="SCENE" value="PATREON" className="col-span-3" />
            </div>

            <div className="border border-white/25 mt-2 px-3 py-8 sm:py-12 text-center">
              <p className="text-[10px] tracking-[0.28em] text-white/70">
                <span className="text-sd-red">★</span> PATRON TIER
              </p>
              <h1 className="font-oswald font-bold text-4xl sm:text-6xl tracking-[0.06em] leading-none mt-3">
                COMING SOON
              </h1>
            </div>
          </div>
        </div>

        {/* Copy */}
        <p className="mt-8 text-sd-ink/70 text-[15px] leading-relaxed">
          Linking your Patreon account isn&apos;t ready yet. Once it is, you&apos;ll link it here to unlock
          Patreon-only drafts and the watch-along Zoom calls.
        </p>
        <Link
          href="/"
          className="mt-6 inline-block bg-sd-ink text-white px-5 py-3 font-oswald font-bold tracking-[0.16em] text-sm rounded-sm hover:bg-sd-ink/90 transition-colors"
        >
          ← BACK TO HOME
        </Link>
      </main>
    </div>
  );
}
