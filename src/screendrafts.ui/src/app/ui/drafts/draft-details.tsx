import { DraftResponse } from "@/app/lib/dto";
import EpisodeInfoCard from "./episode-info-card";
import PicksList from "./picks-list";

export function DraftDetails({ draft, drafterMap }: {
   draft: DraftResponse;
   drafterMap: Map<string, string>;
}) {
   return (
      <div className="mx-auto w-full max-w-3xl">
         <div className="grid gap-16 md:grid-cols-[18rem_minmax(0,38rem)]">
            <EpisodeInfoCard draft={draft} />
            <div className="max-w-prose">
               <article className="prose prose-slate">
                  {draft.description ? (
                     <p>{draft.description}</p>
                  ) : (
                     <p className="text-slate-800">No description available for this draft.</p>
                  )}
               </article>
               <h2 className="text-2xl font-bold mb-4">The following films were drafted:</h2>
               <PicksList
                  picks={draft.draftPicks ?? []}
                  vetoes={draft.vetoes ?? []}
                  vetoesOverrides={draft.vetoOverrides ?? []}
                  commissionerOverride={draft.commissionerOverrides ?? []}
                  drafterMap={drafterMap}
               />
            </div>
         </div>
      </div>
   );
}