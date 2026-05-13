import { Metadata } from "next";
import { notFound } from "next/navigation";
import Link from "next/link";
import { getDraftDetails } from "@/features/drafts/api/fetch-drafts";
import { GetDraftPartResponse, GetDraftPickResponse } from "@/lib/dto";
import SiteHeader from "@/components/layout/header/site-header";
import SiteFooter from "@/components/layout/footer/site-footer";
import DraftSidebar from "@/components/features/drafts/drafts-sidebar";
import { DraftPick } from "@/components/features/drafts/draft-pick";

export const dynamic = "force-dynamic";

type Props = { params: Promise<{ id: string }> };

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { id } = await params;
  try {
    const draft = await getDraftDetails(id);
    return {
      title: draft.title,
      description: draft.description ?? undefined,
    };
  } catch {
    return { title: "Draft" };
  }
}

export default async function DraftDetailPage({ params }: Props) {
  const { id } = await params;

  let draft;
  try {
    draft = await getDraftDetails(id);
  } catch {
    notFound();
  }

  // Build participant name map from parts
  // GetDraftPartParticipantResponse has [key: string]: any so displayName may exist at runtime
  const participantNames = new Map<string, string>();
  const participantIndex = new Map<string, number>();
  let pIdx = 0;

  for (const part of draft.parts ?? []) {
    for (const p of part.participants ?? []) {
      const pid = p.participantIdValue ?? "";
      if (pid && !participantNames.has(pid)) {
        const name = (p as Record<string, unknown>).displayName as string | undefined;
        participantNames.set(pid, name ?? pid);
        participantIndex.set(pid, pIdx++);
      }
    }
  }

  // Flatten all picks from all parts, sorted by position
  const allPicks: { pick: GetDraftPickResponse; partIdx: number }[] = [];
  for (let pi = 0; pi < (draft.parts ?? []).length; pi++) {
    const part: GetDraftPartResponse = (draft.parts ?? [])[pi];
    for (const pick of part.picks ?? []) {
      allPicks.push({ pick, partIdx: pi });
    }
  }
  allPicks.sort((a, b) => (a.pick.playOrder ?? 0) - (b.pick.playOrder ?? 0));

  const isVetoed = (pick: GetDraftPickResponse) =>
    !!pick.veto && !pick.veto.isOverriden;
  const isCommissionerRemoved = (pick: GetDraftPickResponse) =>
    pick.commissionerOverride !== null && pick.commissionerOverride !== undefined;

  const finalPicks = allPicks.filter(({ pick }) =>
    !isVetoed(pick) && !isCommissionerRemoved(pick)
  );

  const totalPicks = finalPicks.length;

  // The top pick is the last pick placed by play order among final picks
  const topPlayOrder = finalPicks.reduce(
    (max, { pick }) => Math.max(max, pick.playOrder ?? 0),
    0
  );

  return (
    <div className="min-h-screen bg-light-blue">
      <SiteHeader activePath="/drafts" />

      <div style={{ padding: "40px 40px 64px" }}>
        {/* Breadcrumb */}
        <nav className="font-mono text-[11px] mb-8 flex items-center gap-1.5">
          <Link href="/drafts" className="text-sd-blue hover:underline">
            / DRAFTS
          </Link>
          <span className="text-sd-ink/30">/</span>
          <span className="text-sd-ink/60 truncate max-w-[400px]">{draft.title}</span>
        </nav>

        {/* Two-column grid */}
        <div
          className="grid gap-10 mx-auto"
          style={{ gridTemplateColumns: "380px 1fr", maxWidth: 1400 }}
        >
          {/* Left sidebar */}
          <DraftSidebar draft={draft} participantNames={participantNames} />

          {/* Right column */}
          <div className="bg-white border-2 border-sd-ink" style={{ padding: "40px 48px" }}>
            <article style={{ maxWidth: 720 }}>
              {/* Section label */}
              <div className="font-mono text-[11px] tracking-widest text-sd-red font-bold mb-3">
                ★ THE EPISODE
              </div>

              {/* Title */}
              <h1 className="font-oswald font-bold text-[56px] text-sd-ink leading-[1] mb-6">
                {draft.title}
              </h1>

              {/* Description */}
              {draft.description && (
                <p className="font-serif text-[18px] leading-[1.55] text-[#2a2f44] mb-10 max-w-[680px]">
                  {draft.description}
                </p>
              )}

              {/* Divider + pick count */}
              <div className="flex items-center gap-4 mb-2">
                <h2 className="font-oswald font-bold text-[32px] text-sd-ink whitespace-nowrap">
                  THE FINAL LIST
                </h2>
                <div className="flex-1 h-0.5 bg-sd-ink" />
                <span className="font-mono text-[11px] text-sd-ink/60 whitespace-nowrap">
                  {totalPicks} PICKS
                </span>
              </div>

              {/* Picks */}
              <div className="mt-6">
                {allPicks.length === 0 ? (
                  <p className="font-mono text-sm text-sd-ink/40">No picks recorded.</p>
                ) : (
                  allPicks.map(({ pick }, i) => (
                    <DraftPick
                      key={`${pick.moviePublicId}-${pick.playOrder ?? i}`}
                      pick={pick}
                      position={pick.position ?? i + 1}
                      isTopPick={pick.playOrder === topPlayOrder}
                      participantNames={participantNames}
                      participantIndex={participantIndex}
                    />
                  ))
                )}
              </div>
            </article>
          </div>
        </div>
      </div>

      <SiteFooter />
    </div>
  );
}
