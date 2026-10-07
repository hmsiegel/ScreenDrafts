// components/features/stats/title-filter-bar.tsx
import { TITLE_SORTS } from "@/services/stats/title-levels";
import type { TitleSort } from "@/services/stats/stats-types";

const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";
const LABEL = "block font-mono text-[10px] tracking-widest text-[#5a6075] mb-1";

/** A plain GET form, so it works without client code and every result page is a shareable link. */
export function TitleFilterBar({
  level,
  query,
  sort,
  includeAll,
}: {
  level: string;
  query: string;
  sort: TitleSort;
  includeAll: boolean;
}) {
  return (
    <form
      method="get"
      action={`/stats/titles/${level}`}
      className="flex flex-col gap-3 sm:flex-row sm:items-end"
    >
      {includeAll && <input type="hidden" name="scope" value="all" />}

      <div className="flex-1">
        <label htmlFor="title-search" className={LABEL}>SEARCH TITLES</label>
        <input
          id="title-search"
          name="q"
          type="search"
          maxLength={100}
          defaultValue={query}
          className={INPUT}
        />
      </div>

      <div className="sm:w-52">
        <label htmlFor="title-sort" className={LABEL}>SORT</label>
        <select id="title-sort" name="sort" defaultValue={sort} className={INPUT}>
          {TITLE_SORTS.map((s) => (
            <option key={s.value} value={s.value}>{s.label}</option>
          ))}
        </select>
      </div>

      <button
        type="submit"
        className="min-h-11 px-6 bg-sd-blue text-white font-oswald font-medium text-sm tracking-[0.14em] rounded hover:bg-blue-700 transition-colors"
      >
        APPLY
      </button>
    </form>
  );
}
