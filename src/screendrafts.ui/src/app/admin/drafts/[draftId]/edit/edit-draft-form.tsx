"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import {
  AdminSeriesOption,
  AdminHostOption,
  AdminDraftDetail,
  DraftPart,
  addHostToDraftPart,
  removeHostFromDraftPart,
  addParticipantToDraftPart,
  removeParticipantFromDraftPart,
  setDraftCategories,
  setDraftCampaign,
  clearDraftCampaign,
  updateDraft,
  createDraftPart,
  removeDraftPartCommunityParticipant,
  setDraftPartCommunityLimits,
  setDraftPartCommunityParticipant,
  addCommunityFilmRule,
  assignFilmToCommunityFilmRule,
  removeCommunityFilmRule,
  updateCommunityFilmRule,
  setDraftPositions,
  listDraftPositions,
} from "@/services/admin/fetch-admin-drafts";
import { CampaignResponse, CategoryResponse, SmartEnumResponse } from "@/lib/dto";
import { formatDraftType } from "@/lib/draft-type-display";
import { ParticipantsSection, InitialParticipant } from "../../new/participants-section";
import DraftImageUpload from "@/components/features/drafts/draft-image-upload";
import { env } from "@/lib/env";
import { CommunityConfig, CommunityFilmRuleState, CommunitySection } from "../../new/community-section";
import {
  PositionConfig,
  PositionsEditor,
  getDefaultPositions,
  isFixedPositionType,
} from "../../new/positions-editor";

const LABEL = "block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1";
const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";
const SELECT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";
const BTN_PRIMARY =
  "bg-sd-red text-white font-oswald font-medium tracking-wide uppercase px-5 py-2.5 hover:bg-sd-red/90 disabled:opacity-50 transition-colors";
const BTN_SECONDARY =
  "border border-sd-ink/20 text-sd-ink font-sans text-sm px-4 py-2 hover:bg-sd-ink/5 disabled:opacity-50 transition-colors rounded";
const SECTION_HEADING =
  "font-oswald font-bold text-[18px] tracking-wide uppercase text-sd-ink mb-4 pb-2 border-b border-sd-ink/10";

function getMaxPositionsConfig(draftTypeName: string): { max: number; locked: boolean } {
  switch (draftTypeName) {
    case "Standard": return { max: 7, locked: true };
    case "SpeedDraft": return { max: 7, locked: true };
    case "MiniSuper": return { max: 5, locked: true };
    default: return { max: 7, locked: false };
  }
}

interface SelectedHost {
  publicId: string;
  displayName: string;
  role: "Primary" | "CoHost";
  persisted: boolean;
}

interface PartEditState {
  partPublicId: string;
  partIndex: number;
  status: number;
  hosts: SelectedHost[];
  existingDrafterIds: Set<string>;
  existingTeamIds: Set<string>;
  pendingDrafterIds: Set<string>;
  pendingTeamIds: Set<string>;
  removingDrafterIds: Set<string>;
  removingTeamIds: Set<string>;
  hostSearch: string;
  initialParticipants: InitialParticipant[];
  communityConfig: CommunityConfig;
  communityWasEnabled: boolean;
  positions: PositionConfig[];
  positionsLoaded: boolean;
}

interface PendingPart {
  tempId: string;
  minPositions: number;
  maxPositions: number;
  maxLocked: boolean;
  positions: PositionConfig[];
}

interface Props {
  draft: AdminDraftDetail;
  seriesList: AdminSeriesOption[];
  hostList: AdminHostOption[];
  categoryList: CategoryResponse[];
  campaignList: CampaignResponse[];
  accessToken: string;
}

