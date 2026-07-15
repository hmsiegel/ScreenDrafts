import { auth } from "@/auth";
import Link from "next/link";
import { listAdminActiveDrafts } from "@/services/admin/fetch-admin-drafts";
import { Metadata } from "next";
import UpcomingDraftsList from "./upcoming-drafts-list";

export const metadata: Metadata = { title: "Draft Management" };
export const dynamic = "force-dynamic";

function AdminCard({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="bg-white border border-sd-ink/10">
      <div className="flex items-center gap-3 px-6 py-4 border-b border-sd-ink/10 bg-sd-ink">
        <div className="w-1 h-5 bg-sd-red shrink-0" />
        <h2 className="font-oswald font-bold text-[16px] tracking-wide uppercase text-white">
          {title}
        </h2>
      </div>
      <div className="p-6">{children}</div>
    </div>
  );
}

export default async function AdminDraftsPage() {
  const session = await auth();
  // includeDeleted: true — this is the admin drafts page, and the client
  // component filters client-side (SHOW DELETED checkbox), same pattern as
  // category-manager.tsx / campaign-manager.tsx. Requires the caller to
  // have AdminViewDeleted or the endpoint 403s — since this route already
  // gates on admin access to be reachable at all, that should hold, but
  // worth confirming once the real endpoint behind listAdminActiveDrafts
  // is known.
  const displayDrafts = await listAdminActiveDrafts(session?.accessToken, true);

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="px-6 md:px-10 py-10 max-w-[1200px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">
          / ADMIN / DRAFTS
        </p>

        <div className="flex items-end justify-between mb-10">
          <h1 className="font-oswald font-bold text-[56px] leading-none text-sd-ink">
            DRAFT MANAGEMENT
          </h1>
          <Link
            href="/admin/drafts/new"
            className="bg-sd-red text-white font-oswald font-medium tracking-wide uppercase px-5 py-3 hover:bg-sd-red/90 transition-colors"
          >
            + Create New Draft
          </Link>
        </div>

        <AdminCard title="Created & Paused Drafts">
          <UpcomingDraftsList
            initialDrafts={displayDrafts}
            accessToken={session?.accessToken ?? ""}
          />
        </AdminCard>
      </div>
    </div>
  );
}