// components/features/drafts/drafts-table.tsx

import DraftTypeBadge from "@/components/ui/draft-type-badge";
import { draftTypeFromNumber } from "@/lib/draft-type-display";
import { ListDraftsResponse } from "@/lib/dto";
import { format } from "date-fns/format";
import { parseISO } from "date-fns/parseISO";
import Link from "next/link";
import { WikiSelectAll, WikiSelectCheckbox } from "@/components/features/wiki-export/wiki-export";

interface DraftsTableProps {
   drafts: ListDraftsResponse[];
   searchParams: Record<string, string | string[] | undefined>;
   isAdmin?: boolean;
}

function formatAirDate(raw: Date | string | undefined): string {
   if (!raw) return "—";
   try {
      // parseISO treats a bare date-only string as local midnight, unlike new Date(...)
      // which treats it as UTC midnight — see drafts-sidebar.tsx's formatDate for the
      // full explanation of the one-day-back shift this was causing.
      const date = typeof raw === "string" ? parseISO(raw) : raw;
      return format(date, "MMM dd, yyyy").toUpperCase();
   } catch {
      return "—";
   }
}

function sortUrl(
   field: string,
   currentSort: string | undefined,
   currentDir: string | undefined,
   params: Record<string, string | string[] | undefined>
): string {
   const isActive = currentSort === field;
   const nextDir = isActive && currentDir === "asc" ? "desc" : "asc";
   const qs = new URLSearchParams();

   for (const [key, value] of Object.entries(params)) {
      if (key === "sort" || key === "dir" || key === "page") continue;
      if (value === undefined) continue;
      if (Array.isArray(value)) {
         value.forEach((v) => qs.append(key, v));
      } else {
         qs.set(key, value);
      }
   }

   qs.set("sort", field);
   qs.set("dir", nextDir);
   qs.set("page", "1");
   return `?${qs.toString()}`;
}

interface SortableHeaderProps {
   field: string;
   label: string;
   currentSort: string | undefined;
   currentDir: string | undefined;
   params: Record<string, string | string[] | undefined>;
   className?: string;
}

function SortableHeader({
   field,
   label,
   currentSort,
   currentDir,
   params,
   className = "",
}: SortableHeaderProps) {
   const isActive = currentSort === field;
   const indicator = isActive ? (currentDir === "asc" ? " ↑" : " ↓") : " ↕";

   return (
      <Link
         href={sortUrl(field, currentSort, currentDir, params)}
         className={`flex items-center gap-1 hover:text-light-blue transition-colors ${isActive ? "text-white" : "text-white/60"
            } ${className}`}
      >
         {label}
         <span className={`text-[9px] ${isActive ? "text-sd-red" : "text-white/30"}`}>
            {indicator}
         </span>
      </Link>
   );
}

// Everything both layouts need from one draft, computed once.
interface DraftRow {
   key: string;
   draftPublicId: string | undefined;
   href: string;
   label: string;
   episodeNumber: number | string | undefined;
   airDate: string;
   participantLine: string | undefined;
   totalPicks: number | undefined;
   typeDisplay: ReturnType<typeof draftTypeFromNumber>;
}

function toRow(draft: ListDraftsResponse): DraftRow {
   return {
      key: (draft.draftPartPublicId ?? draft.draftPublicId) as string,
      draftPublicId: draft.draftPublicId ?? undefined,
      href: `/drafts/${draft.draftPublicId}`,
      label: draft.label ?? "—",
      episodeNumber: draft.releases?.[0]?.episodeNumber ?? undefined,
      airDate: formatAirDate(draft.releases?.[0]?.releaseDate),
      participantLine: draft.participants
         ?.map((p) => p.displayName)
         .filter(Boolean)
         .join(" · "),
      totalPicks: draft.totalPicks ?? undefined,
      typeDisplay: draftTypeFromNumber(draft.draftType),
   };
}

// Below lg the table's ~790px minimum (≈886px for admins) cannot fit, so the same
// rows render as stacked cards. Both share the WikiExportScope context, so a box
// ticked in one layout stays ticked after rotating into the other.
export function DraftsTable({ drafts, searchParams, isAdmin = false }: DraftsTableProps) {
   const currentSort = searchParams.sort as string | undefined;
   const currentDir = searchParams.dir as string | undefined;
   const rows = drafts.map(toRow);
   const selectableIds = Array.from(
      new Set(rows.map((r) => r.draftPublicId).filter((id): id is string => Boolean(id)))
   );
   const sortProps = { currentSort, currentDir, params: searchParams };

   return (
      <>
         <CardList rows={rows} isAdmin={isAdmin} selectableIds={selectableIds} sortProps={sortProps} />
         <WideTable rows={rows} isAdmin={isAdmin} selectableIds={selectableIds} sortProps={sortProps} />
      </>
   );
}