function initPartState(parts: DraftPart[]): PartEditState[] {
  return parts.map((p) => {
    const hosts: SelectedHost[] = [];
    if (p.primaryHost) {
      hosts.push({
        publicId: p.primaryHost.hostPublicId,
        displayName: p.primaryHost.displayName,
        role: "Primary",
        persisted: true,
      });
    }
    for (const ch of p.coHosts) {
      hosts.push({
        publicId: ch.hostPublicId,
        displayName: ch.displayName,
        role: "CoHost",
        persisted: true,
      });
    }

    const existingDrafterIds = new Set<string>(
      p.participants
        .filter((pt) => pt.participantKindValue.value === 0 && pt.participantPublicId)
        .map((pt) => pt.participantPublicId!)
    );
    const existingTeamIds = new Set<string>(
      p.participants
        .filter((pt) => pt.participantKindValue.value === 1 && pt.participantPublicId)
        .map((pt) => pt.participantPublicId!)
    );

    const initialParticipants: InitialParticipant[] = p.participants
      .filter((pt) => pt.participantPublicId && pt.displayName)
      .map((pt) => ({
        publicId: pt.participantPublicId!,
        displayName: pt.displayName!,
        kind: pt.participantKindValue.value === 0 ? "drafter" : "team",
      }));

    const hasCommunity = p.participants.some((pt) => pt.participantKindValue.value === 2);

    const filmRules: CommunityFilmRuleState[] = (p.communityFilmRules ?? []).map((r) => ({
      localId: crypto.randomUUID(),
      publicId: r.publicId,
      ruleKind: r.ruleKind.name === "BoostersPick" ? "BoostersPick" : "BoostersVeto",
      targetSlot: r.targetSlot,
      tmdbId: r.tmdbId,
      title: r.title,
      status: "persisted" as const,
    }));

    const communityConfig: CommunityConfig = {
      enabled: hasCommunity,
      maxCommunityPicks: p.maxCommunityPicks ?? 0,
      maxCommunityVetoes: p.maxCommunityVetoes ?? 0,
      filmRules,
    };

    return {
      partPublicId: p.publicId,
      partIndex: p.partIndex,
      status: p.status.value ?? 0,
      hosts,
      existingDrafterIds,
      existingTeamIds,
      pendingDrafterIds: new Set(),
      pendingTeamIds: new Set(),
      removingDrafterIds: new Set(),
      removingTeamIds: new Set(),
      hostSearch: "",
      initialParticipants,
      communityConfig,
      communityWasEnabled: hasCommunity,
      positions: getDefaultPositions(p.draftType?.name ?? ""),
      positionsLoaded: false,
    };
  });
}

const PART_STATUS_LABELS: Record<number, string> = {
  0: "Created",
  2: "In Progress",
  3: "Completed",
  4: "Cancelled",
};

