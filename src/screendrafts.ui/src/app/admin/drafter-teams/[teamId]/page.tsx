import { auth } from "@/auth";
import { redirect, notFound } from "next/navigation";
import Link from "next/link";
import { getDrafterTeam } from "@/services/admin/fetch-admin-drafts";
import { EditDrafterTeamForm } from "./edit-drafter-team-form";
import { Metadata } from "next";

export const metadata: Metadata = { title: "Edit Drafter Team" };
export const dynamic = "force-dynamic";

interface Props {
  // Next.js 15 — params is a Promise in Server Components.
  params: Promise<{ teamId: string }>;
}

export default async function EditDrafterTeamPage({ params }: Props) {
  const { teamId } = await params;
  const session = await auth();

  if (!session?.accessToken) redirect("/");

  const team = await getDrafterTeam(session.accessToken, teamId);
  if (!team) notFound();

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="px-6 md:px-10 py-10 max-w-[900px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">
          <Link href="/admin" className="hover:text-sd-ink/70">ADMIN</Link>
          {" / "}
          <Link href="/admin/drafter-teams" className="hover:text-sd-ink/70">DRAFTER TEAMS</Link>
          {" / "}
          {team.name.toUpperCase()}
        </p>

        <h1 className="font-oswald font-bold text-[40px] leading-none text-sd-ink mb-2">
          {team.name.toUpperCase()}
        </h1>
        <p className="text-sm text-sd-ink/60 mb-10 max-w-2xl">
          {team.members.length} member{team.members.length !== 1 ? "s" : ""}.
        </p>

        <EditDrafterTeamForm team={team} accessToken={session.accessToken} />
      </div>
    </div>
  );
}