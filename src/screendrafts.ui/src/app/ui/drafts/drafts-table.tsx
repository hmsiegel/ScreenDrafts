import { DraftResponse } from "@/app/lib/dto";
import { inter, roboto } from "../fonts";
import React from "react";
import Link from "next/link";
import { format } from "date-fns/format";

interface DraftsTableProps {
   drafts: DraftResponse[];
}


export function DraftsTable({ drafts }: DraftsTableProps) {
   return (
      <div className="overflow-x-auto max-h-[900px]">
         <div className="table w-full border-collapse bg-[#fffdfd]">
            <div className={`${inter.className} text-center text-lg font-black table-header-group bg-slate-900 text-white sticky top-0`}>
               <div className="table-row">
                  <div className="table-cell align-middle x-4 py-2 font-medium">No.</div>
                  <div className="table-cell align-middle x-4 py-2 font-medium">Title</div>
                  <div className="table-cell align-middle x-4 py-2 font-medium">Drafters</div>
                  <div className="table-cell align-middle x-4 py-2 font-medium">Hosts</div>
                  <div className="table-cell align-middle x-4 py-2 font-medium">No. of Titles</div>
                  <div className="table-cell align-middle x-4 py-2 font-medium">Date(s)</div>
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
                                    <span key={index} className="text-sm">{d.name}</span>
                                 ))}
                              </div>
                           </div>
                           <div className="whitespace-nowrap align-middle table-cell py-3 pl-6 pr-3">
                              <div className="flex flex-col items-start">
                                 {draft.hosts?.map((d, index) => (
                                    <span key={index} className="text-sm">{d.name}</span>
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