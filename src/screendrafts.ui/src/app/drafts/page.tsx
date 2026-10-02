// app/drafts/page.tsx
import DraftsFilter from "@/components/features/drafts/drafts-filter";
import { listDrafts } from "@/services/drafts/fetch-drafts";
import { fetchSiteStats } from "@/services/home/fetch-home-data";
import { Metadata } from "next";
import { Suspense } from "react";
import { listCampaigns } from "@/services/drafts/fetch-campaigns";
import { auth } from "@/auth";
import { listCategories } from "@/services/drafts/fetch-categrories";
import { WikiExportScope } from "@/components/features/wiki-export/wiki-export";
import { draftsQuery, listCacheKey, type QueryRecord } from "@/lib/list-queries";
import { DraftsInfiniteList } from "./drafts-infinite-list";

export const metadata: Metadata = {
   title: "The Archive",
   description: "Every draft, every pick, every veto.",
}

export const dynamic = "force-dynamic"
const ADMIN_ROLES = ["Administrator", "SuperAdministrator"];

type SearchParams = Promise<QueryRecord>;

function formatStat(n: number | undefined): string {
   if (n == null) return "—";
   return n.toLocaleString("en-US");
}

export default async function DraftsPage(props: { searchParams: SearchParams }) {
   const session = await auth();
   const isAdmin = session?.roles?.some(r => ADMIN_ROLES.includes(r)) ?? false;
   const qp = await props.searchParams;

   const { page, pageSize, args } = draftsQuery(qp);

   const [draftsResult, stats, campaigns, categories] = await Promise.all([
      listDrafts({ ...args, page }),
      fetchSiteStats(),
      listCampaigns(),
      listCategories(),
   ]);

   const totalPages = Math.ceil(draftsResult.total / pageSize);
   const cacheKey = listCacheKey("drafts", qp);

   return (
      <div className="min-h-screen bg-light-blue">
         {/* Banner — stacks below lg; title and blurb sit side by side from lg up. */}
         <div className="bg-sd-ink text-white page-x pt-10 pb-8 lg:pt-14 lg:pb-11">
            <p className="font-mono text-[11px] tracking-widest text-light-blue mb-3">/ DRAFTS</p>
            <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between lg:gap-8">
               <h1 className="font-oswald font-bold text-[44px] sm:text-[56px] lg:text-[72px] leading-[0.95] text-white">
                  THE ARCHIVE
               </h1>
               <p className="font-serif italic text-[15px] lg:text-[17px] leading-relaxed text-white/70 max-w-[480px] lg:text-right">
                  Every draft, every pick, every veto. Eight years and three hundred-odd episodes of
                  competitively-collaborative best-of lists.
               </p>
            </div>

            {/* Stat strip — 2×2 on phones, one row from sm up. stats is null if /stats failed; each value shows "—". */}
            <div className="grid grid-cols-2 gap-x-6 gap-y-5 sm:flex sm:gap-10 mt-8 lg:mt-10 border-t border-white/10 pt-6 lg:pt-8">
               {[
                  { label: "EPISODES", value: formatStat(stats?.episodesProduced) },
                  { label: "FILMS DRAFTED", value: formatStat(stats?.filmsDrafted) },
                  { label: "VETOES", value: formatStat(stats?.vetoesDeployed) },
                  { label: "GUEST G.M.S", value: formatStat(stats?.guestGMs) },
               ].map(({ label, value }) => (
                  <div key={label} className="flex flex-col gap-1">
                     <span className="font-oswald font-bold text-[24px] lg:text-[28px] text-sd-red leading-none">{value}</span>
                     <span className="font-mono text-[10px] tracking-widest text-light-blue">{label}</span>
                  </div>
               ))}
            </div>
         </div>

         {/* Filter strip */}
         <Suspense>
            <DraftsFilter campaigns={campaigns} categories={categories} />
         </Suspense>

         {/* List — pt-6 separates it from the filter strip. pb-24 keeps the last row and
             pager clear of the fixed wiki-export bar. */}
         <div className="page-x pt-6 pb-24">
            <WikiExportScope enabled={isAdmin} kind="drafts" accessToken={session?.accessToken ?? ""} >
               {/* Remounts (key) whenever filters, sort or page change, so infinite scroll restarts cleanly. */}
               <DraftsInfiniteList
                  key={cacheKey}
                  cacheKey={cacheKey}
                  query={qp}
                  initialItems={draftsResult.items}
                  initialPage={page}
                  total={draftsResult.total}
                  totalPages={totalPages}
                  isAdmin={isAdmin}
                  paginator={totalPages > 1 ? <Paginator page={page} totalPages={totalPages} searchParams={qp} /> : null}
               />
            </WikiExportScope>
         </div>

      </div>
   );
}

// Desktop only — below lg the list scrolls infinitely instead.
function Paginator({
   page,
   totalPages,
   searchParams,
}: {
   page: number;
   totalPages: number;
   searchParams: QueryRecord;
}) {
   function pageHref(p: number): string {
      const qs = new URLSearchParams();
      Object.entries(searchParams).forEach(([k, v]) => {
         if (k === "page") return;
         if (Array.isArray(v)) v.forEach((s) => qs.append(k, s));
         else if (v) qs.set(k, v);
      });
      qs.set("page", String(p));
      return `?${qs.toString()}`;
   }

   const pages = buildPageRange(page, totalPages);

   return (
      <nav aria-label="Pagination" className="flex items-center gap-1 font-mono text-[11px]">
         {page > 1 && (
            <a href={pageHref(page - 1)} aria-label="Previous page" className="px-2.5 py-1 border border-sd-ink text-sd-ink hover:bg-sd-ink hover:text-white transition-colors">
               ‹
            </a>
         )}
         {pages.map((p, i) =>
            p === "…" ? (
               <span key={`ellipsis-${i}`} className="px-2 text-sd-ink/40">…</span>
            ) : (
               <a
                  key={p}
                  href={pageHref(p as number)}
                  aria-current={p === page ? "page" : undefined}
                  className={`px-2.5 py-1 border transition-colors ${p === page
                     ? "bg-sd-ink text-white border-sd-ink"
                     : "border-sd-ink text-sd-ink hover:bg-sd-ink hover:text-white"
                     }`}
               >
                  {p}
               </a>
            )
         )}
         {page < totalPages && (
            <a href={pageHref(page + 1)} aria-label="Next page" className="px-2.5 py-1 border border-sd-ink text-sd-ink hover:bg-sd-ink hover:text-white transition-colors">
               ›
            </a>
         )}
      </nav>
   );
}

function buildPageRange(current: number, total: number): (number | "…")[] {
   if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
   const pages: (number | "…")[] = [1];
   if (current > 3) pages.push("…");
   for (let p = Math.max(2, current - 1); p <= Math.min(total - 1, current + 1); p++) {
      pages.push(p);
   }
   if (current < total - 2) pages.push("…");
   pages.push(total);
   return pages;
}