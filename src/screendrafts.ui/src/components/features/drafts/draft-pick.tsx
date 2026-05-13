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

export function DraftPick({ pick, position, isTopPick, participantNames, participantIndex }: DraftPickProps) {
  const playedById = pick.playedByParticipantIdValue ?? "";
  const playerName = participantNames.get(playedById) ?? pick.actedByPublicId ?? "Unknown";
  const playerIdx = participantIndex.get(playedById) ?? 0;

  const hasVeto = !!pick.veto;
  const vetoIssuerId = pick.veto?.issuedByParticipantId ?? pick.veto?.actedByPublicId ?? "";
  const vetoerName = pick.veto?.issuedByDisplayName
    ?? participantNames.get(vetoIssuerId)
    ?? vetoIssuerId
    ?? "Unknown";
  const vetoIsOverridden = pick.veto?.isOverriden ?? !!pick.veto?.override;

  const overrideIssuerId = pick.veto?.override?.issuedByParticipantId ?? pick.veto?.override?.actedByPublicId ?? "";
  const overriderName = pick.veto?.override?.issuedByDisplayName
    ?? participantNames.get(overrideIssuerId)
    ?? overrideIssuerId
    ?? "Unknown";

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
        {/* By line */}
        <div className="text-[14px] italic text-[#5a6075]">
          <span className={(hasVeto && !vetoIsOverridden) || hasCommissionerOverride ? "line-through" : ""}>
            by{" "}
            <span className="font-sans font-semibold not-italic text-sd-ink">{playerName}</span>
          </span>
          {hasVeto && !vetoIsOverridden && (
            <span className="text-sd-red ml-2">(vetoed by {vetoerName})</span>
          )}
          {hasVeto && vetoIsOverridden && (
            <>
              <span className="ml-2 line-through text-sd-red/60">(vetoed by {vetoerName})</span>
              <span className="ml-2 text-[#5a6075]">(veto overridden by {overriderName})</span>
            </>
          )}
          {hasCommissionerOverride && (
            <span className="text-sd-red ml-2 no-underline">removed by Commissioner Override</span>
          )}
        </div>

        {/* Commissioner override note */}
        {pick.veto?.note && (
          <div
            className="mt-1 pl-3 py-2 pr-3 font-serif italic text-[13px] text-sd-ink bg-sd-paper"
            style={{ borderLeft: "3px solid #cb2032" }}
          >
            ★ {pick.veto.note}
          </div>
        )}
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
