import { PersonResponse } from "@/app/lib/dto";
import { inter } from "../fonts";
import { SortableTableHeader } from "../sortable-table-header";

interface GuestsTableProps {
   guests: PersonResponse[];
   sort?: string | undefined;
   dir: string | undefined;
}

export function GuestsTable({ guests, sort, dir }: GuestsTableProps) {
   return (
      <div className="overflow-x-auto max-h-[900px]">
         <div className="table w-full border-collapse bg-[#fffdfd]">
            <div className={`${inter.className} text-center text-lg font-black table-header-group bg-slate-900 text-white sticky top-0`}>
               <div className="table-row">
                  <div className="table-cell align-middle px-4 py-2 font-medium">
                     <SortableTableHeader
                        field="firstName"
                        label="First Name"
                        currentSort={sort}
                        dir={dir}
                     />
                  </div>
                  <div className="table-cell align-middle px-6 py-2 font-medium text-left">
                     <SortableTableHeader
                        field="lastName"
                        label="Last Name"
                        currentSort={sort}
                        dir={dir}
                     />
                  </div>
                  <div className="table-cell align-middle px-6 py-2 font-medium text-left">Drafter Profile</div>
                  <div className="table-cell align-middle px-6 py-2 font-medium text-left">Host Profile</div>
               </div>
            </div>
            <div className={`${inter.className} table-row-group bg-white`}>
               {
                  guests.map((guest: PersonResponse) => (
                     <div key={guest.id} className="table-row hover:bg-gray-200">
                        <div className="table-cell align-middle px-4 py-2">
                           {guest.firstName}
                        </div>
                        <div className="table-cell align-middle px-6 py-2">
                           {guest.lastName}
                        </div>
                        <div className="table-cell align-middle px-6 py-2">
                           {guest.isDrafter ? (
                              <a href={`/main/drafters/${guest.id}`} className="text-blue-500 hover:underline">
                                 View Profile
                              </a>
                           ) : (
                              null
                           )}
                        </div>
                        <div className="table-cell align-middle px-6 py-2">
                           {guest.isHost ? (
                              <a href={`/main/hosts/${guest.id}`} className="text-blue-500 hover:underline">
                                 View Profile
                              </a>
                           ) : (
                              null
                           )}
                        </div>
                     </div>
                  ))
               }
            </div>
         </div>
      </div>
   );
}