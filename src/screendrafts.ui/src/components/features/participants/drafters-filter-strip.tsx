// components/features/participants/drafters-filter-strip.tsx
import Link from "next/link";
import HonorificDropdown from "./honorific-dropdown";

interface DraftersFilterStripProps {
  filter: string;
  sort: string;
  q?: string;
  honorific: string;
}

const TABS = [
  { value: "all",           label: "ALL" },
  { value: "commissioners", label: "COMMISSIONERS" },
  { value: "gms",           label: "GUEST G.M.S" },
  { value: "hosts",         label: "HOSTS" },
];

const SORT_OPTIONS = [
  { value: "name",   label: "Name A–Z" },
  { value: "drafts", label: "Most Drafts" },
];

export default function DraftersFilterStrip({
  filter,
  sort,
  q,
  honorific,
}: DraftersFilterStripProps) {
  // Role tabs are visually inactive when an honorific filter is active
  const activeFilter = honorific ? "" : filter;

  function tabHref(value: string) {
    // Selecting a role tab always clears the honorific filter
    const qs = new URLSearchParams({ filter: value, sort, page: "1" });
    if (q) qs.set("q", q);
    return `?${qs.toString()}`;
  }

  function sortHref(value: string) {
    const qs = new URLSearchParams({ sort: value, page: "1" });
    if (!honorific) qs.set("filter", filter);
    if (q) qs.set("q", q);
    if (honorific) qs.set("honorific", honorific);
    return `?${qs.toString()}`;
  }

  // Below xl: tabs on their own row (scrolling sideways on narrow phones), controls wrap beneath.
  // xl+: the original single row — below 1280 the four tabs plus three controls don't fit.
  return (
    <div className="bg-white border-b border-sd-ink/20 page-x py-4 flex flex-col gap-3 xl:flex-row xl:items-center xl:justify-between xl:gap-6">
      {/* Role tabs */}
      <div className="flex gap-1 overflow-x-auto overscroll-x-contain [scrollbar-width:none] -mx-4 px-4 sm:mx-0 sm:px-0">
        {TABS.map((tab) => (
          <Link
            key={tab.value}
            href={tabHref(tab.value)}
            className={`shrink-0 whitespace-nowrap px-4 py-2.5 xl:py-2 font-oswald text-[12px] tracking-wide transition-colors ${
              activeFilter === tab.value
                ? "bg-sd-ink text-white"
                : "text-sd-ink hover:bg-sd-ink/5"
            }`}
          >
            {tab.label}
          </Link>
        ))}
      </div>

      <div className="flex flex-wrap items-center gap-x-4 gap-y-3">
        {/* Search — full width on phones */}
        <form method="get" action="" className="w-full sm:w-auto">
          <input type="hidden" name="filter" value={filter} />
          <input type="hidden" name="sort" value={sort} />
          <input type="hidden" name="page" value="1" />
          {honorific && <input type="hidden" name="honorific" value={honorific} />}
          <input
            name="q"
            defaultValue={q ?? ""}
            placeholder="Search…"
            className="border border-sd-ink/30 px-3 py-1.5 font-mono text-[11px] text-sd-ink placeholder:text-sd-ink/40 focus:outline-none focus:border-sd-ink w-full sm:w-40"
          />
        </form>

        {/* Honorific dropdown — client component */}
        <HonorificDropdown honorific={honorific} sort={sort} q={q} />

        {/* Sort */}
        <div className="flex items-center gap-2">
          <span className="font-mono text-[10px] tracking-widest text-sd-blue shrink-0">
            SORT BY
          </span>
          <div className="flex gap-1">
            {SORT_OPTIONS.map((opt) => (
              <Link
                key={opt.value}
                href={sortHref(opt.value)}
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