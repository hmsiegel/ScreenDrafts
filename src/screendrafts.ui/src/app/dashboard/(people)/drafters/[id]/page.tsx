import { getDrafterProfile } from "@features/guests/api/fetch-people";
import Breadcrumbs from "@/components/ui/breadcrumbs";
import { Metadata, ResolvingMetadata } from "next";
import { DrafterProfileResponse } from "@/lib/dto";
import { DrafterProfile } from "@/features/drafters/components/drafter-profile";

export async function generateMetadata(
   props: { params: Promise<{ id: string }> },
   _parent: ResolvingMetadata
): Promise<Metadata> {
   const { id } = await props.params;
   const drafter = await getDrafterProfile(id) as DrafterProfileResponse;

   return {
      title: `${drafter.displayName}`,
      description: `Drafter details for ${drafter.displayName}`,
      openGraph: {
         title: `${drafter.displayName}`,
         description: `Drafter details for ${drafter.displayName}`,
         url: `/dashboard/drafters/${drafter.id}/profile`,
      },
   };
}

export const dynamic = "force-dynamic"; // Disable caching for this page

export default async function Page(
   props: { params: Promise<{ id: string }> }
) {
   const { id } = await props.params;

   const drafter = await getDrafterProfile(id) as DrafterProfileResponse;

   return (
      <main className="flex flex-col items-center justify-center md:h-screen">
         <div className="bg-[#fffdfd] rounded-lg shadow-lg py-6 px-16 flex flex-col items-center justify-center ">
            <h1 className="text-2xl font-bold mb-4">{drafter.displayName}</h1>
            <Breadcrumbs
               breadcrumbs={[
                  { label: "Home", href: "/" },
                  { label: "Dashboard", href: "/dashboard" },
                  { label: "Drafters", href: "/dashboard/drafters" },
                  { label: `${drafter.displayName}`, href: `/dashboard/drafters/${drafter.id}`, active: true },
               ]}
            />
            <DrafterProfile drafter={drafter} />
         </div>
      </main>
   );
}