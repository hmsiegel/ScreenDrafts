"use client";

import { useState } from "react";
import { startDraftPart } from "@/services/admin/fetch-admin-drafts";
import type { SeedDraftState } from "./seed-draft-wizard";

const BTN_PRIMARY =
  "bg-sd-blue text-white font-oswald font-medium uppercase tracking-wide px-5 py-2.5 hover:bg-sd-blue/90 disabled:opacity-50 transition-colors";

interface Props {
  draft: SeedDraftState;
  accessToken: string;
  onDone: () => void;
}

export function SeedStartStep({ draft, accessToken, onDone }: Props) {
  const [starting, setStarting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleStart() {
    setStarting(true);
    setError(null);
    try {
      await startDraftPart(accessToken, draft.draftPublicId, draft.partIndex);
      onDone();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to start the draft part.");
    } finally {
      setStarting(false);
    }
  }

  return (
    <div className="bg-white border border-sd-ink/10 p-8 max-w-md space-y-4">
      <p className="text-sm text-sd-ink/70">
        Starting locks in hosts, participants, and positions, and computes veto
        rollovers from any prior completed parts for these drafters. Predictions
        should be entered before this step — once started, submissions are only
        accepted until scoring runs, not indefinitely.
      </p>

      {error && (
        <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
          {error}
        </div>
      )}

      <button type="button" onClick={handleStart} disabled={starting} className={BTN_PRIMARY}>
        {starting ? "Starting…" : "Start Draft Part →"}
      </button>
    </div>
  );
}