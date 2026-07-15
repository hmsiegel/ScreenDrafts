import { auth } from "@/auth";
import { redirect } from "next/navigation";
import Link from "next/link";
import { getMyDrafts, joinDraftPart, startDraftPart } from "@/services/drafts/fetch-my-drafts";
import { parseApiErrorMessage } from "@/lib/parse-api-error";
import DraftTypeBadge from "@/components/ui/draft-type-badge";
import { draftTypeFromNumber } from "@/lib/draft-type-display";
import { Metadata } from "next";
import type { MyDraftSummary, MyDraftPartSummary } from "@/lib/dto";
import MyDraftsRealtimeRefresher from "./my-drafts-realtime-refresher";

export const metadata: Metadata = { title: "My Drafts" };
export const dynamic = "force-dynamic";

const STATUS_IN_PROGRESS = 2;
const STATUS_COMPLETED = 3;
const STATUS_CANCELLED = 4;

function SectionHeading({ children }: { children: React.ReactNode }) {
  return (
    <h2 className="font-oswald font-bold text-[28px] uppercase tracking-wide text-sd-ink mb-4">
      {children}
    </h2>
  );
}

function RoleBadge({ isDrafter, isHost }: { isDrafter: boolean; isHost: boolean }) {
  return (
    <div className="flex gap-1">
      {isDrafter && (
        <span className="font-mono text-[9px] tracking-widest uppercase px-1.5 py-0.5 bg-sd-blue/10 text-sd-blue border border-sd-blue/20">
          Drafter
        </span>
      )}
      {isHost && (
        <span className="font-mono text-[9px] tracking-widest uppercase px-1.5 py-0.5 bg-sd-ink/10 text-sd-ink border border-sd-ink/20">
          Host
        </span>
      )}
    </div>
  );
}

