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
