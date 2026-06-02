"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import {
  AdminDraftDetail,
  setDraftCategories,
  setDraftCampaign,
  clearDraftCampaign,
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

interface Props {
  draft: AdminDraftDetail;
  categoryList: CategoryResponse[];
  campaignList: CampaignResponse[];
  accessToken: string;
}

export default function EditMetaForm({ draft, categoryList, campaignList, accessToken }: Props) {
  const router = useRouter();

  const [selectedCategoryIds, setSelectedCategoryIds] = useState<Set<string>>(
    () => new Set(draft.categories?.map((c) => c.publicId) ?? [])
  );
  const [categoriesChanged, setCategoriesChanged] = useState(false);
  const [campaignId, setCampaignId] = useState(draft.campaignPublicId ?? "");

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

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

      setSuccessMsg("Saved.");
      router.refresh();
    } catch (err) {
      setError(err instanceof Error ? err.message : "An unexpected error occurred.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
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
  );
}