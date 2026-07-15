import { auth } from "@/auth";
import { redirect } from "next/navigation";
import Link from "next/link";
import { getMyDraftDetail, joinDraftPart, startDraftPart } from "@/services/drafts/fetch-my-drafts";
import { parseApiErrorMessage } from "@/lib/parse-api-error";
import DraftTypeBadge from "@/components/ui/draft-type-badge";
import { draftTypeFromNumber } from "@/lib/draft-type-display";
import MyDraftTabs from "./my-draft-tabs";
import { Metadata } from "next";
import type { MyDraftPartDetail } from "@/lib/dto";

export const dynamic = "force-dynamic";

export async function generateMetadata(): Promise<Metadata> {
  return { title: "My Draft" };
}

const STATUS_IN_PROGRESS = 2;
const STATUS_COMPLETED = 3;
const STATUS_CANCELLED = 4;

const PART_STATUS_LABELS: Record<number, string> = {
  0: "Created",
  2: "In Progress",
  3: "Completed",
  4: "Cancelled",
};

export default async function MyDraftDetailPage({
  params,
  searchParams,
}: {
  params: Promise<{ draftId: string }>;
  searchParams: Promise<{ error?: string }>;
}) {
  const session = await auth();
  if (!session?.accessToken) redirect("/sign-in");

  const { draftId } = await params;
  const { error } = await searchParams;
  const detail = await getMyDraftDetail(session.accessToken, draftId);

  if (!detail) redirect("/my-drafts");

  const myRoles = detail.myRoles ?? [];
  const parts = detail.parts ?? [];
  const multiPart = parts.length > 1;
  const isDrafter = myRoles.includes("Drafter");
  const isHost = myRoles.includes("Host");

  const hasPredictorRole = parts.some((p) => p.isPredictor);
  if (myRoles.length === 0 && !hasPredictorRole) redirect("/my-drafts");

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="px-6 md:px-10 py-10 max-w-[1100px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">
          <Link href="/my-drafts" className="hover:text-sd-ink transition-colors">
            / MY DRAFTS
          </Link>
          {" / "}
          {(detail.title ?? "").toUpperCase()}
        </p>

        {error && (
          <div className="flex items-start justify-between gap-4 border border-sd-red/30 bg-sd-red/5 text-sd-ink px-4 py-3 mb-6">
            <p className="font-mono text-sm">{error}</p>
            <Link
              href={`/my-drafts/${draftId}`}
              className="font-mono text-xs text-sd-ink/50 hover:text-sd-ink/80 uppercase tracking-widest shrink-0"
            >
              Dismiss
            </Link>
          </div>
        )}

        <div className="mb-8">
          <div className="flex items-start gap-4 flex-wrap mb-3">
            <h1 className="font-oswald font-bold text-[48px] leading-none text-sd-ink">
              {detail.title}
            </h1>
            <DraftTypeBadge type={draftTypeFromNumber(detail.draftType)} />
          </div>

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

          {multiPart && (
            <div className="flex flex-wrap gap-3">
              {parts.map((part) => (
                <PartStrip
                  key={part.draftPartPublicId ?? ""}
                  part={part}
                  draftId={draftId}
                  accessToken={session.accessToken!}
                />
              ))}
            </div>
          )}

          {!multiPart && parts[0] && (
            <SinglePartAction
              part={parts[0]}
              draftId={draftId}
              accessToken={session.accessToken!}
            />
          )}
        </div>

        <MyDraftTabs detail={detail} accessToken={session.accessToken} />
      </div>
    </div>
  );
}

function PartAction({
  part,
  draftId,
  accessToken,
}: {
  part: MyDraftPartDetail;
  draftId: string;
  accessToken: string;
}) {
  const draftPartPublicId = part.draftPartPublicId ?? "";
  const status = part.status ?? 0;
  const isHost = part.isHost ?? false;
  const isJoined = part.attendanceStatus === "Joined";
  const isInProgress = status === STATUS_IN_PROGRESS;
  const isFinished = status === STATUS_COMPLETED || status === STATUS_CANCELLED;

  if (isHost) {
    if (isFinished) {
      return (
        <Link
          href={`/draft-parts/${draftPartPublicId}/live`}
          className="border border-sd-ink/20 text-sd-ink font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-ink/5"
        >
          View
        </Link>
      );
    }
    if (isInProgress) {
      return (
        <Link
          href={`/draft-parts/${draftPartPublicId}/live`}
          className="bg-sd-ink text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-ink/80"
        >
          Open
        </Link>
      );
    }
    return (
      <form
        action={async () => {
          "use server";
          try {
            await startDraftPart(accessToken, draftId, part.partIndex ?? 1);
          } catch (err) {
            const message = parseApiErrorMessage(err, "Couldn't start the draft. Please try again.");
            redirect(`/my-drafts/${draftId}?error=${encodeURIComponent(message)}`);
          }
          redirect(`/draft-parts/${draftPartPublicId}/live`);
        }}
      >
        <button
          type="submit"
          className="bg-sd-red text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-red/90"
        >
          Start
        </button>
      </form>
    );
  }

  if (!isJoined) {
    return (
      <form
        action={async () => {
          "use server";
          try {
            await joinDraftPart(accessToken, draftPartPublicId);
          } catch (err) {
            const message = parseApiErrorMessage(err, "Couldn't join the draft. Please try again.");
            redirect(`/my-drafts/${draftId}?error=${encodeURIComponent(message)}`);
          }
          redirect(`/draft-parts/${draftPartPublicId}/live`);
        }}
      >
        <button
          type="submit"
          className="bg-sd-blue text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-blue/90"
        >
          Join
        </button>
      </form>
    );
  }

  if (isFinished) {
    return (
      <Link
        href={`/draft-parts/${draftPartPublicId}/live`}
        className="border border-sd-ink/20 text-sd-ink font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-ink/5"
      >
        View
      </Link>
    );
  }

  if (isInProgress) {
    return (
      <Link
        href={`/draft-parts/${draftPartPublicId}/live`}
        className="bg-sd-ink text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-ink/80"
      >
        Open
      </Link>
    );
  }

  // Joined, Created — prep page
  return (
    <Link
      href={`/my-drafts/${draftId}`}
      className="bg-sd-ink text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-ink/80"
    >
      Open
    </Link>
  );
}

function SinglePartAction({
  part,
  draftId,
  accessToken,
}: {
  part: MyDraftPartDetail;
  draftId: string;
  accessToken: string;
}) {
  return (
    <div className="flex items-center gap-3">
      <p className="font-mono text-xs text-sd-ink/50">
        {PART_STATUS_LABELS[part.status ?? -1] ?? "Unknown"}
      </p>
      <PartAction part={part} draftId={draftId} accessToken={accessToken} />
    </div>
  );
}

function PartStrip({
  part,
  draftId,
  accessToken,
}: {
  part: MyDraftPartDetail;
  draftId: string;
  accessToken: string;
}) {
  return (
    <div className="flex items-center gap-3 border border-sd-ink/20 bg-white px-4 py-2">
      <div>
        <p className="font-oswald font-bold text-sm uppercase text-sd-ink">
          Part {part.partIndex}
        </p>
        <p className="font-mono text-xs text-sd-ink/50">
          {PART_STATUS_LABELS[part.status ?? -1] ?? "Unknown"}
        </p>
      </div>
      <PartAction part={part} draftId={draftId} accessToken={accessToken} />
    </div>
  );
}