// app/stats/titles/[level]/page.tsx
import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { auth } from "@/auth";
import { ScopeToggle } from "@/components/features/stats/scope-toggle";
import { TitleFilterBar } from "@/components/features/stats/title-filter-bar";
import { TitleLevelTabs } from "@/components/features/stats/title-level-tabs";
import { TitleList } from "@/components/features/stats/title-list";
import { TitlePagination } from "@/components/features/stats/title-pagination";
import { formatStatValue } from "@/components/features/stats/format-stat-value";
import { fetchStatsOptions, fetchTitleHonorifics } from "@/services/stats/fetch-stats";
import { findTitleLevel, toTitleSort } from "@/services/stats/title-levels";

type Props = {
  params: Promise<{ level: string }>;
  searchParams: Promise<{ q?: string; sort?: string; page?: string; scope?: string }>;
};

export const dynamic = "force-dynamic";

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { level } = await params;
  const definition = findTitleLevel(level);
  if (!definition) return {};

  const description = `${definition.blurb} See which titles joined, when, and every draft they appeared on.`;

  return {
    title: definition.label,
    description,
    openGraph: { title: `Screen Drafts ${definition.label}`, description },
  };
}

export default async function TitleHonorificsPage({ params, searchParams }: Props) {
  const { level } = await params;
  const definition = findTitleLevel(level);
  if (!definition) notFound();

  const { q, sort, page, scope } = await searchParams;
  const query = (q ?? "").trim().slice(0, 100);
  const sortValue = toTitleSort(sort);
  const pageNumber = Math.max(1, Number.parseInt(page ?? "1", 10) || 1);

  const session = await auth();
  const accessToken = session?.accessToken;

  // Signed-out visitors always get the canonical list. The API ignores the toggle for anyone it does not apply to.
  const options = accessToken ? await fetchStatsOptions(accessToken) : null;
  const canIncludeAll = options?.canIncludeAll ?? false;
  const includeAll = canIncludeAll && scope === "all";

  const data = await fetchTitleHonorifics({
    level: definition.slug,
    search: query || undefined,
    sort: sortValue,
    page: pageNumber,
    includeAll,
    accessToken,
  });

  if (!data) {
    return (
      <p className="border-2 border-sd-ink bg-white px-5 py-6 text-sm text-sd-ink/70">
        This list is unavailable right now. Try again in a few minutes.
      </p>
    );
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <p className="text-sm text-sd-ink/70 max-w-2xl">
          {definition.blurb}{" "}
          {data.includesNonCanonical
            ? "Every draft, Patreon and Speed included."
            : "Canonical episodes only."}{" "}
          Titles are numbered in the order they joined, by the release date of the part they joined on. Titles that join on the same part follow the order the picks were played.
        </p>
        {canIncludeAll && (
          <ScopeToggle includeAll={includeAll} basePath={`/stats/titles/${definition.slug}`} />
        )}
      </div>

      <TitleLevelTabs activeSlug={definition.slug} counts={data.counts} includeAll={includeAll} />

      <TitleFilterBar level={definition.slug} query={query} sort={sortValue} includeAll={includeAll} />

      <p className="font-mono text-[10px] tracking-widest text-sd-ink/50">
        {formatStatValue(data.totalMatching, "count")} {data.totalMatching === 1 ? "TITLE" : "TITLES"}
        {query ? ` MATCHING "${query.toUpperCase()}"` : ""} · PAGE {data.page} OF {data.totalPages}
      </p>

      <TitleList levelLabel={data.levelLabel} titles={data.titles} />

      <TitlePagination
        level={definition.slug}
        page={data.page}
        totalPages={data.totalPages}
        query={query}
        sort={sortValue}
        includeAll={includeAll}
      />
    </div>
  );
}