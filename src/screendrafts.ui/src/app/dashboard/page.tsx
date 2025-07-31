import CurrentPredictions from "@/features/dashboard/components/current-predictions";
import MostRecentDrafts from "@/features/dashboard/components/most-recent-drafts";
import { getServerSession } from "next-auth";
import { signIn } from "next-auth/react";
import { authOptions } from "../api/auth/[...nextauth]/route";
import UpcomingDrafts from "@/features/dashboard/components/upcoming-drafts";

export const dynamic = 'force-dynamic'; // Force revalidation on every request

export default async function Page() {

   const session = await getServerSession(authOptions);

   if (!session) {
      return (
         <main className="flex flex-col items-center justify-center md:h-screen">
            <div className="bg-[#fffdfd] rounded-lg shadow-lg py-5 px-28 flex flex-col items-center justify-center">
               <MostRecentDrafts />
               <h1 className="text-6xl font-bold text-black mb-10">Please Sign In</h1>
               <button
                  className="btn-blue hover:bg-blue-400 hover:text-black transition ease-out duration-500"
                  onClick={() => signIn('keycloak', { callbackUrl: '/main' })}
               >
                  Sign In
               </button>
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