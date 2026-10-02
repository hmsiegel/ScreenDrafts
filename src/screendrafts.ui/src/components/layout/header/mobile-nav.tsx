// components/layout/header/mobile-nav.tsx
'use client';

import { useEffect, useId, useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { signIn, signOut } from "next-auth/react";
import {
   ADMIN_NAV_ITEMS,
   GUEST_DRAFTS_NAV_ITEM,
   isNavItemActive,
   type NavItem,
   PRIMARY_NAV_ITEMS
} from "../nav-items";

interface MobileNavProps {
   isSignedIn: boolean;
   isAdmin: boolean;
   isDrafter: boolean;
   isGuest: boolean;
}

// Matches Tailwind's `lg`. Below it the desktop nav is hidden and this drawer
// is the only way around the site.
const DESKTOP_QUERY = "(min-width: 1024px)";

export default function MobileNav({ isSignedIn, isAdmin, isDrafter, isGuest }: MobileNavProps) {
   const [open, setOpen] = useState(false);
   const pathname = usePathname();
   const panelId = useId();
   const close = () => setOpen(false);

   // Close on navigation.
   useEffect(() => {
      setOpen(false);
   }, [pathname]);

   // While open: Escape closes, and the page behind stops scrolling.
   useEffect(() => {
      if (!open) return;
      const onKey = (e: KeyboardEvent) => {
         if (e.key === "Escape") setOpen(false);
      };
      const previousOverflow = document.body.style.overflow;
      document.body.style.overflow = "hidden";
      document.addEventListener("keydown", onKey);
      return () => {
         document.body.style.overflow = previousOverflow;
         document.removeEventListener("keydown", onKey);
      };
   }, [open]);

   // Rotating a tablet past `lg` hides the hamburger. Without this the drawer
   // would stay open — and the scroll lock stay on — with no control to close it.
   useEffect(() => {
      if (!open) return;
      const mq = window.matchMedia(DESKTOP_QUERY);
      const onChange = (e: MediaQueryListEvent) => {
         if (e.matches) setOpen(false);
      };
      mq.addEventListener("change", onChange);
      return () => mq.removeEventListener("change", onChange);
   }, [open]);

   return (
      <div className="lg:hidden">
         <button
            type="button"
            onClick={() => setOpen(v => !v)}
            aria-expanded={open}
            aria-controls={panelId}
            aria-label={open ? "Close menu" : "Open menu"}
            className="relative z-50 w-11 h-11 -mr-2 flex items-center justify-center text-sd-ink rounded focus-visible:outline focus-visible:outline-2 focus-visible:outline-sd-blue"
         >
            <svg width="24" height="24" viewBox="0 0 24 24" aria-hidden="true" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="square">
               {open ? (
                  <path d="M5 5l14 14M19 5L5 19" />
               ) : (
                  <path d="M3 6h18M3 12h18M3 18h18" />
               )}
            </svg>
         </button>

         {open && (
            <>
               {/* Backdrop — tap anywhere outside the panel to close. */}
               <div
                  className="fixed inset-0 z-40 bg-sd-ink/40"
                  onClick={close}
                  aria-hidden="true"
               />

               {/* top offset clears the header's 4px red border so the rule stays visible. */}
               <div
                  id={panelId}
                  className="absolute inset-x-0 top-[calc(100%+4px)] z-50 bg-white border-b-4 border-sd-ink shadow-lg max-h-[calc(100dvh-5rem)] overflow-y-auto overscroll-contain"
               >
                  <nav aria-label="Main" className="py-2">
                     {PRIMARY_NAV_ITEMS.map(item => (
                        <PrimaryLink key={item.href} item={item} pathname={pathname} onNavigate={close} />
                     ))}
                     {isGuest && (
                        <PrimaryLink item={GUEST_DRAFTS_NAV_ITEM} pathname={pathname} onNavigate={close} />
                     )}
                  </nav>

                  {isSignedIn && (
                     <Section title="Account">
                        <SecondaryLink item={{ label: "My Dashboard", href: "/profile" }} pathname={pathname} onNavigate={close} />
                        {isDrafter && (
                           <SecondaryLink item={{ label: "My Drafts", href: "/my-drafts" }} pathname={pathname} onNavigate={close} />
                        )}
                        <SecondaryLink item={{ label: "Draft Guide", href: "/draft-guide" }} pathname={pathname} onNavigate={close} />
                     </Section>
                  )}

                  {isAdmin && (
                     <Section title="Admin" accent>
                        {ADMIN_NAV_ITEMS.map(item => (
                           <SecondaryLink key={item.href} item={item} pathname={pathname} onNavigate={close} />
                        ))}
                     </Section>
                  )}

                  <div className="border-t border-sd-ink/10 p-4">
                     {isSignedIn ? (
                        <button
                           type="button"
                           onClick={() => signOut({ callbackUrl: "/" })}
                           className="w-full min-h-12 border-2 border-sd-ink text-sd-ink font-oswald font-medium text-sm tracking-[0.14em] rounded hover:bg-sd-paper transition-colors"
                        >
                           SIGN OUT
                        </button>
                     ) : (
                        <div className="grid grid-cols-2 gap-3">
                           <button
                              type="button"
                              onClick={() => signIn("keycloak", { callbackUrl: "/" })}
                              className="min-h-12 bg-sd-blue text-white font-oswald font-medium text-sm tracking-[0.14em] rounded hover:bg-blue-700 transition-colors"
                           >
                              SIGN IN
                           </button>
                           <Link
                              href="/register"
                              onClick={close}
                              className="min-h-12 flex items-center justify-center bg-sd-red text-white font-oswald font-medium text-sm tracking-[0.14em] rounded hover:bg-red-700 transition-colors"
                           >
                              REGISTER
                           </Link>
                        </div>
                     )}
                  </div>
               </div>
            </>
         )}
      </div>
   );
}

interface LinkProps {
   item: NavItem;
   pathname: string | null;
   onNavigate: () => void;
}

// Oswald caps, matching the desktop nav. 48px rows clear the 44px touch minimum.
function PrimaryLink({ item, pathname, onNavigate }: LinkProps) {
   const active = isNavItemActive(pathname, item);
   return (
      <Link
         href={item.href}
         onClick={onNavigate}
         aria-current={active ? "page" : undefined}
         className={`flex items-center min-h-12 px-5 border-l-4 font-oswald font-medium text-base tracking-[0.1em] text-sd-ink transition-colors hover:bg-sd-paper ${active ? "border-sd-red bg-sd-paper" : "border-transparent"
            }`}
      >
         {item.label}
      </Link>
   );
}

// Sentence case, matching the existing avatar/admin dropdown items.
function SecondaryLink({ item, pathname, onNavigate }: LinkProps) {
   const active = isNavItemActive(pathname, item);
   return (
      <Link
         href={item.href}
         onClick={onNavigate}
         aria-current={active ? "page" : undefined}
         className={`flex items-center min-h-11 px-5 border-l-4 text-[15px] text-sd-ink transition-colors hover:bg-sd-paper ${active ? "border-sd-red bg-sd-paper" : "border-transparent"
            }`}
      >
         {item.label}
      </Link>
   );
}

function Section({ title, accent = false, children }: { title: string; accent?: boolean; children: React.ReactNode }) {
   return (
      <div className="border-t border-sd-ink/10 py-2">
         <div className={`px-5 pt-2 pb-1 font-oswald font-semibold text-xs tracking-widest ${accent ? "text-sd-red" : "text-sd-ink/50"}`}>
            {title.toUpperCase()}
         </div>
         {children}
      </div>
   );
}