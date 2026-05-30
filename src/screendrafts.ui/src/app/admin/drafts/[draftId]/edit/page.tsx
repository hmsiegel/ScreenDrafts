import { auth } from "@/auth";
import { redirect } from "next/navigation";
import Link from "next/link";
import SiteHeader from "@/components/layout/header/site-header";
import SiteFooter from "@/components/layout/footer/site-footer";
import { Metadata } from "next";

export const metadata: Metadata = { title: "Edit Draft" };
export const dynamic = "force-dynamic";

const ADMIN_ROLES = ["Administrator", "SuperAdministrator"];

// TODO: implement full draft edit page
export default async function EditDraftPage({
  params,
}: {
  params: Promise<{ draftId: string }>;
}) {
  const session = await auth();
  const isAdmin = session?.roles?.some((r) => ADMIN_ROLES.includes(r)) ?? false;
  if (!isAdmin) redirect("/");

  const { draftId } = await params;

  return (
    <div className="min-h-screen bg-light-blue">
      <SiteHeader activePath="/admin" />

      <div className="px-6 md:px-10 py-10 max-w-[900px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">
          <Link href="/admin" className="hover:text-sd-ink/70">ADMIN</Link>
          {" / "}
          <Link href="/admin/drafts" className="hover:text-sd-ink/70">DRAFTS</Link>
          {" / EDIT"}
        </p>

        <h1 className="font-oswald font-bold text-[48px] leading-none text-sd-ink mb-6">
          EDIT DRAFT
        </h1>

        <div className="bg-white border border-sd-ink/10 p-8">
          <p className="font-mono text-sm text-sd-ink/60 mb-4">
            Draft ID: <span className="text-sd-ink">{draftId}</span>
          </p>
          <p className="text-sd-ink/50 text-sm">
            Edit functionality coming soon.
          </p>
          <div className="mt-6">
            <Link
              href="/admin/drafts"
              className="border border-sd-ink/20 text-sd-ink font-sans text-sm px-4 py-2 hover:bg-sd-ink/5 transition-colors rounded"
            >
              ← Back to Drafts
            </Link>
          </div>
        </div>
      </div>

      <SiteFooter />
    </div>
  );
}
