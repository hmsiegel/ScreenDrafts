// app/admin/drafter-teams/new/page.tsx
import { auth } from "@/auth";
import { redirect } from "next/navigation";
import Link from "next/link";
import { CreateDrafterTeamForm } from "./create-drafter-team-form";
import { Metadata } from "next";

export const metadata: Metadata = { title: "New Drafter Team" };
export const dynamic = "force-dynamic";

export default async function NewDrafterTeamPage() {
  const session = await auth();

  if (!session?.accessToken) redirect("/");

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="page-x py-8 lg:py-10 max-w-[900px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">
          <Link href="/admin" className="hover:text-sd-ink/70">ADMIN</Link>
          {" / "}
          <Link href="/admin/drafter-teams" className="hover:text-sd-ink/70">DRAFTER TEAMS</Link>
          {" / NEW"}
        </p>

        <h1 className="font-oswald font-bold text-[30px] sm:text-[40px] leading-none text-sd-ink mb-2 [overflow-wrap:anywhere]">
          NEW DRAFTER TEAM
        </h1>
        <p className="text-sm text-sd-ink/60 mb-10 max-w-2xl">
          Members can also be added or removed later from the team&apos;s own page.
        </p>

        <CreateDrafterTeamForm accessToken={session.accessToken} />
      </div>
    </div>
  );
}