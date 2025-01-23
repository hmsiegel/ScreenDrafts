'use client';

import Link from 'next/link';
import { useContext, useState } from 'react';
import { ArrowRightIcon } from '@heroicons/react/24/outline';
import { InitialDraftContext } from '@/app/contexts/initial-draft-context';


export default function Form() {
   const [draftType, setDraftType] = useState('regular');
   const { initialDraftData, setInitialDraftData} = useContext(InitialDraftContext);

   return (
      <form >
         <div className="bg-[#fffdfd] rounded-lg shadow-lg py-5 px-28 flex flex-col items-center justify-center my-4">
            <h1 className="text-3xl font-bold mb-6">Create Draft</h1>
            {/* Title */}
            <div className="mb-4">
               <label htmlFor="draft-title" className="mb-2 block text-sm font-medium">
                  Title
               </label>
               <div className="relative">
                  <input
                     id="draft-title"
                     name="title"
                     type="text"
                     onChange={(e) => setInitialDraftData({ ...initialDraftData, title: e.target.value }) }
                     className="peer block w-96 cursor-pointer rounded-md border border-gray-200 py-2 pl-2 text-sm outline-2 placeholder:text-gray-500"
                     placeholder="Please enter a title"
                     aria-describedby="title-error"
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
                        onChange={(e) => setInitialDraftData({ ...initialDraftData, expandedDraftType: e.target.value }) }
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
            <div className="mb-4">
               <label htmlFor="commissioner" className="mb-2 block text-sm font-medium">
                  Number of Commissioners
               </label>
               <div className="relative mt-2 rounded-md">
                  <div className="relative">
                     <input
                        id="no-of-commissioners"
                        name="no-of-commissioners"
                        type='number'
                        onChange={(e) => setInitialDraftData({ ...initialDraftData, noOfCommissioners: parseInt(e.target.value) }) }
                        className="peer block w-96 rounded-md border border-gray-200 py-2 pl-2 text-sm outline-2 placeholder:text-gray-500"
                        aria-describedby="no-of-commissioners-error">
                     </input>
                  </div>
                  <div id="no-of-commissioners-error" aria-live="polite" aria-atomic="true">
                  </div>
               </div>
            </div>

            {/*  Number of Drafters */}
            <div className="mb-4">
               <label htmlFor="commissioner" className="mb-2 block text-sm font-medium">
                  Number of Drafters
               </label>
               <div className="relative mt-2 rounded-md">
                  <div className="relative">
                     <input
                        id="no-of-drafters"
                        type='number'
                        name="no-of-drafters"
                        onChange={(e) => setInitialDraftData({ ...initialDraftData, noOfDrafters: parseInt(e.target.value) }) }
                        className="peer block w-96 rounded-md border border-gray-200 py-2 pl-2 text-sm outline-2 placeholder:text-gray-500"
                        aria-describedby="no-of-drafters-error">
                     </input>
                  </div>
                  <div id="no-of-drafters-error" aria-live="polite" aria-atomic="true">
                  </div>
               </div>
            </div>

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
                        onChange={(e) => setInitialDraftData({ ...initialDraftData, noOfMovies: parseInt(e.target.value) }) }
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
                  href="/main/"
                  className="flex h-10 items-center rounded-lg bg-gray-100 px-4 text-sm font-medium text-gray-600 transition-colors hover:bg-gray-200"
               >
                  Cancel
               </Link>
               <Link
                  href='/main/drafts/update'
                  className="flex h-10 items-center rounded-lg bg-sd-blue px-4 text-sm font-medium text-white transition-colors hover:bg-blue-400"
                  onClick={() => setInitialDraftData(initialDraftData)}
               >
                  <span>Next</span><ArrowRightIcon className="w-5 h-5 ml-2" />
               </Link>
            </div>
         </div>
      </form >
   );
}
