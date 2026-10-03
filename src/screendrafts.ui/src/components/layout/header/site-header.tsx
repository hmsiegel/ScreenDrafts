// components/layout/header/site-header.tsx
import Image from "next/image";
import Link from "next/link";
import { auth } from "@/auth";
import SignInButton from "./sign-in-button";
import AvatarDropdown from "./avatar-dropdown";
import AdminDropdown from "./admin-dropdown";
import MobileNav from "./mobile-nav";
import { GUEST_DRAFTS_NAV_ITEM, PRIMARY_NAV_ITEMS } from "../nav-items";

// Must match administration.roles.name exactly.
// Run: SELECT name FROM administration.roles WHERE name ILIKE '%admin%';
const ADMIN_ROLES = ["Administrator", "SuperAdministrator"];

// Every account gets the Guest role (per administration.roles) — gating on
// it rather than just "session exists" keeps this consistent with how
// ADMIN_ROLES/isDrafter already check real role names below, and is what
// actually keeps someone with no account out.
const GUEST_ROLE = "Guest";

// Breakpoints:
//   < lg (1024): wordmark + hamburger; MobileNav owns every link.
//   lg–xl:       full desktop nav, tagline hidden (it alone is ~450px wide and
//                pushes the nav past 1024).
//   >= xl:       tagline returns.
export default async function SiteHeader({ activePath }: { activePath?: string } = {}) {
  const session = await auth();
  const isAdmin = session?.roles?.some(r => ADMIN_ROLES.includes(r)) ?? false;
  const isDrafter = session?.roles?.includes("Drafter") ?? false;
  const isGuest = session?.roles?.includes(GUEST_ROLE) ?? false;
  const isGuestDraftsActive = activePath?.startsWith(GUEST_DRAFTS_NAV_ITEM.href) ?? false;

  return (
    <header className="relative bg-white border-b-4 border-sd-red px-4 py-3 sm:px-6 lg:px-8 lg:py-5 flex items-center justify-between gap-4">
      <Link href="/" className="flex items-center gap-2.5 lg:gap-3.5 min-w-0">
        <Image
          src="/screen-drafts.jpg"
          alt="Screen Drafts logo"
          width={56}
          height={56}
          className="rounded-[10px] w-10 h-10 lg:w-14 lg:h-14 shrink-0"
        />
        <div className="min-w-0">
          <div className="font-oswald font-bold text-[22px] sm:text-[26px] lg:text-[32px] leading-none tracking-[0.02em] text-sd-ink whitespace-nowrap">
            SCREEN DRAFTS
          </div>
          <div className="hidden xl:block text-[11px] tracking-[0.22em] text-sd-blue mt-1">
            THE COMPETITIVELY-COLLABORATIVE BEST-OF-LIST PODCAST
          </div>
        </div>
      </Link>

      <nav
        aria-label="Main"
        className="hidden lg:flex items-center gap-5 font-oswald font-medium text-sm tracking-[0.1em]"
      >
        {PRIMARY_NAV_ITEMS.map(({ label, href }) => {
          const isActive = activePath?.startsWith(href);
          return (
            <Link
              key={href}
              href={href}
              className={`pb-0.5 transition-colors hover:text-sd-red ${isActive
                ? "border-b-[3px] border-sd-red text-sd-ink"
                : "text-sd-ink"
                }`}
            >
              {label}
            </Link>
          );
        })}
        {isGuest && (
          <Link
            href={GUEST_DRAFTS_NAV_ITEM.href}
            className={`pb-0.5 transition-colors hover:text-sd-red ${isGuestDraftsActive
              ? "border-b-[3px] border-sd-red text-sd-ink"
              : "text-sd-ink"
              }`}
          >
            {GUEST_DRAFTS_NAV_ITEM.label}
          </Link>
        )}
        {isAdmin && <AdminDropdown />}

        {session ? (
          <AvatarDropdown name={session.user?.name} isAdmin={isAdmin} isDrafter={isDrafter} />
        ) : (
          <>
            <SignInButton />
            <Link
              href="/register"
              className="bg-sd-red text-white px-[18px] py-2.5 rounded font-oswald font-medium text-sm tracking-[0.14em] hover:bg-red-700 transition-colors"
            >
              REGISTER
            </Link>
          </>
        )}
      </nav>

      <MobileNav
        isSignedIn={!!session}
        isAdmin={isAdmin}
        isDrafter={isDrafter}
        isGuest={isGuest}
      />
    </header>
  );
}