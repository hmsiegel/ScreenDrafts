// services/stats/fetch-stats.ts
import { env } from "@/lib/env";
import type {
  GetRecordBookResponse,
  QueryStatsResponse,
  RecordHolder,
  RecordItem,
  StatsQueryOptionsResponse,
} from "@/lib/dto";
import type {
  HolderKind,
  RecordBookView,
  RecordHolderView,
  RecordView,
  StatFormat,
  StatsOptionsView,
  StatsQueryInput,
  StatsQueryOutcome,
  StatsQueryResultView,
} from "./stats-types";

const apiBase = env.apiUrl;

// The API caches the Record Book for an hour and clears it when a draft is recorded, so a short
// Next.js cache for anonymous visitors only smooths bursts.
const PUBLIC_REVALIDATE_SECONDS = 300;

function authHeader(accessToken?: string): Record<string, string> {
  return accessToken ? { Authorization: `Bearer ${accessToken}` } : {};
}

// ── Record Book ──────────────────────────────────────────────────────────

/** Null when the request failed. Signed-in callers always bypass the Next.js cache. */
export async function fetchRecordBook(
  options: { includeAll?: boolean; accessToken?: string } = {}
): Promise<RecordBookView | null> {
  const { includeAll = false, accessToken } = options;
  const url = `${apiBase}/stats/record-book${includeAll ? "?includeAll=true" : ""}`;

  try {
    const res = await fetch(
      url,
      accessToken
        ? { headers: authHeader(accessToken), cache: "no-store" }
        : { next: { revalidate: PUBLIC_REVALIDATE_SECONDS } }
    );

    if (!res.ok) {
      console.error(`[stats] GET ${url} failed: ${res.status} ${res.statusText}`);
      return null;
    }

    return mapRecordBook((await res.json()) as GetRecordBookResponse);
  } catch (err) {
    console.error(`[stats] GET ${url} failed:`, err);
    return null;
  }
}

/** Null when the caller is not signed in or the request failed. */
export async function fetchStatsOptions(accessToken: string): Promise<StatsOptionsView | null> {
  const url = `${apiBase}/stats/query/options`;

  try {
    const res = await fetch(url, { headers: authHeader(accessToken), cache: "no-store" });

    if (!res.ok) {
      console.error(`[stats] GET ${url} failed: ${res.status} ${res.statusText}`);
      return null;
    }

    return mapOptions((await res.json()) as StatsQueryOptionsResponse);
  } catch (err) {
    console.error(`[stats] GET ${url} failed:`, err);
    return null;
  }
}

export async function postStatsQuery(
  accessToken: string,
  input: StatsQueryInput
): Promise<StatsQueryOutcome> {
  const url = `${apiBase}/stats/query`;

  try {
    const res = await fetch(url, {
      method: "POST",
      headers: {
        ...authHeader(accessToken),
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: JSON.stringify(input),
      cache: "no-store",
    });

    if (!res.ok) {
      return { ok: false, message: await problemMessage(res) };
    }

    return { ok: true, data: mapQueryResult((await res.json()) as QueryStatsResponse) };
  } catch (err) {
    console.error(`[stats] POST ${url} failed:`, err);
    return { ok: false, message: "Could not reach the stats service. Try again in a moment." };
  }
}

async function problemMessage(res: Response): Promise<string> {
  if (res.status === 401) return "Your session has expired. Sign in again to run queries.";
  if (res.status === 403) return "You do not have access to run queries.";

  try {
    const problem = (await res.json()) as { detail?: string; title?: string };
    return problem.detail ?? problem.title ?? "The query could not be run.";
  } catch {
    return "The query could not be run.";
  }
}

// ── Mappers ──────────────────────────────────────────────────────────────

const TIER_SUFFIX = /^(.*)\.min(\d+)$/;

function toFormat(value: string | undefined): StatFormat {
  return value === "ratio" || value === "percent" ? value : "count";
}

function toHolderKind(value: string | undefined): HolderKind {
  return value === "draft" || value === "title" ? value : "drafter";
}

function mapHolder(h: RecordHolder): RecordHolderView {
  return {
    kind: toHolderKind(h.kind),
    name: h.name ?? "",
    publicId: h.publicId ?? null,
    context: h.context ?? null,
  };
}

/**
 * Records that carry a minimum-drafts qualifier arrive as separate items with codes ending in ".min5",
 * ".min10" and so on. They become one record with a tier each, so the page shows a single row with a picker.
 */
function toRecordViews(records: RecordItem[]): RecordView[] {
  const byKey = new Map<string, RecordView>();

  for (const r of records) {
    const code = r.code ?? "";
    const match = TIER_SUFFIX.exec(code);
    const key = match ? match[1] : code;
    const tier = match ? Number(match[2]) : 0;

    const view: RecordView = byKey.get(key) ?? {
      key,
      label: r.label ?? "",
      format: toFormat(r.format),
      tiers: [],
    };

    view.tiers.push({
      tier,
      qualifier: r.qualifier ?? null,
      value: r.value ?? 0,
      holders: (r.holders ?? []).map(mapHolder),
    });

    byKey.set(key, view);
  }

  return [...byKey.values()].map((view) => ({
    ...view,
    tiers: [...view.tiers].sort((a, b) => a.tier - b.tier),
  }));
}

function mapRecordBook(dto: GetRecordBookResponse): RecordBookView {
  const generatedAt = dto.generatedAtUtc as Date | string | undefined;

  return {
    generatedAt: generatedAt ? new Date(generatedAt).toISOString() : null,
    includesNonCanonical: dto.includesNonCanonical ?? false,
    totals: (dto.totals ?? []).map((t) => ({
      code: t.code ?? "",
      label: t.label ?? "",
      value: t.value ?? 0,
    })),
    sections: (dto.sections ?? []).map((section) => ({
      key: section.key ?? "",
      title: section.title ?? "",
      groups: (section.groups ?? []).map((group) => ({
        key: group.key ?? "",
        title: group.title ?? "",
        records: toRecordViews(group.records ?? []),
      })),
    })),
  };
}

function mapOptions(dto: StatsQueryOptionsResponse): StatsOptionsView {
  return {
    canIncludeAll: dto.canIncludeAll ?? false,
    metrics: (dto.metrics ?? []).map((m) => ({
      code: m.code ?? "",
      label: m.label ?? "",
      format: m.format === "ratio" ? "ratio" : "count",
      description: m.description ?? "",
      groupBys: m.groupBys ?? [],
    })),
    groupBys: (dto.groupBys ?? []).map((g) => ({ code: g.code ?? "", label: g.label ?? "" })),
    series: dto.series ?? [],
    draftTypes: dto.draftTypes ?? [],
    minEpisode: dto.minEpisode ?? null,
    maxEpisode: dto.maxEpisode ?? null,
  };
}

function mapQueryResult(dto: QueryStatsResponse): StatsQueryResultView {
  return {
    metricLabel: dto.metricLabel ?? "",
    groupBy: dto.groupBy ?? "",
    format: dto.format === "ratio" ? "ratio" : "count",
    includesNonCanonical: dto.includesNonCanonical ?? false,
    totalGroups: dto.totalGroups ?? 0,
    truncated: dto.truncated ?? false,
    rows: (dto.rows ?? []).map((r) => ({
      rank: r.rank ?? 0,
      name: r.name ?? "",
      publicId: r.publicId ?? null,
      value: r.value ?? 0,
      context: r.context ?? null,
    })),
  };
}
