import { DraftResponse } from "@/lib/dto";
import { inter, roboto } from "../../../styles/fonts";
import React from "react";
import Link from "next/link";
import { format } from "date-fns/format";
import { SortableTableHeader } from "../../../components/ui/sortable-table-header";

interface DraftsTableProps {
   drafts: DraftResponse[];
   sort?: string | undefined;
   dir: string | undefined;
}

export function DraftsTable({ drafts, sort, dir }: DraftsTableProps) {
   return (
      <div className="overflow-x-auto max-h-[900px]">
         <div className="table w-full border-collapse bg-[#fffdfd]">
            <div className={`${inter.className} text-center text-lg font-black table-header-group bg-slate-900 text-white sticky top-0`}>
               <div className="table-row">
                  <div className="table-cell align-middle px-4 py-2 font-medium">
                     <SortableTableHeader
                        field="episodeNumber"
                        label="No."
                        currentSort={sort}
                        dir={dir}
                        />
                     </div>
                  <div className="table-cell align-middle px-6 py-2 font-medium text-left">
                     <SortableTableHeader
                        field="title"
                        label="Title"
                        currentSort={sort}
                        dir={dir}
                     />
                  </div>
                  <div className="table-cell align-middle px-6 py-2 font-medium text-left">Drafters</div>
                  <div className="table-cell align-middle px-6 py-2 font-medium text-left">Hosts</div>
                  <div className="table-cell align-middle px-4 py-2 font-medium">
                     <SortableTableHeader
                        field="totalPicks"
                        label="# of Titles"
                        currentSort={sort}
                        dir={dir}
                     />
                  </div>
                  <div className="table-cell align-middle px-6 text-left py-2 font-medium">
                     <SortableTableHeader
                        field="date"
                        label="Date (s)"
                        currentSort={sort}
                        dir={dir}
                     />
                  </div>
               </div>
            </div>
            <div className={`${roboto.className} table-row-group bg-white`}>
               {drafts
                  .map((draft: DraftResponse) => (
                     <div key={draft.id} className="table-row border-2 hover:bg-gray-200">
                        <Link
                           href={`/main/drafts/${draft.id}`}
                           className="contents"
                        >
                           <div className="whitespace-nowrap align-middle table-cell py-3 pl-6 pr-3">{draft.episodeNumber || "N/A"}</div>
                           <div className="whitespace-nowrap align-middle table-cell py-3 pl-6 pr-3">{draft.title}</div>
                           <div className="whitespace-nowrap align-middle table-cell py-3 pl-6 pr-3">
                              <div className="flex flex-col items-start">
                                 {draft.drafters?.map((d, index) => (
                                    <span key={index} className="text-sm">{d.displayName}</span>
                                 ))}
                              </div>
                           </div>
                           <div className="whitespace-nowrap align-middle table-cell py-3 pl-6 pr-3">
                              <div className="flex flex-col items-start">
                                 {draft.hosts?.map((d, index) => (
                                    <span key={index} className="text-sm">{d.displayName}</span>
                                 ))}
                              </div>
                           </div>
                           <div className="whitespace-nowrap align-middle table-cell py-3 pl-6 pr-3">{draft.totalPicks}</div>
                           <div className="whitespace-nowrap table-cell py-3 pl-6 pr-6 align-middle">
                              {draft.releaseDates && draft.releaseDates.length > 0 ? (
                                 <div className="flex flex-col items-start">
                                    {draft.releaseDates.map((d, idx) => (
                                       <span key={idx}>{format(new Date(d.releaseDate), "MM/dd/yyyy")}</span>
                                    ))}
                                 </div>
                              ) : (
                                 <span className="text-gray-500">N/A</span>
                              )}
                           </div>
                        </Link>
                     </div>
                  ))}
            </div>
         </div>
      </div>
   );
}