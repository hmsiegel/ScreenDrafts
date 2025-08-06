import { DrafterProfileResponse } from "@/lib/dto";
import { format } from "date-fns/format";
import Image from "next/image";
import Link from "next/link";

export default function DrafterInfoCard({ drafter }: { drafter: DrafterProfileResponse }) {
   return (
      <aside className="w-80 border rounded-xl shadow-sm p-4 bg-slate-50 sticky top-4 self-start">
         <div className="bg-sd-red rounded-lg">
            <h2 className="text-white py-2 text-2xl uppercase font-bold mb-3 col-span-2 text-center">{drafter.displayName}</h2>
         </div>
         <Image
            className="mb-5 rounded-full"
            src={`/drafters/${drafter.socialHandles?.profilePicturePath ?? "drafters/default-profile.png"}`}
            alt={`${drafter.displayName}'s profile picture`}
            height={400}
            width={400}
         />
         <dl className="grid grid-cols-2 gap-x-4 gap-y-2">
            <Info label="Drafts">{drafter.totalDrafts}</Info>
            <hr className="col-span-2 border-t border-slate-300" />
            <NavItem
               label="First Draft"
               href={`/dashboard/drafts/${drafter.firstDraft?.draftId}`}>{drafter.firstDraft?.title ?? "N/A"}
            </NavItem>
            <Info label="First Draft Date">
               {Array.isArray(drafter.firstDraft?.draftDates)
                  ? drafter.firstDraft.draftDates.map(date => {
                     return format(date instanceof Date ? date : new Date(String(date)), "MMMM dd, yyyy");
                  }).join(", ")
                  : drafter.firstDraft?.draftDates ?? "N/A"}
            </Info>
            <hr className="col-span-2 border-t border-slate-300" />
            <NavItem
               label="Most Recent Draft"
               href={`/dashboard/drafts/${drafter.mostRecentDraft?.draftId}`}>{drafter.mostRecentDraft?.title ?? "N/A"}
            </NavItem>
            <Info label="First Draft Date">
               {Array.isArray(drafter.mostRecentDraft?.draftDates)
                  ? drafter.mostRecentDraft.draftDates.map(date => {
                     return format(date instanceof Date ? date : new Date(String(date)), "MMMM dd, yyyy");
                  }).join(", ")
                  : drafter.mostRecentDraft?.draftDates ?? "N/A"}
            </Info>
            <hr className="col-span-2 border-t border-slate-300" />
            <Info label="Films Drafted">{drafter.filmsDrafted}</Info>
            <hr className="col-span-2 border-t border-slate-300" />
            <Info label="Picks Vetoed">{drafter.timesVetoed}</Info>
            <hr className="col-span-2 border-t border-slate-300" />
            <Info label="Vetoes Deployed">{drafter.vetoesUsed}</Info>
            <hr className="col-span-2 border-t border-slate-300" />
            <Info label="Veto Overrides Used">{drafter.vetoOverridesUsed}</Info>
            <hr className="col-span-2 border-t border-slate-300" />
            <Info label="Times Veto Overrides Against">{drafter.timesVetoOverridesAgainst}</Info>
            <hr className="col-span-2 border-t border-slate-300" />
            <Info label="Commissioner Overrides">{drafter.commissionerOverrides}</Info>
            <hr className="col-span-2 border-t border-slate-300" />
            <Info label="Active Rollover Veto">{drafter.hasRolloverVeto ? "Yes" : "No"}</Info>
            <hr className="col-span-2 border-t border-slate-300" />
            <Info label="Active Rollover Veto Override">{drafter.hasRolloverVetoOverride ? "Yes" : "No"}</Info>
            <hr className="col-span-2 border-t border-slate-300" />
         </dl>
         <div className="bg-sd-red rounded-lg">
            <h2 className="text-white py-2 text-2xl uppercase font-bold mb-3 col-span-2 text-center">Social Media Handles</h2>
         </div>
      </aside>
   );
}

function Info({ label, children }: { label: string; children: React.ReactNode }) {
   return (
      <>
         <dt className="text-sm font-medium text-slate-500">{label}</dt>
         <dd className="text-sm text-slate-800">{children}</dd>
      </>
   );
}

function NavItem({ label, href, children }: { label: string; href: string; children: React.ReactNode }) {
   return (
      <>
         <dt className="text-sm font-medium text-slate-500">{label}</dt>
         <Link href={href} className="text-sm font-medium text-slate-800 hover:underline">
            {children}
         </Link>
      </>
   );
}