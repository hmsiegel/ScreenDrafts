import { getParticipantProfile } from "@/services/participants/fetch-participants";
import { HonorificBanner } from "@/components/features/participants/honorific-banner";
import {
  DraftBrief,
  DraftHistoryItem,
  DrafterStatsResponse,
  GetParticipantProfileResponse,
  HonorificResponse,
  HostStatsResponse,
  PickItem,
  SocialHandles,
  VetoHistoryItem,
} from "@/lib/dto";
import { notFound } from "next/navigation";
import Link from "next/link";
import Image from "next/image";
import ProfileAvatar from "@/components/features/participants/profile-avatar";

type Props = { params: Promise<{ id: string }> };

export const dynamic = "force-dynamic";

export async function generateMetadata({ params }: Props) {
  const { id } = await params;
  try {
    const profile = await getParticipantProfile(id);
    return { title: profile.displayName };
  } catch {
    return { title: "Drafter" };
  }
}

const HONORIFIC_LABELS: Record<number, string> = {
  1: "ALL-STAR",
  2: "HALL OF FAMER",
  3: "MVP",
  4: "LEGEND",
};

export default async function DrafterProfilePage({ params }: Props) {
  const { id } = await params;

  let profile: GetParticipantProfileResponse;
  try {
    profile = await getParticipantProfile(id);
  } catch {
    notFound();
  }

  // Honorific comes directly from the profile response — no separate fetch needed.
  const honorific = profile.honorific ?? null;
  const honorificLabel = honorific ? HONORIFIC_LABELS[honorific.honorificValue] : null;

  // Merge picks and vetoes issued per draft, ordered by play order.
  const vetosByDraft = new Map<string, VetoHistoryItem[]>();
  for (const v of profile.vetoHistory ?? []) {
    const key = v.draft.draftPublicId;
    if (!vetosByDraft.has(key)) vetosByDraft.set(key, []);
    vetosByDraft.get(key)!.push(v);
  }

  const picksByDraft = new Map<string, DraftHistoryItem>();
  for (const h of profile.draftHistory ?? []) {
    picksByDraft.set(h.draft.draftPublicId, h);
  }

  const orderedDraftIds = [
    ...(profile.draftHistory ?? []).map((h) => h.draft.draftPublicId),
    ...Array.from(vetosByDraft.keys()).filter((did) => !picksByDraft.has(did)),
  ];

  return (
    <div className="min-h-screen bg-light-blue">
      {/* Page header */}
      <div className="bg-sd-ink text-white px-10 py-12">
        <p className="font-mono text-[11px] tracking-widest text-light-blue mb-3">
          <Link href="/drafters" className="hover:text-white transition-colors">
            / DRAFTERS
          </Link>
          <span className="text-white/40"> / </span>
          <span>{profile.displayName.toUpperCase()}</span>
        </p>
        <div className="flex items-start justify-between gap-8">
          <div>
            <h1 className="font-oswald font-bold text-[64px] leading-[0.95] text-white">
              {profile.displayName.toUpperCase()}
            </h1>
            {honorificLabel && (
              <p className="font-mono text-[11px] tracking-widest text-sd-red mt-2">
                ★ {honorificLabel}
              </p>
            )}
          </div>
          {profile.isCommissioner && (
            <span className="font-mono text-[11px] tracking-widest text-sd-red font-bold mt-2">
              ★ COMMISSIONER
            </span>
          )}
        </div>
      </div>

      {/* Red accent bar */}
      <div className="h-1 bg-sd-red" />

      {/* Content */}
      <div className="px-10 py-10 max-w-[1400px] mx-auto">
        <div className="grid gap-10" style={{ gridTemplateColumns: "320px 1fr" }}>

          {/* ── Left sidebar ── */}
          <div className="flex flex-col gap-6">
            <ProfileCard profile={profile} honorific={honorific} />
            {profile.drafterStats && (
              <DrafterStatsCard stats={profile.drafterStats} />
            )}
            {profile.hostStats && (
              <HostStatsCard stats={profile.hostStats} />
            )}
          </div>

          {/* ── Right column ── */}
          <div className="flex flex-col gap-10">
            {profile.biography && (
              <div className="bg-white border-2 border-sd-ink p-6">
                <h2 className="font-oswald font-bold text-[13px] tracking-widest text-sd-red mb-4">
                  BIOGRAPHY
                </h2>
                <p className="font-serif text-[16px] leading-relaxed text-sd-ink/80 italic">
                  {profile.biography}
                </p>
              </div>
            )}

            {orderedDraftIds.length > 0 && (
              <>
                <TableOfContents
                  draftIds={orderedDraftIds}
                  picksByDraft={picksByDraft}
                  vetosByDraft={vetosByDraft}
                />
                <DraftHistorySection
                  draftIds={orderedDraftIds}
                  picksByDraft={picksByDraft}
                  vetosByDraft={vetosByDraft}
                />
              </>
            )}
          </div>
        </div>
      </div>

    </div>
  );
}

