// components/features/stats/format-stat-value.ts
import type { HolderKind, StatFormat } from "@/services/stats/stats-types";

const WHOLE = new Intl.NumberFormat("en-US");

export function formatStatValue(value: number, format: StatFormat): string {
  switch (format) {
    case "percent":
      return `${Number.isInteger(value) ? value : value.toFixed(1)}%`;
    case "ratio":
      return value.toFixed(2);
    default:
      return WHOLE.format(value);
  }
}

/** Site route for a record holder or a result row. Null when there is nothing to link to. */
export function statHref(kind: HolderKind, publicId: string | null): string | null {
  if (!publicId) return null;

  switch (kind) {
    case "drafter":
      return `/drafters/${publicId}`;
    case "draft":
      return `/drafts/${publicId}`;
    case "title":
      return `/media/${publicId}`;
  }
}

const MONTH_DAY_YEAR = new Intl.DateTimeFormat("en-US", {
  month: "short",
  day: "numeric",
  year: "numeric",
  timeZone: "UTC",
});

/** "2021-03-04" to "Mar 4, 2021". Null stays null. */
export function formatReleaseDate(iso: string | null): string | null {
  if (!iso) return null;
  const date = new Date(`${iso}T00:00:00Z`);
  return Number.isNaN(date.getTime()) ? null : MONTH_DAY_YEAR.format(date);
}

function plural(n: number, unit: string): string {
  return `${n} ${unit}${n === 1 ? "" : "s"}`;
}

/** Calendar span between two release dates, such as "7 years, 6 months, 22 days". Null when either date is missing. */
export function formatSpan(fromIso: string | null, toIso: string | null): string | null {
  if (!fromIso || !toIso) return null;

  const from = new Date(`${fromIso}T00:00:00Z`);
  const to = new Date(`${toIso}T00:00:00Z`);
  if (Number.isNaN(from.getTime()) || Number.isNaN(to.getTime()) || to < from) return null;

  let years = to.getUTCFullYear() - from.getUTCFullYear();
  let months = to.getUTCMonth() - from.getUTCMonth();
  let days = to.getUTCDate() - from.getUTCDate();

  if (days < 0) {
    months -= 1;
    days += new Date(Date.UTC(to.getUTCFullYear(), to.getUTCMonth(), 0)).getUTCDate();
  }
  if (months < 0) {
    years -= 1;
    months += 12;
  }

  const parts: string[] = [];
  if (years > 0) parts.push(plural(years, "year"));
  if (months > 0) parts.push(plural(months, "month"));
  if (days > 0) parts.push(plural(days, "day"));

  return parts.length > 0 ? parts.join(", ") : "the same day";
}
