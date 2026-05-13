import DrafterCard from "@/components/features/drafters/drafter-card";
import SiteFooter from "@/components/layout/footer/site-footer";
import SiteHeader from "@/components/layout/header/site-header";
import { listDrafters } from "@/services/drafters/fetch-drafters";
import { Metadata } from "next";
import Link from "next/link";
import { Suspense } from "react";

export const metadata: Metadata = {
  title: "The Roster",
  description: "Two commissioners. A revolving cast of guest General Managers.",
};

export const dynamic = "force-dynamic";

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

export default async function DraftersPage(props: { searchParams: SearchParams }) {
  const qp = await props.searchParams;

  const page = asNumber(qp.page) ?? 1;
  const pageSize = asNumber(qp.pageSize) ?? 24;
  const filter = asString(qp.filter) ?? "all";
  const sort = asString(qp.sort);

  const retired =
    filter === "commissioners" ? "false"
    : filter === "guests" ? "false"
    : undefined;

  const result = await listDrafters({
    page,
    pageSize,
    sort,
    retired,
    q: asString(qp.q),
  });

  const totalPages = Math.ceil(result.total / pageSize);

  return (
    <div className="min-h-screen bg-light-blue">
      <SiteHeader activePath="/drafters" />

      {/* Banner */}
      <div className="bg-sd-ink text-white" style={{ padding: "56px 40px 44px" }}>
        <p className="font-mono text-[11px] tracking-widest text-light-blue mb-3">/ DRAFTERS</p>
        <div className="flex items-end justify-between gap-8">
          <h1 className="font-oswald font-bold text-[72px] leading-[0.95] text-white">
            THE ROSTER
          </h1>
          <p className="font-serif italic text-[17px] leading-relaxed text-white/70 max-w-[480px] text-right">
            Two commissioners. A revolving cast of guest General Managers. Every drafter who&rsquo;s
            ever filled out a ballot.
          </p>
        </div>
      </div>

      {/* Filter strip */}
      <Suspense>
        <DrafterFilterStrip filter={filter} sort={sort} />
      </Suspense>

      {/* Grid */}
      <div className="px-10 py-10">
        <div className="grid gap-[22px]" style={{ gridTemplateColumns: "repeat(3, 1fr)" }}>
          {result.items.map((drafter, i) => (
            <DrafterCard key={drafter.drafterId} drafter={drafter} index={i} />
          ))}
        </div>

        {result.items.length === 0 && (
          <div className="text-center font-mono text-sm text-sd-ink/50 py-16">
            No drafters found.
          </div>
        )}

        {/* Pagination */}
        <div className="flex items-center justify-between mt-8">
          <span className="font-mono text-[11px] text-sd-ink/60">
            SHOWING {result.items.length} OF {result.total.toLocaleString("en-US")} DRAFTERS
          </span>
          {totalPages > 1 && (
            <Paginator page={page} totalPages={totalPages} searchParams={qp} />
          )}
        </div>
      </div>

      <SiteFooter />
    </div>
  );
}

function DrafterFilterStrip({ filter, sort }: { filter: string; sort: string }) {
  const tabs = [
    { value: "all", label: "ALL" },
    { value: "commissioners", label: "COMMISSIONERS" },
    { value: "guests", label: "GUEST G.M.S" },
  ];

  const sortOptions = [
    { value: "name", label: "Name A–Z" },
    { value: "drafts", label: "Most Drafts" },
  ];

  function tabHref(value: string): string {
    return `?filter=${value}&sort=${sort}&page=1`;
  }

  function sortHref(value: string): string {
    return `?filter=${filter}&sort=${value}&page=1`;
  }

  return (
    <div className="bg-white border-b border-sd-ink/20 px-10 py-4 flex items-center justify-between">
      <div className="flex gap-1">
        {tabs.map((tab) => (
          <Link
            key={tab.value}
            href={tabHref(tab.value)}
            className={`px-4 py-2 font-oswald text-[12px] tracking-wide transition-colors ${
              filter === tab.value
                ? "bg-sd-ink text-white"
                : "text-sd-ink hover:bg-sd-ink/5"
            }`}
          >
            {tab.label}
          </Link>
        ))}
      </div>
      <div className="flex items-center gap-2">
        <span className="font-mono text-[10px] tracking-widest text-sd-blue">SORT BY</span>
        <div className="flex gap-1">
          {sortOptions.map((opt) => (
            <Link
              key={opt.value}
              href={sortHref(opt.value)}
              className={`px-3 py-1.5 font-mono text-[11px] transition-colors border ${
                sort === opt.value
                  ? "bg-sd-ink text-white border-sd-ink"
                  : "border-sd-ink/30 text-sd-ink hover:border-sd-ink"
              }`}
            >
              {opt.label}
            </Link>
          ))}
        </div>
      </div>
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
            className={`px-2.5 py-1 border transition-colors ${
              p === page
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
