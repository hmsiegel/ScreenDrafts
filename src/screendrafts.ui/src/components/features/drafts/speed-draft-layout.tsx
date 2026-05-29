"use client";

/**
 * SpeedDraftLayout — owns sub-draft tab state and renders the full two-column
 * layout (sidebar + pick section) for SpeedDraft episodes.
 *
 * page.tsx (Server Component) fetches all data and passes it in as plain props;
 * this component handles all interactivity.
 */

import { useState } from "react";
import Link from "next/link";
import {
  GetDraftResponse,
  GetDraftPartResponse,
  GetDraftPickResponse,
  TriviaResultResponse,
} from "@/lib/dto";
import { DraftPick, Avatar } from "@/components/features/drafts/draft-pick";
import { DraftPartPredictionData, PredictionsSection } from "@/components/features/drafts/predictions-section";
import DraftTypeBadge from "@/components/ui/draft-type-badge";
import { draftTypeFromNumber } from "@/lib/draft-type-display";
import EpisodeImage from "@/components/ui/episode-image";
import { format } from "date-fns/format";

// ── Helpers ────────────────────────────────────────────────────────────────

const SUBJECT_KIND_LABELS: Record<number, string> = {
  0: "Actor",
  1: "Director",
  2: "Word",
  3: "Movie",
  4: "Other",
};

function subjectLabel(kind: number | undefined, name: string | undefined): string {
  if (name) return name;
  return SUBJECT_KIND_LABELS[kind ?? 0] ?? "Sub-Draft";
}

function formatDate(raw: Date | string | undefined): string {
  if (!raw) return "—";
  try {
    return format(new Date(raw as string), "MMM dd, yyyy").toUpperCase();
  } catch {
    return "—";
  }
}

function SectionLabel({ children }: { children: React.ReactNode }) {
  return (
    <div className="font-mono text-[10px] tracking-widest text-sd-red font-bold mb-3">
      ★ {children}
    </div>
  );
}

interface PersonEntry {
  id: string;
  name: string;
  index: number;
  role: string;
  personPublicId?: string;
}

function PersonList({ entries }: { entries: PersonEntry[] }) {
  if (entries.length === 0) return null;
  return (
    <div className="flex flex-col gap-3">
      {entries.map(({ id, name, index, role, personPublicId }) => {
        const nameEl = (
          <div className="font-sans font-semibold text-[14px] text-sd-ink leading-tight">
            {name}
          </div>
        );
        return (
          <div key={id} className="flex items-center gap-3">
            <Avatar name={name} colorIndex={index} size={40} />
            <div>
              {personPublicId ? (
                <Link href={`/drafters/${personPublicId}`} className="hover:text-sd-blue transition-colors">
                  {nameEl}
                </Link>
              ) : nameEl}
              <div className="font-mono text-[10px] text-[#5a6075]">{role}</div>
            </div>
          </div>
        );
      })}
    </div>
  );
}

interface NavEntry {
  publicId: string;
  title: string | null | undefined;
  direction: "prev" | "next";
}

function NavBlock({ entries }: { entries: NavEntry[] }) {
  if (entries.length === 0) return null;
  return (
    <div className="flex flex-col gap-2">
      {entries.map(({ publicId, title, direction }) => (
        <Link
          key={publicId}
          href={`/drafts/${publicId}`}
          className={`flex-1 border border-sd-ink text-sd-ink font-oswald text-[12px] tracking-wide px-3 py-2 hover:bg-sd-ink hover:text-white transition-colors truncate ${
            direction === "next" ? "text-right" : "text-left"
          }`}
          title={title ?? (direction === "prev" ? "Previous" : "Next")}
        >
          {direction === "prev" ? `‹ ${title ?? "Previous"}` : `${title ?? "Next"} ›`}
        </Link>
      ))}
    </div>
  );
}

// ── Props ──────────────────────────────────────────────────────────────────

interface SubDraftTab {
  index: number;
  label: string;
}

