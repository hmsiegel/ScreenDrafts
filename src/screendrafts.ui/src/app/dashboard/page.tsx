import CurrentPredictions from "@/features/dashboard/components/current-predictions";
import MostRecentDrafts from "@/features/dashboard/components/most-recent-drafts";
import { auth } from "@/auth";
import UpcomingDrafts from "@/features/dashboard/components/upcoming-drafts";

export const dynamic = 'force-dynamic';

export default async function Page() {

   const session = await auth();

   if (!session) {
      return (
         <main className="flex flex-col items-center justify-center md:h-screen">
            <div className="bg-[#fffdfd] rounded-lg shadow-lg py-5 px-28 flex flex-col items-center justify-center">
               <MostRecentDrafts />
               <h1 className="text-6xl font-bold text-black mb-10">Please Sign In</h1>
            </div>
         </main>
      );
   } else {
      return (
         <main className="flex flex-col items-center justify-center md:h-screen">
            <div className="py-2">
               <UpcomingDrafts />
               <MostRecentDrafts />
               <CurrentPredictions />
            </div>
         </main>
      )
   }
}
