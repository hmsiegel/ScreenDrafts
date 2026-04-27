import Link from "next/link";
import SignOutButton from "./sign-out-button";

interface Props {
  name?: string | null;
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

export default function AvatarDropdown({ name }: Props) {
  return (
    <details className="relative">
      <summary className="list-none cursor-pointer">
        <div className="w-9 h-9 rounded-full bg-sd-blue text-white font-oswald font-bold text-sm flex items-center justify-center select-none">
          {initials(name)}
        </div>
      </summary>

      <div className="absolute right-0 top-full mt-2 w-48 bg-white border border-gray-200 rounded shadow-lg z-50 py-1">
        <Link
          href="/dashboard"
          className="block px-4 py-2 text-sm text-sd-ink hover:bg-gray-50 transition-colors"
        >
          My Dashboard
        </Link>
        <Link
          href="/dashboard/drafts"
          className="block px-4 py-2 text-sm text-sd-ink hover:bg-gray-50 transition-colors"
        >
          My Draft Boards
        </Link>
        <div className="border-t border-gray-100 mt-1 pt-1">
          <SignOutButton />
        </div>
      </div>
    </details>
  );
}
