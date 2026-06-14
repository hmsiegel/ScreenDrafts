import { auth } from "@/auth";
import { redirect } from "next/navigation";
import Link from "next/link";
import { getMyDrafts, joinDraftPart } from "@/services/drafts/fetch-my-drafts";
import DraftTypeBadge from "@/components/ui/draft-type-badge";
import { draftTypeFromNumber } from "@/lib/draft-type-display";
import { Metadata } from "next";
import type { MyDraftSummary, MyDraftPartSummary } from "@/lib/dto";

export const metadata: Metadata = { title: "My Drafts" };
export const dynamic = "force-dynamic";

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

// ── List row for upcoming / in-progress ──────────────────────────────────────

function DraftListRow({
  draft,
  accessToken,
  personPublicId,
}: {
  draft: MyDraftSummary;
  accessToken: string;
  personPublicId: string;
}) {
  const parts = draft.parts ?? [];
  const multiPart = parts.length > 1;
  const isDrafter = parts.some((p) => p.isDrafter ?? false);
  const isHost = parts.some((p) => p.isHost ?? false);

  return (
    <div className="bg-white border border-sd-ink/10">
      {/* Main row */}
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
        {/* Single-part action inline */}
        {!multiPart && parts[0] && (
          <PartActionButton
            draftPublicId={draft.draftPublicId ?? ""}
            part={parts[0]}
            accessToken={accessToken}
            personPublicId={personPublicId}
          />
        )}
      </div>

      {/* Multi-part sub-rows */}
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
                personPublicId={personPublicId}
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
  personPublicId,
}: {
  draftPublicId: string;
  part: MyDraftPartSummary;
  accessToken: string;
  personPublicId: string;
}) {
  const isJoined = part.attendanceStatus === "Joined";

  if (!isJoined) {
    return (
      <form
        action={async () => {
          "use server";
          await joinDraftPart(accessToken, part.draftPartPublicId ?? "", personPublicId);
          redirect(`/my-drafts/${draftPublicId}`);
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

  return (
    <Link
      href={`/my-drafts/${draftPublicId}`}
      className="bg-sd-ink text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-ink/80 shrink-0"
    >
      Open
    </Link>
  );
}

// ── Card for completed ────────────────────────────────────────────────────────

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

// ── Page ──────────────────────────────────────────────────────────────────────

export default async function MyDraftsPage() {
  const session = await auth();
  if (!session?.accessToken) redirect("/sign-in");

  const { upcoming, inProgress, completed } = await getMyDrafts(session.accessToken);

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="px-6 md:px-10 py-10 max-w-[1200px] mx-auto space-y-12">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50">/ MY DRAFTS</p>
        <h1 className="font-oswald font-bold text-[56px] leading-none text-sd-ink -mt-4">MY DRAFTS</h1>

        <section>
          <SectionHeading>Upcoming</SectionHeading>
          {(upcoming ?? []).length === 0 ? (
            <p className="font-mono text-sm text-sd-ink/40">No upcoming drafts.</p>
          ) : (
            <div className="space-y-2">
              {(upcoming ?? []).map((d) => (
                <DraftListRow key={d.draftPublicId ?? ""} draft={d} accessToken={session.accessToken!} personPublicId={session.personPublicId ?? ""} />
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
                <DraftListRow key={d.draftPublicId ?? ""} draft={d} accessToken={session.accessToken!} personPublicId={session.personPublicId ?? ""} />
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