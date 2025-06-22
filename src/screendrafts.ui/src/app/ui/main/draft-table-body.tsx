'use client'

import { deleteDraft, editDraft, playDraft, startDraft } from "@/app/lib/api";
import { UpcomingDraftDto } from "@/app/lib/dto";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { inter, roboto } from "../fonts";
import { DeleteDraft, EditDraft, PlayDraft, StartDraft } from "./buttons";

interface Props {
   drafts: UpcomingDraftDto[];
}

export function UpcomingDraftsTableBody({ drafts }: Props) {
   return (
         <table className="w-full text-gray-900">
            <thead className={`${inter.className} text-center text-lg font-black bg-slate-900 text-white`}>
               <tr>
                  <th className="px-4 py-2 font-medium">Date</th>
                  <th className="px-4 py-2 font-medium">Title</th>
                  {drafts.some(d => d.capabilities?.role) && (
                     <th className="px-4 py-2 font-medium">Role</th>
                  )}
                  <th className="relative pl-6 p3-5 pr-3">
                     <span className="sr-only">Actions</span>
                  </th>
               </tr>
            </thead>
            <tbody className={`${roboto.className} bg-white`}>
               {drafts.map(d => (
                  <UpcomingDraftRow key={d.id} draft={d} />
               ))}
            </tbody>
         </table>
   );
}

function UpcomingDraftRow({ draft }: { draft: UpcomingDraftDto }) {
   const { role, canEdit, canDelete, canStart, canPlay } = draft.capabilities ?? {};

   const qc = useQueryClient();

   const start = useMutation({
      mutationFn: (id: string) => startDraft(id),
      onSuccess: () => {
         qc.invalidateQueries({ queryKey: ["upcomingDrafts"] });
      },
   });

   return (
      <tr className="w-full border-b py-3">
         {/* Date */}
         <td className="whitespace-nowrap py-3 pl-6 pr-3">
            {draft.rawReleaseDates?.[0]
               ? new Date(draft.rawReleaseDates[0]).toLocaleDateString()
               : "TBD"}
         </td>

         {/* Title */}
         <td className="whitespace-nowrap py-1 pl-6 pr-3">{draft.title}</td>

         {/* Role */}
         {role && (
            <td className="whitespace-nowrap py-1 pl-6 pr-3">{role}</td>
         )}

         {/* Actions Buttons (right-aligned)*/}
         <td className="whitespace-nowrap py-1 pl-6 pr-3">
            <div className="flex justify-end gap-3">
               {/* Edit Draft Button */}
               {canEdit && (
                  <EditDraft
                     id={draft.id as string}
                     onClick={async () => { await editDraft(draft.id as string); }}
                  />
               )}
               {/* Start Draft Button */}
               {canStart && draft.draftStatus === 0 && (
                  <StartDraft
                     id={draft.id as string}
                     onClick={() => start.mutate(draft.id as string)}
                  />
               )}

               {/* Play Draft Button */}
               {canPlay && draft.draftStatus === 2 && (
                  <PlayDraft
                     id={draft.id as string}
                     onClick={() => playDraft(draft.id as string)}
                  />
               )}

               {/* Delete Draft Button */}
               {canDelete && (
                  <DeleteDraft
                     id={draft.id as string}
                     onClick={() => deleteDraft(draft.id as string)}
                  />
               )}
            </div>
         </td>
      </tr>
   );
}