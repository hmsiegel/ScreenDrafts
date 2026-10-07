// services/stats/title-levels.ts
import type { TitleSort } from "./stats-types";

export interface TitleLevelDefinition {
  slug: string;
  label: string;
  minAppearances: number;
  blurb: string;
}

/** The four movie honorific levels, in the order the sub-tabs appear. Slugs match the API's level codes. */
export const TITLE_LEVELS: readonly TitleLevelDefinition[] = [
  {
    slug: "marquee-of-fame",
    label: "Marquee of Fame",
    minAppearances: 2,
    blurb: "Titles drafted on at least two separate episodes.",
  },
  {
    slug: "hat-trick",
    label: "Hat Trick",
    minAppearances: 3,
    blurb: "Titles drafted on three or more episodes.",
  },
  {
    slug: "grand-slam",
    label: "Grand Slam",
    minAppearances: 4,
    blurb: "Titles drafted on four or more episodes.",
  },
  {
    slug: "high-five",
    label: "High Five",
    minAppearances: 5,
    blurb: "Titles drafted on five or more episodes.",
  },
];

export function findTitleLevel(slug: string): TitleLevelDefinition | undefined {
  return TITLE_LEVELS.find((level) => level.slug === slug);
}

export const TITLE_SORTS: readonly { value: TitleSort; label: string }[] = [
  { value: "newest", label: "Newest to join" },
  { value: "oldest", label: "Oldest to join" },
  { value: "alphabetical", label: "A to Z" },
  { value: "appearances", label: "Most drafted" },
];

export function toTitleSort(value: string | undefined): TitleSort {
  return TITLE_SORTS.some((s) => s.value === value) ? (value as TitleSort) : "newest";
}
