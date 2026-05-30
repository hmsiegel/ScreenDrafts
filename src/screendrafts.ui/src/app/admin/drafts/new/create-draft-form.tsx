"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import {
  AdminSeriesOption,
  AdminHostOption,
  AdminDrafterOption,
  createDraft,
  createDraftPart,
  addHostToDraftPart,
  addParticipantToDraftPart,
  setDraftCategories,
  setDraftCampaign,
} from "@/services/admin/fetch-admin-drafts";
import { CampaignResponse, CategoryResponse, SmartEnumResponse } from "@/lib/dto";

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

// Max positions per draft type name (locked = user cannot change)
function getMaxPositionsConfig(draftTypeName: string): { max: number; locked: boolean } {
  switch (draftTypeName) {
    case "Standard":
      return { max: 7, locked: true };
    case "SpeedDraft":
      return { max: 7, locked: true };
    default:
      return { max: 5, locked: false };
  }
}

interface PartConfig {
  partIndex: number;
  minPositions: number;
  maxPositions: number;
  maxLocked: boolean;
  collapsed: boolean;
}

interface SelectedHost {
  publicId: string;
  displayName: string;
  role: "Primary" | "CoHost";
}

interface Props {
  seriesList: AdminSeriesOption[];
  hostList: AdminHostOption[];
  drafterList: AdminDrafterOption[];
  categoryList: CategoryResponse[];
  campaignList: CampaignResponse[];
  accessToken: string;
}