// ── Profile card ──────────────────────────────────────────────────────────────

function ProfileCard({
  profile,
  honorific,
}: {
  profile: GetParticipantProfileResponse;
  honorific: HonorificResponse | null;
}) {
  const isGM = profile.drafterStats !== null && profile.drafterStats !== undefined;

  return (
    <div className="bg-white border-2 border-sd-ink overflow-hidden">
      <div className="p-6 flex flex-col items-center gap-4">
        {/* Avatar — profile picture or initials fallback */}
        <ProfileAvatar
          displayName={profile.displayName}
          picturePath={profile.socialHandles?.profilePicturePath}
        />
        <div className="text-center">
          <div className="font-oswald font-bold text-[22px] text-sd-ink leading-tight">
            {profile.displayName}
          </div>
          {profile.location && (
            <div className="font-serif italic text-[13px] text-[#5a6075] mt-1">
              {profile.location}
            </div>
          )}
        </div>

        {profile.socialHandles && (
          <SocialLinks handles={profile.socialHandles} />
        )}
      </div>

      {/* Honorific banner flush to card bottom */}
      <HonorificBanner honorific={honorific} isGM={isGM} size="profile" />
    </div>
  );
}

// ── Social links with SVG icons ───────────────────────────────────────────────

function SocialLinks({ handles }: { handles: SocialHandles }) {
  const links = [
    {
      label: "Letterboxd",
      url: handles.letterboxd,
      icon: (
        <svg viewBox="0 0 24 24" className="w-5 h-5" fill="currentColor">
          <path d="M10.5 0C4.7 0 0 4.7 0 10.5S4.7 21 10.5 21 21 16.3 21 10.5 16.3 0 10.5 0zm0 19.1C5.7 19.1 1.9 15.3 1.9 10.5S5.7 1.9 10.5 1.9s8.6 3.8 8.6 8.6-3.8 8.6-8.6 8.6zM7.4 7.4h1.7v6.2H7.4zm3.4 0h1.7l1.7 3.1 1.7-3.1h1.7l-2.6 4.6 2.6 4.6h-1.7l-1.7-3.1-1.7 3.1h-1.7l2.6-4.6z" />
        </svg>
      ),
    },
    {
      label: "Twitter / X",
      url: handles.twitter,
      icon: (
        <svg viewBox="0 0 24 24" className="w-5 h-5" fill="currentColor">
          <path d="M18.244 2.25h3.308l-7.227 8.26 8.502 11.24H16.17l-5.214-6.817L4.99 21.75H1.68l7.73-8.835L1.254 2.25H8.08l4.713 6.231zm-1.161 17.52h1.833L7.084 4.126H5.117z" />
        </svg>
      ),
    },
    {
      label: "Bluesky",
      url: handles.bluesky,
      icon: (
        <svg viewBox="0 0 24 24" className="w-5 h-5" fill="currentColor">
          <path d="M12 10.8c-1.087-2.114-4.046-6.053-6.798-7.995C2.566.944 1.561 1.266.902 1.565.139 1.908 0 3.08 0 3.768c0 .69.378 5.65.624 6.479.815 2.736 3.713 3.66 6.383 3.364.136-.02.275-.039.415-.056-.138.022-.276.04-.415.056-3.912.58-7.387 2.005-2.83 7.078 5.013 5.19 6.87-1.113 7.823-4.308.953 3.195 2.05 9.271 7.733 4.308 4.267-4.308 1.172-6.498-2.74-7.078a8.741 8.741 0 0 1-.415-.056c.14.017.279.036.415.056 2.67.297 5.568-.628 6.383-3.364.246-.828.624-5.79.624-6.478 0-.69-.139-1.861-.902-2.204-.659-.299-1.664-.62-4.3 1.24C16.046 4.748 13.087 8.687 12 10.8z" />
        </svg>
      ),
    },
    {
      label: "Instagram",
      url: handles.instagram,
      icon: (
        <svg viewBox="0 0 24 24" className="w-5 h-5" fill="currentColor">
          <path d="M12 2.163c3.204 0 3.584.012 4.85.07 3.252.148 4.771 1.691 4.919 4.919.058 1.265.069 1.645.069 4.849 0 3.205-.012 3.584-.069 4.849-.149 3.225-1.664 4.771-4.919 4.919-1.266.058-1.644.07-4.85.07-3.204 0-3.584-.012-4.849-.07-3.26-.149-4.771-1.699-4.919-4.92-.058-1.265-.07-1.644-.07-4.849 0-3.204.013-3.583.07-4.849.149-3.227 1.664-4.771 4.919-4.919 1.266-.057 1.645-.069 4.849-.069zm0-2.163c-3.259 0-3.667.014-4.947.072-4.358.2-6.78 2.618-6.98 6.98-.059 1.281-.073 1.689-.073 4.948 0 3.259.014 3.668.072 4.948.2 4.358 2.618 6.78 6.98 6.98 1.281.058 1.689.072 4.948.072 3.259 0 3.668-.014 4.948-.072 4.354-.2 6.782-2.618 6.979-6.98.059-1.28.073-1.689.073-4.948 0-3.259-.014-3.667-.072-4.947-.196-4.354-2.617-6.78-6.979-6.98-1.281-.059-1.69-.073-4.949-.073zm0 5.838c-3.403 0-6.162 2.759-6.162 6.162s2.759 6.163 6.162 6.163 6.162-2.759 6.162-6.163c0-3.403-2.759-6.162-6.162-6.162zm0 10.162c-2.209 0-4-1.79-4-4 0-2.209 1.791-4 4-4s4 1.791 4 4c0 2.21-1.791 4-4 4zm6.406-11.845c-.796 0-1.441.645-1.441 1.44s.645 1.44 1.441 1.44c.795 0 1.439-.645 1.439-1.44s-.644-1.44-1.439-1.44z" />
        </svg>
      ),
    },
  ].filter((l) => l.url);

  if (links.length === 0) return null;

  return (
    <div className="flex gap-4 border-t border-sd-ink/10 pt-4 w-full justify-center">
      {links.map(({ label, url, icon }) => (
        <a
          key={label}
          href={url!}
          target="_blank"
          rel="noopener noreferrer"
          title={label}
          className="text-sd-blue hover:text-sd-red transition-colors"
        >
          {icon}
        </a>
      ))}
    </div>
  );
}

