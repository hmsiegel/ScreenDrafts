import { DraftHistoryItem, VetoHistoryItem } from "@/lib/dto";

export default function DraftHistory({
   drafts,
   vetoes,
}: {
   drafts: DraftHistoryItem[];
   vetoes: VetoHistoryItem[];
}) {
   return (
      <div>
         <h2 className="text-2xl font-bold mb-4">Draft History</h2>
         <ul className="space-y-2">
            {drafts.map((draft) => (
               <li key={draft.id} className="border-b py-2">
                  <h3 className="font-semibold">{draft.title}</h3>
                  <p className="text-sm text-slate-600">{draft.description}</p>
               </li>
            ))}
         </ul>
         <h2 className="text-2xl font-bold mb-4">Veto History</h2>
         <ul className="space-y-2">
            {vetoes.map((veto) => (
               <li key={veto.id} className="border-b py-2">
                  <h3 className="font-semibold">{veto.title}</h3>
                  <p className="text-sm text-slate-600">{veto.description}</p>
               </li>
            ))}
         </ul>
      </div>
   );
}