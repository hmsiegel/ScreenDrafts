export default function DrafterDropdown( { drafterNumber }: { drafterNumber: number }) {

   return (

      <div className="mb-4">
         <label htmlFor={`drafter-${drafterNumber}`} className="mb-2 block text-sm font-medium">
            Drafter #{drafterNumber}
         </label>
         <div className="relative mt-2 rounded-md">
            <div className="relative">
               <select
                  id={`drafter-${drafterNumber}`}
                  name={`drafter-${drafterNumber}`}
                  className="peer block w-96 rounded-md border border-gray-200 py-2 pl-2 text-sm outline-2 placeholder:text-gray-500"
                  aria-describedby={`drafter-${drafterNumber}-error`}>
                  <option value="">Select a drafter</option>
               </select>
            </div>
            <div id={`drafter-${drafterNumber}-error`}aria-live="polite" aria-atomic="true">
            </div>
         </div>
      </div>
   )
}