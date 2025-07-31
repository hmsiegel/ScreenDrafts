import GuestsFilter from "@/features/guests/components/guests-filter";
import { GuestsTable } from "@/features/guests/components/guests-table";
import Breadcrumbs from "@/components/ui/breadcrumbs";
import Pager from "@/components/ui/pager";
import { Metadata } from "next";
import { listGuests } from "@/features/guests/api/fetch-people";

export const metadata: Metadata = {
   title: "Guests",
   description: "List of guests"
}

export const dynamic = "force-dynamic"; // Disable caching for this page

export default async function Page(
   props: { searchParams: Promise<{ [key: string]: string | string[] | undefined }> }
) {
   // Fetch data for the page
   const qp = await props.searchParams;

   const page = Number(qp.page ?? 1);
   const pageSize = Number(qp.pageSize ?? 10);

   const guests = await listGuests({
      q: qp.q as string | undefined,
      page,
      pageSize,
      sort: qp.sort as string | undefined,
      dir: qp.dir as "asc" | "desc" | undefined,
   });

   return (
      <main className="flex flex-col items-center justify-center md:h-screen">
         <div className="bg-[#fffdfd] rounded-lg shadow-lg py-4 px-28 flex flex-col items-center justify-center my-4">
            <h1 className="text-2xl text-black mb-5 uppercase border-b-2 border-slate-900 pb-2">
               Guests
            </h1>
            <Breadcrumbs
               breadcrumbs={[
                  { label: "Home", href: "/" },
                  { label: "Main", href: "/main" },
                  { label: "Guest", href: "/main/guests", active: true },
               ]}
            />
            <GuestsFilter />
            <GuestsTable
               guests={guests.items}
               sort={qp.sort as string | undefined}
               dir={qp.dir as string | undefined}
            />
            <Pager
               total={guests.total}
               page={guests.page}
               pageSize={guests.pageSize} />
         </div>
      </main>
   );
}