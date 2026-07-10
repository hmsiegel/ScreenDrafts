"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import {
  AdminSeriesOption,
  AdminHostOption,
  createDraft,
  syncPredictionConfig,
  getDraft,
} from "@/services/admin/fetch-admin-drafts";
import { CampaignResponse, CategoryResponse, SmartEnumResponse } from "@/lib/dto";
import { formatDraftType } from "@/lib/draft-type-display";
import { ParticipantsSection } from "./participants-section";
import { CommunityConfig, CommunitySection, defaultCommunityConfig } from "./community-section";
import { getDefaultPositions, isFixedPositionType, PositionConfig, PositionsEditor } from "./positions-editor";
import { defaultPredictionConfig, PredictionConfig, PredictionRulesSection } from "./prediction-rules-section";

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

// ── Series kind groupings ─────────────────────────────────────────────────────
// Main Feed: LegendsSuper (9), Regular (0), LiveDraft (6)
// Patreon:   Regular (0), LiveDraft (6), FranchiseMiniSuper (1),
//            InMemoriam (2), LegendsMega (3), BestPictureNominee (4),
//            PatreonVs (5), Special (7), SpeedDrafts (8)

const MAIN_FEED_KINDS = new Set([0, 6, 9]);
const PATREON_KINDS = new Set([0, 1, 2, 3, 4, 5, 6, 7, 8]);

type FeedType = "main" | "patreon" | "";

function getMaxPositionsConfig(draftTypeName: string): { max: number; locked: boolean } {
  switch (draftTypeName) {
    case "Standard": return { max: 7, locked: true };
    case "SpeedDraft": return { max: 7, locked: true };
    case "MiniSuper": return { max: 5, locked: true };
    default: return { max: 7, locked: false };
  }
}

interface PartConfig {
  partIndex: number;
  minPositions: number;
  maxPositions: number;
  maxLocked: boolean;
  collapsed: boolean;
  communityConfig: CommunityConfig;
  predictionConfig: PredictionConfig;
  positions: PositionConfig[];
}

interface SelectedHost {
  publicId: string;
  displayName: string;
  role: "Primary" | "CoHost";
}

interface Props {
  seriesList: AdminSeriesOption[];
  hostList: AdminHostOption[];
  categoryList: CategoryResponse[];
  campaignList: CampaignResponse[];
  accessToken: string;
}

