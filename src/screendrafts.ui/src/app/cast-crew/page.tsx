// app/cast-crew/page.tsx
import type { Metadata } from "next";
import CastCrewFilterStrip from "@/components/features/cast-crew/cast-crew-filter-strip";
import Paginator from "@/components/ui/paginator";
import { castCrewQuery } from "@/lib/cast-crew-query";
import { listCacheKey, type QueryRecord } from "@/lib/list-queries";
import { fetchMediaPeople } from "@/services/media/fetch-media-people";
import { CastCrewInfiniteList } from "./cast-crew-infinite-list";

export const metadata: Metadata = {
  title: "Cast & Crew",
  description: "Everyone who acted in, directed, wrote or produced a title in the Vault.",
};

export const dynamic = "force-dynamic";

type SearchParams = Promise<QueryRecord>;

export default async function CastCrewPage(props: { searchParams: SearchParams }) {
  const qp = await props.searchParams;

  const { page, sort, search, role, args } = castCrewQuery(qp);

  const result = await fetchMediaPeople({ ...args, page });
  const totalPages = result.totalPages ?? 0;
  const cacheKey = listCacheKey("cast-crew", qp);

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="bg-sd-ink text-white page-x pt-10 pb-8 lg:pt-14 lg:pb-11">
        <p className="font-mono text-[11px] tracking-widest text-light-blue mb-3">/ CAST &amp; CREW</p>
        <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between lg:gap-8">
          <h1 className="font-oswald font-bold text-[44px] sm:text-[56px] lg:text-[72px] leading-[0.95] text-white">
            CAST &amp; CREW
          </h1>
          <p className="font-serif italic text-[15px] lg:text-[17px] leading-relaxed text-white/70 max-w-[480px] lg:text-right">
            Everyone who acted in, directed, wrote or produced a title in the Vault.
          </p>
        </div>
      </div>

      <CastCrewFilterStrip role={role} sort={sort} q={search} />

      <div className="page-x pt-6 pb-16">
        {/* Remounts (key) whenever filters, sort or page change, so infinite scroll restarts cleanly. */}
        <CastCrewInfiniteList
          key={cacheKey}
          cacheKey={cacheKey}
          query={qp}
          sort={sort}
          initialItems={result.items ?? []}
          initialPage={page}
          total={result.totalCount ?? 0}
          totalPages={totalPages}
          paginator={totalPages > 1 ? <Paginator page={page} totalPages={totalPages} searchParams={qp} /> : null}
        />
      </div>
    </div>
  );
}
