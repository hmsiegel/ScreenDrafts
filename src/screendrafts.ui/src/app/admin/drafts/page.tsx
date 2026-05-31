import { auth } from "@/auth";
import { redirect } from "next/navigation";
import Link from "next/link";
import SiteHeader from "@/components/layout/header/site-header";
import SiteFooter from "@/components/layout/footer/site-footer";
import { listAdminActiveDrafts } from "@/services/admin/fetch-admin-drafts";
import { Metadata } from "next";
import DraftTypeBadge from "@/components/ui/draft-type-badge";
import { draftTypeFromNumber } from "@/lib/draft-type-display";

export const metadata: Metadata = { title: "Draft Management" };
export const dynamic = "force-dynamic";

const ADMIN_ROLES = ["Administrator", "SuperAdministrator"];

// Draft status numeric values from backend SmartEnum
const STATUS_LABELS: Record<number, string> = {
  0: "Created",
  1: "In Progress",
  3: "Paused",
  4: "Completed",
  5: "Cancelled",
};

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
  const isAdmin = session?.roles?.some((r) => ADMIN_ROLES.includes(r)) ?? false;
  if (!isAdmin) redirect("/");

  // Show Active (2) and Paused (3) first; fall back to all if none match
  const displayDrafts = await listAdminActiveDrafts(session?.accessToken);

  return (
    <div className="min-h-screen bg-light-blue">
      <SiteHeader activePath="/admin" />

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
          {displayDrafts.length === 0 ? (
            <p className="text-sd-ink/50 text-sm font-mono">No drafts found.</p>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-sd-ink/10">
                    {["Title", "Type", "Series", "Status", ""].map((col) => (
                      <th
                        key={col}
                        className="text-left font-mono text-[11px] tracking-widest uppercase text-sd-ink/50 pb-3 pr-4"
                      >
                        {col}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {displayDrafts.map((d) => (
                    <tr
                      key={d.publicId}
                      className="border-b border-sd-ink/5 hover:bg-sd-paper/60 transition-colors"
                    >
                      <td className="py-3 pr-4 font-medium text-sd-ink">{d.title}</td>
                      <td className="py-3 pr-4 text-sd-ink/70">
                        <DraftTypeBadge type={draftTypeFromNumber(d.draftType)} />
                      </td>
                      <td className="py-3 pr-4 text-sd-ink/70">{d.seriesName ?? "—"}</td>
                      <td className="py-3 pr-4">
                        <span
                          className={`inline-block px-2 py-0.5 text-[11px] font-mono tracking-wide uppercase rounded ${d.draftStatus === 2
                              ? "bg-green-100 text-green-800"
                              : d.draftStatus === 3
                                ? "bg-yellow-100 text-yellow-800"
                                : "bg-gray-100 text-gray-600"
                            }`}
                        >
                          {STATUS_LABELS[d.draftStatus ?? -1] ?? "Unknown"}
                        </span>
                      </td>
                      <td className="py-3 text-right">
                        <Link
                          href={`/admin/drafts/${d.publicId}/edit`}
                          className="text-sd-blue text-sm font-medium hover:underline"
                        >
                          Edit
                        </Link>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </AdminCard>
      </div>

      <SiteFooter />
    </div>
  );
}
