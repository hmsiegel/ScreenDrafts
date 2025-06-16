import { listDrafts } from "@/app/lib/fetch-drafts";
import DraftsFilter from "@/app/ui/drafts/drafts-filter";
import { DraftsTable } from "@/app/ui/drafts/drafts-table";
import Breadcrumbs from "@/app/ui/main/breadcrumbs";
import { Metadata } from "next";

export const metadata: Metadata = {
  title: "Drafts",
  description: "List of drafts"
}


export default async function Page({ searchParams, }: { searchParams: { [key: string]: string | string[] | undefined} }) {
   const draftTypes =
      Array.isArray(searchParams.draftType)
      ? searchParams.draftType.map(Number)
      : searchParams.draftType
        ? [Number(searchParams.draftType)]
        : undefined;


   const drafts = await listDrafts({
      fromDate: searchParams.fromDate as string | undefined,
      toDate: searchParams.toDate as string | undefined,
      draftType: draftTypes,
      minDrafters: asNumber(searchParams.minDrafters),
      maxDrafters: asNumber(searchParams.maxDrafters),
      minPicks: asNumber(searchParams.minPicks),
      maxPicks: asNumber(searchParams.maxPicks),
   });
   return (
      <main className="flex flex-col items-center justify-center md:h-screen">
         <div className="bg-[#fffdfd] rounded-lg shadow-lg py-4 px-28 flex flex-col items-center justify-center my-4">
            <h1 className="text-2xl text-black mb-5 uppercase border-b-2 border-slate-900 pb-2">
               Drafts
            </h1>
            <Breadcrumbs
               breadcrumbs={[
                  { label: "Home", href: "/" },
                  { label: "Main", href: "/main" },
                  { label: "Drafts", href: "/main/drafts", active: true },
               ]}
            />
            <DraftsFilter />
            <DraftsTable drafts={drafts}/>
         </div>
      </main>
   );

   function asNumber(value: string | string[] | undefined): number | undefined {
      return value ? Number(Array.isArray(value) ? value[0] : value) : undefined;
   }
}
