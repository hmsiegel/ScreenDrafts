'use client';

import { useState } from "react";
import Link from "next/link";
import DraftBoardEditor from "@/components/drafts/draft-board-editor";
import CandidateListEditor from "@/components/drafts/candidate-list-editor";
import { type MyDraftDetailResponse, type MyDraftPartDetail } from "@/services/drafts/fetch-my-drafts";
import { type DraftBoardMovie } from "@/services/drafts/fetch-draft-board";
import { type CandidateListEntry } from "@/services/drafts/fetch-candidate-list";

interface MyDraftTabsProps {
  detail: MyDraftDetailResponse;
  accessToken: string;
}

export default function MyDraftTabs({ detail, accessToken }: MyDraftTabsProps) {
  const { myRoles, isSurrogate, parts } = detail;
  const isDrafter = myRoles.includes("Drafter");
  const isHost = myRoles.includes("Host");

  const tabs: string[] = [];
  if (isDrafter) tabs.push("MY BOARD", "CANDIDATE LIST");
  if (isHost) tabs.push("HOSTING");
  if (isSurrogate) tabs.push("PREDICTIONS");

  const [activeTab, setActiveTab] = useState(tabs[0] ?? "");
  const [selectedPartIdx, setSelectedPartIdx] = useState(0);

  const drafterParts = parts.filter((p) => p.drafterPosition != null);
  const hostParts = parts.filter((p) => p.hostRole != null);
  const currentPart: MyDraftPartDetail | undefined = drafterParts[selectedPartIdx];

  return (
    <div>
      <div className="flex border-b border-sd-ink/10 mb-6">
        {tabs.map((tab) => (
          <button
            key={tab}
            type="button"
            onClick={() => setActiveTab(tab)}
            className={`font-oswald font-bold text-sm uppercase tracking-wide px-5 py-3 transition-colors ${
              activeTab === tab
                ? "border-b-2 border-sd-red text-sd-ink"
                : "text-sd-ink/40 hover:text-sd-ink/70"
            }`}
          >
            {tab}
          </button>
        ))}
      </div>

      {activeTab === "MY BOARD" && isDrafter && (
        <div>
          {drafterParts.length > 1 && (
            <div className="flex gap-2 mb-4">
              {drafterParts.map((p, i) => (
                <button
                  key={p.draftPartId}
                  type="button"
                  onClick={() => setSelectedPartIdx(i)}
                  className={`font-mono text-xs uppercase px-3 py-1.5 border ${
                    selectedPartIdx === i
                      ? "border-sd-blue bg-sd-blue text-white"
                      : "border-sd-ink/20 text-sd-ink/60 hover:border-sd-ink/40"
                  }`}
                >
                  Part {p.partNumber}
                </button>
              ))}
            </div>
          )}
          {currentPart ? (
            <DraftBoardEditor
              draftId={detail.draft.publicId}
              accessToken={accessToken}
              initialBoard={currentPart.draftBoard as DraftBoardMovie[]}
            />
          ) : (
            <p className="font-mono text-sm text-sd-ink/40">No drafter parts found.</p>
          )}
        </div>
      )}

      {activeTab === "CANDIDATE LIST" && isDrafter && (
        <div>
          {drafterParts.length > 1 && (
            <div className="flex gap-2 mb-4">
              {drafterParts.map((p, i) => (
                <button
                  key={p.draftPartId}
                  type="button"
                  onClick={() => setSelectedPartIdx(i)}
                  className={`font-mono text-xs uppercase px-3 py-1.5 border ${
                    selectedPartIdx === i
                      ? "border-sd-blue bg-sd-blue text-white"
                      : "border-sd-ink/20 text-sd-ink/60 hover:border-sd-ink/40"
                  }`}
                >
                  Part {p.partNumber}
                </button>
              ))}
            </div>
          )}
          {currentPart ? (
            <CandidateListEditor
              draftPartId={currentPart.draftPartId}
              accessToken={accessToken}
              initialEntries={currentPart.candidateList as CandidateListEntry[]}
              readonly={false}
            />
          ) : (
            <p className="font-mono text-sm text-sd-ink/40">No drafter parts found.</p>
          )}
        </div>
      )}

      {activeTab === "HOSTING" && isHost && (
        <div className="space-y-3">
          {hostParts.length === 0 ? (
            <p className="font-mono text-sm text-sd-ink/40">No hosting assignments found.</p>
          ) : (
            hostParts.map((p) => (
              <div key={p.draftPartId} className="flex items-center justify-between border border-sd-ink/10 px-4 py-3">
                <div>
                  <p className="font-oswald font-bold text-sm uppercase tracking-wide text-sd-ink">
                    Part {p.partNumber}
                  </p>
                  <p className="font-mono text-xs text-sd-ink/50 capitalize">
                    {p.hostRole?.name ?? "Host"}
                  </p>
                </div>
                <Link
                  href={`/gameplay/${p.draftPartId}`}
                  className="bg-sd-blue text-white font-oswald font-medium uppercase tracking-wide text-xs px-4 py-2 hover:bg-sd-blue/90"
                >
                  Open
                </Link>
              </div>
            ))
          )}
        </div>
      )}

      {activeTab === "PREDICTIONS" && isSurrogate && (
        <div className="border border-sd-ink/10 bg-white p-8 text-center">
          <p className="font-oswald font-bold text-xl uppercase tracking-wide text-sd-ink/40">
            Commissioner Predictions — Coming Soon
          </p>
        </div>
      )}
    </div>
  );
}
