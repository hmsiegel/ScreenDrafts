'use client';

import Link from 'next/link';
import { useContext, useState } from 'react';
import { ArrowRightIcon } from '@heroicons/react/24/outline';
import { InitialDraftContext } from '@/features/drafts/contexts/initial-draft-context';
import CommissionerDropdown from './commissioner-dropdown';
import DrafterDropdown from './drafter-dropdown';
import { Button } from '../../../components/ui/button/button';

export default function Form() {
   const { initialDraftData } = useContext(InitialDraftContext);
   const [draftType, setDraftType] = useState('regular');

   function createCommissionerComponent() {
      const arr = [];
      for (let i = 0; i < 3; i++) {
         arr.push(<CommissionerDropdown key={i} commissionerNumber={i + 1} />);
      }
      return arr;
   }

   function createDrafterComponent() {
      const arr = [];
      for (let i = 0; i < 3; i++) {
         arr.push(<DrafterDropdown key={i} drafterNumber={i + 1} />);
      }
      return arr;
   }


   return (
      <form >
         <div className="bg-[#fffdfd] rounded-lg shadow-lg py-5 px-28 flex flex-col items-center justify-center my-4">
            <h1 className="text-3xl font-bold mb-6">Update Draft</h1>
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
                     defaultValue={initialDraftData.title}
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
                        defaultValue={initialDraftData.draftType}
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
                        defaultValue={initialDraftData.expandedDraftType}
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

            {/*  Commissioner */}
            {createCommissionerComponent()}

            <div className="border-b border-gray-200 w-full my-4"></div>

            {/*  Drafter */}
            {createDrafterComponent()}


            <div className="border-b border-gray-200 w-full my-4"></div>

            {/*  Number of Movies */}
            <div className="mb-4">
               <label htmlFor="commissioner" className="mb-2 block text-sm font-medium">
                  Number of Movies
               </label>
               <div className="relative mt-2 rounded-md">
                  <div className="relative">
                     <input
                        id="no-of-movies"
                        type='number'
                        name="no-of-movies"
                        className="peer block w-96 rounded-md border border-gray-200 py-2 pl-2 text-sm outline-2 placeholder:text-gray-500"
                        aria-describedby="no-of-movies-error">
                     </input>
                  </div>
                  <div id="no-of-movies-error" aria-live="polite" aria-atomic="true">
                  </div>
               </div>
            </div>
            <div className="mt-6 flex gap-4">
               <Link
                  href="/main/drafts/create"
                  className="flex h-10 items-center rounded-lg bg-gray-100 px-4 text-sm font-medium text-gray-600 transition-colors hover:bg-gray-200"
               >
                  Cancel
               </Link>
               <Button
                  type="submit"
                  className="flex h-10 items-center rounded-lg bg-sd-blue px-4 text-sm font-medium text-white transition-colors hover:bg-blue-400"
                  >
                  <span>Save</span>
            </Button>
         </div>
      </div>
      </form >
   );
}
