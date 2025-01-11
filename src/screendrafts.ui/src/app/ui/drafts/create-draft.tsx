'use client'

import Link from 'next/link';
import {
   UserCircleIcon,
} from '@heroicons/react/24/outline';
import { Button } from '@/app/ui/button';
import { useState } from 'react';
import { Tooltip } from '../tooltip';
import DrafterDropdown from './drafter-dropdown';

export default function Form() {
   const [draftType, setDraftType] = useState('regular');
   const numberOfDrafters = [2, 3, 4, 5, 6];
   const [drafters, setDrafters] = useState([1, 2]);

   function handleDrafterDropdownChange(e: any) {

   }

   function addDrafter() {

   }

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
                        aria-describedby="draft-type-error"
                        onChange={(e) => setDraftType(e.target.value)}>
                        <option value="regular">Regular</option>
                        <option value="expanded">Expanded</option>
                     </select>
                  </div>
                  <div id="draft-type-error" aria-live="polite" aria-atomic="true">
                  </div>
               </div>
            </div>

            {/*  Expanded Draft Type */}
            <div className={`mb-4 ${draftType === 'expanded' ? '' : 'hidden'}`}>
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

            <div className="flex items-center justify-between pb-4 gap-4 align-middle">
               <div className={`${draftType === 'expanded' ? 'flex' : 'hidden'} items-center gap-4`}>
                  <label htmlFor="number-of drafters" className="block text-sm font-medium">
                     Number of Drafters
                  </label>
                  <div className="relative rounded-md">
                     <div className="relative">
                        <select
                           id="number-of-drafters"
                           name="number-of-drafters"
                           className="peer block w-full rounded-md border border-gray-200 py-2 pl-2 text-sm outline-2 placeholder:text-gray-500"
                           >
                           {numberOfDrafters.map((num) => (
                              <option key={num} value={num}>{num}</option>
                           ))}
                        </select>
                     </div>
                  </div>
               </div>
               <Tooltip content="Create a new drafter">
                  <Link
                     href="drafters"
                     className="flex items-center rounded-lg bg-sd-blue h-8 px-4 text-sm font-medium text-gray-100 transition-colors hover:bg-blue-400"
                  >
                     <UserCircleIcon className="w-5 mr-2" />
                     Create Drafter
                  </Link>
               </Tooltip>
            </div>

            {/*  Drafter 1 */}
            <DrafterDropdown drafterNumber={1} />

            {/*  Drafter 2 */}
            <DrafterDropdown drafterNumber={2} />

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
