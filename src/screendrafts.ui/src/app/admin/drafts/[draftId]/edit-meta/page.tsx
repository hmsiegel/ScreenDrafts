import { auth } from "@/auth";
import { redirect, notFound } from "next/navigation";
import Link from "next/link";
import EditMetaForm from "./edit-meta-form";
import {
  getDraft,
  listAllCategories,
  listAllCampaigns,
  listUnreleasedDraftParts,
} from "@/services/admin/fetch-admin-drafts";
import { Metadata } from "next";

export const metadata: Metadata = { title: "Edit Draft — Campaign & Categories" };
export const dynamic = "force-dynamic";

export default async function EditMetaPage({
  params,
}: {
  params: Promise<{ draftId: string }>;
}) {
  const { draftId } = await params;
  const session = await auth();

  if (!session?.accessToken) redirect("/");

  const [draft, categoryList, campaignList, unreleasedParts] = await Promise.all([
    getDraft(session.accessToken, draftId),
    listAllCategories(session.accessToken),
    listAllCampaigns(session.accessToken),
    listUnreleasedDraftParts(session.accessToken, draftId),
  ]);

  if (!draft) notFound();

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="px-6 md:px-10 py-10 max-w-[700px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">
          <Link href="/drafts" className="hover:text-sd-ink/70">DRAFTS</Link>
          {" / "}
          <Link href={`/drafts/${draftId}`} className="hover:text-sd-ink/70 truncate">
            {draft.title}
          </Link>
          {" / EDIT"}
        </p>

        <h1 className="font-oswald font-bold text-[40px] leading-none text-sd-ink mb-2">
          EDIT CAMPAIGN & CATEGORIES
        </h1>
        <p className="font-mono text-lg text-sd-ink/50 mb-10">{draft.title}</p>

        <div className="bg-white border border-sd-ink/10 p-8">
          <EditMetaForm
            draft={draft}
            categoryList={categoryList}
            campaignList={campaignList}
            unreleasedParts={unreleasedParts}
            accessToken={session.accessToken}
          />
        </div>
      </div>
    </div>
  );
}