// services/stats/title-levels.ts
import type { TitleSort } from "./stats-types";

export interface TitleLevelDefinition {
  slug: string;
  label: string;
  minAppearances: number;
  blurb: string;
  /** True for the four honorifics. False for the plain appearance-count levels (6+ and up). */
  named: boolean;
}

/** The title levels, in the order the sub-tabs appear. Slugs match the API's level codes. */
export const TITLE_LEVELS: readonly TitleLevelDefinition[] = [
  {
    slug: "marquee-of-fame",
    label: "Marquee of Fame",
    minAppearances: 2,
    blurb: "Titles drafted on at least two separate episodes.",
    named: true,
  },
  {
    slug: "hat-trick",
    label: "Hat Trick",
    minAppearances: 3,
    blurb: "Titles drafted on three or more episodes.",
    named: true,
  },
  {
    slug: "grand-slam",
    label: "Grand Slam",
    minAppearances: 4,
    blurb: "Titles drafted on four or more episodes.",
    named: true,
  },
  {
    slug: "high-five",
    label: "High Five",
    minAppearances: 5,
    blurb: "Titles drafted on five or more episodes.",
    named: true,
  },
  {
    slug: "6-drafts",
    label: "6+ Drafts",
    minAppearances: 6,
    blurb: "Titles drafted on six or more episodes.",
    named: false,
  },
  {
    slug: "7-drafts",
    label: "7+ Drafts",
    minAppearances: 7,
    blurb: "Titles drafted on seven or more episodes.",
    named: false,
  },
  {
    slug: "8-drafts",
    label: "8+ Drafts",
    minAppearances: 8,
    blurb: "Titles drafted on eight or more episodes.",
    named: false,
  },
  {
    slug: "9-drafts",
    label: "9+ Drafts",
    minAppearances: 9,
    blurb: "Titles drafted on nine or more episodes.",
    named: false,
  },
  {
    slug: "10-drafts",
    label: "10+ Drafts",
    minAppearances: 10,
    blurb: "Titles drafted on ten or more episodes.",
    named: false,
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