function DraftListRow({
  draft,
  accessToken,
}: {
  draft: MyDraftSummary;
  accessToken: string;
}) {
  const parts = draft.parts ?? [];
  const multiPart = parts.length > 1;
  const isDrafter = parts.some((p) => p.isDrafter ?? false);
  const isHost = parts.some((p) => p.isHost ?? false);

  return (
    <div className="bg-white border border-sd-ink/10">
      <div className="flex items-center gap-4 px-4 py-3">
        <div className="w-1 h-8 bg-sd-red shrink-0" />
        <div className="flex-1 min-w-0">
          <Link
            href={`/my-drafts/${draft.draftPublicId ?? ""}`}
            className="font-oswald font-bold text-[15px] uppercase tracking-wide text-sd-ink hover:text-sd-blue transition-colors truncate block"
          >
            {draft.title}
          </Link>
          <div className="flex items-center gap-2 mt-0.5">
            <RoleBadge isDrafter={isDrafter} isHost={isHost} />
            {multiPart && (
              <span className="font-mono text-[9px] text-sd-ink/40">{parts.length} parts</span>
            )}
          </div>
        </div>
        <DraftTypeBadge type={draftTypeFromNumber(draft.draftType)} />
        {!multiPart && parts[0] && (
          <PartActionButton
            draftPublicId={draft.draftPublicId ?? ""}
            part={parts[0]}
            accessToken={accessToken}
          />
        )}
      </div>

      {multiPart && (
        <div className="border-t border-sd-ink/5">
          {parts.map((part) => (
            <div
              key={part.draftPartPublicId ?? ""}
              className="flex items-center justify-between px-8 py-2 border-b border-sd-ink/5 last:border-b-0"
            >
              <div className="flex items-center gap-3">
                <p className="font-mono text-xs text-sd-ink/60 uppercase tracking-wide">
                  Part {part.partIndex}
                </p>
                {part.releaseDate && (
                  <p className="font-mono text-xs text-sd-ink/40">
                    {new Date(part.releaseDate).toLocaleDateString()}
                  </p>
                )}
              </div>
              <PartActionButton
                draftPublicId={draft.draftPublicId ?? ""}
                part={part}
                accessToken={accessToken}
              />
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

function PartActionButton({
  draftPublicId,
  part,
  accessToken,
}: {
  draftPublicId: string;
  part: MyDraftPartSummary;
  accessToken: string;
}) {
  const draftPartPublicId = part.draftPartPublicId ?? "";
  const status = part.status ?? 0;
  const isHost = part.isHost ?? false;
  const isJoined = part.attendanceStatus === "Joined";
  const isInProgress = status === STATUS_IN_PROGRESS;
  const isFinished = status === STATUS_COMPLETED || status === STATUS_CANCELLED;

  // Host flow — no attendance, go straight to live
  if (isHost) {
    if (isFinished) {
      return (
        <Link
          href={`/draft-parts/${draftPartPublicId}/live`}
          className="border border-sd-ink/20 text-sd-ink font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-ink/5 shrink-0"
        >
          View
        </Link>
      );
    }
    if (isInProgress) {
      return (
        <Link
          href={`/draft-parts/${draftPartPublicId}/live`}
          className="bg-sd-ink text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-ink/80 shrink-0"
        >
          Open
        </Link>
      );
    }
    // Created — START button
    return (
      <form
        action={async () => {
          "use server";
          try {
            await startDraftPart(accessToken, draftPublicId, part.partIndex ?? 1);
          } catch (err) {
            const message = parseApiErrorMessage(err, "Couldn't start the draft. Please try again.");
            redirect(`/my-drafts?error=${encodeURIComponent(message)}`);
          }
          redirect(`/draft-parts/${draftPartPublicId}/live`);
        }}
      >
        <button
          type="submit"
          className="bg-sd-red text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-red/90 shrink-0"
        >
          Start
        </button>
      </form>
    );
  }

  // Drafter / other flow
  if (!isJoined) {
    return (
      <form
        action={async () => {
          "use server";
          try {
            await joinDraftPart(accessToken, draftPartPublicId);
          } catch (err) {
            const message = parseApiErrorMessage(err, "Couldn't join the draft. Please try again.");
            redirect(`/my-drafts?error=${encodeURIComponent(message)}`);
          }
          redirect(`/draft-parts/${draftPartPublicId}/live`);
        }}
      >
        <button
          type="submit"
          className="bg-sd-blue text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-blue/90 shrink-0"
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
        className="border border-sd-ink/20 text-sd-ink font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-ink/5 shrink-0"
      >
        View
      </Link>
    );
  }

  if (isInProgress) {
    return (
      <Link
        href={`/draft-parts/${draftPartPublicId}/live`}
        className="bg-sd-ink text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-ink/80 shrink-0"
      >
        Open
      </Link>
    );
  }

  // Joined, Created — go to prep page (board/candidate list)
  return (
    <Link
      href={`/my-drafts/${draftPublicId}`}
      className="bg-sd-ink text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-ink/80 shrink-0"
    >
      Open
    </Link>
  );
}

function CompletedDraftCard({ draft }: { draft: MyDraftSummary }) {
  const parts = draft.parts ?? [];
  const isDrafter = parts.some((p) => p.isDrafter ?? false);
  const isHost = parts.some((p) => p.isHost ?? false);

  return (
    <Link
      href={`/my-drafts/${draft.draftPublicId ?? ""}`}
      className="block bg-white border border-sd-ink/10 hover:border-sd-ink/30 transition-colors"
    >
      <div className="flex items-center gap-3 px-4 py-3 border-b border-sd-ink/10">
        <span className="font-oswald font-bold text-sm uppercase tracking-wide text-sd-ink flex-1 truncate">
          {draft.title}
        </span>
        <DraftTypeBadge type={draftTypeFromNumber(draft.draftType)} />
      </div>
      <div className="px-4 py-3 space-y-2">
        <RoleBadge isDrafter={isDrafter} isHost={isHost} />
        {parts.length > 1 && (
          <p className="font-mono text-xs text-sd-ink/40">{parts.length} parts</p>
        )}
      </div>
    </Link>
  );
}

export default async function MyDraftsPage({
  searchParams,
}: {
  searchParams: Promise<{ error?: string }>;
}) {
  const session = await auth();
  if (!session?.accessToken) redirect("/sign-in");

  const { error } = await searchParams;
  const { upcoming, inProgress, completed } = await getMyDrafts(session.accessToken);
  const STATUS_CREATED = 0;

  const watchedDraftPartIds = (upcoming ?? [])
    .flatMap((d) => d.parts ?? [])
    .filter((p) => (p.status ?? STATUS_CREATED) === STATUS_CREATED)
    .map((p) => p.draftPartPublicId ?? '')
    .filter((id) => id.length > 0);

  return (
    <div className="min-h-screen bg-light-blue">
      <MyDraftsRealtimeRefresher
        accessToken={session.accessToken!}
        watchedDraftPartIds={watchedDraftPartIds}
      />

      <div className="px-6 md:px-10 py-10 max-w-[1200px] mx-auto space-y-12">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50">/ MY DRAFTS</p>
        <h1 className="font-oswald font-bold text-[56px] leading-none text-sd-ink -mt-4">
          MY DRAFTS
        </h1>

        {error && (
          <div className="flex items-start justify-between gap-4 border border-sd-red/30 bg-sd-red/5 text-sd-ink px-4 py-3 -mt-8">
            <p className="font-mono text-sm">{error}</p>
            <Link
              href="/my-drafts"
              className="font-mono text-xs text-sd-ink/50 hover:text-sd-ink/80 uppercase tracking-widest shrink-0"
            >
              Dismiss
            </Link>
          </div>
        )}

        <section>
          <SectionHeading>Upcoming</SectionHeading>
          {(upcoming ?? []).length === 0 ? (
            <p className="font-mono text-sm text-sd-ink/40">No upcoming drafts.</p>
          ) : (
            <div className="space-y-2">
              {(upcoming ?? []).map((d) => (
                <DraftListRow key={d.draftPublicId ?? ""} draft={d} accessToken={session.accessToken!} />
              ))}
            </div>
          )}
        </section>

        <section>
          <SectionHeading>In Progress</SectionHeading>
          {(inProgress ?? []).length === 0 ? (
            <p className="font-mono text-sm text-sd-ink/40">No drafts in progress.</p>
          ) : (
            <div className="space-y-2">
              {(inProgress ?? []).map((d) => (
                <DraftListRow key={d.draftPublicId ?? ""} draft={d} accessToken={session.accessToken!} />
              ))}
            </div>
          )}
        </section>

        <section>
          <SectionHeading>Completed</SectionHeading>
          {(completed ?? []).length === 0 ? (
            <p className="font-mono text-sm text-sd-ink/40">No completed drafts.</p>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
              {(completed ?? []).map((d) => (
                <CompletedDraftCard key={d.draftPublicId ?? ""} draft={d} />
              ))}
            </div>
          )}
        </section>
      </div>
    </div>
  );
}