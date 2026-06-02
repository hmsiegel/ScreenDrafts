import { auth } from "@/auth";
import { redirect } from "next/navigation";
import Link from "next/link";
import SiteHeader from "@/components/layout/header/site-header";
import SiteFooter from "@/components/layout/footer/site-footer";
import CreateDraftForm from "./create-draft-form";
import {
  listAllSeries,
  searchAllHosts,
  listAllCategories,
  listAllCampaigns,
} from "@/services/admin/fetch-admin-drafts";
import { Metadata } from "next";

export const metadata: Metadata = { title: "Create Draft" };
export const dynamic = "force-dynamic";

const ADMIN_ROLES = ["Administrator", "SuperAdministrator"];

export default async function CreateDraftPage() {
  const session = await auth();
  const isAdmin = session?.roles?.some((r) => ADMIN_ROLES.includes(r)) ?? false;
  if (!isAdmin) redirect("/");

  if (!session?.accessToken) redirect("/");

  const [seriesList, hostList, categoryList, campaignList] = await Promise.all([
    listAllSeries(session.accessToken),
    searchAllHosts(session.accessToken),
    listAllCategories(session.accessToken),
    listAllCampaigns(session.accessToken),
  ]);

  return (
    <div className="min-h-screen bg-light-blue">
      <SiteHeader activePath="/admin" />

      <div className="px-6 md:px-10 py-10 max-w-[900px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">
          <Link href="/admin" className="hover:text-sd-ink/70">ADMIN</Link>
          {" / "}
          <Link href="/admin/drafts" className="hover:text-sd-ink/70">DRAFTS</Link>
          {" / NEW"}
        </p>

        <h1 className="font-oswald font-bold text-[48px] leading-none text-sd-ink mb-10">
          CREATE DRAFT
        </h1>

        <div className="bg-white border border-sd-ink/10 p-8">
          <CreateDraftForm
            seriesList={seriesList}
            hostList={hostList}
            categoryList={categoryList}
            campaignList={campaignList}
            accessToken={session.accessToken}
          />
        </div>
      </div>

      <SiteFooter />
    </div>
  );
}
