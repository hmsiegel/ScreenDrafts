import { DrafterResponse, DraftResponse } from "@/app/lib/dto";
import { getDrafterDetails } from "@/app/lib/fetch-people";
import Breadcrumbs from "@/app/ui/main/breadcrumbs";
import { Metadata, ResolvingMetadata } from "next";

export async function generateMetadata(
   props: { params: Promise<{ id: string }> },
   _parent: ResolvingMetadata
): Promise<Metadata> {
   const { id } = await props.params;
   const drafter = await getDrafterDetails(id) as DrafterResponse;

   return {
      title: `${drafter.displayName}`,
      description: `Drafter details for ${drafter.displayName}`,
      openGraph: {
         title: `${drafter.displayName}`,
         description: `Drafter details for ${drafter.displayName}`,
         url: `/main/drafters/${drafter.id}`,
      },
   };
}

export const dynamic = "force-dynamic"; // Disable caching for this page

export default async function Page(
   props: { params: Promise<{ id: string }> }
) {
   const { id } = await props.params;

   const drafter = await getDrafterDetails(id) as DrafterResponse;

   return (
      <main className="flex flex-col items-center justify-center md:h-screen">
         <div className="bg-[#fffdfd] rounded-lg shadow-lg py-6 px-16 flex flex-col items-center justify-center ">
            <h1 className="text-2xl font-bold mb-4">{drafter.displayName}</h1>
            <Breadcrumbs
               breadcrumbs={[
                  { label: "Home", href: "/" },
                  { label: "Main", href: "/main" },
                  { label: "Drafters", href: "/main/drafters" },
                  { label: `${drafter.displayName}`, href: `/main/drafters/${drafter.id}`, active: true },
               ]}
            />

         </div>
      </main>
   );
}