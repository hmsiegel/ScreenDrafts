"use client";

import { useEffect, useState } from "react";
import {
  getDraftPartPredictions,
  assignSurrogate,
  type DraftPartPrediction,
} from "@/services/admin/fetch-admin-drafts";

const LABEL = "block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1";
const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";
const BTN_PRIMARY =
  "bg-sd-red text-white font-oswald font-medium tracking-wide uppercase px-5 py-2.5 hover:bg-sd-red/90 disabled:opacity-50 transition-colors";

// Confirmed against MergePolicy.cs — UseHigherScore = 0, UseBothScores = 1.
const MERGE_POLICIES = [
  { value: 0, label: "Higher Score", description: "Credits whichever set scored more." },
  { value: 1, label: "Both Scores", description: "Sums both sets' points together." },
];

interface Props {
  draftPartPublicId: string;
  accessToken: string;
}

// Shared between edit-draft-form.tsx and the seed wizard's predictions step.
// Deliberately not offered from create — AssignSurrogate requires both sets
// to already exist (it looks each one up and fails if either is missing),
// and a draft being created fresh has no submitted predictions yet by
// definition. This only ever makes sense post-submission, live or seeded —
// AttachSurrogate has no IsLocked guard, so it stays valid for the entire
// InProgress window, not just before Start.
export function SurrogateAssignmentPanel({ draftPartPublicId, accessToken }: Props) {
  const [sets, setSets] = useState<DraftPartPrediction[]>([]);
  const [loading, setLoading] = useState(true);
  const [primarySetId, setPrimarySetId] = useState("");
  const [surrogateSetId, setSurrogateSetId] = useState("");
  const [mergePolicy, setMergePolicy] = useState(0);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function loadSets() {
    setSets(await getDraftPartPredictions(accessToken, draftPartPublicId));
    setLoading(false);
  }

  useEffect(() => {
    loadSets();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [accessToken, draftPartPublicId]);

  const surrogateOptions = sets.filter((s) => s.publicId !== primarySetId);

  // Real data now — GetDraftPartPredictions exposes Surrogates, so this is
  // an actual check against what's assigned, not a same-session guess.
  const alreadyLinked = sets
    .find((s) => s.publicId === primarySetId)
    ?.surrogates.some((sur) => sur.surrogateSetPublicId === surrogateSetId);

  const existingAssignments = sets.flatMap((s) =>
    s.surrogates.map((sur) => ({
      primaryDisplayName: s.contestantDisplayName,
      surrogateDisplayName: sur.surrogateContestantDisplayName,
      mergePolicy: sur.mergePolicy,
    }))
  );

  // Same invariant SeedCompleteStep enforces as a hard block — shown here
  // too, earlier, so it's visible before reaching Complete rather than
  // discovered there for the first time.
  const surrogateSetIdsInUse = new Set(sets.flatMap((s) => s.surrogates.map((sur) => sur.surrogateSetPublicId)));
  const standaloneSetCount = sets.filter((s) => !surrogateSetIdsInUse.has(s.publicId)).length;

  async function handleAssign() {
    if (!primarySetId || !surrogateSetId || primarySetId === surrogateSetId || submitting) return;
    setSubmitting(true);
    setError(null);
    try {
      await assignSurrogate(accessToken, {
        draftPartId: draftPartPublicId,
        primarySetPublicId: primarySetId,
        surrogateSetPublicId: surrogateSetId,
        mergePolicy,
      });
      // Refetch rather than patch local state — AttachSurrogate has no
      // duplicate guard on the backend, so the list of what's actually
      // assigned only ever comes from the server, never assumed locally.
      await loadSets();
      setPrimarySetId("");
      setSurrogateSetId("");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to assign surrogate.");
    } finally {
      setSubmitting(false);
    }
  }

  if (loading) {
    return <p className="text-sm text-sd-ink/50 font-mono">Loading submitted predictions…</p>;
  }

  if (sets.length < 2) {
    return (
      <p className="text-sm text-sd-ink/50">
        Need at least two submitted prediction sets before a surrogate can be linked.
      </p>
    );
  }

  return (
    <div className="border border-sd-ink/10 rounded p-4 bg-white space-y-4">
      <p className={`${LABEL} mb-1`}>Additional Predictor Scoring</p>
      <p className="text-[11px] font-mono text-sd-ink/50">
        For a second person who submitted their own, separate predictions
        alongside a primary contestant — not someone who typed in the
        primary contestant&apos;s own picks for them (that&apos;s the
        &quot;Using a surrogate&quot; submitter option back in Predictors).
        Both people predicted independently; this just decides whose score
        counts toward the primary contestant&apos;s standing.
      </p>
      <p className="text-[11px] font-mono text-sd-ink/50">
        Standings only ever show 2 contestants — anyone beyond that must be
        linked here before completing this part.
      </p>

      {standaloneSetCount > 2 && (
        <p className="text-[11px] font-mono text-sd-red">
          {standaloneSetCount} sets aren&apos;t linked yet — completing this part
          before fixing that will lock in the wrong standings permanently.
        </p>
      )}

      {existingAssignments.length > 0 && (
        <div className="space-y-1">
          {existingAssignments.map((a, i) => (
            <p key={i} className="text-[11px] font-mono text-sd-ink/60">
              {a.primaryDisplayName} ← {a.surrogateDisplayName} ({a.mergePolicy})
            </p>
          ))}
        </div>
      )}

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className={LABEL}>Primary Set</label>
          <select
            className={INPUT}
            value={primarySetId}
            onChange={(e) => setPrimarySetId(e.target.value)}
          >
            <option value="">Select…</option>
            {sets.map((s) => (
              <option key={s.publicId} value={s.publicId}>
                {s.contestantDisplayName} {s.isLocked ? "(locked)" : ""}
              </option>
            ))}
          </select>
        </div>
        <div>
          <label className={LABEL}>Additional Predictor&apos;s Set</label>
          <select
            className={INPUT}
            value={surrogateSetId}
            onChange={(e) => setSurrogateSetId(e.target.value)}
          >
            <option value="">Select…</option>
            {surrogateOptions.map((s) => (
              <option key={s.publicId} value={s.publicId}>
                {s.contestantDisplayName} {s.isLocked ? "(locked)" : ""}
              </option>
            ))}
          </select>
        </div>
      </div>

      <div>
        <label className={LABEL}>Merge Policy</label>
        <select
          className={INPUT}
          value={mergePolicy}
          onChange={(e) => setMergePolicy(parseInt(e.target.value, 10))}
        >
          {MERGE_POLICIES.map((p) => (
            <option key={p.value} value={p.value}>
              {p.label}
            </option>
          ))}
        </select>
        <p className="text-[11px] font-mono text-sd-ink/40 mt-1">
          {MERGE_POLICIES.find((p) => p.value === mergePolicy)?.description}
        </p>
      </div>

      {alreadyLinked && (
        <p className="text-[11px] font-mono text-sd-red">
          This pair is already linked. Submitting again will add a second assignment,
          not update the existing one — AttachSurrogate has no dedup check.
        </p>
      )}

      {error && (
        <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
          {error}
        </div>
      )}

      <button
        type="button"
        onClick={handleAssign}
        disabled={!primarySetId || !surrogateSetId || primarySetId === surrogateSetId || submitting}
        className={BTN_PRIMARY}
      >
        {submitting ? "Linking…" : "Link Scores"}
      </button>
    </div>
  );
}