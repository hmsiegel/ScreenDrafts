import Link from "next/link";
import MediaTypeTabs from "./media-type-tabs";

export interface MediaFilterStripProps {
  mediaType: string;
  sort: string;
  q?: string;
  year?: string;
}

const SORT_OPTIONS = [
  { value: "title_asc",  label: "Title A–Z" },
  { value: "title_desc", label: "Title Z–A" },
  { value: "year_desc",  label: "Newest" },
  { value: "year_asc",   label: "Oldest" },
];

export default function MediaFilterStrip({ mediaType, sort, q, year }: MediaFilterStripProps) {
  function sortHref(value: string) {
    const qs = new URLSearchParams({ sort: value, page: "1" });
    if (mediaType) qs.set("mediaType", mediaType);
    if (q) qs.set("q", q);
    if (year) qs.set("year", year);
    return `?${qs.toString()}`;
  }

  return (
    <div className="bg-white border-b border-sd-ink/20 px-10 py-4 flex items-center justify-between gap-6">
      {/* Media type tabs — client component */}
      <MediaTypeTabs mediaType={mediaType} sort={sort} q={q} year={year} />

      <div className="flex items-center gap-4">
        {/* Search */}
        <form method="get" action="">
          <input type="hidden" name="sort" value={sort} />
          <input type="hidden" name="page" value="1" />
          {mediaType && <input type="hidden" name="mediaType" value={mediaType} />}
          {year && <input type="hidden" name="year" value={year} />}
          <input
            name="q"
            defaultValue={q ?? ""}
            placeholder="Search…"
            className="border border-sd-ink/30 px-3 py-1.5 font-mono text-[11px] text-sd-ink placeholder:text-sd-ink/40 focus:outline-none focus:border-sd-ink w-40"
          />
        </form>

        {/* Year filter */}
        <form method="get" action="">
          <input type="hidden" name="sort" value={sort} />
          <input type="hidden" name="page" value="1" />
          {mediaType && <input type="hidden" name="mediaType" value={mediaType} />}
          {q && <input type="hidden" name="q" value={q} />}
          <input
            name="year"
            defaultValue={year ?? ""}
            placeholder="Year"
            maxLength={4}
            className="border border-sd-ink/30 px-3 py-1.5 font-mono text-[11px] text-sd-ink placeholder:text-sd-ink/40 focus:outline-none focus:border-sd-ink w-20"
          />
        </form>

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
                className={`px-3 py-1.5 font-mono text-[11px] transition-colors border ${
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