interface LayoutProps {
   rows: DraftRow[];
   isAdmin: boolean;
   selectableIds: string[];
   sortProps: Omit<SortableHeaderProps, "field" | "label" | "className">;
}

// ── Phones and tablets (< lg) ─────────────────────────────────────────────

function CardList({ rows, isAdmin, selectableIds, sortProps }: LayoutProps) {
   return (
      <div className="lg:hidden bg-white border-2 border-sd-ink border-t-0">
         {/* Sort strip stands in for the table's sortable headers. */}
         <div className="flex flex-wrap items-center gap-x-5 gap-y-1 bg-sd-ink text-white font-mono text-[10px] tracking-wide px-4 py-1">
            {isAdmin && (
               <label className="flex items-center gap-2 py-2 cursor-pointer text-white/60">
                  <WikiSelectAll ids={selectableIds} />
                  ALL
               </label>
            )}
            <span className="text-white/40">SORT</span>
            <SortableHeader field="episodenumber" label="EP. NO." {...sortProps} className="py-2" />
            <SortableHeader field="title" label="EPISODE" {...sortProps} className="py-2" />
            <SortableHeader field="date" label="AIR DATE" {...sortProps} className="py-2" />
         </div>

         {rows.length === 0 ? (
            <EmptyState />
         ) : (
            // Two columns on tablets; one on phones.
            <ul className="grid grid-cols-1 md:grid-cols-2">
               {rows.map((row) => (
                  <li
                     key={row.key}
                     className="group flex border-t border-sd-ink/10 md:odd:border-r hover:bg-sd-paper transition-colors duration-100"
                  >
                     {/* Checkbox outside the draft link so ticking it never navigates. The label widens the touch target. */}
                     {isAdmin && (
                        <label className="flex items-start pl-4 pr-1 pt-[18px] cursor-pointer">
                           {row.draftPublicId && <WikiSelectCheckbox id={row.draftPublicId} />}
                        </label>
                     )}

                     <Link href={row.href} className="flex-1 min-w-0 flex gap-3 px-4 py-4">
                        <span className="w-12 shrink-0 font-oswald font-bold text-[26px] text-sd-red leading-none">
                           {row.episodeNumber ?? "—"}
                        </span>
                        <span className="min-w-0 flex-1">
                           <span className="block font-sans font-semibold text-[16px] leading-snug text-sd-ink line-clamp-2 [overflow-wrap:anywhere]">
                              {row.label}
                           </span>
                           <span className="mt-2 flex flex-wrap items-center gap-x-3 gap-y-1.5">
                              {row.typeDisplay && <DraftTypeBadge type={row.typeDisplay} />}
                              <span className="font-mono text-[11px] text-sd-blue">{row.airDate}</span>
                              {row.totalPicks != null && (
                                 <span className="font-mono text-[11px] text-sd-ink/60">{row.totalPicks} PICKS</span>
                              )}
                           </span>
                           {row.participantLine && (
                              <span className="mt-1.5 block text-[13px] italic text-[#5a6075] truncate [overflow-wrap:anywhere]">
                                 {row.participantLine}
                              </span>
                           )}
                        </span>
                        <span aria-hidden="true" className="self-center text-sd-ink/30 group-hover:text-sd-red transition-colors">
                           ›
                        </span>
                     </Link>

                     {isAdmin && (
                        <Link
                           href={`/admin/drafts/${row.draftPublicId}/edit-meta`}
                           className="flex items-start pt-[18px] pr-4 pl-1 text-[11px] font-mono text-sd-ink/40 hover:text-sd-blue transition-colors uppercase tracking-wide"
                           title="Edit campaign & categories"
                        >
                           Edit
                        </Link>
                     )}
                  </li>
               ))}
            </ul>
         )}
      </div>
   );
}

// ── Desktop (lg+) — unchanged layout ──────────────────────────────────────

