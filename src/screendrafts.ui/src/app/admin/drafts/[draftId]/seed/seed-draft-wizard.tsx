// app/admin/drafts/[draftId]/seed/seed-draft-wizard.tsx
"use client";

import { useState } from "react";
import type { AdminDraftDetail } from "@/services/admin/fetch-admin-drafts";
import { SeedEpisodeStep } from "./seed-episode-step";
import { SeedStartStep } from "./seed-start-step";
import { SeedTriviaStep } from "./seed-trivia-step";
import { SeedPicksStep } from "./seed-picks-step";
import { SeedSpeedDraftStep } from "./seed-speed-draft-step";
import { SeedPredictionsStep } from "./seed-predictions-step";
import { SeedCompleteStep } from "./seed-complete-step";

export interface SeedDraftState {
  draftPublicId: string;
  draftPartPublicId: string;
  partIndex: number;
}

type StepKey = "episode" | "predictions" | "start" | "trivia" | "picks" | "speed" | "complete";

interface StepDef {
  key: StepKey;
  label: string;
  ready: boolean;
}

// Speed Drafts replace the separate Trivia + Picks steps with one combined
// "speed" step that loops over all three sub-drafts internally (trivia ->
// position choice -> picks/vetoes -> advance, repeated) — there's no
// part-level trivia round or single flat pick board to enter for this
// draft type.
const REGULAR_STEPS: StepDef[] = [
  { key: "episode", label: "Episode Info", ready: true },
  { key: "predictions", label: "Predictions", ready: true },
  { key: "start", label: "Start", ready: true },
  { key: "trivia", label: "Trivia", ready: true },
  { key: "picks", label: "Picks", ready: true },
  { key: "complete", label: "Complete", ready: true },
];

const SPEED_DRAFT_STEPS: StepDef[] = [
  { key: "episode", label: "Episode Info", ready: true },
  { key: "predictions", label: "Predictions", ready: true },
  { key: "start", label: "Start", ready: true },
  { key: "speed", label: "Sub-Drafts", ready: true },
  { key: "complete", label: "Complete", ready: true },
];

type PartSummary = AdminDraftDetail["parts"][number];

// Compares on status.name rather than status.value — confirmed correct
// against DraftPartStatus.cs: Created=0, InProgress=2, Completed=3,
// Cancelled=4. Value 1 is unused, so a sequential-guess numeric comparison
// would have silently misrouted InProgress and Completed.
function deriveInitialStepIndex(detail: AdminDraftDetail, part: PartSummary, steps: StepDef[]): number {
  const indexOf = (key: StepKey) => steps.findIndex((s) => s.key === key);

  if (detail.episodeNumber == null) return indexOf("episode"); // episode
  if (part.status.name === "InProgress") {
    // trivia/speed self-hydrates and is skippable/resumable, so landing here
    // is a fast click-through if it's already done, but won't silently skip
    // it if it isn't.
    return detail.draftType.name === "SpeedDraft" ? indexOf("speed") : indexOf("trivia");
  }
  if (part.status.name === "Completed") return indexOf("complete");
  return indexOf("predictions"); // Created, episode already set -> predictions
}

// MiniMega / Mega / Super / MiniSuper allow veto overrides; Standard and
// SpeedDraft don't (ApplyVetoOverride refuses both at the domain level).
function allowsOverride(detail: AdminDraftDetail): boolean {
  const name = detail.draftType.name;
  return name !== "Standard" && name !== "SpeedDraft";
}

interface Props {
  detail: AdminDraftDetail;
  accessToken: string;
}

export function SeedDraftWizard({ detail, accessToken }: Props) {
  const isSpeedDraft = detail.draftType.name === "SpeedDraft";
  const STEPS = isSpeedDraft ? SPEED_DRAFT_STEPS : REGULAR_STEPS;

  const part = detail.parts.find((p) => p.status.name !== "Completed") ?? detail.parts[0];

  const [stepIndex, setStepIndex] = useState(() =>
    part ? deriveInitialStepIndex(detail, part, STEPS) : 0
  );

  if (!part) {
    return <p className="text-sm text-sd-ink/60">This draft has no parts to seed.</p>;
  }

  const draft: SeedDraftState = {
    draftPublicId: detail.publicId,
    draftPartPublicId: part.publicId,
    partIndex: part.partIndex,
  };

  const alreadyComplete = part.status.name === "Completed";

  function advance() {
    setStepIndex((i) => Math.min(i + 1, STEPS.length - 1));
  }

  function goToStep(key: StepKey) {
    const idx = STEPS.findIndex((s) => s.key === key);
    if (idx >= 0) setStepIndex(idx);
  }

  const current = STEPS[stepIndex];

  return (
    <div className="space-y-8">
      {detail.parts.length > 1 && (
        <p className="text-[11px] font-mono text-sd-ink/50">
          Multiple parts on this draft — seeding Part {part.partIndex}. Multi-part
          switching isn&apos;t built yet.
        </p>
      )}

      <ol className="flex flex-wrap gap-2">
        {STEPS.map((s, i) => (
          <li key={s.key}>
            <button
              type="button"
              onClick={() => i <= stepIndex && s.ready && goToStep(s.key)}
              disabled={i > stepIndex || !s.ready}
              className={`px-3 py-1.5 text-[11px] font-mono tracking-widest uppercase rounded border transition-colors ${
                i === stepIndex
                  ? "bg-sd-ink text-white border-sd-ink"
                  : i < stepIndex
                    ? "bg-white text-sd-ink border-sd-ink/30 hover:bg-sd-ink/5"
                    : "bg-sd-paper text-sd-ink/30 border-sd-ink/10 cursor-not-allowed"
              }`}
            >
              {s.label}
              {!s.ready && <span className="ml-1">(soon)</span>}
            </button>
          </li>
        ))}
      </ol>

      {current.key === "episode" && (
        <SeedEpisodeStep draft={draft} accessToken={accessToken} onDone={advance} />
      )}

      {current.key === "predictions" && (
        <SeedPredictionsStep
          draftPartPublicId={draft.draftPartPublicId}
          accessToken={accessToken}
          hosts={[...(part.primaryHost ? [part.primaryHost] : []), ...part.coHosts]}
          onDone={advance}
        />
      )}

      {current.key === "start" && (
        <SeedStartStep draft={draft} accessToken={accessToken} onDone={advance} />
      )}

      {current.key === "trivia" && (
        <SeedTriviaStep
          draft={draft}
          participants={part.participants}
          accessToken={accessToken}
          onDone={advance}
        />
      )}

      {current.key === "picks" && (
        <SeedPicksStep
          draft={draft}
          participants={part.participants}
          primaryHost={part.primaryHost}
          coHosts={part.coHosts}
          allowsOverride={allowsOverride(detail)}
          accessToken={accessToken}
          onAllPositionsFilled={advance}
        />
      )}

      {current.key === "speed" && (
        <SeedSpeedDraftStep
          draft={draft}
          participants={part.participants}
          accessToken={accessToken}
          onDone={advance}
        />
      )}

      {current.key === "complete" && (
        <SeedCompleteStep
          draft={draft}
          accessToken={accessToken}
          onDone={() => {}}
          alreadyComplete={alreadyComplete}
        />
      )}
    </div>
  );
}