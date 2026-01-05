import { DrafterProfileResponse } from "@/lib/dto";
import ReactMarkdown from "react-markdown";
import rehypeSanitize from "rehype-sanitize";
import remarkGfm from "remark-gfm";
import DrafterInfoCard from "./drafter-info-card";
import DraftHistory from "./draft-history";

export function DrafterProfile({ drafter }: { drafter: DrafterProfileResponse }) {
   return (
      <div className="mx-auto w-full max-w-3xl">
         <div className="grid gap-16 md:grid-cols-[18rem_minmax(0,38rem)]">
            <DrafterInfoCard drafter={drafter} />
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
                     }}
                  >
                     {drafter.bio ?? "Drafter bio coming soon!"}
                  </ReactMarkdown>
               </article>
               <h2 className="text-3xl font-bold mb-4">Draft History</h2>
               <DraftHistory
                  drafts={drafter.draftHistory ?? []}
                  vetoes={drafter.vetoHistory ?? []}
               />
            </div>
         </div>
      </div>
   );
}