function WideTable({ rows, isAdmin, selectableIds, sortProps }: LayoutProps) {
   // When isAdmin, a 36px wiki-export checkbox column leads and the last column is split:
   // arrow (28px) + edit link (56px)
   const gridCols = isAdmin
      ? "36px 90px minmax(160px, 1.5fr) 130px minmax(160px, 1fr) 90px 130px 28px 56px"
      : "90px minmax(160px, 1.5fr) 130px minmax(160px, 1fr) 90px 130px 28px";

   return (
      <div className="hidden lg:block bg-white border-2 border-sd-ink border-t-0">
         {/* Header */}
         <div
            className="grid bg-sd-ink text-white font-mono text-[10px] tracking-wide"
            style={{ gridTemplateColumns: gridCols }}
         >
            {isAdmin && (
               <div className="px-2 py-3 flex items-center justify-center">
                  <WikiSelectAll ids={selectableIds} />
               </div>
            )}
            <div className="px-4 py-3">
               <SortableHeader field="episodenumber" label="EP. NO." {...sortProps} />
            </div>
            <div className="px-4 py-3">
               <SortableHeader field="title" label="EPISODE" {...sortProps} />
            </div>
            <div className="px-4 py-3 text-white/60">TYPE</div>
            <div className="px-4 py-3 text-white/60">DRAFTERS</div>
            <div className="px-4 py-3 text-right text-white/60">PICKS</div>
            <div className="px-4 py-3 text-right">
               <SortableHeader field="date" label="AIR DATE" {...sortProps} className="justify-end" />
            </div>
            <div className="px-2 py-3" />
            {isAdmin && <div className="px-2 py-3" />}
         </div>

         {/* Rows */}
         {rows.length === 0 ? (
            <EmptyState />
         ) : (
            rows.map((row) => (
               <div
                  key={row.key}
                  className="group grid border-t border-sd-ink/10 hover:bg-sd-paper transition-colors duration-100"
                  style={{ gridTemplateColumns: gridCols }}
               >
                  {/* Wiki export checkbox — outside the draft link so ticking it never navigates */}
                  {isAdmin && (
                     <div className="px-2 py-4 self-center flex items-center justify-center">
                        {row.draftPublicId && <WikiSelectCheckbox id={row.draftPublicId} />}
                     </div>
                  )}

                  {/* Clickable draft link — spans all columns except the checkbox and admin edit slots */}
                  <Link href={row.href} className="contents" aria-label={row.label}>
                     <div className="px-4 py-4 font-oswald font-bold text-[30px] text-sd-red leading-none self-center">
                        {row.episodeNumber ?? "—"}
                     </div>
                     <div className="px-4 py-4 self-center overflow-hidden">
                        <span className="block font-sans font-semibold text-[17px] text-sd-ink truncate">
                           {row.label}
                        </span>
                     </div>
                     <div className="px-4 py-4 self-center">
                        {row.typeDisplay ? (
                           <DraftTypeBadge type={row.typeDisplay} />
                        ) : (
                           <span className="text-sd-ink/40 text-sm">—</span>
                        )}
                     </div>
                     <div className="px-4 py-4 self-center overflow-hidden">
                        <span className="block text-[13px] italic text-[#5a6075] truncate">
                           {row.participantLine ?? "—"}
                        </span>
                     </div>
                     <div className="px-4 py-4 self-center text-right font-mono text-[14px] text-sd-ink">
                        {row.totalPicks ?? "—"}
                     </div>
                     <div className="px-4 py-4 self-center text-right font-mono text-[11px] text-sd-blue">
                        {row.airDate}
                     </div>
                     <div className="px-2 py-4 self-center text-sd-ink/30 group-hover:text-sd-red transition-colors text-center">
                        ›
                     </div>
                  </Link>

                  {/* Admin edit button — sits outside the draft link */}
                  {isAdmin && (
                     <div className="px-2 py-4 self-center text-center">
                        <Link
                           href={`/admin/drafts/${row.draftPublicId}/edit-meta`}
                           className="text-[11px] font-mono text-sd-ink/40 hover:text-sd-blue transition-colors uppercase tracking-wide"
                           title="Edit campaign & categories"
                        >
                           Edit
                        </Link>
                     </div>
                  )}
               </div>
            ))
         )}
      </div>
   );
}

function EmptyState() {
   return (
      <div className="px-4 py-8 text-center font-mono text-sm text-sd-ink/50">
         No drafts found.
      </div>
   );
}