export default function CreateDraftForm({
  seriesList,
  hostList,
  drafterList,
  categoryList,
  campaignList,
  accessToken,
}: Props) {
  const router = useRouter();

  // Section 1 — Core metadata
  const [title, setTitle] = useState("");
  const [selectedSeriesId, setSelectedSeriesId] = useState("");
  const [selectedDraftType, setSelectedDraftType] = useState<SmartEnumResponse | null>(null);

  // Section 2 — Parts
  const [numParts, setNumParts] = useState(1);
  const [parts, setParts] = useState<PartConfig[]>([
    { partIndex: 1, minPositions: 1, maxPositions: 7, maxLocked: true, collapsed: false },
  ]);

  // Section 3 — Hosts
  const [hosts, setHosts] = useState<SelectedHost[]>([]);
  const [hostSearch, setHostSearch] = useState("");

  // Section 4 — Drafters
  const [selectedDrafterIds, setSelectedDrafterIds] = useState<Set<string>>(new Set());
  const [drafterSearch, setDrafterSearch] = useState("");

  // Section 5 — Categories
  const [selectedCategoryIds, setSelectedCategoryIds] = useState<Set<string>>(new Set());

  // Section 6 — Campaign
  const [campaignId, setCampaignId] = useState("");

  // Submit state
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Derived: available draft types for selected series
  const selectedSeries = seriesList.find((s) => s.publicId === selectedSeriesId);
  const availableDraftTypes = selectedSeries?.allowedDraftTypes ?? [];

  // When series changes, reset draft type
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
    setParts(
      Array.from({ length: count }, (_, i) => ({
        partIndex: i + 1,
        minPositions: 1,
        maxPositions: max,
        maxLocked: locked,
        collapsed: i > 0,
      }))
    );
  }

  function handleNumPartsChange(n: number) {
    const count = Math.max(1, n);
    setNumParts(count);
    const typeName = selectedDraftType?.name ?? "";
    const { max, locked } = getMaxPositionsConfig(typeName);
    setParts(
      Array.from({ length: count }, (_, i) => ({
        partIndex: i + 1,
        minPositions: 1,
        maxPositions: parts[i]?.maxPositions ?? max,
        maxLocked: locked,
        collapsed: i > 0,
      }))
    );
  }

  function updatePartMax(idx: number, value: number) {
    setParts((prev) =>
      prev.map((p, i) => (i === idx ? { ...p, maxPositions: Math.max(1, value) } : p))
    );
  }

  function togglePartCollapsed(idx: number) {
    setParts((prev) =>
      prev.map((p, i) => (i === idx ? { ...p, collapsed: !p.collapsed } : p))
    );
  }

  // Hosts
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
      prev.map((h) => (h.publicId === publicId ? { ...h, role } : h))
    );
  }

  // Drafters
  function toggleDrafter(id: string) {
    setSelectedDrafterIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  // Categories
  function toggleCategory(id: string) {
    setSelectedCategoryIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  const filteredHosts = hostList.filter(
    (h) =>
      !hosts.some((sel) => sel.publicId === h.publicId) &&
      h.displayName.toLowerCase().includes(hostSearch.toLowerCase())
  );

  const filteredDrafters = drafterList.filter((d) =>
    d.displayName.toLowerCase().includes(drafterSearch.toLowerCase())
  );

  const canSubmit = title.trim() !== "" && selectedSeriesId !== "" && selectedDraftType !== null;

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!canSubmit || submitting) return;
    setError(null);
    setSubmitting(true);

    try {
      // Step 1: Create draft
      const { publicId: draftPublicId } = await createDraft(accessToken, {
        title: title.trim(),
        draftType: selectedDraftType!.value ?? 0,
        seriesId: selectedSeriesId,
      });

      // Step 2: Create parts
      const partPublicIds: string[] = [];
      for (const part of parts) {
        const partId = await createDraftPart(accessToken, draftPublicId, {
          publicId: draftPublicId,
          partIndex: parts.length === 1 ? 1 : part.partIndex,
          minimumPosition: part.minPositions,
          maximumPosition: part.maxPositions,
        });
        partPublicIds.push(partId);
      }

      // Step 3: Add hosts to each part
      if (hosts.length > 0) {
        for (const partId of partPublicIds) {
          for (const host of hosts) {
            await addHostToDraftPart(accessToken, partId, {
              draftPartId: partId,
              hostPublicId: host.publicId,
              hostRole: host.role === "Primary" ? 0 : 1,
            });
          }
        }
      }

      // Step 4: Add drafters to each part
      if (selectedDrafterIds.size > 0) {
        for (const partId of partPublicIds) {
          for (const drafterId of selectedDrafterIds) {
            await addParticipantToDraftPart(accessToken, partId, {
              draftPartId: partId,
              participantPublicId: drafterId,
              participantKind: 0, // Drafter
            });
          }
        }
      }

      // Step 5: Set categories
      if (selectedCategoryIds.size > 0) {
        await setDraftCategories(accessToken, draftPublicId, [...selectedCategoryIds]);
      }

      // Step 6: Set campaign
      if (campaignId) {
        await setDraftCampaign(accessToken, draftPublicId, campaignId);
      }

      // TODO: redirect to /admin/drafts/{draftPublicId}/edit once edit page exists
      router.push(`/admin/drafts`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "An unexpected error occurred.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-10">
      {/* ── Section 1: Core metadata ── */}
      <section>
        <h2 className={SECTION_HEADING}>Core Details</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="md:col-span-2">
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

          <div>
            <label className={LABEL}>
              Series <span className="text-sd-red">*</span>
            </label>
            <select
              className={SELECT}
              value={selectedSeriesId}
              onChange={(e) => handleSeriesChange(e.target.value)}
              required
            >
              <option value="">— Select series —</option>
              {seriesList.map((s) => (
                <option key={s.publicId} value={s.publicId}>
                  {s.name}
                </option>
              ))}
            </select>
          </div>

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
                  {t.name}
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

        <div className="space-y-3">
          {parts.map((part, idx) => (
            <div key={idx} className="border border-sd-ink/10 rounded">
              <button
                type="button"
                className="w-full flex items-center justify-between px-4 py-3 text-left bg-sd-ink/5 hover:bg-sd-ink/10 transition-colors"
                onClick={() => togglePartCollapsed(idx)}
              >
                <span className="font-oswald font-medium text-sd-ink tracking-wide">
                  {numParts > 1 ? `Part ${part.partIndex}` : "Part Configuration"}
                </span>
                <span className="text-sd-ink/50 text-sm">{part.collapsed ? "▶" : "▼"}</span>
              </button>

              {!part.collapsed && (
                <div className="px-4 py-4 grid grid-cols-1 sm:grid-cols-3 gap-4">
                  {numParts > 1 && (
                    <div>
                      <label className={LABEL}>Part Index</label>
                      <div className="px-3 py-2 bg-sd-ink/5 rounded text-sd-ink text-sm font-mono">
                        {part.partIndex}
                      </div>
                    </div>
                  )}
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
                        <span className="text-[10px] text-sd-ink/40 font-mono uppercase tracking-wide">
                          locked
                        </span>
                      </div>
                    ) : (
                      <input
                        type="number"
                        min={1}
                        className={INPUT}
                        value={part.maxPositions}
                        onChange={(e) =>
                          updatePartMax(idx, parseInt(e.target.value, 10) || 1)
                        }
                      />
                    )}
                  </div>
                </div>
              )}
            </div>
          ))}
        </div>
      </section>

      {/* ── Section 3: Hosts ── */}
      <section>
        <h2 className={SECTION_HEADING}>Hosts (Optional)</h2>

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

      {/* ── Section 4: Participants (Drafters) ── */}
      <section>
        <h2 className={SECTION_HEADING}>Participants (Optional)</h2>
        <div className="border border-sd-ink/10 rounded p-4 bg-white">
          <input
            type="text"
            placeholder="Search drafters…"
            className={`${INPUT} mb-3`}
            value={drafterSearch}
            onChange={(e) => setDrafterSearch(e.target.value)}
          />
          <div className="max-h-48 overflow-y-auto space-y-1">
            {filteredDrafters.map((d) => {
              const checked = selectedDrafterIds.has(d.publicId);
              return (
                <label
                  key={d.publicId}
                  className="flex items-center gap-2 px-3 py-1.5 text-sm text-sd-ink hover:bg-sd-ink/5 rounded cursor-pointer"
                >
                  <input
                    type="checkbox"
                    checked={checked}
                    onChange={() => toggleDrafter(d.publicId)}
                    className="accent-sd-red"
                  />
                  {d.displayName}
                </label>
              );
            })}
          </div>
          {selectedDrafterIds.size > 0 && (
            <p className="mt-2 text-[11px] font-mono text-sd-ink/50">
              {selectedDrafterIds.size} drafter{selectedDrafterIds.size !== 1 ? "s" : ""} selected
            </p>
          )}
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

      {/* ── Error + Submit ── */}
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
