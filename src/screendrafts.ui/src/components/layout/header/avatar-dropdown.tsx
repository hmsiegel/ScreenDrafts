'use client';

import { useEffect, useRef, useState } from "react";
import { usePathname } from "next/navigation";
import Link from "next/link";
import SignOutButton from "./sign-out-button";

interface Props {
  name?: string | null;
  isAdmin?: boolean;
  isDrafter?: boolean;
}

function initials(name?: string | null): string {
  if (!name) return '?';
  return name
    .split(' ')
    .map((n) => n[0] ?? '')
    .join('')
    .slice(0, 2)
    .toUpperCase();
}

export default function AvatarDropdown({ name, isDrafter }: Props) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);
  const pathname = usePathname();

  // Close on navigation
  useEffect(() => {
    setOpen(false);
  }, [pathname]);

  // Close on outside click / Escape
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
        className="cursor-pointer"
      >
        <div className="w-9 h-9 rounded-full bg-sd-blue text-white font-oswald font-bold text-sm flex items-center justify-center select-none">
          {initials(name)}
        </div>
      </button>

      {open && (
        <div className="absolute right-0 top-full mt-2 w-48 bg-white border border-gray-200 rounded shadow-lg z-50 py-1">
          <Link
            href="/profile"
            onClick={() => setOpen(false)}
            className="block px-4 py-2 text-sm text-sd-ink hover:bg-gray-50 transition-colors"
          >
            My Dashboard
          </Link>
          {isDrafter && (
            <Link
              href="/my-drafts"
              onClick={() => setOpen(false)}
              className="block px-4 py-2 text-sm text-sd-ink hover:bg-gray-50 transition-colors"
            >
              My Drafts
            </Link>
          )}
          <Link
            href="/draft-guide"
            onClick={() => setOpen(false)}
            className="block px-4 py-2 text-sm text-sd-ink hover:bg-gray-50 transition-colors"
          >
            Draft Guide
          </Link>
          <div className="border-t border-gray-100 mt-1 pt-1">
            <SignOutButton />
          </div>
        </div>
      )}
    </div>
  );
}