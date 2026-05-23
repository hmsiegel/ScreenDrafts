import DraftTypeBadge from "@/components/ui/draft-type-badge";
import { draftTypeFromNumber } from "@/lib/draft-type-display";
import { GetDraftPartResponse, GetDraftResponse, TriviaResultResponse } from "@/lib/dto";
import Link from "next/link";
import { Avatar } from "./draft-pick";
import { format } from "date-fns/format";
import EpisodeImage from "@/components/ui/episode-image";

function formatDate(raw: Date | string | undefined): string {
  if (!raw) return "—";
  try {
    return format(new Date(raw), "MMM dd, yyyy").toUpperCase();
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

// ── Participant / host list ────────────────────────────────────────────────

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

// ── Trivia block ──────────────────────────────────────────────────────────

function TriviaBlock({ results }: { results: TriviaResultResponse[] }) {
  if (results.length === 0) return null;
  return (
    <div className="flex flex-col gap-2">
      {results.map((t) => (
        <div key={t.position} className="flex items-baseline justify-between text-[13px]">
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
  );
}

// ── Navigation block ──────────────────────────────────────────────────────

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
          className={`flex-1 border border-sd-ink text-sd-ink font-oswald text-[12px] tracking-wide px-3 py-2 hover:bg-sd-ink hover:text-white transition-colors truncate ${direction === "next" ? "text-right" : "text-left"}`}
          title={title ?? (direction === "prev" ? "Previous" : "Next")}
        >
          {direction === "prev" ? `‹ ${title ?? "Previous"}` : `${title ?? "Next"} ›`}
        </Link>
      ))}
    </div>
  );
}

// ── Nav entries derived from a part ──────────────────────────────────────

function partNavEntries(part: GetDraftPartResponse): NavEntry[] {
  return [
    ...(part.previousDraftPublicId ? [{ publicId: part.previousDraftPublicId, title: part.previousDraftTitle, direction: "prev" as const }] : []),
    ...(part.nextDraftPublicId ? [{ publicId: part.nextDraftPublicId, title: part.nextDraftTitle, direction: "next" as const }] : []),
  ];
}

function partCampaignNavEntries(part: GetDraftPartResponse): NavEntry[] {
  return [
    ...(part.previousCampaignDraftPublicId ? [{ publicId: part.previousCampaignDraftPublicId, title: part.previousCampaignDraftTitle, direction: "prev" as const }] : []),
    ...(part.nextCampaignDraftPublicId ? [{ publicId: part.nextCampaignDraftPublicId, title: part.nextCampaignDraftTitle, direction: "next" as const }] : []),
  ];
}

// ── Per-part sidebar section (multi-part only) ────────────────────────────

interface PartSidebarSectionProps {
  part: GetDraftPartResponse;
  partLabel: string;
  participantNames: Map<string, string>;
  triviaByPart: Map<string, TriviaResultResponse[]>;
  colorIndexOffset: number;
}