// ── Stats cards ───────────────────────────────────────────────────────────────

function DrafterStatsCard({ stats }: { stats: DrafterStatsResponse }) {
  return (
    <div className="bg-white border-2 border-sd-ink p-6">
      <h2 className="font-oswald font-bold text-[13px] tracking-widest text-sd-red mb-4">
        DRAFTER STATS
      </h2>
      <div className="flex flex-col gap-3">
        <StatRow label="DRAFTS" value={stats.totalDrafts} />
        {stats.firstDraft && (
          <StatRow label="FIRST DRAFT" value={stats.firstDraft.draftTitle}
            href={`/drafts/${stats.firstDraft.draftPublicId}`} />
        )}
        {stats.mostRecentDraft && (
          <StatRow label="MOST RECENT" value={stats.mostRecentDraft.draftTitle}
            href={`/drafts/${stats.mostRecentDraft.draftPublicId}`} />
        )}
        <StatRow label="FILMS DRAFTED" value={stats.filmsDrafted} />
        <StatRow label="VETOES USED" value={stats.vetoesUsed} />
        <StatRow label="TIMES VETOED" value={stats.timesVetoed} />
        {!!stats.vetoOverridesUsed && (
          <StatRow label="VETO OVERRIDES USED" value={stats.vetoOverridesUsed} />
        )}
        {!!stats.timesVetoOverridden && (
          <StatRow label="TIMES VETO OVERRIDDEN" value={stats.timesVetoOverridden} />
        )}
        {!!stats.commissionerOverrides && (
          <StatRow label="COMMISSIONER OVERRIDES" value={stats.commissionerOverrides} />
        )}
        <div className="border-t border-sd-ink/10 pt-3 mt-1 flex flex-col gap-3">
          <StatRow
            label="ROLLOVER VETO"
            value={stats.hasRolloverVeto ? "YES" : "NO"}
            highlight={stats.hasRolloverVeto}
          />
          {(stats.vetoOverridesUsed ?? 0) > 0 && (
            <StatRow
              label="ROLLOVER VETO OVERRIDE"
              value={stats.hasRolloverVetoOverride ? "YES" : "NO"}
              highlight={stats.hasRolloverVetoOverride}
            />
          )}
        </div>
      </div>
    </div>
  );
}

