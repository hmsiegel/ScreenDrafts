// components/features/about/about-tabs.tsx
"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { isNavItemActive, type NavItem } from "@/components/layout/nav-items";

const TABS: NavItem[] = [
  { label: "THE RULES", href: "/about", exact: true },
  { label: "ACKNOWLEDGEMENTS", href: "/about/acknowledgements" },
];

export function AboutTabs() {
  const pathname = usePathname();

  return (
    <nav aria-label="About" className="flex gap-6 border-b-2 border-sd-ink/15">
      {TABS.map((tab) => {
        const active = isNavItemActive(pathname, tab);
        return (
          <Link
            key={tab.href}
            href={tab.href}
            aria-current={active ? "page" : undefined}
            className={`min-h-11 flex items-center -mb-0.5 border-b-[3px] font-oswald font-medium text-sm tracking-widest transition-colors hover:text-sd-red ${
              active ? "border-sd-red text-sd-ink" : "border-transparent text-sd-ink/60"
            }`}
          >
            {tab.label}
          </Link>
        );
      })}
    </nav>
  );
}