function PartSidebarSection({
  part,
  partLabel,
  participantNames,
  triviaByPart,
  colorIndexOffset,
}: PartSidebarSectionProps) {
  const releaseDate = part.releases?.[0]?.releaseDate;

  // Drafters
  const drafterEntries: PersonEntry[] = [];
  let idx = colorIndexOffset;
  const seenDrafterIds = new Set<string>();
  for (const p of part.participants ?? []) {
    const pid = p.participantIdValue ?? "";
    if (p.participantKindValue?.value === 2) continue;
    if (pid && !seenDrafterIds.has(pid)) {
      seenDrafterIds.add(pid);
      drafterEntries.push({
        id: pid,
        name: participantNames.get(pid) ?? p.displayName ?? "Unknown",
        index: idx++,
        role: "DRAFTER",
        personPublicId: p.personPublicId,
      });
    }
  }

  // Hosts
  const hostEntries: PersonEntry[] = [];
  let hostIdx = 0;
  const seenHostIds = new Set<string>();
  if (part.primaryHost?.hostPublicId && !seenHostIds.has(part.primaryHost.hostPublicId)) {
    seenHostIds.add(part.primaryHost.hostPublicId);
    hostEntries.push({
      id: part.primaryHost.hostPublicId,
      name: part.primaryHost.displayName ?? "Unknown",
      index: hostIdx++,
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
        index: hostIdx++,
        role: "CO-HOST",
        personPublicId: coHost.personPublicId,
      });
    }
  }

  const triviaResults = triviaByPart.get(part.publicId ?? "") ?? [];
  const navEntries = partNavEntries(part);
  const campaignNavEntries = partCampaignNavEntries(part);

  return (
    <div className="border-t border-sd-ink/15 pt-4 mt-4">
      {/* Part label + date */}
      <div className="font-mono text-[10px] tracking-widest text-sd-blue font-bold mb-1">
        {partLabel.toUpperCase()}
      </div>
      {releaseDate && (
        <div className="font-mono font-semibold text-[12px] text-sd-ink/60 mb-3">
          {formatDate(releaseDate)}
        </div>
      )}

      {drafterEntries.length > 0 && (
        <div className="mb-4">
          <SectionLabel>DRAFTERS</SectionLabel>
          <PersonList entries={drafterEntries} />
        </div>
      )}

      {hostEntries.length > 0 && (
        <div className="mb-4">
          <SectionLabel>HOSTS</SectionLabel>
          <PersonList entries={hostEntries} />
        </div>
      )}

      {triviaResults.length > 0 && (
        <div className="mb-4">
          <SectionLabel>TRIVIA</SectionLabel>
          <TriviaBlock results={triviaResults} />
        </div>
      )}

      {navEntries.length > 0 && (
        <div className="mb-3">
          <NavBlock entries={navEntries} />
        </div>
      )}

      {campaignNavEntries.length > 0 && (
        <div>
          <SectionLabel>IN THIS CAMPAIGN</SectionLabel>
          <div className="flex flex-col gap-2">
            {campaignNavEntries.map(({ publicId, title, direction }) => (
              <Link
                key={publicId}
                href={`/drafts/${publicId}`}
                className={`border border-sd-blue text-sd-blue font-oswald text-[12px] tracking-wide px-3 py-2 hover:bg-sd-blue hover:text-white transition-colors ${direction === "next" ? "text-right" : "text-left"}`}
              >
                {direction === "prev" ? `‹ ${title ?? "Previous"}` : `${title ?? "Next"} ›`}
              </Link>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

// ── Main export ───────────────────────────────────────────────────────────

interface DraftSidebarProps {
  draft: GetDraftResponse;
  participantNames: Map<string, string>;
  triviaByPart: Map<string, TriviaResultResponse[]>;
  isMultiPart: boolean;
}

export default function DraftSidebar({
  draft,
  participantNames,
  triviaByPart,
  isMultiPart,
}: DraftSidebarProps) {
  const parts: GetDraftPartResponse[] = draft.parts ?? [];
  const firstPart = parts[0];

  const episodeNumber: number | undefined =
    (draft as Record<string, unknown>).episodeNumber as number | undefined ??
    (firstPart?.releases?.[0] as Record<string, unknown> | undefined)?.episodeNumber as number | undefined;

  // Release date shown at top only for single-part drafts; multi-part shows it per-section
  const releaseDate = isMultiPart ? undefined : firstPart?.releases?.[0]?.releaseDate;

  const draftTypeDisplay =
    draft.draftType?.name
      ? draftTypeFromNumber(draft.draftType.value)
      : draftTypeFromNumber((draft as Record<string, unknown>).draftTypeValue as number | undefined);

  const listenUrl = process.env.NEXT_PUBLIC_SPOTIFY_URL ?? "#";

  // ── Single-part: collect drafters/hosts/trivia flat ───────────────────────
  let singlePartDrafters: PersonEntry[] = [];
  let singlePartHosts: PersonEntry[] = [];
  let singlePartTrivia: TriviaResultResponse[] = [];

  if (!isMultiPart && firstPart) {
    let idx = 0;
    const seenDrafterIds = new Set<string>();
    for (const p of firstPart.participants ?? []) {
      const pid = p.participantIdValue ?? "";
      if (p.participantKindValue?.value === 2) continue;
      if (pid && !seenDrafterIds.has(pid)) {
        seenDrafterIds.add(pid);
        singlePartDrafters.push({
          id: pid,
          name: participantNames.get(pid) ?? p.displayName ?? "Unknown",
          index: idx++,
          role: "DRAFTER",
          personPublicId: p.personPublicId,
        });
      }
    }

    let hostIdx = 0;
    const seenHostIds = new Set<string>();
    if (firstPart.primaryHost?.hostPublicId && !seenHostIds.has(firstPart.primaryHost.hostPublicId)) {
      seenHostIds.add(firstPart.primaryHost.hostPublicId);
      singlePartHosts.push({
        id: firstPart.primaryHost.hostPublicId,
        name: firstPart.primaryHost.displayName ?? "Unknown",
        index: hostIdx++,
        role: "PRIMARY HOST",
        personPublicId: firstPart.primaryHost.personPublicId,
      });
    }
    for (const coHost of firstPart.coHosts ?? []) {
      if (coHost.hostPublicId && !seenHostIds.has(coHost.hostPublicId)) {
        seenHostIds.add(coHost.hostPublicId);
        singlePartHosts.push({
          id: coHost.hostPublicId,
          name: coHost.displayName ?? "Unknown",
          index: hostIdx++,
          role: "CO-HOST",
          personPublicId: coHost.personPublicId,
        });
      }
    }

    singlePartTrivia = triviaByPart.get(firstPart.publicId ?? "") ?? [];
  }

  // ── Color index offsets per part (avatars don't repeat colors across parts) ─
  const partColorOffsets: number[] = [];
  let runningOffset = 0;
  for (const part of parts) {
    partColorOffsets.push(runningOffset);
    runningOffset += (part.participants ?? []).filter(
      (p) => p.participantKindValue?.value !== 2
    ).length;
  }

  return (
    <aside
      className="bg-white border-2 border-sd-ink sticky top-6 self-start"
      style={{ padding: "28px 24px" }}
    >
      {/* Episode label */}
      <div className="font-mono text-[10px] tracking-widest text-sd-blue font-bold mb-1">
        EPISODE
      </div>

      {/* Episode number */}
      <div className="font-oswald font-bold text-[88px] text-sd-red leading-[0.92] mb-2">
        {episodeNumber ?? "—"}
      </div>

      {/* Artwork */}
      <EpisodeImage title={draft.title} />

      {/* Title */}
      <div className="font-oswald font-semibold text-[22px] text-sd-ink leading-[1.15] tracking-tight mb-3">
        {draft.title}
      </div>

      {/* Badge row */}
      <div className="flex items-center gap-2 mb-2 font-mono text-[11px] text-[#5a6075]">
        {draftTypeDisplay && <DraftTypeBadge type={draftTypeDisplay} />}
        {parts.length > 1 && <span>{parts.length} PARTS</span>}
      </div>

      {/* Release date (single-part only) */}
      {releaseDate && (
        <div className="font-mono font-semibold text-[13px] text-sd-blue mb-4">
          {formatDate(releaseDate)}
        </div>
      )}

      {/* Campaign */}
      {draft.campaignName && (
        <div className="border-t border-sd-ink/10 pt-4 mt-4">
          <SectionLabel>CAMPAIGN</SectionLabel>
          <div className="font-oswald font-semibold text-[16px] text-sd-ink">
            {draft.campaignName}
          </div>
        </div>
      )}

      {/* ── Single-part: flat sections ─────────────────────────────────────── */}
      {!isMultiPart && firstPart && (
        <>
          {singlePartDrafters.length > 0 && (
            <div className="border-t border-sd-ink/10 pt-4 mt-4">
              <SectionLabel>DRAFTERS</SectionLabel>
              <PersonList entries={singlePartDrafters} />
            </div>
          )}

          {singlePartHosts.length > 0 && (
            <div className="border-t border-sd-ink/10 pt-4 mt-4">
              <SectionLabel>HOSTS</SectionLabel>
              <PersonList entries={singlePartHosts} />
            </div>
          )}

          {singlePartTrivia.length > 0 && (
            <div className="border-t border-sd-ink/10 pt-4 mt-4">
              <SectionLabel>TRIVIA</SectionLabel>
              <TriviaBlock results={singlePartTrivia} />
            </div>
          )}

          {partNavEntries(firstPart).length > 0 && (
            <div className="border-t border-sd-ink/10 pt-4 mt-4">
              <NavBlock entries={partNavEntries(firstPart)} />
            </div>
          )}

          {partCampaignNavEntries(firstPart).length > 0 && (
            <div className="border-t border-sd-ink/10 pt-4 mt-4">
              <SectionLabel>IN THIS CAMPAIGN</SectionLabel>
              <div className="flex flex-col gap-2">
                {partCampaignNavEntries(firstPart).map(({ publicId, title, direction }) => (
                  <Link
                    key={publicId}
                    href={`/drafts/${publicId}`}
                    className={`border border-sd-blue text-sd-blue font-oswald text-[12px] tracking-wide px-3 py-2 hover:bg-sd-blue hover:text-white transition-colors ${direction === "next" ? "text-right" : "text-left"}`}
                  >
                    {direction === "prev" ? `‹ ${title ?? "Previous"}` : `${title ?? "Next"} ›`}
                  </Link>
                ))}
              </div>
            </div>
          )}
        </>
      )}

      {/* ── Multi-part: per-part sections ─────────────────────────────────── */}
      {isMultiPart && parts.map((part, i) => (
        <PartSidebarSection
          key={part.publicId ?? i}
          part={part}
          partLabel={`Part ${part.partIndex ?? i + 1}`}
          participantNames={participantNames}
          triviaByPart={triviaByPart}
          colorIndexOffset={partColorOffsets[i] ?? 0}
        />
      ))}

      {/* Listen button */}
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
  );
}