function HostStatsCard({ stats }: { stats: HostStatsResponse }) {
  return (
    <div className="bg-white border-2 border-sd-ink p-6">
      <h2 className="font-oswald font-bold text-[13px] tracking-widest text-sd-red mb-4">
        HOST STATS
      </h2>
      <div className="flex flex-col gap-3">
        <StatRow label="DRAFTS HOSTED" value={stats.draftsHosted} />
        {stats.firstHostedDraft && (
          <StatRow label="FIRST HOSTED" value={stats.firstHostedDraft.draftTitle}
            href={`/drafts/${stats.firstHostedDraft.draftPublicId}`} />
        )}
        {stats.mostRecentHostedDraft && (
          <StatRow label="MOST RECENT" value={stats.mostRecentHostedDraft.draftTitle}
            href={`/drafts/${stats.mostRecentHostedDraft.draftPublicId}`} />
        )}
      </div>
    </div>
  );
}

function StatRow({
  label,
  value,
  href,
  highlight,
}: {
  label: string;
  value: string | number | undefined;
  href?: string;
  highlight?: boolean;
}) {
  return (
    <div className="flex justify-between items-baseline gap-2">
      <span className="font-mono text-[10px] tracking-widest text-[#5a6075] shrink-0">
        {label}
      </span>
      {href ? (
        <Link
          href={href}
          className="font-oswald font-bold text-[14px] text-sd-blue hover:text-sd-red transition-colors text-right max-w-[170px] leading-tight"
        >
          {value ?? "—"}
        </Link>
      ) : (
        <span
          className={`font-oswald font-bold text-[15px] text-right max-w-[170px] leading-tight ${highlight ? "text-sd-red" : "text-sd-ink"
            }`}
        >
          {value ?? "—"}
        </span>
      )}
    </div>
  );
}

// ── Table of contents ─────────────────────────────────────────────────────────

function TableOfContents({
  draftIds,
  picksByDraft,
  vetosByDraft,
}: {
  draftIds: string[];
  picksByDraft: Map<string, DraftHistoryItem>;
  vetosByDraft: Map<string, VetoHistoryItem[]>;
}) {
  return (
    <div className="bg-white border-2 border-sd-ink p-6">
      <h2 className="font-oswald font-bold text-[13px] tracking-widest text-sd-red mb-4">
        DRAFT HISTORY
        <span className="text-sd-ink/40 ml-2 font-normal text-[12px]">
          ({draftIds.length})
        </span>
      </h2>
      <div className="columns-2 gap-6">
        {draftIds.map((draftId) => {
          const item = picksByDraft.get(draftId);
          const vetoes = vetosByDraft.get(draftId) ?? [];
          const draft = item?.draft ?? vetoes[0]?.draft;
          if (!draft) return null;
          const releaseDate = draft.releaseDates?.[0];

          return (
            <a
              key={draftId}
              href={`#draft-${draftId}`}
              className="flex items-baseline justify-between gap-2 py-2 border-b border-sd-ink/5 hover:border-sd-red group transition-colors break-inside-avoid"
            >
              <span className="font-oswald text-[14px] text-sd-ink group-hover:text-sd-blue transition-colors leading-tight">
                {draft.draftTitle}
              </span>
              <span className="font-mono text-[10px] tracking-widest text-[#5a6075] shrink-0">
                {releaseDate ? new Date(releaseDate).getFullYear() : "TBD"}
              </span>
            </a>
          );
        })}
      </div>
    </div>
  );
}

// ── Draft history ─────────────────────────────────────────────────────────────

