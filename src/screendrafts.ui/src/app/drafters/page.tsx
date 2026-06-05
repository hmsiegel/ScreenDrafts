import DraftersFilterStrip from "@/components/features/participants/drafters-filter-strip";
import ParticipantCard from "@/components/features/participants/participant-card";
import { listParticipants } from "@/services/participants/fetch-participants";
import { Metadata } from "next";
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

export default async function ParticipantsPage(props: { searchParams: SearchParams }) {
  const qp = await props.searchParams;

  const page = asNumber(qp.page) ?? 1;
  const pageSize = asNumber(qp.pageSize) ?? 24;
  const sort = asString(qp.sort) ?? "name";
  const q = asString(qp.q);
  const honorific = asString(qp.honorific) ?? "";

  const filter = honorific ? "all" : (asString(qp.filter) ?? "all");

  // Map UI filter tab → API role param
  const role =
  honorific ? undefined
    : filter === "commissioners" ? "commissioner"
    : filter === "gms" ? "gm"
    : filter === "hosts" ? "host"
    : undefined;

  const result = await listParticipants({
    q,
    role,
    sort,
    page,
    pageSize,
    honorific: honorific || undefined,
  });

  const totalPages = Math.ceil(result.total / pageSize);

  return (
    <div className="min-h-screen bg-light-blue">
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
        <DraftersFilterStrip
          filter={filter}
          sort={sort}
          q={q}
          honorific={honorific} />
      </Suspense>

      {/* Grid */}
      <div className="px-10 py-10">
        <div
          className="grid gap-[22px]"
          style={{ gridTemplateColumns: "repeat(3, 1fr)" }}
        >
          {result.items.map((participant, i) => (
            <ParticipantCard
              key={participant.personPublicId}
              participant={participant}
              index={i}
              honorific={participant.honorific ?? null}
            />
          ))}
        </div>

        {result.items.length === 0 && (
          <div className="text-center font-mono text-sm text-sd-ink/50 py-16">
            No participants found.
          </div>
        )}

        {/* Pagination */}
        <div className="flex items-center justify-between mt-8">
          <span className="font-mono text-[11px] text-sd-ink/60">
            SHOWING {result.items.length} OF {result.total.toLocaleString("en-US")} PARTICIPANTS
          </span>
          {totalPages > 1 && (
            <Paginator page={page} totalPages={totalPages} searchParams={qp} />
          )}
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
        <a
          href={pageHref(page - 1)}
          className="px-2.5 py-1 border border-sd-ink text-sd-ink hover:bg-sd-ink hover:text-white transition-colors"
        >
          ‹
        </a>
      )}
      {pages.map((p, i) =>
        p === "…" ? (
          <span key={`ellipsis-${i}`} className="px-2 text-sd-ink/40">
            …
          </span>
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
        <a
          href={pageHref(page + 1)}
          className="px-2.5 py-1 border border-sd-ink text-sd-ink hover:bg-sd-ink hover:text-white transition-colors"
        >
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