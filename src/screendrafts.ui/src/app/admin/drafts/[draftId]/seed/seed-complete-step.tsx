// app/admin/drafts/[draftId]/seed/seed-complete-step.tsx
"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { completeDraftPart, getDraftPartPredictions } from "@/services/admin/fetch-admin-drafts";
import type { SeedDraftState } from "./seed-draft-wizard";

const BTN_PRIMARY =
  "bg-sd-red text-white font-oswald font-medium uppercase tracking-wide px-5 py-2.5 hover:bg-sd-red/90 disabled:opacity-50 transition-colors";

interface Props {
  draft: SeedDraftState;
  accessToken: string;
  onDone: () => void;
  alreadyComplete?: boolean;
}

export function SeedCompleteStep({ draft, accessToken, onDone, alreadyComplete }: Props) {
  const router = useRouter();
  const [completing, setCompleting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [done, setDone] = useState(false);

  // Standings always show exactly two contestants — Ryan and Clay. Any
  // additional predictor (a sponsor, a guest) is never an independent
  // standings entry; it must be linked as a surrogate to one of the two
  // real ones. Completing triggers scoring, and scoring can't be re-run
  // once PredictionResult rows exist — so this has to be checked and
  // blocked here, not just left as an available-but-skippable tool in the
  // Predictions step. This computes "how many submitted sets would show up
  // as independent standings entries" directly from real data — total
  // submitted sets minus however many are already referenced as someone
  // else's surrogate — rather than trying to know in advance which
  // contestants are "the real ones."
  const [checkingPredictions, setCheckingPredictions] = useState(true);
  const [standaloneSetCount, setStandaloneSetCount] = useState<number | null>(null);

  useEffect(() => {
    if (alreadyComplete) {
      setCheckingPredictions(false);
      return;
    }
    (async () => {
      const sets = await getDraftPartPredictions(accessToken, draft.draftPartPublicId);
      const surrogateSetIds = new Set(
        sets.flatMap((s) => s.surrogates.map((sur) => sur.surrogateSetPublicId))
      );
      setStandaloneSetCount(sets.filter((s) => !surrogateSetIds.has(s.publicId)).length);
      setCheckingPredictions(false);
    })();
  }, [accessToken, draft.draftPartPublicId, alreadyComplete]);

  const predictionsInvalid = standaloneSetCount != null && standaloneSetCount > 2;

  async function handleComplete() {
    if (predictionsInvalid) return;
    setCompleting(true);
    setError(null);
    try {
      await completeDraftPart(accessToken, draft.draftPublicId, draft.partIndex);
      onDone();
      setDone(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to complete the draft part.");
    } finally {
      setCompleting(false);
    }
  }

  if (alreadyComplete || done) {
    return (
      <div className="bg-white border border-sd-ink/10 p-8 max-w-md space-y-4">
        <p className="text-sm text-sd-ink">
          {done
            ? "Draft part marked complete. Honorifics and prediction scoring will run from here automatically."
            : "This draft part is already complete."}
        </p>
        <button
          type="button"
          onClick={() => router.push("/admin/drafts")}
          className={BTN_PRIMARY}
        >
          Back to Draft Management
        </button>
      </div>
    );
  }

  return (
    <div className="bg-white border border-sd-ink/10 p-8 max-w-md space-y-4">
      <p className="text-sm text-sd-ink/70">
        Completing requires every board position to be filled by a landed pick
        (or, for Speed Drafts, all three sub-drafts completed).
      </p>

      {checkingPredictions && (
        <p className="text-[11px] font-mono text-sd-ink/40">Checking predictions…</p>
      )}

      {!checkingPredictions && predictionsInvalid && (
        <div className="border border-sd-red/30 bg-sd-red/5 text-sd-ink text-sm px-4 py-3 rounded space-y-2">
          <p className="font-medium text-sd-red">
            {standaloneSetCount} predictors would show up as independent standings
            entries — there should only ever be 2.
          </p>
          <p className="text-sd-ink/70">
            Go back to Predictions and link any extra predictor as a surrogate
            before completing. Once this draft part is marked complete,
            scoring runs immediately and can&apos;t be re-run — this has to be
            right before that happens, not fixed after.
          </p>
        </div>
      )}

      {error && (
        <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
          {error}
        </div>
      )}

      <button
        type="button"
        onClick={handleComplete}
        disabled={completing || checkingPredictions || predictionsInvalid}
        className={BTN_PRIMARY}
      >
        {completing ? "Completing…" : "Complete Draft Part"}
      </button>
    </div>
  );
}