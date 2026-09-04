// src/components/layout/header/site-header.tsx
import Image from "next/image";
import Link from "next/link";
import { auth } from "@/auth";
import SignInButton from "./sign-in-button";
import AvatarDropdown from "./avatar-dropdown";
import AdminDropdown from "./admin-dropdown";

// Must match administration.roles.name exactly.
// Run: SELECT name FROM administration.roles WHERE name ILIKE '%admin%';
const ADMIN_ROLES = ["Administrator", "SuperAdministrator"];

// Every account gets the Guest role (per administration.roles) — gating on
// it rather than just "session exists" keeps this consistent with how
// ADMIN_ROLES/isDrafter already check real role names below, and is what
// actually keeps someone with no account out.
const GUEST_ROLE = "Guest";

type NavItem = { label: string; href: string; external?: boolean; className?: string };

const NAV_ITEMS: NavItem[] = [
  { label: "DRAFTS", href: "/drafts" },
  { label: "DRAFTERS", href: "/drafters" },
  { label: "FILMS", href: "/media" },
  { label: "PREDICTIONS", href: "/predictions" },
];

export default async function SiteHeader({ activePath }: { activePath?: string } = {}) {
  const session = await auth();
  const isAdmin = session?.roles?.some(r => ADMIN_ROLES.includes(r)) ?? false;
  const isDrafter = session?.roles?.includes("Drafter") ?? false;
  const isGuest = session?.roles?.includes(GUEST_ROLE) ?? false;
  const isGuestDraftsActive = activePath?.startsWith("/guest-drafts") ?? false;

  return (
    <header className="bg-white border-b-4 border-sd-red px-8 py-5 flex items-center justify-between">
      <Link href="/" className="flex items-center gap-3.5">
        <Image
          src="/screen-drafts.jpg"
          alt="Screen Drafts logo"
          width={56}
          height={56}
          className="rounded-[10px]"
        />
        <div>
          <div className="font-oswald font-bold text-[32px] leading-none tracking-[0.02em] text-sd-ink">
            SCREEN DRAFTS
          </div>
          <div className="text-[11px] tracking-[0.22em] text-sd-blue mt-1">
            THE COMPETITIVELY-COLLABORATIVE BEST-OF-LIST PODCAST
          </div>
        </div>
      </Link>

      <nav className="flex items-center gap-5 font-oswald font-medium text-sm tracking-[0.1em]">
        {NAV_ITEMS.map(({ label, href }) => {
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
            href="/guest-drafts"
            className={`pb-0.5 transition-colors hover:text-sd-red ${isGuestDraftsActive
              ? "border-b-[3px] border-sd-red text-sd-ink"
              : "text-sd-ink"
              }`}
          >
            GUEST DRAFTS
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
    </header>
  );
}