'use client';

import { signOut } from "next-auth/react";

export default function SignOutButton() {
  return (
    <button
      onClick={() => signOut({ callbackUrl: '/' })}
      className="w-full text-left px-4 py-2 text-sm text-sd-ink hover:bg-gray-50 transition-colors"
    >
      Sign Out
    </button>
  );
}
