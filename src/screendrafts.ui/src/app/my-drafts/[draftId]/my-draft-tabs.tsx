"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import DraftBoardEditor from "@/components/drafts/draft-board-editor";
import CandidateListEditor from "@/components/drafts/candidate-list-editor";
import InfoTooltip from "@/components/ui/info-tooltip";
import { getDraftBoard } from "@/services/drafts/fetch-draft-board";
import type { GetMyDraftDetailResponse, MyDraftPartDetail } from "@/lib/dto";
import type { DraftBoardItemResponse } from "@/lib/dto";
import PredictionSubmission from "./prediction-submission";

interface MyDraftTabsProps {
  detail: GetMyDraftDetailResponse;
  accessToken: string;
}

const TAB_TOOLTIPS: Record<string, React.ReactNode> = {
  "MY BOARD": (
    <>
      Your personal research list. Add films you&apos;re considering, rank them by priority,
      and add notes. Only you can see your board.
    </>
  ),
  "CANDIDATE LIST": (
    <>
      A shared public list of eligible titles for drafts with a narrower topic.
      Drafters contribute to it collaboratively; hosts and admins can also manage it.
    </>
  ),
};

function TabLabel({ tab }: { tab: string }) {
  const tooltip = TAB_TOOLTIPS[tab];
  if (!tooltip) return <>{tab}</>;
  return (
    <span className="flex items-center gap-1.5">
      {tab}
      <InfoTooltip position="bottom">{tooltip}</InfoTooltip>
    </span>
  );
}

