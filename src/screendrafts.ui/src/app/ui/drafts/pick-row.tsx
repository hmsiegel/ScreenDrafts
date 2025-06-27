import { CommissionerOverrideResponse, DraftPickResponse, VetoOverrideResponse, VetoResponse } from "@/app/lib/dto";

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
         {pick.movieTitle || "N/A"}{" "}
         <span className="text-slate-600">by {pick.drafterName ?? "-"}</span>
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
            {line}{" "} <span className="font-medium italic text-red-600">(vetoed by &nbsp;{getName(veto.drafterId ?? "")})</span>
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
            </span>{", "}
            <span className="font-medium italic">
               (veto overridden by &nbsp;{getName(vetoOverride.drafterId ?? "")})
            </span>
         </li>
      );
   }

   return <li className={base}>{line}</li>
}