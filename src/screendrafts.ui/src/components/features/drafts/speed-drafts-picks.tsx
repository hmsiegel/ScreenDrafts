"use client";

import { GetDraftPickResponse, GetDraftPartResponse } from "@/lib/dto";
import { DraftPick } from "@/components/features/drafts/draft-pick";

// SubjectKind values mirror the C# SubjectKind SmartEnum
const SUBJECT_KIND_LABELS: Record<number, string> = {
  0: "Actor",
  1: "Director",
  2: "Word",
  3: "Movie",
  4: "Other",
};

function subjectKindLabel(kind: number): string {
  return SUBJECT_KIND_LABELS[kind] ?? "Subject";
}

interface SubDraftMeta {
  index: number;
  subjectKind: number;
  subjectName: string;
}

interface SpeedDraftPicksProps {
  part: GetDraftPartResponse;
  participantNames: Map<string, string>;
  participantIndex: Map<string, number>;
}

export function SpeedDraftPicks({
  part,
  participantNames,
  participantIndex,
}: SpeedDraftPicksProps) {
  const allPicks = [...(part.picks ?? [])].sort(
    (a, b) => (a.playOrder ?? 0) - (b.playOrder ?? 0)
  );

  // Build sub-draft metadata list from part.subDrafts, sorted by index.
  // Fall back to deriving the set of indices from picks when subDrafts is absent
  // (old API response before regeneration).
  const subDraftMetas: SubDraftMeta[] = part.subDrafts && part.subDrafts.length > 0
    ? [...part.subDrafts].sort((a, b) => (a.index ?? 0) - (b.index ?? 0)).map((sd) => ({
        index: sd.index ?? 0,
        subjectKind: sd.subjectKind ?? 0,
        subjectName: sd.subjectName ?? "",
      }))
    : [...new Set(allPicks.map((p) => p.subDraftIndex ?? 1))]
        .sort((a, b) => a - b)
        .map((idx) => ({ index: idx, subjectKind: 0, subjectName: "" }));

  // Group picks by subDraftIndex (null picks fall into index 1 as a safe default)
  const picksBySubDraft = new Map<number, GetDraftPickResponse[]>();
  for (const meta of subDraftMetas) {
    picksBySubDraft.set(meta.index, []);
  }
  for (const pick of allPicks) {
    const idx = pick.subDraftIndex ?? 1;
    if (!picksBySubDraft.has(idx)) {
      picksBySubDraft.set(idx, []);
    }
    picksBySubDraft.get(idx)!.push(pick);
  }

  const isVetoed = (pick: GetDraftPickResponse) =>
    !!pick.veto && !pick.veto.isOverriden;
  const isCommissionerRemoved = (pick: GetDraftPickResponse) =>
    pick.commissionerOverride !== null && pick.commissionerOverride !== undefined;

  return (
    <div className="space-y-10">
      {subDraftMetas.map((meta) => {
        const picks = picksBySubDraft.get(meta.index) ?? [];
        const finalPicks = picks.filter((p) => !isVetoed(p) && !isCommissionerRemoved(p));

        // Top pick = highest playOrder among final picks in this sub-draft
        const topPlayOrder = finalPicks.reduce(
          (max, p) => Math.max(max, p.playOrder ?? 0),
          0
        );

        const kindLabel = meta.subjectName
          ? `${subjectKindLabel(meta.subjectKind)}: ${meta.subjectName}`
          : `Sub-Draft ${meta.index}`;

        return (
          <section key={meta.index}>
            {/* Sub-draft header */}
            <div className="flex items-center gap-3 mb-5">
              <div className="font-mono text-[10px] tracking-widest text-sd-red font-bold whitespace-nowrap">
                ★ {kindLabel.toUpperCase()}
              </div>
              <div className="flex-1 h-px bg-sd-ink/15" />
              <span className="font-mono text-[11px] text-sd-ink/50 whitespace-nowrap">
                {finalPicks.length} PICKS
              </span>
            </div>

            {/* Picks for this sub-draft — position resets to 1 */}
            <div>
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
          </section>
        );
      })}
    </div>
  );
}