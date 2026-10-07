// services/stats/stats-types.ts
// Strict view models for the Record Book and the custom query. The generated dto types make every
// field optional, so fetch-stats.ts maps them into these once and the components never check for undefined.

export type StatFormat = "count" | "ratio" | "percent";
export type HolderKind = "drafter" | "draft" | "title";

export interface RecordHolderView {
  kind: HolderKind;
  name: string;
  /** Drafter: the person's public id (the /drafters/{id} route). Draft and title: their own public id. */
  publicId: string | null;
  /** The draft a single-draft record was set in, or "12 drafts" on a qualified record. */
  context: string | null;
}

export interface RecordTierView {
  /** Minimum drafts for a qualified record (5, 10, 15, 20). 0 for a record with no qualifier. */
  tier: number;
  qualifier: string | null;
  value: number;
  holders: RecordHolderView[];
}

export interface RecordView {
  key: string;
  label: string;
  format: StatFormat;
  /** One entry, or several for a record that has a minimum-drafts qualifier. */
  tiers: RecordTierView[];
}

export interface RecordGroupView {
  key: string;
  title: string;
  records: RecordView[];
}

export interface RecordSectionView {
  key: string;
  title: string;
  groups: RecordGroupView[];
}

export interface RecordTotalView {
  code: string;
  label: string;
  value: number;
}

export interface RecordBookView {
  generatedAt: string | null;
  includesNonCanonical: boolean;
  totals: RecordTotalView[];
  sections: RecordSectionView[];
}

export interface StatsMetricOptionView {
  code: string;
  label: string;
  format: "count" | "ratio";
  description: string;
  groupBys: string[];
}

export interface StatsGroupByOptionView {
  code: string;
  label: string;
}

export interface StatsOptionsView {
  canIncludeAll: boolean;
  metrics: StatsMetricOptionView[];
  groupBys: StatsGroupByOptionView[];
  series: string[];
  draftTypes: string[];
  minEpisode: number | null;
  maxEpisode: number | null;
}

export interface StatsQueryInput {
  metric: string;
  groupBy: string;
  series?: string[];
  draftTypes?: string[];
  episodeFrom?: number;
  episodeTo?: number;
  minAppearances?: number;
  ascending?: boolean;
  limit?: number;
  includeAll?: boolean;
}

export interface StatsQueryRowView {
  rank: number;
  name: string;
  publicId: string | null;
  value: number;
  context: string | null;
}

export interface StatsQueryResultView {
  metricLabel: string;
  groupBy: string;
  format: "count" | "ratio";
  includesNonCanonical: boolean;
  totalGroups: number;
  truncated: boolean;
  rows: StatsQueryRowView[];
}

export type StatsQueryOutcome =
  | { ok: true; data: StatsQueryResultView }
  | { ok: false; message: string };

// ── Title honorifics (Marquee of Fame, Hat Trick, Grand Slam, High Five) ───────────────────────────

export type TitleSort = "newest" | "oldest" | "alphabetical" | "appearances";

export interface TitleAppearanceView {
  appearanceNumber: number;
  draftTitle: string;
  draftPublicId: string;
  episodeNumber: number | null;
  partIndex: number;
  totalParts: number;
  /** yyyy-MM-dd */
  releasedOn: string | null;
  position: number;
}

export interface TitleHonorificEntryView {
  /** Join order at this level: 1 is the first title to join. */
  number: number;
  mediaPublicId: string;
  title: string;
  appearanceCount: number;
  joinedDraftTitle: string;
  joinedDraftPublicId: string;
  joinedEpisode: number | null;
  joinedPartIndex: number;
  joinedTotalParts: number;
  joinedOn: string | null;
  firstDraftTitle: string;
  firstEpisode: number | null;
  firstOn: string | null;
  /** Main-feed episodes between the first appearance and the joining one, counting every released part. */
  gapEpisodes: number | null;
  appearances: TitleAppearanceView[];
}

export interface TitleHonorificCountView {
  code: string;
  label: string;
  count: number;
}

export interface TitleHonorificsView {
  level: string;
  levelLabel: string;
  minAppearances: number;
  includesNonCanonical: boolean;
  page: number;
  pageSize: number;
  totalPages: number;
  totalMatching: number;
  counts: TitleHonorificCountView[];
  titles: TitleHonorificEntryView[];
}
