import { DraftResponse } from "@/lib/dto";
import EpisodeInfoCard from "./episode-info-card";
import PicksList from "./picks-list";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import rehypeSanitize from "rehype-sanitize";

export function DraftDetails({ draft, drafterMap }: {
   draft: DraftResponse;
   drafterMap: Map<string, string>;
}) {
   return (
      <div className="mx-auto w-full max-w-3xl">
         <div className="grid gap-16 md:grid-cols-[18rem_minmax(0,38rem)]">
            <EpisodeInfoCard draft={draft} />
            <div className="max-w-prose">
               <article className="prose prose-slate mb-4">
                  <ReactMarkdown
                     remarkPlugins={[remarkGfm]}
                     rehypePlugins={[rehypeSanitize]}
                     components={{
                        a: ({ node, ...props }) => (
                           <a
                              {...props}
                              className="underline text-blue-600 hover:text-blue-800"
                           />
                        ),
                     }}>
                     {draft.description ?? "Draft description coming soon!"}
                  </ReactMarkdown>
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