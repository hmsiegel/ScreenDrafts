import { DrafterListItem } from "@/lib/dto";
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

interface DrafterCardProps {
  drafter: DrafterListItem;
  index: number;
}

export default function DrafterCard({ drafter, index }: DrafterCardProps) {
  const d = drafter as Record<string, unknown>;
  const isCommissioner = (d.isCommissioner as boolean | undefined) ?? false;
  const location = d.location as string | undefined;
  const role = (d.role as string | undefined) ?? (drafter.isRetired ? "RETIRED" : "GUEST G.M.");
  const totalDrafts = (d.totalDrafts as number | undefined) ?? 0;
  const filmsDrafted = (d.filmsDrafted as number | undefined) ?? 0;
  const vetoes = (d.vetoesUsed as number | undefined) ?? (d.vetoes as number | undefined) ?? 0;

  const bg = avatarColor(index);

  return (
    <Link
      href={`/drafters/${drafter.drafterId}`}
      className="group block bg-white border-2 border-sd-ink p-6 relative transition-all duration-[140ms] hover:-translate-y-[3px] hover:shadow-[6px_6px_0_#0d1430]"
    >
      {/* Commissioner badge */}
      {isCommissioner && (
        <div className="absolute top-4 right-4 font-mono text-[9px] tracking-widest text-sd-red font-bold">
          ★ COMMISSIONER
        </div>
      )}

      {/* Avatar */}
      <div
        className={`${bg} text-white flex items-center justify-center rounded-full font-oswald font-bold text-[20px] mb-4`}
        style={{ width: 64, height: 64 }}
      >
        {initials(drafter.displayName)}
      </div>

      {/* Name */}
      <div className="font-oswald font-bold text-[24px] text-sd-ink leading-tight mb-0.5">
        {drafter.displayName}
      </div>

      {/* Role */}
      <div className="font-mono text-[10px] tracking-wide text-sd-blue mb-1">
        {role.toUpperCase()}
      </div>

      {/* Location */}
      {location && (
        <div className="font-serif italic text-[13px] text-[#5a6075] mb-3">
          {location}
        </div>
      )}

      {/* Stats row */}
      <div className="grid grid-cols-3 border-t border-sd-ink/10 pt-3 mt-3 gap-2">
        <StatCell value={totalDrafts} label="DRAFTS" />
        <StatCell value={filmsDrafted} label="FILMS DRAFTED" />
        <StatCell value={vetoes} label="VETOES" />
      </div>

      {/* View profile link */}
      <div className="mt-4 font-oswald font-semibold text-[12px] tracking-wide text-sd-blue group-hover:text-sd-red transition-colors">
        VIEW PROFILE →
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
