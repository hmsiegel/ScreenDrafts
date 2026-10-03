// app/predictions/page.tsx
import { listPredictionSeasons } from "@/services/drafts/fetch-drafts";
import { Metadata } from "next";
import { PredictionsList } from "./predictions-list";

export const metadata: Metadata = {
  title: "Predictions",
  description: "Every prediction season, and the drafts that made it.",
};

export const dynamic = "force-dynamic";

export default async function PredictionsPage() {
  const { seasons } = await listPredictionSeasons();

  return (
    <div className="min-h-screen bg-light-blue">
      {/* Banner — stacks below lg; title and blurb sit side by side from lg up. */}
      <div className="bg-sd-ink text-white page-x pt-10 pb-8 lg:pt-14 lg:pb-11">
        <p className="font-mono text-[11px] tracking-widest text-light-blue mb-3">/ PREDICTIONS</p>
        <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between lg:gap-8">
          <h1 className="font-oswald font-bold text-[44px] sm:text-[56px] lg:text-[72px] leading-[0.95] text-white">
            PREDICTIONS
          </h1>
          <p className="font-serif italic text-[15px] lg:text-[17px] leading-relaxed text-white/70 max-w-[480px] lg:text-right">
            Every season, every set of picks locked in before the board was ever built.
          </p>
        </div>
      </div>

      <div className="page-x py-6 lg:py-10 max-w-[1000px] mx-auto">
        <PredictionsList seasons={seasons ?? []} />
      </div>
    </div>
  );
}