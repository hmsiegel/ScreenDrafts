import { Metadata } from "next";
import { notFound } from "next/navigation";
import Link from "next/link";
import { GetDraftPartResponse, GetDraftPickResponse, TriviaResultResponse } from "@/lib/dto";
import SiteHeader from "@/components/layout/header/site-header";
import SiteFooter from "@/components/layout/footer/site-footer";
import DraftSidebar from "@/components/features/drafts/drafts-sidebar";
import { DraftPick } from "@/components/features/drafts/draft-pick";
import { SpeedDraftLayout } from "@/components/features/drafts/speed-draft-layout";
import { DraftPartPredictionData, PredictionsSection } from "@/components/features/drafts/predictions-section";
import {
  getDraftDetails,
  getDraftPartPredictions,
  getDraftPartTriviaResults,
  getPredictionStandings
} from "@/services/drafts/fetch-drafts";

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

  const parts: GetDraftPartResponse[] = draft.parts ?? [];
  const isMultiPart = parts.length > 1;

  // Build participant name + index maps
  const participantNames = new Map<string, string>();
  const participantIndex = new Map<string, number>();
  const COMMUNITY_PARTICIPANT_ID = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";
  participantNames.set(COMMUNITY_PARTICIPANT_ID, "Patreon Members");
  participantIndex.set(COMMUNITY_PARTICIPANT_ID, 99);
  let pIdx = 0;

  for (const part of draft.parts ?? []) {
    for (const p of part.participants ?? []) {
      const pid = p.participantIdValue ?? "";
      if (pid && !participantNames.has(pid)) {
        const name = p.displayName;
        participantNames.set(pid, name ?? pid);
        participantIndex.set(pid, pIdx++);
      }
    }
  }

  // ── Parallel fetch: trivia + predictions + standings per part ──────────────
  const partDataResults = await Promise.allSettled(
    parts.map(async (part) => {
      const partPublicId = part.publicId ?? "";
      const seasonPublicId = part.predictionSeasonPublicId ?? null;

      const [triviaResult, predictionsResult, standingsResult] =
        await Promise.allSettled([
          getDraftPartTriviaResults(partPublicId),
          getDraftPartPredictions(partPublicId),
          seasonPublicId
            ? getPredictionStandings(seasonPublicId, partPublicId)
            : Promise.resolve(null),
        ]);

      return {
        partPublicId,
        trivia: triviaResult.status === "fulfilled" ? triviaResult.value : null,
        predictions: predictionsResult.status === "fulfilled" ? predictionsResult.value : [],
        standings: standingsResult.status === "fulfilled" ? standingsResult.value : null,
      };
    })
  );

  const partData = partDataResults.map((r, i) =>
    r.status === "fulfilled" ? r.value : {
      partPublicId: parts[i]?.publicId ?? "",
      trivia: null,
      predictions: [],
      standings: null,
    }
  );

  // ── Trivia map keyed by partPublicId (used by standard DraftSidebar) ───────
  const triviaByPart = new Map<string, TriviaResultResponse[]>(
    partData
      .filter((d) => d.trivia && d.trivia.results.length > 0)
      .map((d) => [d.partPublicId, d.trivia!.results])
  );

  // ── Per-part structured data ───────────────────────────────────────────────
  const isVetoed = (pick: GetDraftPickResponse) =>
    !!pick.veto && !pick.veto.isOverriden;
  const isCommissionerRemoved = (pick: GetDraftPickResponse) =>
    pick.commissionerOverride !== null && pick.commissionerOverride !== undefined;

  const partSections = parts.map((part, i) => {
    const partLabel = `Part ${part.partIndex ?? i + 1}`;
    const picks = [...(part.picks ?? [])].sort(
      (a, b) => (a.playOrder ?? 0) - (b.playOrder ?? 0)
    );
    const finalPicks = picks.filter(
      (p) => !isVetoed(p) && !isCommissionerRemoved(p)
    );
    const topPlayOrder = finalPicks.reduce(
      (max, p) => Math.max(max, p.playOrder ?? 0),
      0
    );
    const predictionData: DraftPartPredictionData = {
      draftPartPublicId: part.publicId ?? "",
      partLabel: "",
      predictions: partData[i]?.predictions ?? [],
      standings: partData[i]?.standings ?? null,
    };

    return { part, partLabel, picks, finalPicks, topPlayOrder, predictionData };
  });

  const totalFinalPicks = partSections.reduce((n, s) => n + s.finalPicks.length, 0);

  // ── SpeedDraft single-part: delegate entirely to SpeedDraftLayout ──────────
  const firstPart = parts[0];
  const isSpeedDraft = !isMultiPart && firstPart?.draftType?.name === "SpeedDraft";

  if (isSpeedDraft && firstPart) {
    const triviaResults = partData[0]?.trivia?.results ?? [];
    const predictionData = partSections[0]!.predictionData;

    return (
      <div className="min-h-screen bg-light-blue">
        <SiteHeader activePath="/drafts" />
        <div style={{ padding: "40px 40px 64px" }}>
          <nav className="font-mono text-[11px] mb-8 flex items-center gap-1.5">
            <Link href="/drafts" className="text-sd-blue hover:underline">
              / DRAFTS
            </Link>
            <span className="text-sd-ink/30">/</span>
            <span className="text-sd-ink/60 truncate max-w-[400px]">{draft.title}</span>
          </nav>

          <SpeedDraftLayout
            draft={draft}
            part={firstPart}
            participantNames={participantNames}
            participantIndex={participantIndex}
            triviaResults={triviaResults}
            predictionData={predictionData}
          />
        </div>
        <SiteFooter />
      </div>
    );
  }

  // ── Standard layout (single-part non-SpeedDraft + all multi-part) ──────────
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
          <DraftSidebar
            draft={draft}
            participantNames={participantNames}
            triviaByPart={triviaByPart}
            isMultiPart={isMultiPart}
          />

          {/* Right column */}
          <div className="bg-white border-2 border-sd-ink" style={{ padding: "40px 48px", maxWidth: 720 }}>
            <article>
              <div className="font-mono text-[11px] tracking-widest text-sd-red font-bold mb-3">
                ★ THE EPISODE
              </div>

              <h1 className="font-oswald font-bold text-[56px] text-sd-ink leading-[1] mb-6">
                {draft.title}
              </h1>

              {draft.description && (
                <p className="font-serif text-[18px] leading-[1.55] text-[#2a2f44] mb-10 max-w-[680px]">
                  {draft.description}
                </p>
              )}

              {/* ── Single-part layout ───────────────────────────────────── */}
              {!isMultiPart && (() => {
                const { picks, finalPicks, topPlayOrder, predictionData } = partSections[0]!;
                return (
                  <>
                    <div className="flex items-center gap-4 mb-2">
                      <h2 className="font-oswald font-bold text-[32px] text-sd-ink whitespace-nowrap">
                        THE FINAL LIST
                      </h2>
                      <div className="flex-1 h-0.5 bg-sd-ink" />
                      <span className="font-mono text-[11px] text-sd-ink/60 whitespace-nowrap">
                        {finalPicks.length} PICKS
                      </span>
                    </div>
                    <div className="mt-6">
                      {picks.length === 0 ? (
                        <p className="font-mono text-sm text-sd-ink/40">No picks recorded.</p>
                      ) : (
                        picks.map((pick, i) => (
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
                    <PredictionsSection parts={[predictionData]} />
                  </>
                );
              })()}

              {/* ── Multi-part layout ────────────────────────────────────── */}
              {isMultiPart && (
                <>
                  <div className="flex items-center gap-4 mb-8">
                    <h2 className="font-oswald font-bold text-[32px] text-sd-ink whitespace-nowrap">
                      THE FINAL LIST
                    </h2>
                    <div className="flex-1 h-0.5 bg-sd-ink" />
                    <span className="font-mono text-[11px] text-sd-ink/60 whitespace-nowrap">
                      {totalFinalPicks} PICKS
                    </span>
                  </div>

                  <div className="space-y-12">
                    {partSections.map(({ part, partLabel, picks, finalPicks, topPlayOrder, predictionData }, i) => (
                      <section key={part.publicId ?? i}>
                        <div className="flex items-center gap-3 mb-5">
                          <div className="font-mono text-[10px] tracking-widest text-sd-red font-bold whitespace-nowrap">
                            ★ {partLabel.toUpperCase()}
                          </div>
                          <div className="flex-1 h-px bg-sd-ink/15" />
                          <span className="font-mono text-[11px] text-sd-ink/50 whitespace-nowrap">
                            {finalPicks.length} PICKS
                          </span>
                        </div>

                        {part.releases?.[0]?.releaseDate && (
                          <div className="font-mono text-[11px] text-sd-blue mb-4">
                            {new Date(part.releases[0].releaseDate as string | Date).toLocaleDateString("en-US", {
                              month: "short",
                              day: "2-digit",
                              year: "numeric",
                            }).toUpperCase()}
                          </div>
                        )}

                        <div>
                          {picks.length === 0 ? (
                            <p className="font-mono text-sm text-sd-ink/40">No picks recorded.</p>
                          ) : (
                            picks.map((pick, j) => (
                              <DraftPick
                                key={`${pick.moviePublicId}-${pick.playOrder ?? j}`}
                                pick={pick}
                                position={pick.position ?? j + 1}
                                isTopPick={pick.playOrder === topPlayOrder}
                                participantNames={participantNames}
                                participantIndex={participantIndex}
                              />
                            ))
                          )}
                        </div>

                        <PredictionsSection parts={[predictionData]} />
                      </section>
                    ))}
                  </div>
                </>
              )}
            </article>
          </div>
        </div>
      </div>

      <SiteFooter />
    </div>
  );
}