function DraftHistorySection({
  draftIds,
  picksByDraft,
  vetosByDraft,
}: {
  draftIds: string[];
  picksByDraft: Map<string, DraftHistoryItem>;
  vetosByDraft: Map<string, VetoHistoryItem[]>;
}) {
  return (
    <div className="flex flex-col gap-6">
      {draftIds.map((draftId) => {
        const item = picksByDraft.get(draftId);
        const vetoes = vetosByDraft.get(draftId) ?? [];
        const draft = item?.draft ?? vetoes[0]?.draft;
        if (!draft) return null;

        return (
          <DraftBlock
            key={draftId}
            draft={draft}
            picks={item?.picks ?? []}
            vetoesIssued={vetoes}
          />
        );
      })}
    </div>
  );
}

type PickActivity = { kind: "pick"; playOrder: number; pick: PickItem };
type VetoActivity = { kind: "veto"; playOrder: number; veto: VetoHistoryItem };
type Activity = PickActivity | VetoActivity;

function DraftBlock({
  draft,
  picks,
  vetoesIssued,
}: {
  draft: DraftBrief;
  picks: PickItem[];
  vetoesIssued: VetoHistoryItem[];
}) {
  const releaseDate = draft.releaseDates?.[0];
  const finalPicks = picks.filter((p) => !p.wasVetoed || p.wasVetoOverridden);

  const activity: Activity[] = [
    ...picks.map((p) => ({
      kind: "pick" as const,
      playOrder: p.playOrder ?? 0,
      pick: p,
    })),
    ...vetoesIssued.map((v) => ({
      kind: "veto" as const,
      playOrder: v.playOrder ?? 0,
      veto: v,
    })),
  ].sort((a, b) => a.playOrder - b.playOrder);

  return (
    <div
      id={`draft-${draft.draftPublicId}`}
      className="bg-white border-2 border-sd-ink scroll-mt-6"
    >
      {/* Header */}
      <div className="bg-sd-ink px-6 py-5 flex items-center justify-between gap-4">
        <Link
          href={`/drafts/${draft.draftPublicId}`}
          className="font-oswald font-bold text-[22px] text-white hover:text-sd-red transition-colors leading-tight"
        >
          {draft.draftTitle}
        </Link>
        <div className="flex items-center gap-6 shrink-0">
          <span className="font-mono text-[12px] tracking-widest text-white/80">
            {finalPicks.length} FILM{finalPicks.length !== 1 ? "S" : ""}
          </span>
          {releaseDate && (
            <span className="font-mono text-[12px] tracking-widest text-white/80">
              {new Date(releaseDate as unknown as string).toLocaleDateString("en-US", {
                year: "numeric",
                month: "short",
                day: "numeric",
              })}
            </span>
          )}
        </div>
      </div>

      {/* Activity rows */}
      <div className="divide-y divide-sd-ink/5">
        {activity.map((a, i) =>
          a.kind === "pick" ? (
            <PickRow
              key={`pick-${a.pick.playOrder}-${a.pick.moviePublicId}-${i}`}
              pick={a.pick}
            />
          ) : (
            <VetoIssuedRow
              key={`veto-${a.veto.targetPickPublicId}`}
              veto={a.veto}
            />
          )
        )}
      </div>
    </div>
  );
}

