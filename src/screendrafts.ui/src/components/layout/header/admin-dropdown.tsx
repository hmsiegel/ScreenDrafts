'use client';

import { useEffect, useRef, useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";

const ADMIN_ITEMS = [
  { label: "User Management", href: "/admin" },
  { label: "Draft Management", href: "/admin/drafts" },
  { label: "Spotlight Management", href: "/admin/spotlight"},
  { label: "Campaigns", href: "/admin/campaigns" },
  { label: "Categories", href: "/admin/categories" },
  { label: "Series", href: "/admin/series" },
];

export default function AdminDropdown() {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);
  const pathname = usePathname();

  useEffect(() => {
    function onKey(e: KeyboardEvent) {
      if (e.key === "Escape") setOpen(false);
    }
    function onClickOutside(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    }
    document.addEventListener("keydown", onKey);
    document.addEventListener("mousedown", onClickOutside);
    return () => {
      document.removeEventListener("keydown", onKey);
      document.removeEventListener("mousedown", onClickOutside);
    };
  }, []);

  return (
    <div ref={ref} className="relative">
      <button
        onClick={() => setOpen((v) => !v)}
        aria-haspopup="true"
        aria-expanded={open}
        className="text-sd-red font-oswald font-semibold tracking-widest text-sm hover:text-sd-red/80 transition-colors"
      >
        ADMIN
      </button>

      {open && (
        <div className="absolute right-0 top-full mt-2 w-52 bg-white border border-sd-ink rounded shadow-lg z-50 py-1">
          {ADMIN_ITEMS.map(({ label, href }) => {
            const isActive = pathname === href || (href !== "/admin" && pathname.startsWith(href));
            return (
              <Link
                key={href}
                href={href}
                onClick={() => setOpen(false)}
                className={`block px-4 py-2.5 text-sm text-sd-ink hover:bg-sd-paper transition-colors ${
                  isActive ? "border-l-4 border-sd-red pl-3" : ""
                }`}
              >
                {label}
              </Link>
            );
          })}
        </div>
      )}
    </div>
  );
}
