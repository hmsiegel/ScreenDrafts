'use client';

import { PersonResponse } from "@/lib/dto";
import { env } from "@/lib/env";
import { useEffect, useState } from "react";

export default function CommissionerDropdown({ commissionerNumber }: { commissionerNumber: number }) {
   const [commissioners, setCommissioners] = useState<PersonResponse[]>([]);

   useEffect(() => {
      async function fetchCommissioners() {
         try {
            const response = await fetch(`${env.apiUrl}/people`);
            if (response.ok) {
               const data = await response.json() as PersonResponse[];
               setCommissioners(data);
            }
         } catch (error) {
            console.error('Error fetching commissioners:', error);
         }
      }
      fetchCommissioners();
   }, []);

   return (
      <div className="mb-4">
         <label htmlFor={`commissioner-${commissionerNumber}`} className="mb-2 block text-sm font-medium">
            Commissioner #{commissionerNumber}
         </label>
         <div className="relative mt-2 rounded-md">
            <div className="relative">
               <select
                  id={`commissioner-${commissionerNumber}`}
                  name={`commissioner-${commissionerNumber}`}
                  className="peer block w-96 rounded-md border border-gray-200 py-2 pl-2 text-sm outline-2 placeholder:text-gray-500"
                  aria-describedby={`commissioner-${commissionerNumber}-error`}>
                  <option value="">Select a commissioner</option>
                  {commissioners.map((commissioner: PersonResponse) => (
                     <option key={commissioner.id} value={commissioner.displayName ?? ''}>
                        {commissioner.displayName ?? `${commissioner.firstName ?? ''} ${commissioner.lastName ?? ''}`.trim()}
                     </option>
                  ))}
               </select>
            </div>
            <div id={`commissioner-${commissionerNumber}-error`} aria-live="polite" aria-atomic="true" />
         </div>
      </div>
   );
}
