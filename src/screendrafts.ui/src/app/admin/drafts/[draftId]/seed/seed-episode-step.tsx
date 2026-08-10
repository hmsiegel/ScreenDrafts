"use client";

import { useState } from "react";
import { setDraftEpisodeNumber } from "@/services/admin/fetch-admin-drafts";
import type { SeedDraftState } from "./seed-draft-wizard";

const LABEL = "block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1";
const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";
const BTN_PRIMARY =
  "bg-sd-red text-white font-oswald font-medium tracking-wide uppercase px-5 py-2.5 hover:bg-sd-red/90 disabled:opacity-50 transition-colors";

// PLACEHOLDER — no ReleaseChannel option list was available from any
// reviewed file. setDraftEpisodeNumber's own default (0) is the only
// confirmed value. Replace with the real SmartEnum options before this
// ships; wrong channel here silently affects IsPatreon-driven honorifics
// eligibility downstream.
const RELEASE_CHANNELS = [
  { value: 0, label: "Main Feed" },
  { value: 1, label: "Patreon" },
];

interface Props {
  draft: SeedDraftState;
  accessToken: string;
  onDone: () => void;
}

export function SeedEpisodeStep({ draft, accessToken, onDone }: Props) {
  const [episodeNumber, setEpisodeNumber] = useState<number | "">("");
  const [releaseChannel, setReleaseChannel] = useState(0);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (episodeNumber === "" || submitting) return;
    setSubmitting(true);
    setError(null);
    try {
      await setDraftEpisodeNumber(accessToken, draft.draftPublicId, episodeNumber, releaseChannel);
      onDone();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to set episode info.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="bg-white border border-sd-ink/10 p-8 space-y-6 max-w-md">
      <div>
        <label className={LABEL}>
          Episode Number <span className="text-sd-red">*</span>
        </label>
        <input
          type="number"
          min={1}
          className={INPUT}
          value={episodeNumber}
          onChange={(e) =>
            setEpisodeNumber(e.target.value === "" ? "" : parseInt(e.target.value, 10))
          }
          required
        />
      </div>

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

      {error && (
        <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
          {error}
        </div>
      )}

      <button type="submit" disabled={episodeNumber === "" || submitting} className={BTN_PRIMARY}>
        {submitting ? "Saving…" : "Continue →"}
      </button>
    </form>
  );
}