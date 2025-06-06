import { inter, roboto } from "@/app/ui/fonts";
import Link from "next/link";
import React from "react";
import type { DraftResponse } from "@/app/lib/dto";
import { getLatestDrafts } from "@/app/lib/fetch-drafts";
import { format } from "date-fns/format";

export default async function MostRecentDrafts() {
   let latestDrafts: DraftResponse[] = [];
   let debug: { step: string; info: unknown } | null = null;

   try {
      debug = { step: "calling getLatestDrafts()", info: null };

      latestDrafts = await getLatestDrafts();

      debug = { step: "fetch OK", info: latestDrafts.length + " drafts" };
   } catch (error: any) {
      debug = { step: "catch()", info: String(error) };

      if (error?.response)
         debug.info = {
            url: error.response.url,
            status: error.response.status,
            body: await error.response.text().catch(() => "<body unreadable>"),
         };
   }

   if (debug?.step !== "fetch OK") {
      return(
         <pre style={{ whiteSpace: "pre-wrap", color: "#dc2626" }}>
            MostRecentDrafts Debug
            {JSON.stringify(debug, null, 2)}
            </pre>
      );
   }

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
               {latestDrafts.map((draft: DraftResponse) => {
                  return (
                     <React.Fragment key={draft.id}>
                        <Link
                           key={draft.id}
                           href={`/drafts/${draft.id}`}
                           className="table-row">
                           <div className="whitespace-nowrap table-cell table-data py-3 pl-6 pr-3">{draft.episodeNumber}</div>
                           <div className="whitespace-nowrap table-cell table-data py-3 pl-6 pr-3">{draft.title}</div>
                           <div className="whitespace-nowrap table-cell table-data text-right py-3 pl-6 pr-3">
                              {draft.rawReleaseDates?.[0]
                                 ? format(new Date(draft.rawReleaseDates[0]), "MM/dd/yyyy")
                                 : "N/A"}
                           </div>
                        </Link>
                     </React.Fragment>
                  );
               })}
            </div>
         </div>
      </div>
   )
}