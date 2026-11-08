"use client";
// app/admin/drafts/[draftId]/seed/seed-episode-step.tsx

import { useState } from "react";
import { setDraftEpisodeNumber } from "@/services/admin/fetch-admin-drafts";
import type { SeedDraftState } from "./seed-draft-wizard";

const LABEL = "block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1";
const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";
const BTN_PRIMARY =
  "bg-sd-red text-white font-oswald font-medium tracking-wide uppercase px-5 py-2.5 hover:bg-sd-red/90 disabled:opacity-50 transition-colors";

// Matches ReleaseChannel SmartEnum: MainFeed = 0, Patreon = 1.
const MAIN_FEED = 0;
const RELEASE_CHANNELS = [
  { value: MAIN_FEED, label: "Main Feed" },
  { value: 1, label: "Patreon" },
];

interface Props {
  draft: SeedDraftState;
  accessToken: string;
  onDone: () => void;
}

export function SeedEpisodeStep({ draft, accessToken, onDone }: Props) {
  const [episodeNumber, setEpisodeNumber] = useState<number | "">("");
  const [releaseChannel, setReleaseChannel] = useState(MAIN_FEED);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const episodeRequired = releaseChannel === MAIN_FEED;
  const canSubmit = !submitting && (!episodeRequired || episodeNumber !== "");

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!canSubmit) return;
    setSubmitting(true);
    setError(null);
    try {
      await setDraftEpisodeNumber(
        accessToken,
        draft.draftPublicId,
        episodeNumber === "" ? null : episodeNumber,
        releaseChannel
      );
      onDone();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to set episode info.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="bg-white border border-sd-ink/10 p-4 sm:p-8 space-y-6 max-w-md">
      <div>
        <label className={LABEL}>Release Channel</label>
        <select
          className={INPUT}
          value={releaseChannel}
          onChange={(e) => setReleaseChannel(parseInt(e.target.value, 10))}
        >
          {RELEASE_CHANNELS.map((c) => (
            <option key={c.value} value={c.value}>
              {c.label}
            </option>
          ))}
        </select>
      </div>

      <div>
        <label className={LABEL}>
          Episode Number{" "}
          {episodeRequired ? (
            <span className="text-sd-red">*</span>
          ) : (
            <span className="normal-case tracking-normal text-sd-ink/40">(optional)</span>
          )}
        </label>
        <input
          type="number"
          min={1}
          className={INPUT}
          value={episodeNumber}
          onChange={(e) =>
            setEpisodeNumber(e.target.value === "" ? "" : parseInt(e.target.value, 10))
          }
          required={episodeRequired}
        />
      </div>

      {error && (
        <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
          {error}
        </div>
      )}

      <button type="submit" disabled={!canSubmit} className={BTN_PRIMARY}>
        {submitting ? "Saving…" : "Continue →"}
      </button>
    </form>
  );
}
