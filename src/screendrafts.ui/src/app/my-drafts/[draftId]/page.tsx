import { auth } from "@/auth";
import { redirect } from "next/navigation";
import Link from "next/link";
import {
  getMyDraftDetail,
  joinDraftPart,
} from "@/services/drafts/fetch-my-drafts";
import DraftTypeBadge from "@/components/ui/draft-type-badge";
import { draftTypeFromNumber } from "@/lib/draft-type-display";
import MyDraftTabs from "./my-draft-tabs";
import { Metadata } from "next";

export const dynamic = "force-dynamic";

export async function generateMetadata({ params }: { params: Promise<{ draftId: string }> }): Promise<Metadata> {
  return { title: "My Draft" };
}

const STATUS_LABELS: Record<number, string> = {
  0: "Created",
  1: "In Progress",
  2: "Active",
  3: "Paused",
  4: "Completed",
  5: "Cancelled",
};

export default async function MyDraftDetailPage({ params }: { params: Promise<{ draftId: string }> }) {
  const session = await auth();
  if (!session?.accessToken) redirect("/sign-in");

  const { draftId } = await params;
  const detail = await getMyDraftDetail(session.accessToken, draftId);

  if (!detail) redirect("/my-drafts");
  if (detail.myRoles.length === 0 && !detail.isSurrogate) redirect("/my-drafts");

  const { draft, parts } = detail;

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="px-6 md:px-10 py-10 max-w-[1100px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">
          / MY DRAFTS / {draft.title.toUpperCase()}
        </p>

        {/* Header */}
        <div className="mb-8">
          <div className="flex items-start gap-4 flex-wrap">
            <div>
              <h1 className="font-oswald font-bold text-[48px] leading-none text-sd-ink">
                {draft.title}
              </h1>
              {draft.episodeNumber != null && (
                <p className="font-mono text-xs text-sd-ink/50 mt-1">Episode {draft.episodeNumber}</p>
              )}
            </div>
            <DraftTypeBadge type={draftTypeFromNumber(draft.draftType?.value)} />
          </div>

          {/* Part strip */}
          <div className="flex flex-wrap gap-3 mt-6">
            {parts.map((part) => (
              <div
                key={part.draftPartId}
                className="flex items-center gap-3 border border-sd-ink/20 bg-white px-4 py-2"
              >
                <div>
                  <p className="font-oswald font-bold text-sm uppercase text-sd-ink">
                    Part {part.partNumber}
                  </p>
                  <p className="font-mono text-xs text-sd-ink/50">
                    {STATUS_LABELS[part.status?.value ?? -1] ?? "Unknown"}
                  </p>
                </div>
                {!part.isJoined ? (
                  <form
                    action={async () => {
                      'use server';
                      await joinDraftPart(session.accessToken!, part.draftPartId);
                      redirect(`/my-drafts/${draftId}`);
                    }}
                  >
                    <button
                      type="submit"
                      className="bg-sd-blue text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-blue/90"
                    >
                      Join
                    </button>
                  </form>
                ) : part.status?.value === 4 ? (
                  <Link
                    href={`/gameplay/${part.draftPartId}`}
                    className="border border-sd-ink/20 text-sd-ink font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-ink/5"
                  >
                    View
                  </Link>
                ) : (
                  <Link
                    href={`/gameplay/${part.draftPartId}`}
                    className="bg-sd-blue text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-blue/90"
                  >
                    Open
                  </Link>
                )}
              </div>
            ))}
          </div>
        </div>

        {/* Tabs */}
        <MyDraftTabs detail={detail} accessToken={session.accessToken} />
      </div>
    </div>
  );
}
