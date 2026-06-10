import { auth } from "@/auth";
import { redirect } from "next/navigation";
import { getDraft, getDraftPool, getMediaByTmdbIds } from "@/services/admin/fetch-admin-drafts";
import DraftPoolManager from "./draft-pool-manager";
import { Metadata } from "next";

export const metadata: Metadata = { title: "Draft Pool" };
export const dynamic = "force-dynamic";

const SUPER_DRAFT_TYPE = 3;

export default async function DraftPoolPage({
  params,
}: {
  params: Promise<{ draftId: string }>;
}) {
  const session = await auth();
  if (!session?.accessToken) redirect("/sign-in");

  const { draftId } = await params;
  const draft = await getDraft(session.accessToken, draftId);

  if (!draft) redirect("/admin/drafts");
  if (draft.draftType?.value !== SUPER_DRAFT_TYPE) redirect("/admin/drafts");

  const pool = await getDraftPool(session.accessToken, draftId);
  const tmdbIds = (pool?.tmdbIds ?? []) as number[];

  const mediaResponse = tmdbIds.length > 0
    ? await getMediaByTmdbIds(session.accessToken, tmdbIds)
    : { items: [] };

  const mediaItems = mediaResponse.items ?? [];

  const mediaByTmdbId = new Map(mediaItems.map((m) => [m.tmdbId, m]));

  const initialMovies = tmdbIds.map((tmdbId) => {
    const meta = mediaByTmdbId.get(tmdbId);
    return {
      tmdbId,
      title: meta?.title ?? `TMDb #${tmdbId}`,
      year: meta?.year ?? null,
    };
  });

  return (
    <DraftPoolManager
      draftId={draftId}
      draftName={draft.title ?? ""}
      accessToken={session.accessToken}
      initialPool={pool}
      initialMovies={initialMovies}
    />
  );
}