import { CommissionerOverrideResponse, DraftPickResponse, VetoOverrideResponse, VetoResponse } from "@/app/lib/dto";
import Link from "next/link";

export function PickRow({
   pick,
   veto,
   vetoOverride,
   commissionerOverride,
   drafterMap,
}: {
   pick: DraftPickResponse;
   veto?: VetoResponse;
   vetoOverride?: VetoOverrideResponse;
   commissionerOverride?: CommissionerOverrideResponse;
   drafterMap: Map<string, string>;
}) {
   const getName = (id: string) => drafterMap.get(id) ?? "Unknown";

   const base = "leading-snug flex flex-wrap items-start gap-x-1 " +
      "whitespace-normal break-words";

   // Base line for the row
   const line = (
      <>
         <span className="font-semibold">{pick.position}.</span>{" "}
         <MovieLink href={`/main/movies/${pick.movieId}`}>
            {pick.movieTitle ? pick.movieTitle : "N/A"}
         </MovieLink>{" by "}
         <DrafterLink href={`/main/drafters/${pick.drafterId}`}>
            {getName(pick.drafterId ?? "")}
         </DrafterLink>{" "}
      </>
   );

   // Commissioner override removes the pick
   if (commissionerOverride) {
      return (
         <li className="text-red-600 line-through">
            {line}{" "} <span className="font-medium italic">(removed by Commissioner Override)</span>
         </li>
      );
   }

   // Veto
   if (veto && !vetoOverride) {
      return (
         <li className="line-through">
            {line}{" "}
            <DrafterVetoLink href={`/main/drafters/${veto.drafterId}`}>
               (vetoed by &nbsp;{getName(veto.drafterId ?? "")})
            </DrafterVetoLink>
         </li>
      );
   }

   // Veto followed by an override
   if (veto && vetoOverride) {
      return (
         <li >
            {line}{" "}
            <span className="font-medium italic line-through">
               (vetoed by &nbsp;{getName(veto.drafterId ?? "")})
            </span>
            <span className="font-medium italic">
               (veto overridden by &nbsp;{getName(vetoOverride.drafterId ?? "")})
            </span>
         </li>
      );
   }

   return <li className={base}>{line}</li>
}

function DrafterLink({ href, children }: { href: string; children: React.ReactNode }) {
   return (
      <Link href={href} className="font-medium text-sd-blue hover:underline">
         {children}
      </Link>
   );
}

function DrafterVetoLink({ href, children }: { href: string; children: React.ReactNode }) {
   return (
      <Link href={href} className="font-medium italic text-sd-red hover:underline">
         {children}
      </Link>
   );
}

function MovieLink({ href, children }: { href: string; children: React.ReactNode }) {
   return (
      <Link href={href} className="text-sd-blue hover:underline">
         {children}
      </Link>
   );
}