import { DrafterResponse } from "@/lib/dto";

export function DrafterDetails({ drafter }: { drafter: DrafterResponse }) {
   return (
      <div>
         <h2 className="text-xl font-bold">{drafter.displayName}</h2>
         <p className="text-gray-600">{drafter.bio}</p>
      </div>
   );
}
