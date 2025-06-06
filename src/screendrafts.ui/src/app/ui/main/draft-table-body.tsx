'use client'

import { deleteDraft, editDraft, playDraft, startDraft } from "@/app/lib/api";
import { DraftResponse } from "@/app/lib/dto";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { inter, roboto } from "../fonts";
import { DeleteDraft, EditDraft, PlayDraft, StartDraft } from "./buttons";

interface Props {
   drafts: DraftResponse[];
}

export function UpcomingDraftsTableBody({ drafts }: Props) {
   const qc = useQueryClient();

   const start = useMutation({
      mutationFn: (id: string) => startDraft(id),
      onSuccess: () => {
         qc.invalidateQueries({ queryKey: ["upcomingDrafts"] });
      },
   });

   return (
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
               {drafts
                  .filter((d: DraftResponse) => typeof d.id === "string")
                  .map((d: DraftResponse) => (
                  <tr key={d.id} className="w-full border-b py-3">
                     <td className="whitespace-nowrap py-3 pl-6 pr-3">
                        {d.rawReleaseDates?.[0] ? new Date(d.rawReleaseDates[0]).toLocaleDateString() : "TBD"}
                     </td>
                  <td className="whitespace-nowrap py-1 pl-6 pr-3">{d.title}</td>
                  <td className="whitespace-nowrap py-1 pl-6 pr-3">Commissioner</td>
                  <td className="whitespace-nowrap py-1 pl-6 pr-3">
                     <div className="flex justify-end gap-3">
                        <EditDraft id={d.id as string} onClick={async () => { await editDraft(d.id as string); }} />
                        {d.draftStatus === 0 && (
                           <StartDraft
                              id={d.id as string}
                              onClick={() => start.mutate(d.id as string)}
                           />
                        )}
                        {d.draftStatus === 2 && (
                           <PlayDraft id={d.id as string} onClick={() => playDraft(d.id as string)}/>
                        )}
                        <DeleteDraft id={d.id as string} onClick={() => deleteDraft(d.id as string)}/>
                     </div>
                  </td>
               </tr>
               ))}
            </tbody>
         </table>
   );
}