export interface SpeedDraftLayoutProps {
  draft: GetDraftResponse;
  part: GetDraftPartResponse;
  participantNames: Map<string, string>;
  participantIndex: Map<string, number>;
  triviaResults: TriviaResultResponse[];
  predictionData: DraftPartPredictionData;
}

// ── Main component ─────────────────────────────────────────────────────────

export function SpeedDraftLayout({
  draft,
  part,
  participantNames,
  participantIndex,
  triviaResults,
  predictionData,
}: SpeedDraftLayoutProps) {

  // ── Tabs ─────────────────────────────────────────────────────────────────

  const tabs: SubDraftTab[] =
    (part.subDrafts ?? []).length > 0
      ? [...(part.subDrafts ?? [])]
          .sort((a, b) => (a.index ?? 0) - (b.index ?? 0))
          .map((sd) => ({
            index: sd.index ?? 1,
            label: subjectLabel(sd.subjectKind, sd.subjectName),
          }))
      : [...new Set((part.picks ?? []).map((p) => p.subDraftIndex ?? 1))]
          .sort((a, b) => a - b)
          .map((idx) => ({ index: idx, label: `Sub-Draft ${idx}` }));

  const [activeIndex, setActiveIndex] = useState<number>(tabs[0]?.index ?? 1);

  // ── Pick filtering ────────────────────────────────────────────────────────

  const isVetoed = (pick: GetDraftPickResponse) =>
    !!pick.veto && !pick.veto.isOverriden;
  const isCommissionerRemoved = (pick: GetDraftPickResponse) =>
    pick.commissionerOverride !== null && pick.commissionerOverride !== undefined;

  const allPicks = [...(part.picks ?? [])].sort(
    (a, b) => (a.playOrder ?? 0) - (b.playOrder ?? 0)
  );

  const activePicks = allPicks.filter((p) => (p.subDraftIndex ?? 1) === activeIndex);
  const activeFinalPicks = activePicks.filter(
    (p) => !isVetoed(p) && !isCommissionerRemoved(p)
  );
  const topPlayOrder = activeFinalPicks.reduce(
    (max, p) => Math.max(max, p.playOrder ?? 0),
    0
  );
  const totalFinalPicks = allPicks.filter(
    (p) => !isVetoed(p) && !isCommissionerRemoved(p)
  ).length;

  // ── Sidebar data ──────────────────────────────────────────────────────────

  const episodeNumber = (draft as Record<string, unknown>).episodeNumber as number | undefined;
  const releaseDate = part.releases?.[0]?.releaseDate;
  const draftTypeDisplay = draft.draftType?.name
    ? draftTypeFromNumber(draft.draftType.value)
    : undefined;
  const listenUrl = process.env.NEXT_PUBLIC_SPOTIFY_URL ?? "#";

  // Drafters
  const drafterEntries: PersonEntry[] = [];
  let dIdx = 0;
  const seenDrafterIds = new Set<string>();
  for (const p of part.participants ?? []) {
    const pid = p.participantIdValue ?? "";
    if (p.participantKindValue?.value === 2) continue;
    if (pid && !seenDrafterIds.has(pid)) {
      seenDrafterIds.add(pid);
      drafterEntries.push({
        id: pid,
        name: participantNames.get(pid) ?? p.displayName ?? "Unknown",
        index: dIdx++,
        role: "DRAFTER",
        personPublicId: p.personPublicId,
      });
    }
  }

  // Hosts
  const hostEntries: PersonEntry[] = [];
  let hIdx = 0;
  const seenHostIds = new Set<string>();
  if (part.primaryHost?.hostPublicId && !seenHostIds.has(part.primaryHost.hostPublicId)) {
    seenHostIds.add(part.primaryHost.hostPublicId);
    hostEntries.push({
      id: part.primaryHost.hostPublicId,
      name: part.primaryHost.displayName ?? "Unknown",
      index: hIdx++,
      role: "PRIMARY HOST",
      personPublicId: part.primaryHost.personPublicId,
    });
  }
  for (const coHost of part.coHosts ?? []) {
    if (coHost.hostPublicId && !seenHostIds.has(coHost.hostPublicId)) {
      seenHostIds.add(coHost.hostPublicId);
      hostEntries.push({
        id: coHost.hostPublicId,
        name: coHost.displayName ?? "Unknown",
        index: hIdx++,
        role: "CO-HOST",
        personPublicId: coHost.personPublicId,
      });
    }
  }

  // Trivia — filtered to active sub-draft
  const activeTrivia = triviaResults.filter(
    (t) => (t as Record<string, unknown>).subDraftIndex === activeIndex
  );

  // Nav
  const navEntries: NavEntry[] = [
    ...(part.previousDraftPublicId
      ? [{ publicId: part.previousDraftPublicId, title: part.previousDraftTitle, direction: "prev" as const }]
      : []),
    ...(part.nextDraftPublicId
      ? [{ publicId: part.nextDraftPublicId, title: part.nextDraftTitle, direction: "next" as const }]
      : []),
  ];

  const campaignNavEntries: NavEntry[] = [
    ...(part.previousCampaignDraftPublicId
      ? [{ publicId: part.previousCampaignDraftPublicId, title: part.previousCampaignDraftTitle, direction: "prev" as const }]
      : []),
    ...(part.nextCampaignDraftPublicId
      ? [{ publicId: part.nextCampaignDraftPublicId, title: part.nextCampaignDraftTitle, direction: "next" as const }]
      : []),
  ];

  // ── Render ────────────────────────────────────────────────────────────────

  return (
    <div
      className="grid gap-10 mx-auto"
      style={{ gridTemplateColumns: "380px 1fr", maxWidth: 1400 }}
    >
      {/* ── Left sidebar ───────────────────────────────────────────────── */}
      <aside
        className="bg-white border-2 border-sd-ink sticky top-6 self-start"
        style={{ padding: "28px 24px" }}
      >
        <div className="font-mono text-[10px] tracking-widest text-sd-blue font-bold mb-1">
          EPISODE
        </div>
        <div className="font-oswald font-bold text-[88px] text-sd-red leading-[0.92] mb-2">
          {episodeNumber ?? "—"}
        </div>

        <EpisodeImage title={draft.title} />

        <div className="font-oswald font-semibold text-[22px] text-sd-ink leading-[1.15] tracking-tight mb-3">
          {draft.title}
        </div>

        <div className="flex items-center gap-2 mb-2 font-mono text-[11px] text-[#5a6075]">
          {draftTypeDisplay && <DraftTypeBadge type={draftTypeDisplay} />}
        </div>

        {releaseDate && (
          <div className="font-mono font-semibold text-[13px] text-sd-blue mb-4">
            {formatDate(releaseDate)}
          </div>
        )}

        {draft.campaignName && (
          <div className="border-t border-sd-ink/10 pt-4 mt-4">
            <SectionLabel>CAMPAIGN</SectionLabel>
            <div className="font-oswald font-semibold text-[16px] text-sd-ink">
              {draft.campaignName}
            </div>
          </div>
        )}

        {drafterEntries.length > 0 && (
          <div className="border-t border-sd-ink/10 pt-4 mt-4">
            <SectionLabel>DRAFTERS</SectionLabel>
            <PersonList entries={drafterEntries} />
          </div>
        )}

        {hostEntries.length > 0 && (
          <div className="border-t border-sd-ink/10 pt-4 mt-4">
            <SectionLabel>HOSTS</SectionLabel>
            <PersonList entries={hostEntries} />
          </div>
        )}

        {/* Trivia — label shows which sub-draft is active */}
        {triviaResults.length > 0 && (
          <div className="border-t border-sd-ink/10 pt-4 mt-4">
            <SectionLabel>
              TRIVIA
              {tabs.length > 1 && (
                <span className="ml-1 text-sd-ink/40 normal-case font-normal tracking-normal">
                  — {tabs.find((t) => t.index === activeIndex)?.label ?? ""}
                </span>
              )}
            </SectionLabel>
            {activeTrivia.length > 0 ? (
              <div className="flex flex-col gap-2">
                {activeTrivia.map((t) => (
                  <div
                    key={t.position}
                    className="flex items-baseline justify-between text-[13px]"
                  >
                    <div className="flex items-baseline gap-2">
                      <span className="font-mono text-sd-red text-[11px] font-bold w-4 shrink-0">
                        {t.position}.
                      </span>
                      <span className="font-sans font-semibold text-sd-ink">
                        {t.participantDisplayName}
                      </span>
                    </div>
                    <span className="font-mono text-[#5a6075]">{t.questionsWon}</span>
                  </div>
                ))}
              </div>
            ) : (
              <p className="font-mono text-[11px] text-sd-ink/40">No trivia for this sub-draft.</p>
            )}
          </div>
        )}

        {navEntries.length > 0 && (
          <div className="border-t border-sd-ink/10 pt-4 mt-4">
            <NavBlock entries={navEntries} />
          </div>
        )}

        {campaignNavEntries.length > 0 && (
          <div className="border-t border-sd-ink/10 pt-4 mt-4">
            <SectionLabel>IN THIS CAMPAIGN</SectionLabel>
            <div className="flex flex-col gap-2">
              {campaignNavEntries.map(({ publicId, title, direction }) => (
                <Link
                  key={publicId}
                  href={`/drafts/${publicId}`}
                  className={`border border-sd-blue text-sd-blue font-oswald text-[12px] tracking-wide px-3 py-2 hover:bg-sd-blue hover:text-white transition-colors ${
                    direction === "next" ? "text-right" : "text-left"
                  }`}
                >
                  {direction === "prev" ? `‹ ${title ?? "Previous"}` : `${title ?? "Next"} ›`}
                </Link>
              ))}
            </div>
          </div>
        )}

        <div className="mt-4">
          <a
            href={listenUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="block w-full bg-sd-ink text-white text-center font-oswald text-[12px] tracking-wide px-4 py-3 hover:opacity-80 transition-opacity"
          >
            ▶ LISTEN ON SPOTIFY
          </a>
        </div>
      </aside>

      {/* ── Right column ───────────────────────────────────────────────── */}
      <div
        className="bg-white border-2 border-sd-ink"
        style={{ padding: "40px 48px", maxWidth: 720 }}
      >
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

          <h2 className="font-oswald font-bold text-[32px] text-sd-ink mb-6">
            THE FINAL LIST
          </h2>

          {/* Tab bar */}
          <div className="flex items-end gap-0 border-b-2 border-sd-ink mb-0">
            {tabs.map((tab) => {
              const isActive = tab.index === activeIndex;
              return (
                <button
                  key={tab.index}
                  onClick={() => setActiveIndex(tab.index)}
                  className={`font-oswald font-bold text-[13px] tracking-wide px-5 py-2.5 transition-colors border-t-2 border-l-2 border-r-2 -mb-[2px] ${
                    isActive
                      ? "bg-sd-ink text-white border-sd-ink"
                      : "bg-white text-sd-ink/50 border-sd-ink/20 hover:text-sd-ink hover:border-sd-ink/50"
                  }`}
                >
                  {tab.label.toUpperCase()}
                </button>
              );
            })}
            <div className="ml-auto pb-2 font-mono text-[11px] text-sd-ink/50 self-end">
              {totalFinalPicks} PICKS TOTAL · {activeFinalPicks.length} HERE
            </div>
          </div>

          {/* Active sub-draft picks */}
          <div className="mt-6">
            {activePicks.length === 0 ? (
              <p className="font-mono text-sm text-sd-ink/40">No picks recorded.</p>
            ) : (
              activePicks.map((pick, i) => (
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
        </article>
      </div>
    </div>
  );
}