export default function CreateDraftForm({
  seriesList,
  hostList,
  categoryList,
  campaignList,
  accessToken,
}: Props) {
  const router = useRouter();

  const [title, setTitle] = useState("");
  const [selectedFeed, setSelectedFeed] = useState<FeedType>("");
  const [selectedSeriesId, setSelectedSeriesId] = useState("");
  const [selectedDraftType, setSelectedDraftType] = useState<SmartEnumResponse | null>(null);

  const [numParts, setNumParts] = useState(1);
  const [parts, setParts] = useState<PartConfig[]>([
    {
      partIndex: 1,
      minPositions: 1,
      maxPositions: 7,
      maxLocked: true,
      collapsed: false,
      communityConfig: defaultCommunityConfig(),
      predictionConfig: defaultPredictionConfig(),
      positions: getDefaultPositions("Standard"),
    },
  ]);

  const [hosts, setHosts] = useState<SelectedHost[]>([]);
  const [hostSearch, setHostSearch] = useState("");
  const [selectedDrafterIds, setSelectedDrafterIds] = useState<Set<string>>(new Set());
  const [selectedTeamIds, setSelectedTeamIds] = useState<Set<string>>(new Set());
  const [selectedCategoryIds, setSelectedCategoryIds] = useState<Set<string>>(new Set());
  const [campaignId, setCampaignId] = useState("");

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Filter series by selected feed
  const filteredSeriesList = selectedFeed === ""
    ? []
    : seriesList.filter((s) =>
      selectedFeed === "main"
        ? MAIN_FEED_KINDS.has(s.kindValue)
        : PATREON_KINDS.has(s.kindValue)
    );

  const selectedSeries = seriesList.find((s) => s.publicId === selectedSeriesId);
  const availableDraftTypes = selectedSeries?.allowedDraftTypes ?? [];

  function handleFeedChange(feed: FeedType) {
    setSelectedFeed(feed);
    setSelectedSeriesId("");
    setSelectedDraftType(null);
  }

  function handleSeriesChange(seriesId: string) {
    setSelectedSeriesId(seriesId);
    const series = seriesList.find((s) => s.publicId === seriesId);
    const defaultType = series?.defaultDraftType ?? series?.allowedDraftTypes?.[0] ?? null;
    setSelectedDraftType(defaultType ?? null);
    if (defaultType?.name) {
      syncPartsToType(defaultType.name, numParts);
    }
  }

  function handleDraftTypeChange(typeName: string) {
    const typeObj = availableDraftTypes.find((t) => t.name === typeName) ?? null;
    setSelectedDraftType(typeObj);
    syncPartsToType(typeName, numParts);
  }

  function syncPartsToType(typeName: string, count: number) {
    const { max, locked } = getMaxPositionsConfig(typeName);
    setParts((prev) =>
      Array.from({ length: count }, (_, i) => ({
        partIndex: i + 1,
        minPositions: 1,
        maxPositions: max,
        maxLocked: locked,
        collapsed: i > 0,
        communityConfig: prev[i]?.communityConfig ?? defaultCommunityConfig(),
        predictionConfig: prev[i]?.predictionConfig ?? defaultPredictionConfig(),
        positions: getDefaultPositions(typeName),
      }))
    );
  }

  function handleNumPartsChange(n: number) {
    const count = Math.max(1, n);
    setNumParts(count);
    const typeName = selectedDraftType?.name ?? "";
    const { max, locked } = getMaxPositionsConfig(typeName);
    setParts((prev) =>
      Array.from({ length: count }, (_, i) => ({
        partIndex: i + 1,
        minPositions: 1,
        maxPositions: parts[i]?.maxPositions ?? max,
        maxLocked: locked,
        collapsed: i > 0,
        communityConfig: prev[i]?.communityConfig ?? defaultCommunityConfig(),
        predictionConfig: prev[i]?.predictionConfig ?? defaultPredictionConfig(),
        positions: prev[i]?.positions ?? getDefaultPositions(typeName),
      }))
    );
  }

  function updatePartMax(idx: number, value: number) {
    setParts((prev) =>
      prev.map((p, i) => (i === idx ? { ...p, maxPositions: Math.max(1, value) } : p))
    );
  }

  function updatePartPositions(idx: number, positions: PositionConfig[]) {
    setParts((prev) =>
      prev.map((p, i) => (i === idx ? { ...p, positions } : p))
    );
  }

  function updatePartCommunityConfig(idx: number, config: CommunityConfig) {
    setParts((prev) =>
      prev.map((p, i) => (i === idx ? { ...p, communityConfig: config } : p))
    );
  }

  function updatePartPredictionConfig(idx: number, config: PredictionConfig) {
    setParts((prev) =>
      prev.map((p, i) => (i === idx ? { ...p, predictionConfig: config } : p))
    );
  }

  function togglePartCollapsed(idx: number) {
    setParts((prev) =>
      prev.map((p, i) => (i === idx ? { ...p, collapsed: !p.collapsed } : p))
    );
  }

  function addHost(host: AdminHostOption) {
    if (hosts.some((h) => h.publicId === host.publicId)) return;
    setHosts((prev) => [
      ...prev,
      { publicId: host.publicId, displayName: host.displayName, role: "CoHost" },
    ]);
  }

  function removeHost(publicId: string) {
    setHosts((prev) => prev.filter((h) => h.publicId !== publicId));
  }

  function setHostRole(publicId: string, role: "Primary" | "CoHost") {
    setHosts((prev) =>
      prev.map((h) => {
        if (h.publicId === publicId) return { ...h, role };
        if (role === "Primary" && h.role === "Primary") return { ...h, role: "CoHost" };
        return h;
      })
    );
  }

  function toggleDrafter(id: string) {
    setSelectedDrafterIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  }

  function toggleTeam(id: string) {
    setSelectedTeamIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  }

  function toggleCategory(id: string) {
    setSelectedCategoryIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  }

  const filteredHosts = hostList.filter(
    (h) =>
      !hosts.some((sel) => sel.publicId === h.publicId) &&
      h.displayName.toLowerCase().includes(hostSearch.toLowerCase())
  );

  const hasPrimaryHost = hosts.length === 0 || hosts.some((h) => h.role === "Primary");
  const canSubmit =
    title.trim() !== "" && selectedSeriesId !== "" && selectedDraftType !== null && hasPrimaryHost;

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!canSubmit || submitting) return;
    setError(null);
    setSubmitting(true);

    try {
      const created = await createDraft(accessToken, {
        title: title.trim(),
        draftType: selectedDraftType!.value ?? 0,
        seriesId: selectedSeriesId,
        parts: parts.map((part) => ({
          partIndex: parts.length === 1 ? 1 : part.partIndex,
          minimumPosition: part.minPositions,
          maximumPosition: part.maxPositions,
          community: part.communityConfig.enabled
            ? {
              maxCommunityPicks: part.communityConfig.maxCommunityPicks,
              maxCommunityVetoes: part.communityConfig.maxCommunityVetoes,
              filmRules: part.communityConfig.filmRules
                .filter((r) => r.status !== "removing")
                .map((r) => ({
                  ruleKind: r.ruleKind === "BoostersVeto" ? 0 : 1,
                  targetSlot: r.targetSlot ?? null,
                  tmdbId: r.tmdbId ?? null,
                })),
            }
            : null,
          positions: part.positions.map((p) => ({
            name: p.name,
            picks: p.picks,
            hasBonusVeto: p.hasBonusVeto,
            hasBonusVetoOverride: p.hasBonusVetoOverride,
          })),
        })),
        hosts: hosts.map((h) => ({
          hostPublicId: h.publicId,
          hostRole: h.role === "Primary" ? 0 : 1,
        })),
        drafterIds: [...selectedDrafterIds],
        teamIds: [...selectedTeamIds],
        categoryIds: [...selectedCategoryIds],
        campaignId: campaignId || null,
      });

      const hasPredictions = parts.some((p) => p.predictionConfig.enabled);
      if (hasPredictions) {
        const detail = await getDraft(accessToken, created.publicId);
        if (detail) {
          for (let i = 0; i < parts.length; i++) {
            const config = parts[i].predictionConfig;
            if (!config.enabled) continue;
            const detailPart = detail.parts.find((dp) => dp.partIndex === parts[i].partIndex);
            if (detailPart) {
              await syncPredictionConfig(accessToken, detailPart.publicId, config);
            }
          }
        }
      }

      router.push(`/admin/drafts`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "An unexpected error occurred.");
    } finally {
      setSubmitting(false);
    }
  }

  const draftTypeName = selectedDraftType?.name ?? "";
  const fixedPositions = isFixedPositionType(draftTypeName);

  return (
    <form onSubmit={handleSubmit} className="space-y-10">
      {/* ── Section 1: Core metadata ── */}
      <section>
        <h2 className={SECTION_HEADING}>Core Details</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="md:col-span-3">
            <label className={LABEL}>
              Title <span className="text-sd-red">*</span>
            </label>
            <input
              type="text"
              className={INPUT}
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="Draft title"
              required
            />
          </div>

          {/* Feed selector */}
          <div>
            <label className={LABEL}>
              Feed <span className="text-sd-red">*</span>
            </label>
            <select
              className={SELECT}
              value={selectedFeed}
              onChange={(e) => handleFeedChange(e.target.value as FeedType)}
              required
            >
              <option value="">— Select feed —</option>
              <option value="main">Main Feed</option>
              <option value="patreon">Patreon</option>
            </select>
          </div>

          {/* Series selector — appears once feed is chosen */}
          <div>
            <label className={LABEL}>
              Series <span className="text-sd-red">*</span>
            </label>
            <select
              className={SELECT}
              value={selectedSeriesId}
              onChange={(e) => handleSeriesChange(e.target.value)}
              disabled={selectedFeed === ""}
              required
            >
              <option value="">
                {selectedFeed === "" ? "— Select feed first —" : "— Select series —"}
              </option>
              {filteredSeriesList.map((s) => (
                <option key={s.publicId} value={s.publicId}>
                  {s.name}
                </option>
              ))}
            </select>
          </div>

          {/* Draft type selector */}
          <div>
            <label className={LABEL}>
              Draft Type <span className="text-sd-red">*</span>
            </label>
            <select
              className={SELECT}
              value={selectedDraftType?.name ?? ""}
              onChange={(e) => handleDraftTypeChange(e.target.value)}
              disabled={availableDraftTypes.length === 0}
              required
            >
              <option value="">— Select draft type —</option>
              {availableDraftTypes.map((t) => (
                <option key={t.name} value={t.name ?? ""}>
                  {formatDraftType(t.name)}
                </option>
              ))}
            </select>
            {selectedSeriesId && availableDraftTypes.length === 0 && (
              <p className="text-[11px] text-sd-ink/50 mt-1 font-mono">
                No draft types available for this series.
              </p>
            )}
          </div>
        </div>
      </section>

      {/* ── Section 2: Parts configuration ── */}
      <section>
        <h2 className={SECTION_HEADING}>Parts Configuration</h2>
        <div className="mb-6 max-w-[160px]">
          <label className={LABEL}>Number of Parts</label>
          <input
            type="number"
            min={1}
            max={10}
            className={INPUT}
            value={numParts}
            onChange={(e) => handleNumPartsChange(parseInt(e.target.value, 10) || 1)}
          />
        </div>

        {numParts === 1 && parts[0]?.maxLocked && (
          <div className="space-y-4">
            <p className="text-[11px] font-mono text-sd-ink/50">
              Positions 1–{parts[0].maxPositions} (fixed by draft type)
            </p>
            <div>
              <p className={LABEL}>Positions</p>
              <PositionsEditor
                positions={parts[0].positions}
                onChange={(pos) => updatePartPositions(0, pos)}
                totalPicks={parts[0].maxPositions}
                readonly={fixedPositions}
              />
            </div>
          </div>
        )}

        {numParts === 1 && !parts[0]?.maxLocked && (
          <div className="space-y-4">
            <div className="max-w-[160px]">
              <label className={LABEL}>Max Positions</label>
              <input
                type="number"
                min={1}
                className={INPUT}
                value={parts[0]?.maxPositions ?? 1}
                onChange={(e) => updatePartMax(0, parseInt(e.target.value, 10) || 1)}
              />
            </div>
            <div>
              <p className={LABEL}>Positions</p>
              <PositionsEditor
                positions={parts[0].positions}
                onChange={(pos) => updatePartPositions(0, pos)}
                totalPicks={parts[0].maxPositions}
              />
            </div>
          </div>
        )}

        {numParts > 1 && (
          <div className="space-y-3">
            {parts.map((part, idx) => (
              <div key={idx} className="border border-sd-ink/10 rounded">
                <button
                  type="button"
                  className="w-full flex items-center justify-between px-4 py-3 text-left bg-sd-ink/5 hover:bg-sd-ink/10 transition-colors"
                  onClick={() => togglePartCollapsed(idx)}
                >
                  <span className="font-oswald font-medium text-sd-ink tracking-wide">
                    Part {part.partIndex}
                  </span>
                  <span className="text-sd-ink/50 text-sm">{part.collapsed ? "▶" : "▼"}</span>
                </button>

                {!part.collapsed && (
                  <div className="px-4 py-4 space-y-4">
                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                      <div>
                        <label className={LABEL}>Part Index</label>
                        <div className="px-3 py-2 bg-sd-ink/5 rounded text-sd-ink text-sm font-mono">
                          {part.partIndex}
                        </div>
                      </div>
                      <div>
                        <label className={LABEL}>Min Positions</label>
                        <div className="px-3 py-2 bg-sd-ink/5 rounded text-sd-ink text-sm font-mono">
                          {part.minPositions}
                        </div>
                      </div>
                      <div>
                        <label className={LABEL}>Max Positions</label>
                        {part.maxLocked ? (
                          <div className="px-3 py-2 bg-sd-ink/5 rounded text-sd-ink text-sm font-mono flex items-center gap-2">
                            {part.maxPositions}
                            <span className="text-[10px] text-sd-ink/40 font-mono uppercase tracking-wide">locked</span>
                          </div>
                        ) : (
                          <input
                            type="number"
                            min={1}
                            className={INPUT}
                            value={part.maxPositions}
                            onChange={(e) => updatePartMax(idx, parseInt(e.target.value, 10) || 1)}
                          />
                        )}
                      </div>
                    </div>

                    <div>
                      <p className={LABEL}>Positions</p>
                      <PositionsEditor
                        positions={part.positions}
                        onChange={(pos) => updatePartPositions(idx, pos)}
                        totalPicks={part.maxPositions}
                        readonly={fixedPositions}
                      />
                    </div>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </section>

      {/* ── Section 3: Hosts ── */}
      <section>
        <h2 className={SECTION_HEADING}>Hosts (Optional)</h2>

        {hosts.length > 0 && !hasPrimaryHost && (
          <p className="mb-3 text-sm text-sd-red font-mono">
            Select a primary host before saving — the draft can&apos;t be started without one.
          </p>
        )}

        {hosts.length > 0 && (
          <div className="mb-4 space-y-2">
            {hosts.map((h) => (
              <div
                key={h.publicId}
                className="flex items-center justify-between gap-3 px-3 py-2 bg-white border border-sd-ink/10 rounded"
              >
                <span className="text-sm text-sd-ink font-medium">{h.displayName}</span>
                <div className="flex items-center gap-3">
                  <label className="flex items-center gap-1.5 text-sm text-sd-ink/70 cursor-pointer">
                    <input
                      type="radio"
                      name={`host-role-${h.publicId}`}
                      checked={h.role === "Primary"}
                      onChange={() => setHostRole(h.publicId, "Primary")}
                      className="accent-sd-red"
                    />
                    Primary
                  </label>
                  <label className="flex items-center gap-1.5 text-sm text-sd-ink/70 cursor-pointer">
                    <input
                      type="radio"
                      name={`host-role-${h.publicId}`}
                      checked={h.role === "CoHost"}
                      onChange={() => setHostRole(h.publicId, "CoHost")}
                      className="accent-sd-red"
                    />
                    Co-Host
                  </label>
                  <button
                    type="button"
                    onClick={() => removeHost(h.publicId)}
                    className="text-sd-ink/40 hover:text-sd-red text-lg leading-none"
                    aria-label={`Remove ${h.displayName}`}
                  >
                    ×
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}

        <div className="border border-sd-ink/10 rounded p-4 bg-white">
          <input
            type="text"
            placeholder="Search hosts…"
            className={`${INPUT} mb-3`}
            value={hostSearch}
            onChange={(e) => setHostSearch(e.target.value)}
          />
          <div className="max-h-40 overflow-y-auto space-y-1">
            {filteredHosts.length === 0 ? (
              <p className="text-sm text-sd-ink/40 font-mono">No hosts found.</p>
            ) : (
              filteredHosts.map((h) => (
                <button
                  key={h.publicId}
                  type="button"
                  onClick={() => addHost(h)}
                  className="w-full text-left px-3 py-1.5 text-sm text-sd-ink hover:bg-sd-ink/5 rounded transition-colors"
                >
                  {h.displayName}
                </button>
              ))
            )}
          </div>
        </div>
      </section>

      {/* ── Section 4: Participants ── */}
      <ParticipantsSection
        accessToken={accessToken}
        selectedDrafterIds={selectedDrafterIds}
        selectedTeamIds={selectedTeamIds}
        onToggleDrafter={toggleDrafter}
        onToggleTeam={toggleTeam}
      />

      {/* ── Section 4c: Community (per part) ── */}
      <section>
        <h2 className={SECTION_HEADING}>Community (Optional)</h2>
        <div className="space-y-4">
          {parts.map((part, idx) => (
            <div key={idx}>
              {parts.length > 1 && (
                <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 uppercase mb-2">
                  Part {part.partIndex}
                </p>
              )}
              <CommunitySection
                config={part.communityConfig}
                onChange={(next) => updatePartCommunityConfig(idx, next)}
              />
            </div>
          ))}
        </div>
      </section>

      {/* ── Section 4d: Commissioner Predictions (per part) ── */}
      <section>
        <h2 className={SECTION_HEADING}>Commissioner Predictions (Optional)</h2>
        <div className="space-y-4">
          {parts.map((part, idx) => (
            <div key={idx}>
              {parts.length > 1 && (
                <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 uppercase mb-2">
                  Part {part.partIndex}
                </p>
              )}
              <PredictionRulesSection
                config={part.predictionConfig}
                onChange={(next) => updatePartPredictionConfig(idx, next)}
                accessToken={accessToken}
              />
            </div>
          ))}
        </div>
      </section>

      {/* ── Section 5: Category ── */}
      <section>
        <h2 className={SECTION_HEADING}>Category (Optional)</h2>
        <div className="border border-sd-ink/10 rounded p-4 bg-white">
          <div className="max-h-40 overflow-y-auto columns-2 gap-4">
            {categoryList.map((c) => {
              const checked = selectedCategoryIds.has(c.publicId);
              return (
                <label
                  key={c.publicId}
                  className="flex items-center gap-2 mb-1 text-sm text-sd-ink hover:bg-sd-ink/5 rounded px-2 py-1 cursor-pointer break-inside-avoid"
                >
                  <input
                    type="checkbox"
                    checked={checked}
                    onChange={() => toggleCategory(c.publicId)}
                    className="accent-sd-red"
                  />
                  {c.name}
                </label>
              );
            })}
          </div>
          {selectedCategoryIds.size > 0 && (
            <p className="mt-2 text-[11px] font-mono text-sd-ink/50">
              {selectedCategoryIds.size} categor{selectedCategoryIds.size !== 1 ? "ies" : "y"} selected
            </p>
          )}
        </div>
      </section>

      {/* ── Section 6: Campaign ── */}
      <section>
        <h2 className={SECTION_HEADING}>Campaign (Optional)</h2>
        <div className="max-w-sm">
          <select
            className={SELECT}
            value={campaignId}
            onChange={(e) => setCampaignId(e.target.value)}
          >
            <option value="">— No campaign —</option>
            {campaignList.map((c) => (
              <option key={c.publicId} value={c.publicId}>
                {c.name}
              </option>
            ))}
          </select>
        </div>
      </section>

      {error && (
        <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
          {error}
        </div>
      )}

      <div className="flex items-center gap-4 pt-2">
        <button
          type="submit"
          disabled={!canSubmit || submitting}
          className={BTN_PRIMARY}
        >
          {submitting ? "Creating…" : "Create Draft"}
        </button>
        <a href="/admin/drafts" className={BTN_SECONDARY}>
          Cancel
        </a>
      </div>
    </form>
  );
}