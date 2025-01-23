import { inter, roboto } from "@/app/ui/fonts";
import { getLatestDrafts } from "@/app/lib/data";
import Link from "next/link";
import { Draft } from "@/app/lib/definitions";
import React from "react";

export default async function MostRecentDrafts() {
   const latestDrafts = await getLatestDrafts();
   return (
      <div className="bg-[#fffdfd] rounded-lg shadow-lg py-4 px-28 flex flex-col items-center justify-center my-4">
         <h1 className={`${inter.className} text-2xl text-black mb-5 uppercase border-b-2 border-slate-900 pb-2`}>
            Most Recent Drafts
         </h1>
         <div className="table w-max">
            <div className={`${inter.className} text-center text-lg font-black table-header-group bg-slate-900 text-white`}>
               <div className="table-row">
                  <div className="table-cell px-4 py-2 font-medium">No.</div>
                  <div className="table-cell px-4 py-2 font-medium">Episode</div>
                  <div className="table-cell px-4 py-2 font-medium">Date</div>
               </div>
            </div>
            <div className={`${roboto.className} table-row-group bg-white`}>
               {latestDrafts.map((draft: Draft) => {
                  return (
                     <React.Fragment key={draft.id}>
                        <Link
                           href={`/draft/${draft.episode}`}
                           className="table-row">
                           <div className="whitespace-nowrap table-cell table-data py-3 pl-6 pr-3">{draft.episode}.</div>
                           <div className="whitespace-nowrap table-cell table-data py-3 pl-6 pr-3">{draft.title}</div>
                           <div className="whitespace-nowrap table-cell table-data text-right py-3 pl-6 pr-3">{draft.draft_dates.toString()}</div>
                        </Link>
                     </React.Fragment>
                  );
               })}
            </div>
         </div>
      </div>
   )
}