function PickRow({ pick }: { pick: PickItem }) {
  const isVetoed = pick.wasVetoed && !pick.wasVetoOverridden;
  const isVetoOverridden = pick.wasVetoed && pick.wasVetoOverridden;
  const isCommissionerOverridden = pick.wasCommissionerOverridden;
  const isStruck = isVetoed || isCommissionerOverridden;

  return (
    <div className="px-6 py-4 flex items-center gap-4">
      <div
        className={`w-9 h-9 shrink-0 flex items-center justify-center border-2 font-oswald font-bold text-[15px] ${isStruck
            ? "border-sd-ink/20 text-sd-ink/30"
            : isVetoOverridden
              ? "border-sd-blue text-sd-blue"
              : "border-sd-red text-sd-red"
          }`}
      >
        <span className={isStruck ? "line-through" : ""}>{pick.position}</span>
      </div>

      <div className="flex-1 min-w-0">
        <div className={`font-oswald font-bold text-[18px] leading-tight ${isStruck ? "line-through text-sd-ink/35" : "text-sd-ink"}`}>
          <Link
            href={`/media/${pick.moviePublicId}`}
            className="hover:text-sd-blue transition-colors"
          >
            {pick.movieTitle}
            {pick.movieVersionName && (
              <span className="font-normal text-[15px] text-sd-ink/50 ml-1.5">
                ({pick.movieVersionName})
              </span>
            )}
          </Link>
        </div>

        <div className="mt-1 flex flex-wrap gap-x-4 gap-y-1">
          {isVetoed && (
            <span className="font-mono text-[11px] tracking-widest text-sd-red font-bold">
              ✕ VETOED
              {pick.vetoedByDisplayName && pick.vetoedByPublicId && (
                <span className="font-normal">
                  {" "}BY{" "}
                  <Link
                    href={`/drafters/${pick.vetoedByPublicId}`}
                    className="hover:underline"
                  >
                    {pick.vetoedByDisplayName.toUpperCase()}
                  </Link>
                </span>
              )}
              {pick.vetoedByDisplayName && !pick.vetoedByPublicId && (
                <span className="font-normal"> BY {pick.vetoedByDisplayName.toUpperCase()}</span>
              )}
            </span>
          )}
          {isVetoOverridden && (
            <>
              <span className="font-mono text-[11px] tracking-widest text-sd-ink/40 line-through">
                VETOED
                {pick.vetoedByDisplayName ? ` BY ${pick.vetoedByDisplayName.toUpperCase()}` : ""}
              </span>
              <span className="font-mono text-[11px] tracking-widest text-sd-blue font-bold">
                ↩ VETO OVERRIDDEN
                {pick.overrideByDisplayName && pick.overrideByPublicId && (
                  <span className="font-normal">
                    {" "}BY{" "}
                    <Link
                      href={`/drafters/${pick.overrideByPublicId}`}
                      className="hover:underline"
                    >
                      {pick.overrideByDisplayName.toUpperCase()}
                    </Link>
                  </span>
                )}
                {pick.overrideByDisplayName && !pick.overrideByPublicId && (
                  <span className="font-normal"> BY {pick.overrideByDisplayName.toUpperCase()}</span>
                )}
              </span>
            </>
          )}
          {isCommissionerOverridden && (
            <span className="font-mono text-[11px] tracking-widest text-[#5a6075]">
              ⚑ COMMISSIONER OVERRIDE
            </span>
          )}
        </div>
      </div>
    </div>
  );
}

function VetoIssuedRow({ veto }: { veto: VetoHistoryItem }) {
  return (
    <div className="px-6 py-4 flex items-center gap-4 bg-sd-red/[0.03]">
      <div className="w-9 h-9 shrink-0 flex items-center justify-center border-2 border-sd-red font-oswald font-bold text-[15px] text-sd-red">
        <span className="line-through">{veto.position}</span>
      </div>

      <div className="flex-1 min-w-0">
        <div className="font-mono text-[11px] tracking-widest text-sd-red font-bold leading-snug">
          ✕ VETOED{" "}
          <Link
            href={`/media/${veto.moviePublicId}`}
            className="line-through text-sd-ink/50 font-normal hover:text-sd-blue transition-colors"
          >
            {veto.movieTitle}
          </Link>
          {" "}PLAYED BY{" "}
          {veto.targetDrafterPublicId ? (
            <Link
              href={`/drafters/${veto.targetDrafterPublicId}`}
              className="font-normal hover:underline"
            >
              {veto.targetDrafterDisplayName.toUpperCase()}
            </Link>
          ) : (
            <span className="font-normal">
              {veto.targetDrafterDisplayName.toUpperCase()}
            </span>
          )}
        </div>
        {veto.wasVetoOverridden && (
          <div className="mt-1 font-mono text-[11px] tracking-widests text-sd-blue">
            ↩ VETO OVERRIDDEN
            {veto.overrideByDisplayName && veto.overrideByPublicId && (
              <span className="font-normal">
                {" "}BY{" "}
                <Link
                  href={`/drafters/${veto.overrideByPublicId}`}
                  className="hover:underline"
                >
                  {veto.overrideByDisplayName.toUpperCase()}
                </Link>
              </span>
            )}
            {veto.overrideByDisplayName && !veto.overrideByPublicId && (
              <span className="font-normal"> BY {veto.overrideByDisplayName.toUpperCase()}</span>
            )}
          </div>
        )}
      </div>
    </div>
  );
}