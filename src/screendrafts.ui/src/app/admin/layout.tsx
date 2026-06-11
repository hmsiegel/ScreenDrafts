import { auth } from "@/auth";
import { redirect } from "next/navigation";

const ADMIN_ROLES = ["Administrator", "SuperAdministrator"];

export default async function AdminLayout({ children }: { children: React.ReactNode }) {
  const session = await auth();
  const isAdmin = session?.roles?.some(r => ADMIN_ROLES.includes(r)) ?? false;
  if (!isAdmin) redirect("/");

  return (
    <div className="min-h-screen bg-light-blue">
      {children}
    </div>
  );
}
