// app/admin/drafts/[draftId]/edit/page.tsx
import { auth } from "@/auth";
import { redirect, notFound } from "next/navigation";
import Link from "next/link";
import EditDraftForm from "./edit-draft-form";
import {
  getDraft,
  listAllSeries,
  searchAllHosts,
  listAllCategories,
  listAllCampaigns,
} from "@/services/admin/fetch-admin-drafts";
import { Metadata } from "next";

export const metadata: Metadata = { title: "Edit Draft" };
export const dynamic = "force-dynamic";

export default async function EditDraftPage({
  params,
}: {
  params: Promise<{ draftId: string }>;
}) {
  const { draftId } = await params;
  const session = await auth();

  if (!session?.accessToken) redirect("/");

  const [draft, seriesList, hostList, categoryList, campaignList] =
    await Promise.all([
      getDraft(session.accessToken, draftId),
      listAllSeries(session.accessToken),
      searchAllHosts(session.accessToken),
      listAllCategories(session.accessToken),
      listAllCampaigns(session.accessToken),
    ]);

  if (!draft) notFound();

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="page-x py-8 lg:py-10 max-w-[900px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">
          <Link href="/admin" className="hover:text-sd-ink/70">
            ADMIN
          </Link>
          {" / "}
          <Link href="/admin/drafts" className="hover:text-sd-ink/70">
            DRAFTS
          </Link>
          {" / EDIT"}
        </p>

        <h1 className="font-oswald font-bold text-[32px] sm:text-[48px] leading-none text-sd-ink mb-2 [overflow-wrap:anywhere]">
          EDIT DRAFT
        </h1>

        <div className="bg-white border border-sd-ink/10 p-4 sm:p-8">
          <EditDraftForm
            draft={draft}
            seriesList={seriesList}
            hostList={hostList}
            categoryList={categoryList}
            campaignList={campaignList}
            accessToken={session.accessToken}
          />
        </div>
      </div>
    </div>
  );
}