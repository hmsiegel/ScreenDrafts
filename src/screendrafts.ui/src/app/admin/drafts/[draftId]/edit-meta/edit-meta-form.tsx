"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import {
  AdminDraftDetail,
  setDraftCategories,
  setDraftCampaign,
  clearDraftCampaign,
  UnreleasedDraftPart,
  setDraftPartRelease,
  setDraftEpisodeNumber,
} from "@/services/admin/fetch-admin-drafts";
import { CampaignResponse, CategoryResponse } from "@/lib/dto";
import DraftImageUpload from "@/components/features/drafts/draft-image-upload";

const SELECT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";
const BTN_PRIMARY =
  "bg-sd-red text-white font-oswald font-medium tracking-wide uppercase px-5 py-2.5 hover:bg-sd-red/90 disabled:opacity-50 transition-colors";
const BTN_SECONDARY =
  "border border-sd-ink/20 text-sd-ink font-sans text-sm px-4 py-2 hover:bg-sd-ink/5 disabled:opacity-50 transition-colors rounded";
const SECTION_HEADING =
  "font-oswald font-bold text-[18px] tracking-wide uppercase text-sd-ink mb-4 pb-2 border-b border-sd-ink/10";

const API_URL = process.env.NEXT_PUBLIC_API_URL;

const RELEASE_CHANNEL_OPTIONS = [
  { value: 0, label: "Main Feed" },
  { value: 1, label: "Patreon" },
];

interface Props {
  draft: AdminDraftDetail;
  categoryList: CategoryResponse[];
  campaignList: CampaignResponse[];
  unreleasedParts: UnreleasedDraftPart[];
  accessToken: string;
}

