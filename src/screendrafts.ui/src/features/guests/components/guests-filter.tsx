'use client';

import { useRouter, useSearchParams } from "next/navigation";
import { useEffect, useState } from "react";

export default function GuestsFilter() {
   const router = useRouter();
   const params = useSearchParams();

   const [search, setSearch] = useState(params.get("q") ?? "");
   const [open, setOpen] = useState(false);

   const apply = () => {
      const qs = new URLSearchParams();
      if (search) qs.set("q", search);
      router.push(`?${qs.toString()}`);
   };


   return (
      <div className="mb-6">
         {/* toggle button */}
         <button
            onClick={() => setOpen(o => !o)}
            className="btn btn-outline mb-2"
         >
            {open ? "Hide Filters ▲" : "Show Filters ▼"}
         </button>

         {/* collapsible panel */}
         {open && (
            <>
               <div className="grid md:grid-cols-3 gap-6 border border-slate-300 bg-slate-50 p-4 rounded-lg">
                  {/* search input */}
                  <div className="flex flex-col col-span-3">
                     <label className="text-sm">Search</label>
                     <input
                        type="text"
                        value={search}
                        onChange={e => setSearch(e.target.value)}
                        className="input input-bordered w-full"
                        placeholder="Search by name"
                     />
                  </div>
                  {/* apply button */}
                  <div className="col-span-3 flex justify-center">
                     <button onClick={apply} className="btn btn-primary bg-sd-blue items-center rounded-lg text-lg px-4 text-white h-10">
                        Apply
                     </button>
                  </div>
               </div>
            </>
         )}
      </div>
   );
}