"use client";

import { useEffect, useState } from "react";
import {
  getDraftPartGameplay,
  assignTriviaResults,
  type DraftPartParticipant,
} from "@/services/admin/fetch-admin-drafts";
import type { SeedDraftState } from "./seed-draft-wizard";

const LABEL = "block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1";
const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded";
const BTN_PRIMARY =
  "bg-sd-red text-white font-oswald font-medium tracking-wide uppercase px-5 py-2.5 hover:bg-sd-red/90 disabled:opacity-50 transition-colors";

interface TriviaRow {
  participantIdValue: string;
  displayName: string;
  position: number | "";
  questionsWon: number | "";
}

interface Props {
  draft: SeedDraftState;
  participants: DraftPartParticipant[];
  accessToken: string;
  onDone: () => void;
}

// Community participants are excluded — confirmed against
// TriviaResultRequestItem.cs, ParticipantPublicId is a required (non-null)
// string there, and Community has no real public ID to supply (it resolves
// via null + Kind elsewhere, e.g. ApplyVeto, which this endpoint has no
// equivalent path for). Filtering on .name rather than a numeric
// ParticipantKind value — same reasoning as the DraftStatus fix earlier:
// no confirmed enum values for ParticipantKind, and .name is far less
// likely to be silently wrong. Teams keep a row; they have a real public ID.
export function SeedTriviaStep({ draft, participants, accessToken, onDone }: Props) {
  const eligibleParticipants = participants.filter(
    (p) => p.participantKindValue.name !== "Community" && p.participantPublicId != null
  );

  const [rows, setRows] = useState<TriviaRow[]>(() =>
    eligibleParticipants.map((p) => ({
      participantIdValue: p.participantIdValue,
      displayName: p.displayName ?? p.participantIdValue,
      position: "",
      questionsWon: "",
    }))
  );
  const [hydrated, setHydrated] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Hydrates from /gameplay same as SeedPicksStep — if trivia was already
  // entered in a prior session, it shows up pre-filled rather than blank.
  useEffect(() => {
    (async () => {
      const gameplay = await getDraftPartGameplay(accessToken, draft.draftPartPublicId);
      if (gameplay && gameplay.triviaResults.length > 0) {
        setRows((prev) =>
          prev.map((row) => {
            const existing = gameplay.triviaResults.find(
              (t) => t.participantId === row.participantIdValue
            );
            return existing
              ? { ...row, position: existing.position, questionsWon: existing.questionsWon }
              : row;
          })
        );
      }
      setHydrated(true);
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [accessToken, draft.draftPartPublicId]);

  function updateRow(idx: number, field: "position" | "questionsWon", value: number | "") {
    setRows((prev) => prev.map((r, i) => (i === idx ? { ...r, [field]: value } : r)));
  }

  const filledPositions = rows.map((r) => r.position).filter((p) => p !== "");
  const hasDuplicatePositions = new Set(filledPositions).size !== filledPositions.length;
  const allFilled = rows.every((r) => r.position !== "" && r.questionsWon !== "");

  async function handleSubmit() {
    if (!allFilled || hasDuplicatePositions || submitting) return;
    setSubmitting(true);
    setError(null);
    try {
      const byIdValue = new Map(eligibleParticipants.map((p) => [p.participantIdValue, p]));
      await assignTriviaResults(accessToken, {
        draftPartId: draft.draftPartPublicId,
        results: rows.flatMap((r) => {
          const participant = byIdValue.get(r.participantIdValue);
          if (!participant?.participantPublicId || participant.participantKindValue.value == null) {
            return [];
          }
          return [
            {
              participantPublicId: participant.participantPublicId,
              kind: participant.participantKindValue.value,
              position: Number(r.position),
              questionsWon: Number(r.questionsWon),
            },
          ];
        }),
      });
      onDone();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save trivia results.");
    } finally {
      setSubmitting(false);
    }
  }

  if (!hydrated) {
    return <p className="text-sm text-sd-ink/50 font-mono">Loading…</p>;
  }

  if (rows.length === 0) {
    return (
      <div className="bg-white border border-sd-ink/10 rounded p-8 max-w-md text-center space-y-4">
        <p className="text-sm text-sd-ink/60">No participants on this part to assign trivia to.</p>
        <button type="button" onClick={onDone} className={BTN_PRIMARY}>
          Continue →
        </button>
      </div>
    );
  }

  return (
    <div className="bg-white border border-sd-ink/10 rounded p-8 max-w-md space-y-6">
      <p className="text-sm text-sd-ink/60">
        Position is finishing place — 1 for whoever won trivia. Two participants
        can&apos;t share a position.
      </p>

      <div className="space-y-4">
        {rows.map((row, idx) => (
          <div key={row.participantIdValue} className="grid grid-cols-[1fr_auto_auto] gap-3 items-end">
            <div className="text-sm font-medium text-sd-ink pb-2">{row.displayName}</div>
            <div>
              <label className={LABEL}>Position</label>
              <input
                type="number"
                min={1}
                className={`${INPUT} w-20`}
                value={row.position}
                onChange={(e) =>
                  updateRow(idx, "position", e.target.value === "" ? "" : parseInt(e.target.value, 10))
                }
              />
            </div>
            <div>
              <label className={LABEL}>Questions Won</label>
              <input
                type="number"
                min={0}
                className={`${INPUT} w-24`}
                value={row.questionsWon}
                onChange={(e) =>
                  updateRow(idx, "questionsWon", e.target.value === "" ? "" : parseInt(e.target.value, 10))
                }
              />
            </div>
          </div>
        ))}
      </div>

      {hasDuplicatePositions && (
        <p className="text-[11px] font-mono text-sd-red">
          Two participants can&apos;t share the same finishing position.
        </p>
      )}

      {error && (
        <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
          {error}
        </div>
      )}

      <div className="flex items-center gap-4">
        <button
          type="button"
          onClick={handleSubmit}
          disabled={!allFilled || hasDuplicatePositions || submitting}
          className={BTN_PRIMARY}
        >
          {submitting ? "Saving…" : "Save & Continue →"}
        </button>
        <button
          type="button"
          onClick={onDone}
          className="text-[11px] font-mono text-sd-ink/50 uppercase tracking-widest hover:underline"
        >
          Skip — no trivia recorded
        </button>
      </div>
    </div>
  );
}