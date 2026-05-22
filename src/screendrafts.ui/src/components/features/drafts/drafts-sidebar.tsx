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

interface SidebarLabelProps {
  children: React.ReactNode;
}

function SectionLabel({ children }: SidebarLabelProps) {
  return (
    <div className="font-mono text-[10px] tracking-widest text-sd-red font-bold mb-3">
      ★ {children}
    </div>
  );
}

interface DraftSidebarProps {
  draft: GetDraftResponse;
  participantNames: Map<string, string>;
  triviaByPart: Map<string, TriviaResultResponse[]>;
}

export default function DraftSidebar({ draft, participantNames, triviaByPart }: DraftSidebarProps) {
  // Flatten participants and hosts across all parts
  const parts: GetDraftPartResponse[] = draft.parts ?? [];
  const firstPart = parts[0];

  // Episode number — try from release's episodeNumber via index sig
  const episodeNumber: number | undefined =
    (draft as Record<string, unknown>).episodeNumber as number | undefined ??
    (firstPart?.releases?.[0] as Record<string, unknown> | undefined)?.episodeNumber as number | undefined;

  // Release date
  const releaseDate = firstPart?.releases?.[0]?.releaseDate;

  // Collect unique participants across parts (by participantIdValue)
  const seenParticipantIds = new Set<string>();
  const participantEntries: { id: string; name: string; index: number; personPublicId?: string; }[] = [];
  let idx = 0;
  for (const part of parts) {
    for (const p of part.participants ?? []) {
      const pid = p.participantIdValue ?? "";
      if (p.participantKindValue?.value === 2) continue;
      if (pid && !seenParticipantIds.has(pid)) {
        seenParticipantIds.add(pid);
        const name =
          participantNames.get(pid) ?? p.displayName ?? "Unknown";
        const personPublicId = p.personPublicId;
        participantEntries.push({ id: pid, name, index: idx++, personPublicId });
      }
    }
  }

  // Collect hosts (primary + co-hosts) across parts
  const seenHostIds = new Set<string>();
  const hostEntries: { id: string; name: string; role: string; index: number, personPublicId?: string; }[] = [];
  let hostIdx = 0;
  for (const part of parts) {
    if (part.primaryHost?.hostPublicId && !seenHostIds.has(part.primaryHost.hostPublicId)) {
      seenHostIds.add(part.primaryHost.hostPublicId);
      hostEntries.push({
        id: part.primaryHost.hostPublicId,
        name: part.primaryHost.displayName ?? "Unknown",
        role: "PRIMARY HOST",
        index: hostIdx++,
        personPublicId: part.primaryHost.personPublicId,
      });
    }
    for (const coHost of part.coHosts ?? []) {
      if (coHost.hostPublicId && !seenHostIds.has(coHost.hostPublicId)) {
        seenHostIds.add(coHost.hostPublicId);
        hostEntries.push({
          id: coHost.hostPublicId,
          name: coHost.displayName ?? "Unknown",
          role: "CO-HOST",
          index: hostIdx++,
          personPublicId: coHost.personPublicId,
        });
      }
    }
  }

  // Trivia results — via index sig
  const allTriviaResults: TriviaResultResponse[] = parts.flatMap(
    (part) => triviaByPart.get(part.publicId ?? "") ?? []
  );
  const hasTriviaResults = allTriviaResults.length > 0;

  // Draft type
  const draftTypeDisplay =
    draft.draftType?.name
      ? draftTypeFromNumber(draft.draftType.value)
      : draftTypeFromNumber((draft as Record<string, unknown>).draftTypeValue as number | undefined);

  const listenUrl = process.env.NEXT_PUBLIC_SPOTIFY_URL ?? "#";

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

      {/* Episode Artwork  */}
      <EpisodeImage title={draft.title} />

      {/* Title */}
      <div className="font-oswald font-semibold text-[22px] text-sd-ink leading-[1.15] tracking-tight mb-3">
        {draft.title}
      </div>

      {/* Badge row */}
      <div className="flex items-center gap-2 mb-2 font-mono text-[11px] text-[#5a6075]">
        {draftTypeDisplay && <DraftTypeBadge type={draftTypeDisplay} />}
        {parts.length > 1 && (
          <span>{parts.length} PARTS</span>
        )}
      </div>

      {/* Air date */}
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

      {/* Drafters */}
      {participantEntries.length > 0 && (
        <div className="border-t border-sd-ink/10 pt-4 mt-4">
          <SectionLabel>DRAFTERS</SectionLabel>
          <div className="flex flex-col gap-3">
            {participantEntries.map(({ id, name, index, personPublicId }) => {
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
                      <Link
                        href={`/drafters/${personPublicId}`}
                        className="hover:text-sd-blue transition-colors"
                      >
                        {nameEl}
                      </Link>
                    ) : nameEl}
                    <div className="font-mono text-[10px] text-[#5a6075]">DRAFTER</div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* Hosts */}
      {hostEntries.length > 0 && (
        <div className="border-t border-sd-ink/10 pt-4 mt-4">
          <SectionLabel>HOSTS</SectionLabel>
          <div className="flex flex-col gap-3">
            {hostEntries.map(({ id, name, role, index, personPublicId }) => {
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
                      <Link
                        href={`/drafters/${personPublicId}`}
                        className="hover:text-sd-blue transition-colors"
                      >
                        {nameEl}
                      </Link>
                    ) : nameEl}
                    <div className="font-mono text-[10px] text-[#5a6075]">{role}</div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* Trivia */}
      {hasTriviaResults && (
        <div className="border-t border-sd-ink/10 pt-4 mt-4">
          <SectionLabel>TRIVIA</SectionLabel>
          <div className="flex flex-col gap-2">
            {allTriviaResults.map((t) => (
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
        </div>
      )}

      {/* Episode navigation */}
      {(draft.previousDraftPublicId || draft.nextDraftPublicId) && (
        <div className="border-t border-sd-ink/10 pt-4 mt-4">
          <div className="flex flex-col gap-2">
            {draft.previousDraftPublicId ? (
              <Link
                href={`/drafts/${draft.previousDraftPublicId}`}
                className="flex-1 border border-sd-ink text-sd-ink font-oswald text-[12px] tracking-wide px-3 py-2 hover:bg-sd-ink hover:text-white transition-colors text-left truncate"
                title={draft.previousDraftTitle ?? "Previous"}
              >
                ‹ {draft.previousDraftTitle ?? "Previous"}
              </Link>
            ) : null}
            {draft.nextDraftPublicId ? (
              <Link
                href={`/drafts/${draft.nextDraftPublicId}`}
                className="flex-1 border border-sd-ink text-sd-ink font-oswald text-[12px] tracking-wide px-3 py-2 hover:bg-sd-ink hover:text-white transition-colors text-right truncate"
                title={draft.nextDraftTitle ?? "Next"}
              >
                {draft.nextDraftTitle ?? "Next"} ›
              </Link>
            ) : null}
          </div>
        </div>
      )}

      {/* Campaign navigation */}
      {(draft.previousCampaignDraftPublicId || draft.nextCampaignDraftPublicId) && (
        <div className="border-t border-sd-ink/10 pt-4 mt-4">
          <SectionLabel>IN THIS CAMPAIGN</SectionLabel>
          <div className="flex flex-col gap-2">
            {draft.previousCampaignDraftPublicId && (
              <Link
                href={`/drafts/${draft.previousCampaignDraftPublicId}`}
                className="border border-sd-blue text-sd-blue font-oswald text-[12px] tracking-wide px-3 py-2 hover:bg-sd-blue hover:text-white transition-colors text-left"
              >
                ‹ {draft.previousCampaignDraftTitle ?? "Previous"}
              </Link>
            )}
            {draft.nextCampaignDraftPublicId && (
              <Link
                href={`/drafts/${draft.nextCampaignDraftPublicId}`}
                className="border border-sd-blue text-sd-blue font-oswald text-[12px] tracking-wide px-3 py-2 hover:bg-sd-blue hover:text-white transition-colors text-right"
              >
                {draft.nextCampaignDraftTitle ?? "Next"} ›
              </Link>
            )}
          </div>
        </div>
      )}

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