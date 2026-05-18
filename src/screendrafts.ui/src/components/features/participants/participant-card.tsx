import { HonorificResponse, ParticipantListItem } from "@/lib/dto";
import Link from "next/link";
import { HonorificBanner } from "./honorific-banner";

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

function roleLabel(participant: ParticipantListItem): string {
  if (participant.isCommissioner) return "COMMISSIONER";
  const roles = participant.roles ?? [];
  if (roles.includes("GM") && roles.includes("Host")) return "G.M. / HOST";
  if (roles.includes("Host")) return "HOST";
  return "GUEST G.M.";
}

interface ParticipantCardProps {
  participant: ParticipantListItem;
  index: number;
  honorific: HonorificResponse | null | undefined;
}

export default function ParticipantCard({ participant, index, honorific }: ParticipantCardProps) {
  const bg = avatarColor(index);
  const isGM = (participant.roles ?? []).includes("GM");
  const isHost = (participant.roles ?? []).includes("Host");

  return (
    <Link
      href={`/drafters/${participant.personPublicId}`}
      className="group block bg-white border-2 border-sd-ink relative transition-all duration-[140ms] hover:-translate-y-[3px] hover:shadow-[6px_6px_0_#0d1430]"
    >
      <div className="p-6">
        {participant.isCommissioner && (
          <div className="absolute top-4 right-4 font-mono text-[9px] tracking-widest text-sd-red font-bold">
            ★ COMMISSIONER
          </div>
        )}

        {/* Avatar row: avatar left, banner right */}
        <div className="flex items-center gap-4 mb-3">
          {/* Avatar */}
          <div
            className={`${bg} text-white flex items-center justify-center rounded-full font-oswald font-bold text-[20px] shrink-0`}
            style={{ width: 64, height: 64 }}
          >
            {initials(participant.displayName)}
          </div>

          {/* Banner slot — always reserves space so card height is stable */}
          <div className="flex-1 flex items-center" style={{ height: 64 }}>
              <HonorificBanner honorific={honorific ?? null} isGM={isGM} size="card" />
          </div>
        </div>

        {/* Role label */}
        <div className="font-mono text-[10px] tracking-wide text-sd-blue mb-1">
          {roleLabel(participant)}
        </div>

        {/* Name */}
        <div className="font-oswald font-bold text-[24px] text-sd-ink leading-tight mb-3">
          {participant.displayName}
        </div>

        {/* Stats */}
        <div
          className="grid border-t border-sd-ink/10 pt-3 mt-3 gap-2"
          style={{
            gridTemplateColumns:
              isGM && isHost ? "repeat(4, 1fr)"
                : isGM ? "repeat(3, 1fr)"
                  : "1fr",
          }}
        >
          {isGM && (
            <>
              <StatCell value={participant.totalDrafts ?? 0} label="DRAFTS" />
              <StatCell value={participant.filmsDrafted ?? 0} label="FILMS" />
              <StatCell value={participant.vetoesUsed ?? 0} label="VETOES" />
            </>
          )}
          {isHost && (
            <StatCell
              value={participant.draftsHosted ?? 0}
              label={isGM ? "HOSTED" : "DRAFTS HOSTED"}
            />
          )}
        </div>

        <div className="mt-4 font-oswald font-semibold text-[12px] tracking-wide text-sd-blue group-hover:text-sd-red transition-colors">
          VIEW PROFILE →
        </div>
      </div>
    </Link>
  );
}

function StatCell({ value, label }: { value: number; label: string }) {
  return (
    <div className="flex flex-col items-center gap-0.5">
      <span className="font-oswald font-bold text-[28px] text-sd-ink leading-none">{value}</span>
      <span className="font-mono text-[9px] text-[#5a6075] text-center">{label}</span>
    </div>
  );
}