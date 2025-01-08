import CurrentPredictions from "@/app/ui/landing/current-predictions";
import MostRecentDrafts from "@/app/ui/landing/most-recent-drafts";

export default function Page() {
   return (
      <main className="flex flex-col items-center justify-center md:h-screen">
         <MostRecentDrafts />
         <CurrentPredictions />
      </main>
   )
}