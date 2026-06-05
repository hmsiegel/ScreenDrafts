import { auth } from "@/auth";
import { fetchAdminUsers, fetchAdminRoles } from "@/services/admin/fetch-admin";
import UserTable from "./user-table";
import PasswordResetCard from "./password-reset-card";
import RolesAccordion from "./roles-accordion";
import AdminTabs from "./admin-tabs";
import { Metadata } from "next";
import { env } from "@/lib/env";

export const metadata: Metadata = { title: "Administration" };
export const dynamic = "force-dynamic";

interface AdminCardProps {
  title: string;
  children: React.ReactNode;
}

function AdminCard({ title, children }: AdminCardProps) {
  return (
    <div className="bg-white border border-sd-ink/10">
      <div className="flex items-center gap-3 px-6 py-4 border-b border-sd-ink/10 bg-sd-ink">
        <div className="w-1 h-5 bg-sd-red shrink-0" />
        <h2 className="font-oswald font-bold text-[16px] tracking-wide uppercase text-white">{title}</h2>
      </div>
      <div className="p-6">{children}</div>
    </div>
  );
}

export default async function AdminPage() {
  const session = await auth();
  const apiBase = env.apiUrl ?? '';
  const [users, roles] = await Promise.all([
    fetchAdminUsers(session?.accessToken),
    fetchAdminRoles(session?.accessToken),
  ]);

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="px-6 md:px-10 py-10 max-w-[1200px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">/ ADMIN</p>
        <h1 className="font-oswald font-bold text-[56px] leading-none text-sd-ink mb-10">
          ADMINISTRATION
        </h1>

        <AdminTabs
          usersPanel={
            <AdminCard title="Users">
              <UserTable
                initialData={users}
                allRoles={roles}
                accessToken={session?.accessToken}
                apiBase={apiBase}
              />
            </AdminCard>
          }
          passwordPanel={
            <AdminCard title="Reset User Password">
              <PasswordResetCard accessToken={session?.accessToken} apiBase={apiBase} />
            </AdminCard>
          }
          rolesPanel={
            <AdminCard title="Roles & Permissions">
              <RolesAccordion roles={roles} accessToken={session?.accessToken} apiBase={apiBase} />
            </AdminCard>
          }
        />
      </div>
    </div>
  );
}
