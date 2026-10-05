// components/features/stats/scope-toggle.tsx
import Link from "next/link";

/**
 * Switches the Record Book between canonical drafts and everything (Patreon and Speed included).
 * Plain links, so it needs no client state. Only rendered for callers the API will honor.
 */
export function ScopeToggle({ includeAll }: { includeAll: boolean }) {
  const base = "min-h-10 px-4 flex items-center font-mono text-[11px] tracking-widest transition-colors";
  const on = "bg-sd-ink text-white";
  const off = "bg-white text-sd-ink hover:bg-sd-paper";

  return (
    <nav aria-label="Record Book scope" className="inline-flex border-2 border-sd-ink">
      <Link href="/stats" aria-current={includeAll ? undefined : "true"} className={`${base} ${includeAll ? off : on}`}>
        CANONICAL
      </Link>
      <Link
        href="/stats?scope=all"
        aria-current={includeAll ? "true" : undefined}
        className={`${base} border-l-2 border-sd-ink ${includeAll ? on : off}`}
      >
        INCLUDE PATREON &amp; SPEED
      </Link>
    </nav>
  );
}
