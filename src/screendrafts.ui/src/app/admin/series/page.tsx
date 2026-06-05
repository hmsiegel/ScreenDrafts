import { auth } from "@/auth";
import { listAllSeries } from "@/services/admin/fetch-admin-series";
import SeriesManager from "./series-manager";
import { Metadata } from "next";

export const metadata: Metadata = { title: "Series — ScreenDrafts Admin" };
export const dynamic = "force-dynamic";

export default async function SeriesPage() {
  const session = await auth();
  const series = await listAllSeries();

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="px-6 md:px-10 py-10 max-w-[1200px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">/ ADMIN / SERIES</p>
        <h1 className="font-oswald font-bold text-[56px] leading-none text-sd-ink mb-10">
          SERIES
        </h1>
        <SeriesManager initialData={series} accessToken={session?.accessToken} />
      </div>
    </div>
  );
}
