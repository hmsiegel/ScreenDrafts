import { auth } from "@/auth";
import { redirect } from "next/navigation";
import Link from "next/link";
import { DrafterTeamsList } from "./drafter-teams-list";
import { Metadata } from "next";

export const metadata: Metadata = { title: "Drafter Teams" };
export const dynamic = "force-dynamic";

export default async function DrafterTeamsPage() {
  const session = await auth();

  if (!session?.accessToken) redirect("/");

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="px-6 md:px-10 py-10 max-w-[900px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">
          <Link href="/admin" className="hover:text-sd-ink/70">ADMIN</Link>
          {" / DRAFTER TEAMS"}
        </p>

        <h1 className="font-oswald font-bold text-[40px] leading-none text-sd-ink mb-2">
          DRAFTER TEAMS
        </h1>
        <p className="text-sm text-sd-ink/60 mb-10 max-w-2xl">
          Groups of drafters that can play a single pick together — e.g. &quot;Screen
          Drafts Legends&quot;.
        </p>

        <DrafterTeamsList accessToken={session.accessToken} />
      </div>
    </div>
  );
}