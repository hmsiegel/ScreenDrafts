import { listDrafts } from "@/app/lib/fetch-drafts";
import DraftsFilter from "@/app/ui/drafts/drafts-filter";
import { DraftsTable } from "@/app/ui/drafts/drafts-table";
import Breadcrumbs from "@/app/ui/main/breadcrumbs";
import { Metadata } from "next";

export const metadata: Metadata = {
  title: "Drafts",
  description: "List of drafts"
}

export const dynamic = "force-dynamic"; // Disable caching for this page

export default async function Page({ searchParams: qp, }: { searchParams: { [key: string]: string | string[] | undefined} }) {
   const draftTypes =
      Array.isArray(qp.draftType)
      ? qp.draftType.map(Number)
      : qp.draftType
        ? [Number(qp.draftType)]
        : undefined;


   const drafts = await listDrafts({
      fromDate: qp.fromDate as string | undefined,
      toDate: qp.toDate as string | undefined,
      draftType: draftTypes,
      minDrafters: asNumber(qp.minDrafters),
      maxDrafters: asNumber(qp.maxDrafters),
      minPicks: asNumber(qp.minPicks),
      maxPicks: asNumber(qp.maxPicks),
      sort: qp.sort as string | undefined,
      dir: qp.dir as "asc" | "desc" | undefined,
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
            <DraftsTable
               drafts={drafts}
               sort={qp.sort as string | undefined}
               dir={qp.dir as string | undefined}
            />
         </div>
      </main>
   );

   function asNumber(value: string | string[] | undefined): number | undefined {
      return value ? Number(Array.isArray(value) ? value[0] : value) : undefined;
   }
}
