// app/admin/drafts/[draftId]/seed/seed-predictions-setup.tsx
"use client";

import { useState } from "react";
import {
  PredictionRulesSection,
  defaultPredictionConfig,
  PredictionConfig,
} from "../../new/prediction-rules-section";
import { syncPredictionConfig, type DraftPartHost } from "@/services/admin/fetch-admin-drafts";

const BTN_PRIMARY =
  "bg-sd-red text-white font-oswald font-medium tracking-wide uppercase px-5 py-2.5 hover:bg-sd-red/90 disabled:opacity-50 transition-colors";

interface Props {
  draftPartPublicId: string;
  accessToken: string;
  hosts: DraftPartHost[];
  onSaved: () => void;
}

// Shown when this draft part has no prediction rules/predictors yet — reuses
// PredictionRulesSection exactly as create-draft-form.tsx does, just against
// an existing part instead of one being created. Once saved, the parent
// re-fetches rules/predictors and swaps to the entries-submission view.
export function SeedPredictionsSetup({ draftPartPublicId, accessToken, hosts, onSaved }: Props) {
  const [config, setConfig] = useState<PredictionConfig>(() => ({
    ...defaultPredictionConfig(),
    enabled: true,
  }));
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSave() {
    setSaving(true);
    setError(null);
    try {
      await syncPredictionConfig(accessToken, draftPartPublicId, config);
      onSaved();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save prediction rules.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="bg-white border border-sd-ink/10 p-8 space-y-6">
      <p className="text-sm text-sd-ink/60">
        No prediction rules exist for this part yet — set them up before entering
        historical picks. If this episode had no predictions at all, use Skip below.
      </p>

      <PredictionRulesSection
        config={config}
        onChange={setConfig}
        accessToken={accessToken}
        hosts={hosts}
      />

      {error && (
        <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
          {error}
        </div>
      )}

      <div className="flex items-center gap-4">
        <button type="button" onClick={handleSave} disabled={saving} className={BTN_PRIMARY}>
          {saving ? "Saving…" : "Save Rules & Continue"}
        </button>
        <button
          type="button"
          onClick={onSaved}
          className="text-[11px] font-mono text-sd-ink/50 uppercase tracking-widest hover:underline"
        >
          Skip — no predictions for this episode
        </button>
      </div>
    </div>
  );
}