export default function EditDraftForm({
  draft,
  seriesList,
  hostList,
  categoryList,
  campaignList,
  accessToken,
}: Props) {
  const router = useRouter();

  const [saveCount, setSaveCount] = useState(0);
  const [title, setTitle] = useState(draft.title);
  const [description, setDescription] = useState(draft.description ?? "");
  const [selectedSeriesId, setSelectedSeriesId] = useState(draft.seriesPublicId ?? "");
  const [selectedDraftType, setSelectedDraftType] = useState<SmartEnumResponse | null>(() => {
    const series = seriesList.find((s) => s.publicId === draft.seriesPublicId);
    return (
      series?.allowedDraftTypes.find((t) => t.value === draft.draftType.value) ??
      series?.allowedDraftTypes[0] ??
      null
    );
  });

  const [partStates, setPartStates] = useState<PartEditState[]>(() => initPartState(draft.parts));
  const [pendingParts, setPendingParts] = useState<PendingPart[]>([]);
  const [expandedPart, setExpandedPart] = useState<number | null>(0);

  const [selectedCategoryIds, setSelectedCategoryIds] = useState<Set<string>>(
    () => new Set(draft.categories?.map((c) => c.publicId) ?? [])
  );
  const [categoriesChanged, setCategoriesChanged] = useState(false);
  const [campaignId, setCampaignId] = useState(draft.campaignPublicId ?? "");

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  const selectedSeries = seriesList.find((s) => s.publicId === selectedSeriesId);
  const availableDraftTypes = selectedSeries?.allowedDraftTypes ?? [];
  const anyPartStarted = draft.parts.some((p) => p.status.name !== "Created");

  // Load positions for each existing part on mount and after save
  useEffect(() => {
    const states = initPartState(draft.parts);
    setPartStates(states);

    // Fetch positions for each part
    states.forEach((part, idx) => {
      listDraftPositions(accessToken, part.partPublicId).then((loaded) => {
        const typeName = draft.parts[idx]?.draftType?.name ?? "";
        const fixed = isFixedPositionType(typeName);
        // For fixed types always use the canonical layout; for variable use what the API returns,
        // falling back to defaults if nothing is set yet.
        const resolved =
          fixed
            ? getDefaultPositions(typeName)
            : loaded.length > 0
              ? loaded
              : getDefaultPositions(typeName);

        setPartStates((prev) =>
          prev.map((p, i) =>
            i === idx ? { ...p, positions: resolved, positionsLoaded: true } : p
          )
        );
      });
    });
  }, [saveCount]); // eslint-disable-line react-hooks/exhaustive-deps

  function handleSeriesChange(seriesId: string) {
    setSelectedSeriesId(seriesId);
    const series = seriesList.find((s) => s.publicId === seriesId);
    setSelectedDraftType(series?.defaultDraftType ?? series?.allowedDraftTypes?.[0] ?? null);
  }

  function handleDraftTypeChange(typeName: string) {
    const typeObj = availableDraftTypes.find((t) => t.name === typeName) ?? null;
    setSelectedDraftType(typeObj);
    const { max, locked } = getMaxPositionsConfig(typeName);
    setPendingParts((prev) =>
      prev.map((p) => ({
        ...p,
        maxPositions: max,
        maxLocked: locked,
        positions: getDefaultPositions(typeName),
      }))
    );
  }

  function addPendingPart() {
    const typeName = selectedDraftType?.name ?? "";
    const { max, locked } = getMaxPositionsConfig(typeName);
    setPendingParts((prev) => [
      ...prev,
      {
        tempId: crypto.randomUUID(),
        minPositions: 1,
        maxPositions: max,
        maxLocked: locked,
        positions: getDefaultPositions(typeName),
      },
    ]);
  }

  function removePendingPart(tempId: string) {
    setPendingParts((prev) => prev.filter((p) => p.tempId !== tempId));
  }

  function updatePendingPartMax(tempId: string, value: number) {
    setPendingParts((prev) =>
      prev.map((p) => (p.tempId === tempId ? { ...p, maxPositions: Math.max(1, value) } : p))
    );
  }

  function updatePendingPartPositions(tempId: string, positions: PositionConfig[]) {
    setPendingParts((prev) =>
      prev.map((p) => (p.tempId === tempId ? { ...p, positions } : p))
    );
  }

  function updatePartPositions(partIdx: number, positions: PositionConfig[]) {
    setPartStates((prev) =>
      prev.map((p, i) => (i === partIdx ? { ...p, positions } : p))
    );
  }

  function addHostToPart(partIdx: number, host: AdminHostOption) {
    setPartStates((prev) =>
      prev.map((p, i) => {
        if (i !== partIdx || p.hosts.some((h) => h.publicId === host.publicId)) return p;
        return {
          ...p,
          hosts: [
            ...p.hosts,
            { publicId: host.publicId, displayName: host.displayName, role: "CoHost", persisted: false },
          ],
        };
      })
    );
  }

  function removeHostFromPart(partIdx: number, hostPublicId: string) {
    setPartStates((prev) =>
      prev.map((p, i) =>
        i !== partIdx ? p : { ...p, hosts: p.hosts.filter((h) => h.publicId !== hostPublicId) }
      )
    );
  }

  function setHostRoleOnPart(partIdx: number, hostPublicId: string, role: "Primary" | "CoHost") {
    setPartStates((prev) =>
      prev.map((p, i) =>
        i !== partIdx
          ? p
          : { ...p, hosts: p.hosts.map((h) => (h.publicId === hostPublicId ? { ...h, role } : h)) }
      )
    );
  }

  function toggleDrafterOnPart(partIdx: number, drafterId: string) {
    setPartStates((prev) =>
      prev.map((p, i) => {
        if (i !== partIdx) return p;
        if (p.existingDrafterIds.has(drafterId)) {
          const next = new Set(p.removingDrafterIds);
          if (next.has(drafterId)) next.delete(drafterId); else next.add(drafterId);
          return { ...p, removingDrafterIds: next };
        } else {
          const next = new Set(p.pendingDrafterIds);
          if (next.has(drafterId)) next.delete(drafterId); else next.add(drafterId);
          return { ...p, pendingDrafterIds: next };
        }
      })
    );
  }

  function toggleTeamOnPart(partIdx: number, teamId: string) {
    setPartStates((prev) =>
      prev.map((p, i) => {
        if (i !== partIdx) return p;
        if (p.existingTeamIds.has(teamId)) {
          const next = new Set(p.removingTeamIds);
          if (next.has(teamId)) next.delete(teamId); else next.add(teamId);
          return { ...p, removingTeamIds: next };
        } else {
          const next = new Set(p.pendingTeamIds);
          if (next.has(teamId)) next.delete(teamId); else next.add(teamId);
          return { ...p, pendingTeamIds: next };
        }
      })
    );
  }

  function getEffectiveDrafterIds(part: PartEditState): Set<string> {
    const s = new Set<string>();
    for (const id of part.existingDrafterIds) { if (!part.removingDrafterIds.has(id)) s.add(id); }
    for (const id of part.pendingDrafterIds) s.add(id);
    return s;
  }

  function getEffectiveTeamIds(part: PartEditState): Set<string> {
    const s = new Set<string>();
    for (const id of part.existingTeamIds) { if (!part.removingTeamIds.has(id)) s.add(id); }
    for (const id of part.pendingTeamIds) s.add(id);
    return s;
  }

  function toggleCategory(id: string) {
    setCategoriesChanged(true);
    setSelectedCategoryIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  }

  function updateCommunityConfigOnPart(partIdx: number, config: CommunityConfig) {
    setPartStates((prev) =>
      prev.map((p, i) => (i !== partIdx ? p : { ...p, communityConfig: config }))
    );
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (submitting) return;
    setError(null);
    setSuccessMsg(null);
    setSubmitting(true);

    try {
      // Step 1: Core metadata
      if (draft.draftStatus.name !== "Completed" && draft.draftStatus.name !== "Cancelled") {
        await updateDraft(accessToken, draft.publicId, {
          title: title.trim(),
          description: description.trim() || undefined,
          seriesPublicId: selectedSeriesId,
          draftTypeValue: selectedDraftType?.value ?? draft.draftType.value ?? 0,
        });
      }

      // Step 2: Campaign
      if (!campaignId && draft.campaignPublicId) {
        await clearDraftCampaign(accessToken, draft.publicId);
      } else if (campaignId && campaignId !== draft.campaignPublicId) {
        await setDraftCampaign(accessToken, draft.publicId, campaignId);
      }

      // Step 3: Categories
      if (categoriesChanged && selectedCategoryIds.size > 0) {
        await setDraftCategories(accessToken, draft.publicId, [...selectedCategoryIds]);
      }

      // Step 4: Per-part updates
      if (draft.draftStatus.name !== "Completed" && draft.draftStatus.name !== "Cancelled") {
        for (const part of partStates) {
          const { partPublicId } = part;
          const originalPart = draft.parts.find((p) => p.publicId === partPublicId);
          const originalHostIds = new Set<string>([
            ...(originalPart?.primaryHost ? [originalPart.primaryHost.hostPublicId] : []),
            ...(originalPart?.coHosts.map((h) => h.hostPublicId) ?? []),
          ]);
          const currentHostIds = new Set(part.hosts.map((h) => h.publicId));

          for (const id of originalHostIds) {
            if (!currentHostIds.has(id)) await removeHostFromDraftPart(accessToken, partPublicId, id);
          }
          for (const host of part.hosts) {
            if (!host.persisted) {
              await addHostToDraftPart(accessToken, partPublicId, {
                draftPartId: partPublicId,
                hostPublicId: host.publicId,
                hostRole: host.role === "Primary" ? 0 : 1,
              });
            }
          }

          for (const id of part.removingDrafterIds) {
            await removeParticipantFromDraftPart(accessToken, partPublicId, { participantPublicId: id, participantKind: 0 });
          }
          for (const id of part.removingTeamIds) {
            await removeParticipantFromDraftPart(accessToken, partPublicId, { participantPublicId: id, participantKind: 1 });
          }
          for (const id of part.pendingDrafterIds) {
            await addParticipantToDraftPart(accessToken, partPublicId, { draftPartId: partPublicId, participantPublicId: id, participantKind: 0 });
          }
          for (const id of part.pendingTeamIds) {
            await addParticipantToDraftPart(accessToken, partPublicId, { draftPartId: partPublicId, participantPublicId: id, participantKind: 1 });
          }

          // Positions — always resend (idempotent)
          if (part.positionsLoaded) {
            await setDraftPositions(accessToken, partPublicId, part.positions);
          }

          // Community participant
          if (part.communityConfig.enabled && !part.communityWasEnabled) {
            await setDraftPartCommunityParticipant(accessToken, partPublicId);
          } else if (!part.communityConfig.enabled && part.communityWasEnabled) {
            await removeDraftPartCommunityParticipant(accessToken, partPublicId);
          }

          if (part.communityConfig.enabled) {
            await setDraftPartCommunityLimits(accessToken, partPublicId, {
              maxCommunityPicks: part.communityConfig.maxCommunityPicks,
              maxCommunityVetoes: part.communityConfig.maxCommunityVetoes,
            });
          }

          for (const rule of part.communityConfig.filmRules) {
            if (rule.status === "removing" && rule.publicId) {
              await removeCommunityFilmRule(accessToken, partPublicId, rule.publicId);
            } else if (rule.status === "pending") {
              await addCommunityFilmRule(accessToken, partPublicId, {
                ruleKind: rule.ruleKind === "BoostersVeto" ? 0 : 1,
                targetSlot: rule.targetSlot,
              });
            } else if (rule.status === "persisted" && rule.publicId) {
              await updateCommunityFilmRule(accessToken, partPublicId, rule.publicId, {
                ruleKind: rule.ruleKind === "BoostersVeto" ? 0 : 1,
                targetSlot: rule.targetSlot,
              });
              if (rule.tmdbId) {
                await assignFilmToCommunityFilmRule(accessToken, partPublicId, rule.publicId, rule.tmdbId);
              }
            }
          }
        }

        // Step 5: Create pending parts + set their positions
        for (let i = 0; i < pendingParts.length; i++) {
          const pp = pendingParts[i];
          const newPartId = await createDraftPart(accessToken, draft.publicId, {
            publicId: draft.publicId,
            partIndex: partStates.length + i + 1,
            minimumPosition: pp.minPositions,
            maximumPosition: pp.maxPositions,
          });
          await setDraftPositions(accessToken, newPartId, pp.positions);
        }
      }

      setSuccessMsg("Draft saved.");
      router.refresh();
      setSaveCount((n) => n + 1);
    } catch (err) {
      setError(err instanceof Error ? err.message : "An unexpected error occurred.");
    } finally {
      setSubmitting(false);
    }
  }

  const isCompletedOrCancelled =
    draft.draftStatus.name === "Completed" || draft.draftStatus.name === "Cancelled";

  const draftTypeName = selectedDraftType?.name ?? draft.draftType?.name ?? "";
  const fixedPositions = isFixedPositionType(draftTypeName);

  return (
    <form onSubmit={handleSubmit} className="space-y-10">
      {isCompletedOrCancelled && (
        <div className="border border-sd-ink/20 bg-sd-ink/5 text-sd-ink/70 text-sm px-4 py-3 rounded font-mono">
          This draft is {draft.draftStatus.name?.toLowerCase()}. Only campaign and categories can be edited.
        </div>
      )}

      {/* ── Section 1: Core metadata ── */}
      {!isCompletedOrCancelled && (
        <section>
          <h2 className={SECTION_HEADING}>Core Details</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="md:col-span-2">
              <label className={LABEL}>Title <span className="text-sd-red">*</span></label>
              <input type="text" className={INPUT} value={title} onChange={(e) => setTitle(e.target.value)} required />
            </div>

            <div className="md:col-span-2">
              <label className={LABEL}>Description</label>
              <textarea
                className={`${INPUT} min-h-[80px] resize-y`}
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Optional description"
              />
            </div>

            <div className="md:col-span-2">
              <DraftImageUpload
                draftPublicId={draft.publicId}
                currentImagePath={draft.imagePath}
                accessToken={accessToken}
                apiBase={env.apiUrl!}
              />
            </div>

            <div>
              <label className={LABEL}>Series <span className="text-sd-red">*</span></label>
              <select
                className={SELECT}
                value={selectedSeriesId}
                onChange={(e) => handleSeriesChange(e.target.value)}
                disabled={anyPartStarted}
                required
              >
                <option value="">— Select series —</option>
                {seriesList.map((s) => (
                  <option key={s.publicId} value={s.publicId}>{s.name}</option>
                ))}
              </select>
              {anyPartStarted && (
                <p className="text-[11px] text-sd-ink/40 mt-1 font-mono">Locked — a part has already started.</p>
              )}
            </div>

            <div>
              <label className={LABEL}>Draft Type <span className="text-sd-red">*</span></label>
              <select
                className={SELECT}
                value={selectedDraftType?.name ?? ""}
                onChange={(e) => handleDraftTypeChange(e.target.value)}
                disabled={availableDraftTypes.length === 0 || anyPartStarted}
                required
              >
                <option value="">— Select draft type —</option>
                {availableDraftTypes.map((t) => (
                  <option key={t.name} value={t.name ?? ""}>{formatDraftType(t.name)}</option>
                ))}
              </select>
              {anyPartStarted && (
                <p className="text-[11px] text-sd-ink/40 mt-1 font-mono">Locked — a part has already started.</p>
              )}
            </div>
          </div>
        </section>
      )}

      {/* ── Section 2: Parts ── */}
      {!isCompletedOrCancelled && (
        <section>
          <h2 className={SECTION_HEADING}>Parts</h2>
          <div className="space-y-3">
            {partStates.map((part, idx) => {
              const effectiveDrafterIds = getEffectiveDrafterIds(part);
              const effectiveTeamIds = getEffectiveTeamIds(part);
              const isExpanded = expandedPart === idx;
              const filteredHosts = hostList.filter(
                (h) =>
                  !part.hosts.some((sel) => sel.publicId === h.publicId) &&
                  h.displayName.toLowerCase().includes(part.hostSearch.toLowerCase())
              );
              const totalParts = partStates.length + pendingParts.length;
              // Max positions: infer from existing positions or fall back to standard
              const maxPos = part.positions.flatMap((p) => p.picks).length ||
                (fixedPositions ? getMaxPositionsConfig(draftTypeName).max : 7);

              return (
                <div key={part.partPublicId} className="border border-sd-ink/10 rounded">
                  <button
                    type="button"
                    className="w-full flex items-center justify-between px-4 py-3 text-left bg-sd-ink/5 hover:bg-sd-ink/10 transition-colors"
                    onClick={() => setExpandedPart(isExpanded ? null : idx)}
                  >
                    <span className="font-oswald font-medium text-sd-ink tracking-wide">
                      {totalParts > 1 ? `Part ${part.partIndex}` : "Part"}
                      <span className="ml-3 text-[11px] font-mono text-sd-ink/40 font-normal">
                        {PART_STATUS_LABELS[part.status] ?? "Unknown"}
                      </span>
                    </span>
                    <span className="text-sd-ink/50 text-sm">{isExpanded ? "▼" : "▶"}</span>
                  </button>

                  {isExpanded && (
                    <div className="px-4 py-4 space-y-6">
                      {/* Hosts */}
                      <div>
                        <h2 className={SECTION_HEADING}>Hosts</h2>
                        {part.hosts.length > 0 && (
                          <div className="mb-3 space-y-2">
                            {part.hosts.map((h) => (
                              <div key={h.publicId} className="flex items-center justify-between gap-3 px-3 py-2 bg-white border border-sd-ink/10 rounded">
                                <span className="text-sm text-sd-ink font-medium">
                                  {h.displayName}
                                  {h.persisted && <span className="ml-2 text-[10px] font-mono text-sd-ink/30">saved</span>}
                                </span>
                                <div className="flex items-center gap-3">
                                  <label className="flex items-center gap-1.5 text-sm text-sd-ink/70 cursor-pointer">
                                    <input type="radio" name={`host-role-${part.partPublicId}-${h.publicId}`} checked={h.role === "Primary"} onChange={() => setHostRoleOnPart(idx, h.publicId, "Primary")} className="accent-sd-red" />
                                    Primary
                                  </label>
                                  <label className="flex items-center gap-1.5 text-sm text-sd-ink/70 cursor-pointer">
                                    <input type="radio" name={`host-role-${part.partPublicId}-${h.publicId}`} checked={h.role === "CoHost"} onChange={() => setHostRoleOnPart(idx, h.publicId, "CoHost")} className="accent-sd-red" />
                                    Co-Host
                                  </label>
                                  <button type="button" onClick={() => removeHostFromPart(idx, h.publicId)} className="text-sd-ink/40 hover:text-sd-red text-lg leading-none" aria-label={`Remove ${h.displayName}`}>×</button>
                                </div>
                              </div>
                            ))}
                          </div>
                        )}
                        <div className="border border-sd-ink/10 rounded p-3 bg-white">
                          <input
                            type="text"
                            placeholder="Search hosts…"
                            className={`${INPUT} mb-2`}
                            value={part.hostSearch}
                            onChange={(e) =>
                              setPartStates((prev) =>
                                prev.map((p, i) => i === idx ? { ...p, hostSearch: e.target.value } : p)
                              )
                            }
                          />
                          <div className="max-h-32 overflow-y-auto space-y-0.5">
                            {filteredHosts.length === 0 ? (
                              <p className="text-sm text-sd-ink/40 font-mono px-2">No hosts found.</p>
                            ) : (
                              filteredHosts.map((h) => (
                                <button key={h.publicId} type="button" onClick={() => addHostToPart(idx, h)} className="w-full text-left px-3 py-1.5 text-sm text-sd-ink hover:bg-sd-ink/5 rounded transition-colors">
                                  {h.displayName}
                                </button>
                              ))
                            )}
                          </div>
                        </div>
                      </div>

                      {/* Participants */}
                      <ParticipantsSection
                        key={`${part.partPublicId}-${saveCount}`}
                        accessToken={accessToken}
                        selectedDrafterIds={effectiveDrafterIds}
                        selectedTeamIds={effectiveTeamIds}
                        onToggleDrafter={(id) => toggleDrafterOnPart(idx, id)}
                        onToggleTeam={(id) => toggleTeamOnPart(idx, id)}
                        initialParticipants={part.initialParticipants}
                      />

                      {/* Positions */}
                      <div>
                        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 uppercase mb-3">
                          Positions
                        </p>
                        {part.positionsLoaded ? (
                          <PositionsEditor
                            positions={part.positions}
                            onChange={(pos) => updatePartPositions(idx, pos)}
                            totalPicks={maxPos}
                            readonly={fixedPositions}
                          />
                        ) : (
                          <p className="text-[11px] font-mono text-sd-ink/40">Loading…</p>
                        )}
                      </div>

                      {/* Community */}
                      <div>
                        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 uppercase mb-3">
                          Community
                        </p>
                        <CommunitySection
                          config={part.communityConfig}
                          onChange={(next) => updateCommunityConfigOnPart(idx, next)}
                        />
                      </div>
                    </div>
                  )}
                </div>
              );
            })}

            {/* Pending parts */}
            {!anyPartStarted && pendingParts.map((pp, idx) => {
              const partNumber = partStates.length + idx + 1;
              const isPendingExpanded = expandedPart === partStates.length + idx;
              const ppFixed = isFixedPositionType(selectedDraftType?.name ?? "");
              return (
                <div key={pp.tempId} className="border border-sd-blue/30 rounded">
                  <button
                    type="button"
                    className="w-full flex items-center justify-between px-4 py-3 text-left bg-sd-blue/5 hover:bg-sd-blue/10 transition-colors"
                    onClick={() => setExpandedPart(isPendingExpanded ? null : partStates.length + idx)}
                  >
                    <span className="font-oswald font-medium text-sd-ink tracking-wide">
                      Part {partNumber}
                      <span className="ml-3 text-[11px] font-mono text-sd-blue/60 font-normal">unsaved</span>
                    </span>
                    <div className="flex items-center gap-3">
                      <button type="button" onClick={(e) => { e.stopPropagation(); removePendingPart(pp.tempId); }} className="text-sd-ink/40 hover:text-sd-red text-lg leading-none" aria-label="Remove part">×</button>
                      <span className="text-sd-ink/50 text-sm">{isPendingExpanded ? "▼" : "▶"}</span>
                    </div>
                  </button>
                  {isPendingExpanded && (
                    <div className="px-4 py-4 space-y-4">
                      {pp.maxLocked ? (
                        <p className="text-[11px] font-mono text-sd-ink/50">
                          Positions 1–{pp.maxPositions} (fixed by draft type)
                        </p>
                      ) : (
                        <div className="max-w-[160px]">
                          <label className={LABEL}>Max Positions</label>
                          <input
                            type="number"
                            min={1}
                            className={INPUT}
                            value={pp.maxPositions}
                            onChange={(e) => updatePendingPartMax(pp.tempId, parseInt(e.target.value, 10) || 1)}
                          />
                        </div>
                      )}
                      <div>
                        <p className={LABEL}>Positions</p>
                        <PositionsEditor
                          positions={pp.positions}
                          onChange={(pos) => updatePendingPartPositions(pp.tempId, pos)}
                          totalPicks={pp.maxPositions}
                          readonly={ppFixed}
                        />
                      </div>
                    </div>
                  )}
                </div>
              );
            })}
          </div>

          {!anyPartStarted && (
            <button type="button" className={`${BTN_SECONDARY} mt-4`} onClick={addPendingPart}>
              + Add Part
            </button>
          )}
        </section>
      )}

      {/* ── Section 3: Categories ── */}
      <section>
        <h2 className={SECTION_HEADING}>Category (Optional)</h2>
        <div className="border border-sd-ink/10 rounded p-4 bg-white">
          <div className="max-h-40 overflow-y-auto columns-2 gap-4">
            {categoryList.map((c) => (
              <label key={c.publicId} className="flex items-center gap-2 mb-1 text-sm text-sd-ink hover:bg-sd-ink/5 rounded px-2 py-1 cursor-pointer break-inside-avoid">
                <input type="checkbox" checked={selectedCategoryIds.has(c.publicId!)} onChange={() => toggleCategory(c.publicId!)} className="accent-sd-red" />
                {c.name}
              </label>
            ))}
          </div>
          {selectedCategoryIds.size > 0 && (
            <p className="mt-2 text-[11px] font-mono text-sd-ink/50">
              {selectedCategoryIds.size} categor{selectedCategoryIds.size !== 1 ? "ies" : "y"} selected
            </p>
          )}
        </div>
      </section>

      {/* ── Section 4: Campaign ── */}
      <section>
        <h2 className={SECTION_HEADING}>Campaign (Optional)</h2>
        <div className="max-w-sm">
          <select className={SELECT} value={campaignId} onChange={(e) => setCampaignId(e.target.value)}>
            <option value="">— No campaign —</option>
            {campaignList.map((c) => (
              <option key={c.publicId} value={c.publicId ?? ""}>{c.name}</option>
            ))}
          </select>
        </div>
      </section>

      {error && (
        <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">{error}</div>
      )}
      {successMsg && (
        <div className="border border-green-300 bg-green-50 text-green-800 text-sm px-4 py-3 rounded">{successMsg}</div>
      )}

      <div className="flex items-center gap-4 pt-2">
        <button type="submit" disabled={submitting} className={BTN_PRIMARY}>
          {submitting ? "Saving…" : "Save Changes"}
        </button>
        <a href="/admin/drafts" className={BTN_SECONDARY}>Cancel</a>
      </div>
    </form>
  );
}