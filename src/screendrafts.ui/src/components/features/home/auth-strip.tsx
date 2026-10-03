// components/features/home/auth-strip.tsx
import Link from "next/link";
import SignInButton from "@/components/layout/header/sign-in-button";

// Stacked below md, side by side from md up.
export default function AuthStrip() {
  return (
    <div className="page-x pb-10 grid grid-cols-1 md:grid-cols-2 gap-6">
      {/* Join the league */}
      <div className="bg-sd-ink text-white p-6 sm:p-9 rounded relative overflow-hidden min-w-0">
        <div className="font-oswald font-bold text-[24px] sm:text-[30px] leading-[1.1] tracking-[0.02em]">
          JOIN THE LEAGUE.<br />
          <span className="text-light-blue">PLAY THE GAME.</span>
        </div>
        <p className="text-sm opacity-70 max-w-md mt-3 leading-relaxed">
          Create and host your own drafts. Have your friends join in.
        </p>
        <div className="flex flex-wrap gap-2.5 mt-6">
          <SignInButton />
          <Link
            href="/register"
            className="border-2 border-white text-white px-5 py-2.5 font-oswald font-bold tracking-[0.16em] text-sm rounded hover:bg-white/10 transition-colors"
          >
            REGISTER FREE
          </Link>
        </div>
      </div>

      {/* Patron tier */}
      <div className="bg-sd-red text-white p-6 sm:p-9 rounded relative overflow-hidden min-w-0">
        <div className="relative text-[11px] tracking-[0.28em] opacity-85">★ PATRON TIER</div>
        <div className="relative font-oswald font-bold text-[24px] sm:text-[30px] leading-[1.1] tracking-[0.02em] mt-1">
          UNLOCK BONUS DRAFTS.<br />JOIN THE ZOOM ROOM.
        </div>
        <p className="relative text-sm opacity-85 max-w-md mt-3 leading-relaxed">
          Patreon-only drafts, watch-along Zoom calls with the hosts, access to all the wonders of Draftland.
        </p>
        <div className="relative mt-6">
          <Link
            href="/patreon"
            className="bg-white text-sd-red px-5 py-3 font-oswald font-bold tracking-[0.16em] text-sm rounded hover:bg-gray-100 transition-colors inline-block"
          >
            LINK PATREON →
          </Link>
        </div>
        {/* Ghost star */}
        <span className="font-oswald font-bold absolute top-0 right-4 text-[150px] sm:text-[220px] opacity-[0.08] leading-none select-none pointer-events-none text-white">
          ★
        </span>
      </div>
    </div>
  );
}