import { auth } from "@/auth";
import { redirect } from "next/navigation";
import Link from "next/link";
import { getMyDraftDetail, joinDraftPart } from "@/services/drafts/fetch-my-drafts";
import DraftTypeBadge from "@/components/ui/draft-type-badge";
import { draftTypeFromNumber } from "@/lib/draft-type-display";
import MyDraftTabs from "./my-draft-tabs";
import { Metadata } from "next";
import type { MyDraftPartDetail } from "@/lib/dto";

export const dynamic = "force-dynamic";

export async function generateMetadata(): Promise<Metadata> {
  return { title: "My Draft" };
}

const PART_STATUS_LABELS: Record<number, string> = {
  0: "Created",
  2: "In Progress",
  3: "Completed",
  4: "Cancelled",
};

export default async function MyDraftDetailPage({
  params,
}: {
  params: Promise<{ draftId: string }>;
}) {
  const session = await auth();
  if (!session?.accessToken) redirect("/sign-in");

  const { draftId } = await params;
  const detail = await getMyDraftDetail(session.accessToken, draftId);

  if (!detail) redirect("/my-drafts");

  const myRoles = detail.myRoles ?? [];
  const parts = detail.parts ?? [];
  const multiPart = parts.length > 1;
  const isDrafter = myRoles.includes("Drafter");
  const isHost = myRoles.includes("Host");

  if (myRoles.length === 0 && !detail.isSurrogate) redirect("/my-drafts");

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="px-6 md:px-10 py-10 max-w-[1100px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">
          <Link href="/my-drafts" className="hover:text-sd-ink transition-colors">/ MY DRAFTS</Link>
          {" / "}{(detail.title ?? "").toUpperCase()}
        </p>

        {/* Header */}
        <div className="mb-8">
          <div className="flex items-start gap-4 flex-wrap mb-3">
            <h1 className="font-oswald font-bold text-[48px] leading-none text-sd-ink">
              {detail.title}
            </h1>
            <DraftTypeBadge type={draftTypeFromNumber(detail.draftType)} />
          </div>

          {/* Role badges */}
          <div className="flex gap-2 mb-6">
            {isDrafter && (
              <span className="font-mono text-[10px] tracking-widest uppercase px-2 py-1 bg-sd-blue/10 text-sd-blue border border-sd-blue/20">
                Drafter
              </span>
            )}
            {isHost && (
              <span className="font-mono text-[10px] tracking-widest uppercase px-2 py-1 bg-sd-ink/10 text-sd-ink border border-sd-ink/20">
                Host
              </span>
            )}
          </div>

          {/* Part strip — only shown when more than one part */}
          {multiPart && (
            <div className="flex flex-wrap gap-3">
              {parts.map((part) => (
                <PartStrip
                  key={part.draftPartPublicId ?? ""}
                  part={part}
                  draftId={draftId}
                  accessToken={session.accessToken!}
                  statusLabels={PART_STATUS_LABELS}
                  personPublicId={session.personPublicId ?? ""}
                />
              ))}
            </div>
          )}

          {/* Single part — show join/open inline under the role badges */}
          {!multiPart && parts[0] && (
            <SinglePartAction
              part={parts[0]}
              draftId={draftId}
              accessToken={session.accessToken!}
              statusLabels={PART_STATUS_LABELS}
              personPublicId={session.personPublicId ?? ""}
            />
          )}
        </div>

        {/* Tabs */}
        <MyDraftTabs detail={detail} accessToken={session.accessToken} />
      </div>
    </div>
  );
}

function SinglePartAction({
  part,
  draftId,
  accessToken,
  statusLabels,
  personPublicId,
}: {
  part: MyDraftPartDetail;
  draftId: string;
  accessToken: string;
  statusLabels: Record<number, string>;
  personPublicId: string;
}) {
  const isJoined = part.attendanceStatus === "Joined";
  const isCompleted = (part.status ?? 0) === 3 || (part.status ?? 0) === 4;
  const draftPartPublicId = part.draftPartPublicId ?? "";

  return (
    <div className="flex items-center gap-3">
      <p className="font-mono text-xs text-sd-ink/50">
        {statusLabels[part.status ?? -1] ?? "Unknown"}
      </p>
      {!isJoined ? (
        <form
          action={async () => {
            "use server";
            await joinDraftPart(accessToken, draftPartPublicId, personPublicId);
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
      ) : isCompleted ? (
        <Link
          href={`/draft-parts/${draftPartPublicId}/live`}
          className="border border-sd-ink/20 text-sd-ink font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-ink/5"
        >
          View
        </Link>
      ) : (
        <Link
          href={`/draft-parts/${draftPartPublicId}/live`}
          className="bg-sd-blue text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-blue/90"
        >
          Open
        </Link>
      )}
    </div>
  );
}

function PartStrip({
  part,
  draftId,
  accessToken,
  statusLabels,
  personPublicId,
}: {
  part: MyDraftPartDetail;
  draftId: string;
  accessToken: string;
  statusLabels: Record<number, string>;
  personPublicId: string;
}) {
  const isJoined = part.attendanceStatus === "Joined";
  const isCompleted = (part.status ?? 0) === 3 || (part.status ?? 0) === 4;
  const draftPartPublicId = part.draftPartPublicId ?? "";

  return (
    <div className="flex items-center gap-3 border border-sd-ink/20 bg-white px-4 py-2">
      <div>
        <p className="font-oswald font-bold text-sm uppercase text-sd-ink">Part {part.partIndex}</p>
        <p className="font-mono text-xs text-sd-ink/50">{statusLabels[part.status ?? -1] ?? "Unknown"}</p>
      </div>
      {!isJoined ? (
        <form
          action={async () => {
            "use server";
            await joinDraftPart(accessToken, draftPartPublicId, personPublicId);
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
      ) : isCompleted ? (
        <Link
          href={`/draft-parts/${draftPartPublicId}/live`}
          className="border border-sd-ink/20 text-sd-ink font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-ink/5"
        >
          View
        </Link>
      ) : (
        <Link
          href={`/draft-parts/${draftPartPublicId}/live`}
          className="bg-sd-blue text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-blue/90"
        >
          Open
        </Link>
      )}
    </div>
  );
}