export default function MyDraftTabs({ detail, accessToken }: MyDraftTabsProps) {
  const myRoles = detail.myRoles ?? [];
  const parts = detail.parts ?? [];
  const predictorParts = parts.filter((p) => p.isPredictor ?? false);
  const hasPredictorRole = predictorParts.length > 0;

  const isDrafter = myRoles.includes("Drafter");
  const isHost = myRoles.includes("Host");

  const tabs: string[] = [];
  if (isDrafter) tabs.push("MY BOARD", "CANDIDATE LIST");
  if (isHost) tabs.push("HOSTING");
  if (hasPredictorRole) tabs.push("PREDICTIONS");

  const [activeTab, setActiveTab] = useState(tabs[0] ?? "");
  const [selectedPartIdx, setSelectedPartIdx] = useState(0);
  const [board, setBoard] = useState<DraftBoardItemResponse[]>([]);
  const [boardLoading, setBoardLoading] = useState(false);
  const [selectedPredictionPartIdx, setSelectedPredictionPartIdx] = useState(0);

  const drafterParts = parts.filter((p) => p.isDrafter ?? false);
  const hostParts = parts.filter((p) => p.isHost ?? false);
  const currentPart: MyDraftPartDetail | undefined = drafterParts[selectedPartIdx];
  const draftPublicId = detail.draftPublicId ?? "";

  useEffect(() => {
    if (!isDrafter || !draftPublicId) return;
    setBoardLoading(true);
    getDraftBoard(accessToken, draftPublicId)
      .then(setBoard)
      .finally(() => setBoardLoading(false));
  }, [isDrafter, draftPublicId, accessToken]);

  return (
    <div>
      {/* Tab bar */}
      <div className="flex border-b border-sd-ink/10 mb-6">
        {tabs.map((tab) => (
          <button
            key={tab}
            type="button"
            onClick={() => setActiveTab(tab)}
            className={`font-oswald font-bold text-sm uppercase tracking-wide px-5 py-3 transition-colors ${activeTab === tab
                ? "border-b-2 border-sd-red text-sd-ink"
                : "text-sd-ink/40 hover:text-sd-ink/70"
              }`}
          >
            <TabLabel tab={tab} />
          </button>
        ))}
      </div>

      {/* MY BOARD */}
      {activeTab === "MY BOARD" && isDrafter && (
        <div>
          {drafterParts.length > 1 && (
            <div className="flex gap-2 mb-4">
              {drafterParts.map((p, i) => (
                <button
                  key={p.draftPartPublicId ?? ""}
                  type="button"
                  onClick={() => setSelectedPartIdx(i)}
                  className={`font-mono text-xs uppercase px-3 py-1.5 border ${selectedPartIdx === i
                      ? "border-sd-blue bg-sd-blue text-white"
                      : "border-sd-ink/20 text-sd-ink/60 hover:border-sd-ink/40"
                    }`}
                >
                  Part {p.partIndex}
                </button>
              ))}
            </div>
          )}
          {boardLoading ? (
            <p className="font-mono text-sm text-sd-ink/40">Loading board…</p>
          ) : currentPart ? (
            <DraftBoardEditor
              draftId={draftPublicId}
              accessToken={accessToken}
              initialBoard={board}
            />
          ) : (
            <p className="font-mono text-sm text-sd-ink/40">No drafter parts found.</p>
          )}
        </div>
      )}

      {/* CANDIDATE LIST */}
      {activeTab === "CANDIDATE LIST" && isDrafter && (
        <div>
          {drafterParts.length > 1 && (
            <div className="flex gap-2 mb-4">
              {drafterParts.map((p, i) => (
                <button
                  key={p.draftPartPublicId ?? ""}
                  type="button"
                  onClick={() => setSelectedPartIdx(i)}
                  className={`font-mono text-xs uppercase px-3 py-1.5 border ${selectedPartIdx === i
                      ? "border-sd-blue bg-sd-blue text-white"
                      : "border-sd-ink/20 text-sd-ink/60 hover:border-sd-ink/40"
                    }`}
                >
                  Part {p.partIndex}
                </button>
              ))}
            </div>
          )}
          {currentPart ? (
            <CandidateListEditor
              draftPartId={currentPart.draftPartPublicId ?? ""}
              accessToken={accessToken}
              initialEntries={[]}
              readonly={false}
            />
          ) : (
            <p className="font-mono text-sm text-sd-ink/40">No drafter parts found.</p>
          )}
        </div>
      )}

      {/* HOSTING */}
      {activeTab === "HOSTING" && isHost && (
        <div className="space-y-3">
          {hostParts.length === 0 ? (
            <p className="font-mono text-sm text-sd-ink/40">No hosting assignments found.</p>
          ) : (
            hostParts.map((p) => (
              <div
                key={p.draftPartPublicId ?? ""}
                className="flex items-center justify-between border border-sd-ink/10 px-4 py-3"
              >
                <div>
                  <p className="font-oswald font-bold text-sm uppercase tracking-wide text-sd-ink">
                    {parts.length > 1 ? `Part ${p.partIndex}` : detail.title}
                  </p>
                </div>
                <Link
                  href={`/draft-parts/${p.draftPartPublicId ?? ""}/live`}
                  className="bg-sd-blue text-white font-oswald font-medium uppercase tracking-wide text-xs px-4 py-2 hover:bg-sd-blue/90"
                >
                  Open
                </Link>
              </div>
            ))
          )}
        </div>
      )}

      {/* PREDICTIONS */}
      {activeTab === "PREDICTIONS" && hasPredictorRole && (
        <div>
          {predictorParts.length > 1 && (
            <div className="flex gap-2 mb-4">
              {predictorParts.map((p, i) => (
                <button
                  key={p.draftPartPublicId ?? ""}
                  type="button"
                  onClick={() => setSelectedPredictionPartIdx(i)}
                  className={`font-mono text-xs uppercase px-3 py-1.5 border ${selectedPredictionPartIdx === i
                      ? "border-sd-blue bg-sd-blue text-white"
                      : "border-sd-ink/20 text-sd-ink/60 hover:border-sd-ink/40"
                    }`}
                >
                  Part {p.partIndex}
                </button>
              ))}
            </div>
          )}
          {predictorParts[selectedPredictionPartIdx] && (
            <PredictionSubmission
              accessToken={accessToken}
              draftPartId={predictorParts[selectedPredictionPartIdx].draftPartPublicId ?? ""}
              contestantPublicId={predictorParts[selectedPredictionPartIdx].contestantPublicId ?? ""}
              hasSubmitted={predictorParts[selectedPredictionPartIdx].hasSubmittedPrediction ?? false}
            />
          )}
        </div>
      )}
    </div>
  );
}