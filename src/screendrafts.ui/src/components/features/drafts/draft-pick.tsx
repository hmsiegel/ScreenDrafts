// src/components/features/drafts/draft-pick.tsx

import { GetDraftPickResponse } from "@/lib/dto";
import Link from "next/link";

const AVATAR_COLORS = ["bg-sd-blue", "bg-sd-red", "bg-sd-ink", "bg-[#6a6f7e]"];

function avatarColor(index: number): string {
  return AVATAR_COLORS[index % AVATAR_COLORS.length];
}

function initials(name: string): string {
  return name
    .split(" ")
    .slice(0, 2)
    .map((w) => w[0]?.toUpperCase() ?? "")
    .join("");
}

interface DraftPickProps {
  pick: GetDraftPickResponse;
  position: number;
  isTopPick: boolean;
  participantNames: Map<string, string>;
  participantIndex: Map<string, number>;
}

type VetoNarrativeSegment =
  | { kind: "veto"; text: string; resolved: boolean }
  | { kind: "override"; text: string }
  | { kind: "arrow"; text: string };

export function DraftPick({ pick, position, isTopPick, participantNames, participantIndex }: DraftPickProps) {
  const playedById = pick.playedByParticipantIdValue ?? "";
  const playerName = participantNames.get(playedById) ?? pick.actedByPublicId ?? "Unknown";
  const playerIdx = participantIndex.get(playedById) ?? 0;

  // GetDraftPickResponse.Vetoes is the pick's full history, ordered by Sequence — there
  // is no singular "veto" field. Current state is always the last entry. Rather than
  // splitting the current entry from earlier ones into two separate lines, the whole
  // chain renders as one narrative: "vetoed by A → overridden by B → vetoed by C". See
  // Pick.CurrentVeto on the domain side for the equivalent "last by Sequence" logic.
  const vetoes = pick.vetoes ?? [];
  const currentVeto = vetoes.length > 0 ? vetoes[vetoes.length - 1] : null;
  const hasVeto = !!currentVeto;
  const vetoIsOverridden = currentVeto?.isOverridden ?? false;

  const nameFor = (participantId: string | undefined, actedByPublicId: string | undefined, displayName: string | undefined) =>
    displayName ?? participantNames.get(participantId ?? "") ?? participantId ?? actedByPublicId ?? "Unknown";

  const vetoNarrative: VetoNarrativeSegment[] = vetoes.flatMap((v, i) => {
    const segs: VetoNarrativeSegment[] = [];
    if (i > 0) {
      segs.push({ kind: "arrow", text: " → " });
    }
    const vetoerName = nameFor(v.issuedByParticipantId, v.actedByPublicId, v.issuedByDisplayName);
    segs.push({ kind: "veto", text: `vetoed by ${vetoerName}`, resolved: v.isOverridden ?? false });
    if (v.isOverridden) {
      const overriderName = nameFor(
        v.override?.issuedByParticipantId,
        v.override?.actedByPublicId,
        v.override?.issuedByDisplayName
      );
      segs.push({ kind: "arrow", text: " → " });
      segs.push({ kind: "override", text: `overridden by ${overriderName}` });
    }
    return segs;
  });

  const hasCommissionerOverride = pick.commissionerOverride !== null && pick.commissionerOverride !== undefined;

  // Access possibly-present year via index signature
  const movieYear = (pick as Record<string, unknown>).movieYear as string | undefined;

  const titleSize = isTopPick ? "text-[30px]" : "text-[22px]";
  const numSize = isTopPick ? "text-[56px] text-sd-red" : "text-[40px] text-sd-blue";

  const filmLink = pick.moviePublicId
    ? `/media/${pick.moviePublicId}`
    : null;

  const titleEl = (
    <span
      className={`font-sans font-bold ${titleSize} leading-tight ${(hasVeto && !vetoIsOverridden) || hasCommissionerOverride
          ? "line-through text-sd-red"
          : "text-sd-ink"
        }`}
    >
      {pick.movieTitle || "Unknown Film"}
      {movieYear && (
        <span className="font-mono text-[13px] font-normal text-[#5a6075] ml-2">{movieYear}</span>
      )}
    </span>
  );

  return (
    <div
      className="grid py-5 border-t border-sd-ink/10 first:border-t-0"
      style={{ gridTemplateColumns: "76px 1fr" }}
    >
      {/* Position number */}
      <div className={`font-oswald font-bold ${numSize} leading-none pt-1`}>
        {position}
      </div>

      {/* Film details */}
      <div className="flex flex-col gap-1.5">
        {filmLink ? (
          <Link href={filmLink} className="hover:opacity-80 transition-opacity inline-flex flex-wrap items-baseline gap-1">
            {titleEl}
          </Link>
        ) : (
          <div>{titleEl}</div>
        )}

        {/* By line */}
        <div className="text-[14px] italic text-[#5a6075]">
          <span className={(hasVeto && !vetoIsOverridden) || hasCommissionerOverride ? "line-through" : ""}>
            by{" "}
            <span className="font-sans font-semibold not-italic text-sd-ink">{playerName}</span>
          </span>
          {hasVeto && (
            <span className="ml-2 not-italic">
              (
              {vetoNarrative.map((seg, i) =>
                seg.kind === "arrow" ? (
                  <span key={i} className="text-[#5a6075]">
                    {seg.text}
                  </span>
                ) : seg.kind === "veto" ? (
                  <span key={i} className={seg.resolved ? "text-sd-red/60 line-through" : "text-sd-red"}>
                    {seg.text}
                  </span>
                ) : (
                  <span key={i} className="text-[#5a6075]">
                    {seg.text}
                  </span>
                )
              )}
              {!vetoIsOverridden && <span className="text-sd-red"></span>}
              )
            </span>
          )}
          {hasCommissionerOverride && (
            <span className="text-sd-red ml-2 no-underline">removed by Commissioner Override</span>
          )}
        </div>
      </div>
    </div>
  );
}

interface AvatarProps {
  name: string;
  colorIndex: number;
  size?: number;
}

export function Avatar({ name, colorIndex, size = 40 }: AvatarProps) {
  const bg = avatarColor(colorIndex);
  return (
    <div
      className={`${bg} text-white flex items-center justify-center rounded-[2px] font-oswald font-bold text-[14px] shrink-0`}
      style={{ width: size, height: size }}
    >
      {initials(name)}
    </div>
  );
}

export { avatarColor, initials };