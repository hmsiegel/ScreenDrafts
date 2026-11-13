// components/features/cast-crew/cast-crew-filter-strip.tsx
import Link from "next/link";

const ROLE_TABS = [
  { value: "", label: "ALL" },
  { value: "actor", label: "ACTORS" },
  { value: "director", label: "DIRECTORS" },
  { value: "writer", label: "WRITERS" },
  { value: "producer", label: "PRODUCERS" },
];

const SORT_OPTIONS = [
  { value: "name_asc", label: "Last name A–Z" },
  { value: "name_desc", label: "Last name Z–A" },
  { value: "titles_desc", label: "Most titles" },
  { value: "titles_asc", label: "Fewest titles" },
];

export interface CastCrewFilterStripProps {
  role: string;
  sort: string;
  q?: string;
}

export default function CastCrewFilterStrip({ role, sort, q }: CastCrewFilterStripProps) {
  // Every link rebuilds the query string, so each carries the params it does not change.
  function href(next: { role?: string; sort?: string }) {
    const nextRole = next.role ?? role;
    const nextSort = next.sort ?? sort;
    const qs = new URLSearchParams({ sort: nextSort, page: "1" });
    if (nextRole) qs.set("role", nextRole);
    if (q) qs.set("q", q);
    return `?${qs.toString()}`;
  }

  return (
    <div className="bg-white border-b border-sd-ink/20 page-x py-4 flex flex-col gap-3 xl:flex-row xl:items-center xl:justify-between xl:gap-6">
      {/* Role tabs */}
      <div className="flex gap-1 overflow-x-auto overscroll-x-contain scrollbar-none -mx-4 px-4 sm:mx-0 sm:px-0">
        {ROLE_TABS.map((tab) => (
          <Link
            key={tab.value}
            href={href({ role: tab.value })}
            aria-current={role === tab.value ? "page" : undefined}
            className={`shrink-0 whitespace-nowrap px-4 py-2.5 xl:py-2 font-oswald text-[12px] tracking-wide transition-colors ${
              role === tab.value ? "bg-sd-ink text-white" : "text-sd-ink hover:bg-sd-ink/5"
            }`}
          >
            {tab.label}
          </Link>
        ))}
      </div>

      <div className="flex flex-wrap items-center gap-x-4 gap-y-3">
        {/* Search */}
        <form method="get" action="" className="flex-1 min-w-0 sm:flex-none">
          <input type="hidden" name="sort" value={sort} />
          <input type="hidden" name="page" value="1" />
          {role && <input type="hidden" name="role" value={role} />}
          <input
            name="q"
            defaultValue={q ?? ""}
            placeholder="Search by name…"
            aria-label="Search cast and crew by name"
            className="border border-sd-ink/30 px-3 py-1.5 font-mono text-[11px] text-sd-ink placeholder:text-sd-ink/40 focus:outline-hidden focus:border-sd-ink w-full sm:w-56"
          />
        </form>

        {/* Sort — hidden on phones, where the list's pinned sort strip replaces it */}
        <div className="hidden sm:flex items-center gap-2">
          <span className="font-mono text-[10px] tracking-widest text-sd-blue shrink-0">SORT BY</span>
          <div className="flex flex-wrap gap-1">
            {SORT_OPTIONS.map((opt) => (
              <Link
                key={opt.value}
                href={href({ sort: opt.value })}
                className={`whitespace-nowrap px-3 py-2 xl:py-1.5 font-mono text-[11px] transition-colors border ${
                  sort === opt.value
                    ? "bg-sd-ink text-white border-sd-ink"
                    : "border-sd-ink/30 text-sd-ink hover:border-sd-ink"
                }`}
              >
                {opt.label}
              </Link>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