export default function EditMetaForm({ draft, categoryList, campaignList, unreleasedParts, accessToken }: Props) {
  const router = useRouter();

  const [selectedCategoryIds, setSelectedCategoryIds] = useState<Set<string>>(
    () => new Set(draft.categories?.map((c) => c.publicId) ?? [])
  );
  const [categoriesChanged, setCategoriesChanged] = useState(false);
  const [campaignId, setCampaignId] = useState(draft.campaignPublicId ?? "");
  const [episodeNumber, setEpisodeNumber] = useState(
    draft.episodeNumber != null ? String(draft.episodeNumber) : ""
  );

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  const [pendingParts, setPendingParts] = useState(unreleasedParts);
  const [releaseDrafts, setReleaseDrafts] = useState<Record<string, { date: string; channel: number }>>(
    () =>
      Object.fromEntries(
        unreleasedParts.map((p) => [p.draftPartPublicId, { date: "", channel: 0 }])
      )
  );
  const [releaseSubmittingId, setReleaseSubmittingId] = useState<string | null>(null);
  const [releaseError, setReleaseError] = useState<string | null>(null);

  function updateReleaseDraft(partId: string, patch: Partial<{ date: string; channel: number }>) {
    setReleaseDrafts((prev) => ({
      ...prev,
      [partId]: { ...prev[partId], ...patch },
    }));
  }

  async function handleSetRelease(partId: string) {
    const entry = releaseDrafts[partId];
    if (!entry?.date) {
      setReleaseError("Pick a release date first.");
      return;
    }
    setReleaseError(null);
    setReleaseSubmittingId(partId);
    try {
      await setDraftPartRelease(accessToken, partId, entry.date, entry.channel);
      setPendingParts((prev) => prev.filter((p) => p.draftPartPublicId !== partId));
      router.refresh();
    } catch (err) {
      setReleaseError(err instanceof Error ? err.message : "An unexpected error occurred.");
    } finally {
      setReleaseSubmittingId(null);
    }
  }

  function toggleCategory(id: string) {
    setCategoriesChanged(true);
    setSelectedCategoryIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (submitting) return;
    setError(null);
    setSuccessMsg(null);
    setSubmitting(true);

    try {
      if (!campaignId && draft.campaignPublicId) {
        await clearDraftCampaign(accessToken, draft.publicId);
      } else if (campaignId && campaignId !== draft.campaignPublicId) {
        await setDraftCampaign(accessToken, draft.publicId, campaignId);
      }

      if (categoriesChanged) {
        if (selectedCategoryIds.size > 0) {
          await setDraftCategories(accessToken, draft.publicId, [...selectedCategoryIds]);
        }
      }

      const trimmedEpisode = episodeNumber.trim();
      if (trimmedEpisode !== "") {
        const parsed = Number(trimmedEpisode);
        if (!Number.isInteger(parsed) || parsed <= 0) {
          throw new Error("Episode number must be a positive whole number.");
        }
        if (parsed !== draft.episodeNumber) {
          await setDraftEpisodeNumber(accessToken, draft.publicId, parsed);
        }
      }

      setSuccessMsg("Saved.");
      router.refresh();
    } catch (err) {
      setError(err instanceof Error ? err.message : "An unexpected error occurred.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="space-y-8">
      {pendingParts.length > 0 && (
        <section>
          <h2 className={SECTION_HEADING}>Release</h2>
          <div className="space-y-3">
            {pendingParts.map((p) => {
              const entry = releaseDrafts[p.draftPartPublicId] ?? { date: "", channel: 0 };
              return (
                <div
                  key={p.draftPartPublicId}
                  className="border border-sd-ink/10 rounded p-4 bg-white flex flex-wrap items-end gap-4"
                >
                  <div>
                    <p className="font-oswald font-bold text-sm text-sd-ink uppercase">
                      Part {p.partIndex}
                    </p>
                    <p className="font-mono text-[11px] text-sd-ink/50">No release set yet</p>
                  </div>
                  <div>
                    <label className="block font-mono text-[11px] tracking-widest text-sd-ink/60 mb-1">
                      Date
                    </label>
                    <input
                      type="date"
                      className={SELECT}
                      value={entry.date}
                      onChange={(e) =>
                        updateReleaseDraft(p.draftPartPublicId, { date: e.target.value })
                      }
                    />
                  </div>
                  <div>
                    <label className="block font-mono text-[11px] tracking-widest text-sd-ink/60 mb-1">
                      Channel
                    </label>
                    <select
                      className={SELECT}
                      value={entry.channel}
                      onChange={(e) =>
                        updateReleaseDraft(p.draftPartPublicId, { channel: Number(e.target.value) })
                      }
                    >
                      {RELEASE_CHANNEL_OPTIONS.map((o) => (
                        <option key={o.value} value={o.value}>{o.label}</option>
                      ))}
                    </select>
                  </div>
                  <button
                    type="button"
                    disabled={releaseSubmittingId === p.draftPartPublicId}
                    onClick={() => handleSetRelease(p.draftPartPublicId)}
                    className={BTN_PRIMARY}
                  >
                    {releaseSubmittingId === p.draftPartPublicId ? "Saving…" : "Set Release"}
                  </button>
                </div>
              );
            })}
          </div>
          {releaseError && (
            <div className="mt-3 border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
              {releaseError}
            </div>
          )}
        </section>
      )}

      <form onSubmit={handleSubmit} className="space-y-8">
        {/* Campaign */}
        <section>
          <h2 className={SECTION_HEADING}>Campaign</h2>
          <div className="max-w-sm">
            <select className={SELECT} value={campaignId} onChange={(e) => setCampaignId(e.target.value)}>
              <option value="">— No campaign —</option>
              {campaignList.map((c) => (
                <option key={c.publicId} value={c.publicId ?? ""}>{c.name}</option>
              ))}
            </select>
          </div>
        </section>

        {/* Episode Number */}
        <section>
          <h2 className={SECTION_HEADING}>Episode Number</h2>
          <div className="max-w-[160px]">
            <input
              type="number"
              min={1}
              step={1}
              className={SELECT}
              value={episodeNumber}
              onChange={(e) => setEpisodeNumber(e.target.value)}
              placeholder="e.g. 214"
            />
          </div>
          <p className="mt-2 text-[11px] font-mono text-sd-ink/50">
            Main Feed episode number. Leave blank for drafts that don&apos;t have one
            (specials, etc). Set manually — nothing here is auto-assigned.
          </p>
        </section>

        <DraftImageUpload
          draftPublicId={draft.publicId}
          currentImagePath={draft.imagePath}
          accessToken={accessToken}
          apiBase={API_URL!}
        />

        {/* Categories */}
        <section>
          <h2 className={SECTION_HEADING}>Categories</h2>
          <div className="border border-sd-ink/10 rounded p-4 bg-white">
            <div className="max-h-48 overflow-y-auto columns-2 gap-4">
              {categoryList.map((c) => (
                <label
                  key={c.publicId}
                  className="flex items-center gap-2 mb-1 text-sm text-sd-ink hover:bg-sd-ink/5 rounded px-2 py-1 cursor-pointer break-inside-avoid"
                >
                  <input
                    type="checkbox"
                    checked={selectedCategoryIds.has(c.publicId!)}
                    onChange={() => toggleCategory(c.publicId!)}
                    className="accent-sd-red"
                  />
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

        {error && (
          <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">{error}</div>
        )}
        {successMsg && (
          <div className="border border-green-300 bg-green-50 text-green-800 text-sm px-4 py-3 rounded">{successMsg}</div>
        )}

        <div className="flex items-center gap-4 pt-2">
          <button type="submit" disabled={submitting} className={BTN_PRIMARY}>
            {submitting ? "Saving…" : "Save"}
          </button>
          <a href={`/drafts/${draft.publicId}`} className={BTN_SECONDARY}>Back to Draft</a>
        </div>
      </form>
    </div>
  );
}