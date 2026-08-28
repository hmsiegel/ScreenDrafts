// app/admin/drafts/[draftId]/seed/seed-positions-step.tsx
"use client";

import { useEffect, useState } from "react";
import {
  getDraftPartGameplay,
  assignParticipantToDraftPosition,
  clearDraftPositionAssignment,
  type GameplayDraftPosition,
  type GameplayParticipant,
} from "@/services/admin/fetch-admin-drafts";
import type { SeedDraftState } from "./seed-draft-wizard";

const BTN_PRIMARY =
  "bg-sd-red text-white font-oswald font-medium tracking-wide uppercase px-5 py-2.5 hover:bg-sd-red/90 disabled:opacity-50 transition-colors";
const SELECT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm rounded min-w-[200px] focus:outline-none focus:ring-2 focus:ring-sd-blue";

interface Props {
  draft: SeedDraftState;
  accessToken: string;
  onDone: () => void;
}

// Assigns each drafter to their board position via the same live-gameplay
// command primary-host-tab.tsx's DraftPositionsForm uses during an actual
// draft (PUT .../positions/{positionPublicId}/participant). This is the only
// thing that grants a position's bonus veto/override/fungible token — the
// seeding wizard previously went straight from Trivia to Picks, so positions
// were never assigned and those bonuses never fired. No seed-only command
// here; this calls the real gameplay endpoint, immediate-action per
// selection (matching the host tab, not batched).
export function SeedPositionsStep({ draft, accessToken, onDone }: Props) {
  const [positions, setPositions] = useState<GameplayDraftPosition[]>([]);
  const [participants, setParticipants] = useState<GameplayParticipant[]>([]);
  const [hydrated, setHydrated] = useState(false);
  const [saving, setSaving] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    const gameplay = await getDraftPartGameplay(accessToken, draft.draftPartPublicId);
    setPositions((gameplay?.draftPositions ?? []).filter((p) => !p.isCommunityPosition));
    setParticipants(gameplay?.participants ?? []);
    setHydrated(true);
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [accessToken, draft.draftPartPublicId]);

  async function handleAssign(positionPublicId: string, participantPublicId: string) {
    if (!participantPublicId || saving) return;
    const participant = participants.find((p) => p.participantPublicId === participantPublicId);
    if (!participant?.participantPublicId) return;

    setSaving(positionPublicId);
    setError(null);
    try {
      await assignParticipantToDraftPosition(
        accessToken,
        draft.draftPartPublicId,
        positionPublicId,
        participant.participantPublicId,
        participant.participantKind
      );
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to assign position.");
    } finally {
      setSaving(null);
    }
  }

  async function handleClear(positionPublicId: string) {
    if (saving) return;
    setSaving(positionPublicId);
    setError(null);
    try {
      await clearDraftPositionAssignment(accessToken, draft.draftPartPublicId, positionPublicId);
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to clear position.");
    } finally {
      setSaving(null);
    }
  }

  if (!hydrated) {
    return <p className="text-sm text-sd-ink/50 font-mono">Loading…</p>;
  }

  if (positions.length === 0) {
    return (
      <div className="bg-white border border-sd-ink/10 rounded p-8 max-w-md text-center space-y-4">
        <p className="text-sm text-sd-ink/60">
          No positions to assign on this part — Set Draft Positions wasn&apos;t run, or
          this part has none.
        </p>
        <button type="button" onClick={onDone} className={BTN_PRIMARY}>
          Continue →
        </button>
      </div>
    );
  }

  const allAssigned = positions.every((p) => p.assignedParticipantId !== null);

  return (
    <div className="bg-white border border-sd-ink/10 rounded p-8 max-w-lg space-y-6">
      <p className="text-sm text-sd-ink/60">
        Who plays each position — this is what actually grants a position&apos;s bonus
        veto, override, or fungible token, so it has to happen even when seeding.
      </p>

      <div className="space-y-3">
        {positions.map((pos) => (
          <div key={pos.positionPublicId} className="flex items-center gap-4">
            <div className="w-28 shrink-0">
              <span className="font-oswald text-sd-red font-bold">{pos.positionName}</span>
              <span className="block text-[11px] text-sd-ink/40 font-mono">
                Picks: {[...pos.ownedBoardSlots].sort((a, b) => b - a).join(", ")}
              </span>
              {pos.hasBonusVeto && (
                <span className="text-[10px] text-sd-blue font-mono">+1 veto</span>
              )}
              {pos.hasBonusVetoOverride && (
                <span className="text-[10px] text-sd-blue font-mono ml-1">+1 override</span>
              )}
              {pos.hasBonusFungibleToken && (
                <span className="text-[10px] text-sd-blue font-mono ml-1">+1 token</span>
              )}
            </div>

            {pos.assignedParticipantId ? (
              <div className="flex items-center gap-3">
                <span className="text-sm text-sd-ink font-medium">
                  {pos.assignedParticipantName}
                </span>
                <button
                  type="button"
                  onClick={() => handleClear(pos.positionPublicId)}
                  disabled={saving === pos.positionPublicId}
                  className="text-[11px] font-mono text-sd-ink/40 hover:text-sd-red"
                >
                  {saving === pos.positionPublicId ? "…" : "clear"}
                </button>
              </div>
            ) : (
              <select
                defaultValue=""
                onChange={(e) => e.target.value && handleAssign(pos.positionPublicId, e.target.value)}
                disabled={saving === pos.positionPublicId}
                className={SELECT}
              >
                <option value="" disabled>
                  Assign participant…
                </option>
                {participants
                  .filter(
                    (p) =>
                      p.participantPublicId &&
                      !positions.some(
                        (other) =>
                          other.positionPublicId !== pos.positionPublicId &&
                          other.assignedParticipantId === p.participantPublicId
                      )
                  )
                  .map((p) => (
                    <option key={p.participantPublicId} value={p.participantPublicId!}>
                      {p.participantName}
                    </option>
                  ))}
              </select>
            )}
          </div>
        ))}
      </div>

      {error && (
        <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
          {error}
        </div>
      )}

      <div className="flex items-center gap-4">
        <button
          type="button"
          onClick={onDone}
          disabled={!allAssigned}
          className={BTN_PRIMARY}
        >
          Continue →
        </button>
        {!allAssigned && (
          <span className="text-[11px] font-mono text-sd-ink/40">
            Assign every position to continue.
          </span>
        )}
      </div>
    </div>
  );
}