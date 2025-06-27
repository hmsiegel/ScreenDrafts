import { CommissionerOverrideResponse, DraftPickResponse, VetoOverrideResponse, VetoResponse } from "@/app/lib/dto";
import { blessingsMap } from "@/app/lib/utils";
import { PickRow } from "./pick-row";

export default function PicksList({
   picks,
   vetoes,
   vetoesOverrides,
   commissionerOverride,
   drafterMap,
}: {
   picks: DraftPickResponse[];
   vetoes: VetoResponse[];
   vetoesOverrides: VetoOverrideResponse[];
   commissionerOverride: CommissionerOverrideResponse[];
   drafterMap?: Map<string, string>;
}) {
   const { vetoBySlot, vetoOverrideBySlot, commissionerOverrideBySlot } = blessingsMap(
      vetoes,
      vetoesOverrides,
      commissionerOverride
   );

   const rows = [...picks].sort((a, b) => (a.playOrder ?? 0) - (b.playOrder ?? 0));

   return(
      <ul className="space-y-3">
         {rows.map(p => (
            <PickRow
               key={p.playOrder}
               pick={p}
               veto={p.playOrder !== undefined ? vetoBySlot.get(p.playOrder) : undefined}
               vetoOverride={p.playOrder !== undefined ? vetoOverrideBySlot.get(p.playOrder) : undefined}
               commissionerOverride={p.playOrder !== undefined ? commissionerOverrideBySlot.get(p.playOrder) : undefined}
               drafterMap={drafterMap ?? new Map()}
            />
         ))}
      </ul>
   );
}