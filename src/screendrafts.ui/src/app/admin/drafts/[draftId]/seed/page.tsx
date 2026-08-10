import { auth } from "@/auth";
import { redirect, notFound } from "next/navigation";
import Link from "next/link";
import { getDraft } from "@/services/admin/fetch-admin-drafts";
import { SeedDraftWizard } from "./seed-draft-wizard";
import { Metadata } from "next";

export const metadata: Metadata = { title: "Seed Draft" };
export const dynamic = "force-dynamic";

interface Props {
  // Next.js 15 — params is a Promise in Server Components.
  params: Promise<{ draftId: string }>;
}

export default async function SeedDraftPage({ params }: Props) {
  const { draftId } = await params;
  const session = await auth();

  if (!session?.accessToken) redirect("/");

  const detail = await getDraft(session.accessToken, draftId);
  if (!detail) notFound();

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="px-6 md:px-10 py-10 max-w-[900px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">
          <Link href="/admin" className="hover:text-sd-ink/70">ADMIN</Link>
          {" / "}
          <Link href="/admin/drafts" className="hover:text-sd-ink/70">DRAFTS</Link>
          {" / SEED"}
        </p>

        <h1 className="font-oswald font-bold text-[40px] leading-none text-sd-ink mb-2">
          SEED: {detail.title.toUpperCase()}
        </h1>
        <p className="text-sm text-sd-ink/60 mb-10 max-w-2xl">
          Enter the rest of this episode&apos;s historical data. Honorifics and
          prediction scoring run automatically once marked complete.
        </p>

        <SeedDraftWizard detail={detail} accessToken={session.accessToken} />
      </div>
    </div>
  );
}