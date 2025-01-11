import { inter, roboto } from "@/app/ui/fonts";
import Link from "next/link";

export default function MostRecentDrafts() {
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
               <Link href="/draft/307" className="table-row w-full border-b py-3">
                     <div className="whitespace-nowrap table-cell table-data py-3 pl-6 pr-3">307.</div>
                     <div className="whitespace-nowrap table-cell table-data py-3 pl-6 pr-3">1999 Mini-Mega</div>
                     <div className="whitespace-nowrap table-cell table-data text-right py-3 pl-6 pr-3">12/30/2024</div>
               </Link>
               <Link href="/draft/306" className="table-row">
                  <div className="table-cell table-data py-3 pl-6 pr-3">306.</div>
                  <div className="table-cell table-data py-3 pl-6 pr-3">Whit Stillman Mini-Super</div>
                  <div className="table-cell table-data text-right py-3 pl-6 pr-3">12/24/2024</div>
               </Link>
               <Link href="/draft/305" className="table-row">
                  <div className="table-cell table-data py-3 pl-6 pr-3">305.</div>
                  <div className="table-cell table-data py-3 pl-6 pr-3">Charlie Kaufman Super</div>
                  <div className="table-cell table-data text-right py-3 pl-6 pr-3">12/9/2024</div>
               </Link>
               <Link href="/draft/304" className="table-row">
                  <div className="table-cell table-data">304.</div>
                  <div className="table-cell table-data">Holiday Horror</div>
                  <div className="table-cell table-data text-right">12/2/2024</div>
               </Link>
               <Link href="/draft/303" className="table-row">
                  <div className="table-cell table-data">303.</div>
                  <div className="table-cell table-data">Francois Truffaut Super</div>
                  <div className="table-cell table-data text-right">11/4/2024</div>
               </Link>
            </div>
         </div>
      </div>
   )
}