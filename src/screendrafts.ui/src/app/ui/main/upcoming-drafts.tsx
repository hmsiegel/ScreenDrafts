import { inter, roboto } from "@/app/ui/fonts";
import { DeleteDraft, EditDraft, PlayDraft, StartDraft } from "./buttons";
import Link from "next/link";
import { PlusIcon } from "@heroicons/react/24/outline";

export default function UpcomingDrafts() {
   return (
      <div className="bg-[#fffdfd] rounded-lg shadow-lg py-5 px-28 flex flex-col items-center justify-center my-4">
         <div className="flex items-center justify-center border-b-2 border-slate-900">
            <h1 className={`${inter.className} text-2xl text-black uppercase`}>
               Upcoming Drafts
            </h1>
         </div>
         <div className="flex items-center justify-center">
            <Link href="/main/drafts/create" className="flex items-center justify-center btn-red hover:bg-red-400 hover:text-black transition ease-out duration-500">
               create draft
               <PlusIcon className="w-5 ml-2" />
            </Link>
         </div>
         <table className="w-full text-gray-900">
            <thead className={`${inter.className} text-center text-lg font-black bg-slate-900 text-white`}>
               <tr>
                  <th className="px-4 py-2 font-medium">Date</th>
                  <th className="px-4 py-2 font-medium">Name</th>
                  <th className="px-4 py-2 font-medium">Role</th>
                  <th className="relative pl-6 p3-5 pr-3">
                     <span className="sr-only">Edit</span>
                  </th>
               </tr>
            </thead>
            <tbody className={`${roboto.className} bg-white`}>
               <tr className="w-full border-b py-3">
                  <td className="whitespace-nowrap py-3 pl-6 pr-3">TBD</td>
                  <td className="whitespace-nowrap py-3 pl-6 pr-3">Francis Ford Coppola Super</td>
                  <td className="whitespace-nowrap py-3 pl-6 pr-3">Commissioner</td>
                  <td className="whitespace-nowrap py-3 pl-6 pr-3">
                     <div className="flex justify-end gap-3">
                        <EditDraft id="1" />
                        <StartDraft id="1" />
                        <DeleteDraft id="1" />
                     </div>
                  </td>
               </tr>
               <tr className="w-full border-b py-3">
                  <td className="whitespace-nowrap py-3 pl-6 pr-3">TBD</td>
                  <td className="whitespace-nowrap py-3 pl-6 pr-3">American Zoetrope</td>
                  <td className="whitespace-nowrap py-3 pl-6 pr-3">Commissioner</td>
                  <td className="whitespace-nowrap py-3 pl-6 pr-3">
                     <div className="flex justify-end gap-3">
                        <EditDraft id="1" />
                        <StartDraft id="1" />
                        <DeleteDraft id="1" />
                     </div>
                  </td>
               </tr>
               <tr className="w-full border-b py-3">
                  <td className="whitespace-nowrap py-3 pl-6 pr-3">TBD</td>
                  <td className="whitespace-nowrap py-3 pl-6 pr-3">Vincente Minnelli</td>
                  <td className="whitespace-nowrap py-3 pl-6 pr-3">GM</td>
                  <td className="whitespace-nowrap py-3 pl-6 pr-3">
                     <div className="flex justify-end gap-3">
                        <PlayDraft id="1" />
                     </div>
                  </td>
               </tr>
            </tbody>
         </table>
      </div>
   )
}