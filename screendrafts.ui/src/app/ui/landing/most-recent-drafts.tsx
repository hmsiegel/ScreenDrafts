import { inter, roboto } from "@/app/ui/fonts";
import Link from "next/link";

export default function MostRecentDrafts() {
   return (
      <div className="bg-[#fffdfd] rounded-lg shadow-lg py-5 px-28 flex flex-col items-center justify-center my-8">
         <h1 className={`${inter.className} text-4xl text-black mb-5 uppercase border-b-2 border-slate-900 pb-2`}>
            Most Recent Drafts
         </h1>
         <div className="table w-full">
            <div className={`${inter.className} text-center text-2xl table-header-group`}>
               <div className="bg-slate-900 text-white font-black table-row">
                  <div className="table-cell">No.</div>
                  <div className="table-cell">Episode</div>
                  <div className="table-cell">Date</div>
               </div>
            </div>
            <div className={`${roboto.className} text-center table-row-group`}>
               <Link href="/draft/307" className="table-row">
                     <div className="table-cell table-data">307.</div>
                     <div className="table-cell table-data">1999 Mini-Mega</div>
                     <div className="table-cell table-data text-right">12/30/2024</div>
               </Link>
               <Link href="/draft/306" className="table-row">
                  <div className="table-cell table-data">306.</div>
                  <div className="table-cell table-data">Whit Stillman Mini-Super</div>
                  <div className="table-cell table-data text-right">12/24/2024</div>
               </Link>
               <Link href="/draft/305" className="table-row">
                  <div className="table-cell table-data">305.</div>
                  <div className="table-cell table-data">Charlie Kaufman Super</div>
                  <div className="table-cell table-data text-right">12/9/2024</div>
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