// src/app/predictions/page.tsx
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
      {/* Banner */}
      <div className="bg-sd-ink text-white" style={{ padding: "56px 40px 44px" }}>
        <p className="font-mono text-[11px] tracking-widest text-light-blue mb-3">/ PREDICTIONS</p>
        <div className="flex items-end justify-between gap-8">
          <h1 className="font-oswald font-bold text-[72px] leading-[0.95] text-white">
            PREDICTIONS
          </h1>
          <p className="font-serif italic text-[17px] leading-relaxed text-white/70 max-w-[480px] text-right">
            Every season, every set of picks locked in before the board was ever built.
          </p>
        </div>
      </div>

      <div className="px-10 py-10 max-w-[1000px] mx-auto">
        <PredictionsList seasons={seasons ?? []} />
      </div>
    </div>
  );
}