import { DraftResponse } from "@/lib/dto";
import { format } from "date-fns/format";
import Link from "next/link";
import Image from "next/image";

export default function EpisodeInfoCard({ draft }: { draft: DraftResponse }) {
   return (
      <aside className="w-80 border rounded-xl shadow-sm p-4 bg-slate-50 sticky top-4 self-start">
         <div className="bg-sd-red rounded-lg">
            <h2 className="text-white py-2 text-2xl uppercase font-bold mb-3 col-span-2 text-center">{draft.title}</h2>
         </div>

        <Image
          className="mb-5"
          src="/screen-drafts.jpg"
          alt="ScreenDrafts logo"
          height={400}
          width={400}
        />

         <dl className="grid grid-cols-2 gap-x-4 gap-y-2">
            <Info label="Episode #">{draft.episodeNumber ?? "-"}</Info>
            <hr className="col-span-2 border-t border-slate-300" />
            <Info label="Date">
               {draft.releaseDates && draft.releaseDates.length > 0 ? (
                  <div className="flex flex-col">
                     {draft.releaseDates.map((date, index) => (
                        <span key={index} className="text-sm text-slate-800">
                           {format(new Date(date.releaseDate), "MMMM dd, yyyy")}
                        </span>
                     ))}
                  </div>
               ) : (
                  <span className="text-sm text-slate-800">N/A</span>
               )}
            </Info>
            <hr className="col-span-2 border-t border-slate-300" />
            <Info label="Drafters">
               {draft.drafters && draft.drafters.length > 0 ? (
                  <div className="flex flex-col">
                     {draft.drafters.map((d, index) => (
                        <DrafterLink key={index} href={`/dashboard/drafters/${d.id}`}>
                           {d.displayName}
                        </DrafterLink>
                     ))}
                  </div>
               ) : (
                  <span className="text-sm text-slate-800">N/A</span>
               )}
            </Info>
            <hr className="col-span-2 border-t border-slate-300" />
            <Info label="Hosts">
               {draft.hosts && draft.hosts.length > 0 ? (
                  <div className="flex flex-col">
                     {draft.hosts.map((h, index) => (
                        <HostLink key={index} href={`/dashboard/hosts/${h.id}`}>
                           {h.displayName}
                        </HostLink>
                     ))}
                  </div>
               ) : (
                  <span className="text-sm text-slate-800">N/A</span>
               )}
            </Info>
            <div className="col-span-2 bg-sd-red text-xl font-bold py-2 text-white rounded-xl text-center">Episode Navigation</div>
            {(draft.previousDraftId || draft.nextDraftId) && (
               <>
                  {/* Both links are present, so we can show them */}
                  {draft.previousDraftId && draft.nextDraftId && (
                     <div className="col-span-2 grid grid-cols-2 pt-2">
                        <div className="pr-2">
                           <NavItem
                              label="Previous"
                              href={`/dashboard/drafts/${draft.previousDraftId}`} >
                              {draft.previousDraftTitle ?? "View"}
                           </NavItem>
                        </div>
                        <div className="pl-2 border-l border-slate-500">
                           <NavItem label="Next"
                              href={`/dashboard/drafts/${draft.nextDraftId}`}>
                              {draft.nextDraftTitle ?? ""}
                           </NavItem>
                        </div>
                     </div>
                  )}
                  {/* Only previous link is present - center it */}
                  {draft.previousDraftId && !draft.nextDraftId && (
                     <div className="col-span-2 flex justify-center pt-2">
                        <NavItem
                           label="Previous"
                           href={`/dashboard/drafts/${draft.previousDraftId}`} >
                           {draft.previousDraftTitle ?? "View"}
                        </NavItem>
                     </div>
                  )}
                  {/* Only next link is present - center it */}
                  {draft.nextDraftId && !draft.previousDraftId && (
                     <div className="col-span-2 flex justify-center pt-2">
                        <NavItem
                           label="Next"
                           href={`/dashboard/drafts/${draft.nextDraftId}`} >
                           {draft.nextDraftTitle ?? "View"}
                        </NavItem>
                     </div>
                  )}
               </>
            )}
         </dl>
      </aside>
   );
}

function Info({ label, children }: { label: string; children: React.ReactNode }) {
   return (
      <>
         <dt className="font-semibold text-sm text-slate-600">{label}:</dt>
         <dd className="text-sm text-slate-800 mb-2">{children}</dd>
      </>
   );
}

function NavItem({ label, href, children }: { label: string; href: string; children?: React.ReactNode }) {
   return (
      <div className="flex flex-col text-center">
         <span className="text-sm text-slate-600 font-semibold tracking-wide mb-0.5">{label}</span>
         <Link href={href} className="text-sd-red hover:underline text-sm">
            {children || "N/A"}
         </Link>
      </div>
   );
}

function DrafterLink({ href, children }: { href: string; children: React.ReactNode }) {
   return (
      <Link href={href} className="text-sm text-sd-blue hover:underline">
         {children}
      </Link>
   );
}
function HostLink({ href, children }: { href: string; children: React.ReactNode }) {
   return (
      <Link href={href} className="text-sm text-sd-blue hover:underline">
         {children}
      </Link>
   );
}