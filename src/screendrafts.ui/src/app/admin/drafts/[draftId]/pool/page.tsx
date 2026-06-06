import { auth } from "@/auth";
import { redirect } from "next/navigation";
import { getDraft, getDraftPool } from "@/services/admin/fetch-admin-drafts";
import DraftPoolManager from "./draft-pool-manager";
import { Metadata } from "next";

export const metadata: Metadata = { title: "Draft Pool" };
export const dynamic = "force-dynamic";

// Super draft type numeric value
const SUPER_DRAFT_TYPE = 3;

export default async function DraftPoolPage({ params }: { params: Promise<{ draftId: string }> }) {
  const session = await auth();
  if (!session?.accessToken) redirect("/sign-in");

  const { draftId } = await params;
  const draft = await getDraft(session.accessToken, draftId);

  if (!draft) redirect("/admin/drafts");
  if (draft.draftType?.value !== SUPER_DRAFT_TYPE) redirect("/admin/drafts");

  const pool = await getDraftPool(session.accessToken, draftId);

  // Resolve movie metadata for existing pool items
  // TODO: replace with real movie metadata lookup once endpoint is confirmed
  const initialMovies = (pool?.tmdbIds ?? []).map((tmdbId) => ({
    tmdbId,
    title: `Movie ${tmdbId}`,
    year: 0,
    posterUrl: undefined,
  }));

  return (
    <DraftPoolManager
      draftId={draftId}
      draftName={draft.title}
      accessToken={session.accessToken}
      initialPool={pool}
      initialMovies={initialMovies}
    />
  );
}
