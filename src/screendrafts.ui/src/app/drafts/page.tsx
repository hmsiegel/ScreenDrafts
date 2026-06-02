import SiteHeader from "@/components/layout/header/site-header";
import DraftsFilter from "@/components/features/drafts/drafts-filter";
import { listDrafts } from "@/services/drafts/fetch-drafts";
import { fetchSiteStats } from "@/services/home/fetch-home-data";
import { Metadata } from "next";
import { Suspense } from "react";
import { DraftsTable } from "@/components/features/drafts/drafts-table";
import SiteFooter from "@/components/layout/footer/site-footer";
import { listCampaigns } from "@/services/drafts/fetch-campaigns";
import { auth } from "@/auth";

export const metadata: Metadata = {
   title: "The Archive",
   description: "Every draft, every pick, every veto.",
}

export const dynamic = "force-dynamic"
const ADMIN_ROLES = ["Administrator", "SuperAdministrator"];

type SearchParams = Promise<{ [key: string]: string | string[] | undefined }>;

function asNumber(v: string | string[] | undefined): number | undefined {
   if (!v) return undefined;
   const n = Number(Array.isArray(v) ? v[0] : v);
   return isNaN(n) ? undefined : n;
}
function asString(v: string | string[] | undefined): string | undefined {
   if (!v) return undefined;
   return Array.isArray(v) ? v[0] : v;
}

function formatStat(n: number | undefined): string {
   if (n == null) return "—";
   return n.toLocaleString("en-US");
}

export default async function DraftsPage(props: { searchParams: SearchParams }) {
   const session = await auth();
   const isAdmin = session?.roles?.some(r => ADMIN_ROLES.includes(r)) ?? false;
   const qp = await props.searchParams;

   const page = asNumber(qp.page) ?? 1;
   const pageSize = asNumber(qp.pageSize) ?? 25;
   const draftType = asNumber(qp.draftType);

   const sort = asString(qp.sort) ?? "date";
   const dir = (asString(qp.dir) ?? "desc") as "asc" | "desc";

   const [draftsResult, stats, campaigns] = await Promise.all([
      listDrafts({
         q: asString(qp.q),
         fromDate: asString(qp.fromDate),
         toDate: asString(qp.toDate),
         draftType,
         minDrafters: asNumber(qp.minDrafters),
         maxDrafters: asNumber(qp.maxDrafters),
         page,
         pageSize,
         campaignPublicId: asString(qp.campaignPublicId),
         sort,
         dir,
      }),
      fetchSiteStats(),
      listCampaigns(),
   ]);

   const totalPages = Math.ceil(draftsResult.total / pageSize);

   return (
      <div className="min-h-screen bg-light-blue">
         <SiteHeader activePath="/drafts" />

         {/* Banner */}
         <div className="bg-sd-ink text-white" style={{ padding: "56px 40px 44px" }}>
            <p className="font-mono text-[11px] tracking-widest text-light-blue mb-3">/ DRAFTS</p>
            <div className="flex items-end justify-between gap-8">
               <h1 className="font-oswald font-bold text-[72px] leading-[0.95] text-white">
                  THE ARCHIVE
               </h1>
               <p className="font-serif italic text-[17px] leading-relaxed text-white/70 max-w-[480px] text-right">
                  Every draft, every pick, every veto. Eight years and three hundred-odd episodes of
                  competitively-collaborative best-of lists.
               </p>
            </div>

            {/* Stat strip */}
            <div className="flex gap-10 mt-10 border-t border-white/10 pt-8">
               {[
                  { label: "EPISODES", value: formatStat(stats.episodesProduced) },
                  { label: "FILMS DRAFTED", value: formatStat(stats.filmsDrafted) },
                  { label: "VETOES", value: formatStat(stats.vetoesDeployed) },
                  { label: "GUEST G.M.S", value: formatStat(stats.guestGMs) },
               ].map(({ label, value }) => (
                  <div key={label} className="flex flex-col gap-1">
                     <span className="font-oswald font-bold text-[28px] text-sd-red leading-none">{value}</span>
                     <span className="font-mono text-[10px] tracking-widest text-light-blue">{label}</span>
                  </div>
               ))}
            </div>
         </div>

         {/* Filter strip */}
         <Suspense>
            <DraftsFilter campaigns={campaigns} />
         </Suspense>

         {/* Table container */}
         <div className="px-10 pt-0 pb-16">
            <DraftsTable drafts={draftsResult.items} searchParams={qp} isAdmin={isAdmin} />

            {/* Pagination */}
            <div className="flex items-center justify-between mt-4">
               <span className="font-mono text-[11px] text-sd-ink/60">
                  SHOWING {draftsResult.items.length} OF {draftsResult.total.toLocaleString("en-US")} EPISODES
               </span>
               {totalPages > 1 && (
                  <Paginator page={page} totalPages={totalPages} total={draftsResult.total} pageSize={pageSize} searchParams={qp} />
               )}
            </div>
         </div>

         <SiteFooter />
      </div>
   );
}

function Paginator({
   page,
   totalPages,
   searchParams,
}: {
   page: number;
   totalPages: number;
   total: number;
   pageSize: number;
   searchParams: { [key: string]: string | string[] | undefined };
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
      <nav className="flex items-center gap-1 font-mono text-[11px]">
         {page > 1 && (
            <a href={pageHref(page - 1)} className="px-2.5 py-1 border border-sd-ink text-sd-ink hover:bg-sd-ink hover:text-white transition-colors">
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
            <a href={pageHref(page + 1)} className="px-2.5 py-1 border border-sd-ink text-sd-ink hover:bg-sd-ink hover:text-white transition-colors">
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