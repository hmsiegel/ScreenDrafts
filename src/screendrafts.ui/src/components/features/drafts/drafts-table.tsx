import DraftTypeBadge from "@/components/ui/draft-type-badge";
import { draftTypeFromNumber } from "@/lib/draft-type-display";
import { ListDraftsResponse } from "@/lib/dto";
import { format } from "date-fns/format";
import Link from "next/link";

interface DraftsTableProps {
   drafts: ListDraftsResponse[];
   searchParams: Record<string, string | string[] | undefined>;
}

function formatAirDate(raw: Date | string | undefined): string {
   if (!raw) return "—";
   try {
      return format(new Date(raw), "MMM dd, yyyy").toUpperCase();
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

export function DraftsTable({ drafts, searchParams }: DraftsTableProps) {
   const currentSort = searchParams.sort as string | undefined;
   const currentDir = searchParams.dir as string | undefined;

   return (
      <div className="bg-white border-2 border-sd-ink border-t-0">
         {/* Header */}
         <div
            className="grid bg-sd-ink text-white font-mono text-[10px] tracking-wide"
            style={{ gridTemplateColumns: "90px minmax(160px, 1.5fr) 130px minmax(160px, 1fr) 90px 130px 28px" }}
         >
            <div className="px-4 py-3">
               <SortableHeader
                  field="episodenumber"
                  label="EP. NO."
                  currentSort={currentSort}
                  currentDir={currentDir}
                  params={searchParams}
               />
            </div>
            <div className="px-4 py-3">
               <SortableHeader
                  field="title"
                  label="EPISODE"
                  currentSort={currentSort}
                  currentDir={currentDir}
                  params={searchParams}
               />
            </div>
            <div className="px-4 py-3 text-white/60">TYPE</div>
            <div className="px-4 py-3 text-white/60">DRAFTERS</div>
            <div className="px-4 py-3 text-right text-white/60">PICKS</div>
            <div className="px-4 py-3 text-right">
               <SortableHeader
                  field="date"
                  label="AIR DATE"
                  currentSort={currentSort}
                  currentDir={currentDir}
                  params={searchParams}
                  className="justify-end"
               />
            </div>
            <div className="px-2 py-3" />
         </div>

         {/* Rows */}
         {drafts.length === 0 ? (
            <div className="px-4 py-8 text-center font-mono text-sm text-sd-ink/50">
               No drafts found.
            </div>
         ) : (
            drafts.map((draft) => {
               const episodeNumber = draft.releases?.[0]?.episodeNumber;
               const releaseDate = draft.releases?.[0]?.releaseDate;
               const participantLine = draft.participants
                  ?.map((p) => p.displayName)
                  .filter(Boolean)
                  .join(" · ");
               const draftTypeDisplay = draftTypeFromNumber(draft.draftType);

               return (
                  <Link
                     key={draft.draftPartPublicId ?? draft.draftPublicId}
                     href={`/drafts/${draft.draftPublicId}`}
                     className="group grid border-t border-sd-ink/10 hover:bg-sd-paper transition-colors duration-100 cursor-pointer"
                     style={{ gridTemplateColumns: "90px minmax(160px, 1.5fr) 130px minmax(160px, 1fr) 90px 130px 28px" }}
                  >
                     <div className="px-4 py-4 font-oswald font-bold text-[30px] text-sd-red leading-none self-center">
                        {episodeNumber ?? "—"}
                     </div>
                     <div className="px-4 py-4 self-center overflow-hidden">
                        <span className="block font-sans font-semibold text-[17px] text-sd-ink truncate">
                           {draft.label ?? "—"}
                        </span>
                     </div>
                     <div className="px-4 py-4 self-center">
                        {draftTypeDisplay ? <DraftTypeBadge type={draftTypeDisplay} /> : <span className="text-sd-ink/40 text-sm">—</span>}
                     </div>
                     <div className="px-4 py-4 self-center overflow-hidden">
                        <span className="block text-[13px] italic text-[#5a6075] truncate">
                           {participantLine ?? "—"}
                        </span>
                     </div>
                     <div className="px-4 py-4 self-center text-right font-mono text-[14px] text-sd-ink">
                        {draft.totalPicks ?? "—"}
                     </div>
                     <div className="px-4 py-4 self-center text-right font-mono text-[11px] text-sd-blue">
                        {formatAirDate(releaseDate)}
                     </div>
                     <div className="px-2 py-4 self-center text-sd-ink/30 group-hover:text-sd-red transition-colors text-center">
                        ›
                     </div>
                  </Link>
               );
            })
         )}
      </div>
   );
}