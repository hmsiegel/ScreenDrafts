'use client'

import Link from 'next/link';
import {
   CheckIcon,
   ClockIcon,
   CurrencyDollarIcon,
   PlusIcon,
   UserCircleIcon,
} from '@heroicons/react/24/outline';
import { Button } from '@/app/ui/button';
import { useActionState } from 'react';
import { inter } from '../fonts';
import { Tooltip } from '../tooltip';

export default function Form() {
   return (
      <form >
         <div className="bg-[#fffdfd] rounded-lg shadow-lg py-5 px-28 flex flex-col items-center justify-center my-4">
            {/* Title */}
            <div className="mb-4">
               <label htmlFor="draft-title" className="mb-2 block text-sm font-medium">
                  Title
               </label>
               <div className="relative">
                  <input
                     id="draft-title"
                     name="title"
                     className="peer block w-96 cursor-pointer rounded-md border border-gray-200 py-2 pl-2 text-sm outline-2 placeholder:text-gray-500"
                     placeholder="Please enter a title"
                     defaultValue=""
                     aria-describedby="customer-error"
                  />
               </div>
               <div id="title-error" aria-live="polite" aria-atomic="true">
               </div>
            </div>

            {/*  Draft Type */}
            <div className="mb-4">
               <label htmlFor="draft-type" className="mb-2 block text-sm font-medium">
                  Draft Type
               </label>
               <div className="relative mt-2 rounded-md">
                  <div className="relative">
                     <select
                        id="draft-type"
                        name="draft-type"
                        className="peer block w-96 rounded-md border border-gray-200 py-2 pl-2 text-sm outline-2 placeholder:text-gray-500"
                        aria-describedby="draft-type-error">
                        <option value="regular">Regular</option>
                        <option value="expanded">Expanded</option>
                     </select>
                  </div>
                  <div id="draft-type-error" aria-live="polite" aria-atomic="true">
                  </div>
               </div>
            </div>

            {/*  Expanded Draft Type */}
            <div className="mb-4 hidden">
               <label htmlFor="expanded-draft-type" className="mb-2 block text-sm font-medium">
                  Expanded Draft Type
               </label>
               <div className="relative mt-2 rounded-md">
                  <div className="relative">
                     <select
                        id="expanded-draft-type"
                        name="expanded-draft-type"
                        className="peer block w-96 rounded-md border border-gray-200 py-2 pl-2 text-sm outline-2 placeholder:text-gray-500"
                        aria-describedby="draft-type-error">
                        <option value="mini-mega">Mini-Mega</option>
                        <option value="super">Super</option>
                        <option value="mega">Mega</option>
                        <option value="mini-super">Mini-Super</option>
                     </select>
                  </div>
                  <div id="expanded-draft-type-error" aria-live="polite" aria-atomic="true">
                  </div>
               </div>
            </div>

            <div className="border-b border-gray-200 w-full my-4"></div>

            <div className="flex items-center justify-between pb-2">
               <h2 className="text-base font-medium text-gray-600">Commissioners</h2>
            </div>

            {/*  Commissioner */}
            <div className="mb-4">
               <label htmlFor="commissioner" className="mb-2 block text-sm font-medium">
                  Commissioner
               </label>
               <div className="relative mt-2 rounded-md">
                  <div className="relative">
                     <select
                        id="commissioner"
                        name="commissioner"
                        className="peer block w-96 rounded-md border border-gray-200 py-2 pl-2 text-sm outline-2 placeholder:text-gray-500"
                        aria-describedby="commissioner-error">
                        <option value="">Select a Commissioner</option>
                     </select>
                  </div>
                  <div id="commissioner-error" aria-live="polite" aria-atomic="true">
                  </div>
               </div>
            </div>

            {/*  Co-Commissioner */}
            <div className="mb-4">
               <label htmlFor="co-commissioner" className="mb-2 block text-sm font-medium">
                  Co-Commissioner
               </label>
               <div className="relative mt-2 rounded-md">
                  <div className="relative">
                     <select
                        id="co-commissioner"
                        name="co-commissioner"
                        className="peer block w-96 rounded-md border border-gray-200 py-2 pl-2 text-sm outline-2 placeholder:text-gray-500"
                        aria-describedby="co-commissioner-error">
                        <option value="">Select a Co-Commissioner</option>
                     </select>
                  </div>
                  <div id="co-commissioner-error" aria-live="polite" aria-atomic="true">
                  </div>
               </div>
            </div>

            <div className="border-b border-gray-200 w-full my-4"></div>

            <div className="flex items-center justify-between pb-2">
               <h2 className="text-base font-medium text-gray-600 pr-4">Drafters</h2>
            </div>

            <div className="flex items-center justify-between pb-2">
               <AddDrafter />
               <Tooltip content="Create a new drafter">
                  <Link
                     href="drafters"
                     className="flex items-center rounded-lg bg-sd-blue ml-4 h-8 px-4 text-sm font-medium text-gray-100 transition-colors hover:bg-blue-400"
                  >
                     <UserCircleIcon className="w-5 mr-2" />
                     Create Drafter
                  </Link>
               </Tooltip>
            </div>

            {/*  Drafter 1 */}
            <div className="mb-4">
               <label htmlFor="drafter-1" className="mb-2 block text-sm font-medium">
                  Drafter #1
               </label>
               <div className="relative mt-2 rounded-md">
                  <div className="relative">
                     <select
                        id="drafter-1"
                        name="drafter-1"
                        className="peer block w-96 rounded-md border border-gray-200 py-2 pl-2 text-sm outline-2 placeholder:text-gray-500"
                        aria-describedby="drafter-1-error">
                        <option value="">Select a drafter</option>
                     </select>
                  </div>
                  <div id="drafter-1-error" aria-live="polite" aria-atomic="true">
                  </div>
               </div>
            </div>

            {/*  Drafter 2 */}
            <div className="mb-4">
               <label htmlFor="drafter-2" className="mb-2 block text-sm font-medium">
                  Drafter #2
               </label>
               <div className="relative mt-2 rounded-md">
                  <div className="relative">
                     <select
                        id="drafter-2"
                        name="drafter-2"
                        className="peer block w-96 rounded-md border border-gray-200 py-2 pl-2 text-sm outline-2 placeholder:text-gray-500"
                        aria-describedby="drafter-2-error">
                        <option value="">Select a drafter</option>
                     </select>
                  </div>
                  <div id="drafter-2-error" aria-live="polite" aria-atomic="true">
                  </div>
               </div>
            </div>

            {/*  Drafter 3 */}
            <div className="mb-4 hidden">
               <label htmlFor="drafter-3" className="mb-2 block text-sm font-medium">
                  Drafter #3
               </label>
               <div className="relative mt-2 rounded-md">
                  <div className="relative">
                     <select
                        id="drafter-3"
                        name="drafter-3"
                        className="peer block w-96 rounded-md border border-gray-200 py-2 pl-2 text-sm outline-2 placeholder:text-gray-500"
                        aria-describedby="drafter-3-error">
                        <option value="">Select a drafter</option>
                     </select>
                  </div>
                  <div id="drafter-3-error" aria-live="polite" aria-atomic="true">
                  </div>
               </div>
            </div>

         </div>
         <div className="mt-6 flex justify-end gap-4">
            <Link
               href=".."
               className="flex h-10 items-center rounded-lg bg-gray-100 px-4 text-sm font-medium text-gray-600 transition-colors hover:bg-gray-200"
            >
               Cancel
            </Link>
            <Button type="submit">Create Draft</Button>
         </div>
      </form>
   );
}

export function AddDrafter() {
   return (
      <Tooltip content="Add Drafter to Draft">
         <button className="rounded-md border p-2 hover:bg-gray-200">
            <PlusIcon className="w-5" />
         </button>
      </Tooltip>
   )
}
