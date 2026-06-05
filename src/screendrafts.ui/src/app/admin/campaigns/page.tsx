import { auth } from "@/auth";
import { listAllCampaigns } from "@/services/admin/fetch-admin-campaigns";
import CampaignManager from "./campaign-manager";
import { Metadata } from "next";

export const metadata: Metadata = { title: "Campaigns — ScreenDrafts Admin" };
export const dynamic = "force-dynamic";

export default async function CampaignsPage() {
  const session = await auth();
  const campaigns = await listAllCampaigns();

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="px-6 md:px-10 py-10 max-w-[1200px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">/ ADMIN / CAMPAIGNS</p>
        <h1 className="font-oswald font-bold text-[56px] leading-none text-sd-ink mb-10">
          CAMPAIGNS
        </h1>
        <p className="font-serif italic text-[16px] text-sd-ink/70 max-w-2xl mb-10">
          A campaign groups a set of drafts around a shared theme — typically a seasonal run or
          a special event. Drafts belong to at most one campaign. Campaigns recur at most once or twice a year.
        </p>
        <CampaignManager initialData={campaigns} accessToken={session?.accessToken} />
      </div>
    </div>
  );
}
