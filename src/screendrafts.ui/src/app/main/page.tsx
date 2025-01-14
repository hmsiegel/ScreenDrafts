import CurrentPredictions from "@/app/ui/main/current-predictions";
import MostRecentDrafts from "@/app/ui/main/most-recent-drafts";
import UpcomingDrafts from "@/app/ui/main/upcoming-drafts";

export default function Page() {
   return (
      <main className="flex flex-col items-center justify-center md:h-screen">
         <UpcomingDrafts />
         <MostRecentDrafts />
         <CurrentPredictions />
      </main>
   )
}