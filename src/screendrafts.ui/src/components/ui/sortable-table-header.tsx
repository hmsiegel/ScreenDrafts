'use client';

import { ChevronDownIcon, ChevronUpIcon } from "@heroicons/react/24/outline";
import { useRouter, useSearchParams } from "next/navigation";

export function SortableTableHeader({
   field,
   label,
   currentSort,
   dir,
}: {
   field: string;
   label: string;
   currentSort?: string | undefined;
   dir: string | undefined;
}) {
   const router = useRouter();
   const params = new URLSearchParams(useSearchParams());
   const nextDir = currentSort === field && dir === "asc" ? "desc" : "asc";

   const handleSort = () => {
      params.set("sort", field);
      params.set("dir", nextDir);
      router.push(`?${params.toString()}`);
   };

   return (
      <button onClick={handleSort} className="flex items-center">
         {label}
         {currentSort === field && (
            dir === "asc" ? <ChevronUpIcon className="h-4 w-4" />
               : <ChevronDownIcon className="h-4 w-4 " />
         )}
      </button>
   );
}