import { DraftResponse } from "@/lib/dto";
import { getDraftDetails } from "@features/drafts/api/fetch-drafts";
import { DraftDetails } from "@/features/drafts/components/draft-details";
import Breadcrumbs from "@/components/ui/breadcrumbs";
import { Metadata, ResolvingMetadata } from "next";

export async function generateMetadata(
   props: { params: Promise<{ id: string }> },
   _parent: ResolvingMetadata
): Promise<Metadata> {
   const { id } = await props.params;
   const draft = await getDraftDetails(id) as DraftResponse;

   return {
      title: `${draft.title}`,
      description: `Episode details for ${draft.title}`,
      openGraph: {
         title: `${draft.title}`,
         description: `Episode details for ${draft.title}`,
         url: `/dashboard/drafts/${draft.id}`,
      },
   };
}

export const dynamic = "force-dynamic"; // Disable caching for this page

export default async function Page(
   props: { params: Promise<{ id: string }> }
) {
   const { id } = await props.params;

   const draft = await getDraftDetails(id) as DraftResponse;

   const drafterMap = new Map(
      (draft.drafters ?? [])
         .filter((d): d is { id: string; displayName: string } => typeof d.id === "string" && typeof d.displayName === "string")
         .map(d => [d.id, d.displayName])
   );

   return (
      <main className="flex flex-col items-center justify-center md:h-screen">
         <div className="bg-[#fffdfd] rounded-lg shadow-lg py-6 px-16 flex flex-col items-center justify-center ">
            <Breadcrumbs
               breadcrumbs={[
                  { label: "Home", href: "/" },
                  { label: "Main", href: "/main" },
                  { label: "Drafts", href: "/main/drafts" },
                  { label: `${draft.title}`, href: `/main/drafts/${draft.title}`, active: true },
               ]}
            />
            <DraftDetails
               draft={draft}
               drafterMap={drafterMap} />
         </div>
      </main>
   );
}

