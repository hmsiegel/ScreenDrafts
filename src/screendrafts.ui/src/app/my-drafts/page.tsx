import { auth } from "@/auth";
import { redirect } from "next/navigation";
import Link from "next/link";
import { getMyDrafts, joinDraftPart, type MyDraftSummary } from "@/services/drafts/fetch-my-drafts";
import DraftTypeBadge from "@/components/ui/draft-type-badge";
import { draftTypeFromNumber } from "@/lib/draft-type-display";
import { Metadata } from "next";

export const metadata: Metadata = { title: "My Drafts" };
export const dynamic = "force-dynamic";

function SectionHeading({ children }: { children: React.ReactNode }) {
  return (
    <h2 className="font-oswald font-bold text-[28px] uppercase tracking-wide text-sd-ink mb-4">
      {children}
    </h2>
  );
}

function DraftCard({ draft, accessToken }: { draft: MyDraftSummary; accessToken: string }) {
  return (
    <div className="bg-white border border-sd-ink/10">
      <div className="flex items-center gap-3 px-4 py-3 border-b border-sd-ink/10 bg-sd-ink">
        <div className="w-1 h-4 bg-sd-red shrink-0" />
        <span className="font-oswald font-bold text-sm uppercase tracking-wide text-white flex-1 truncate">
          {draft.name}
        </span>
        <DraftTypeBadge type={draftTypeFromNumber(draft.draftType?.value)} />
      </div>
      <div className="p-4 space-y-3">
        {draft.episodeNumber != null && (
          <p className="font-mono text-xs text-sd-ink/50">Episode {draft.episodeNumber}</p>
        )}
        <div className="space-y-2">
          {draft.parts.map((part) => (
            <div key={part.draftPartId} className="flex items-center justify-between border border-sd-ink/10 px-3 py-2">
              <div>
                <p className="font-mono text-xs text-sd-ink/60 uppercase tracking-wide">
                  Part {part.partNumber}
                </p>
                {part.releaseDate && (
                  <p className="font-mono text-xs text-sd-ink/40">{part.releaseDate}</p>
                )}
              </div>
              <PartActionButton
                draftId={draft.draftId}
                part={part}
                accessToken={accessToken}
              />
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

function PartActionButton({
  draftId,
  part,
  accessToken,
}: {
  draftId: string;
  part: MyDraftSummary["parts"][number];
  accessToken: string;
}) {
  if (!part.isJoined) {
    return (
      <form
        action={async () => {
          'use server';
          await joinDraftPart(accessToken, part.draftPartId);
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
    );
  }
  return (
    <Link
      href={`/my-drafts/${draftId}`}
      className="bg-sd-ink text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-ink/80"
    >
      Open
    </Link>
  );
}

function CompletedDraftCard({ draft }: { draft: MyDraftSummary }) {
  return (
    <Link href={`/my-drafts/${draft.draftId}`} className="block bg-white border border-sd-ink/10 hover:border-sd-ink/30 transition-colors">
      <div className="flex items-center gap-3 px-4 py-3 border-b border-sd-ink/10">
        <span className="font-oswald font-bold text-sm uppercase tracking-wide text-sd-ink flex-1 truncate">
          {draft.name}
        </span>
        <DraftTypeBadge type={draftTypeFromNumber(draft.draftType?.value)} />
      </div>
      <div className="px-4 py-3">
        {draft.episodeNumber != null && (
          <p className="font-mono text-xs text-sd-ink/50">Episode {draft.episodeNumber}</p>
        )}
        <p className="font-mono text-xs text-sd-ink/40 mt-1">{draft.parts.length} part{draft.parts.length !== 1 ? "s" : ""}</p>
      </div>
    </Link>
  );
}

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
          {upcoming.length === 0 ? (
            <p className="font-mono text-sm text-sd-ink/40">No upcoming drafts.</p>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
              {upcoming.map((d) => (
                <DraftCard key={d.draftId} draft={d} accessToken={session.accessToken!} />
              ))}
            </div>
          )}
        </section>

        <section>
          <SectionHeading>In Progress</SectionHeading>
          {inProgress.length === 0 ? (
            <p className="font-mono text-sm text-sd-ink/40">No drafts in progress.</p>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
              {inProgress.map((d) => (
                <DraftCard key={d.draftId} draft={d} accessToken={session.accessToken!} />
              ))}
            </div>
          )}
        </section>

        <section>
          <SectionHeading>Completed</SectionHeading>
          {completed.length === 0 ? (
            <p className="font-mono text-sm text-sd-ink/40">No completed drafts.</p>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
              {completed.map((d) => (
                <CompletedDraftCard key={d.draftId} draft={d} />
              ))}
            </div>
          )}
        </section>
      </div>
    </div>
  );
}
