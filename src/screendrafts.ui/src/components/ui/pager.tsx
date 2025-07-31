'use client';

import { useRouter, useSearchParams } from "next/navigation";

export default function Pager({
   total,
   page,
   pageSize
}: {
   page: number;
   total: number;
   pageSize: number;
}) {
   const router = useRouter();
   const pages = Math.ceil(total / pageSize);
   const searchParams = new URLSearchParams(useSearchParams());

   if (pages <= 1) return null;

   const go = (newPage: number) => {
      searchParams.set("page", String(newPage));
      searchParams.set("pageSize", String(pageSize));
      router.push(`?${searchParams.toString()}`);
   };

   const numbers = Array.from({ length: pages }, (_, i) => i + 1)
      .slice(Math.max(0, page - 3), Math.min(pages, page + 2))

   return (
      <nav className="mt-6 flex justify-center gap-2">
         <button
            onClick={() => go(page - 1)}
            className="btn btn-sm">«</button>
            {numbers.map(n => (
               <button
                  key={n}
                  onClick={() => go(n)}
                  className={`btn btn-sm ${n === page ? "btn-primary" : "btn-outline"}`}
               >
                  {n}
               </button>
            ))}
         <button
            onClick={() => go(page + 1)}
            disabled={page === pages}
            className="btn btn-sm">»</button>
      </nav>
   );
}