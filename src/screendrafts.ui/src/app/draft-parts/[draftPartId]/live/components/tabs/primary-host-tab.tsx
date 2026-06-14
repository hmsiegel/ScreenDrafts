// app/draft-parts/[draftPartId]/live/components/tabs/primary-host-tab.tsx
'use client';

import { useState } from 'react';
import { useLiveDraft } from '../../live-draft-context';
import { DraftBoard } from '../draft-board';
import { DraftPickList } from '../draft-pick-list';
import { submitTriviaResults, completeDraftPart } from '../../gameplay-fetchers';

interface Props {
  accessToken: string;
  draftPartId: string;
  userPublicId: string | null;
}

export function PrimaryHostTab({ accessToken, draftPartId }: Props) {
  const { gameplay, draftPositions, picks } = useLiveDraft();

  const allPositionsAssigned = draftPositions.every(
    (p) => p.assignedParticipantId !== null,
  );
  const triviaComplete = (gameplay.triviaResults?.length ?? 0) > 0;
  const draftStarted = allPositionsAssigned && picks.length > 0;

  // If all positions assigned and we have picks — skip to gameplay view
  if (allPositionsAssigned && draftStarted) {
    return <GameplayView accessToken={accessToken} draftPartId={draftPartId} />;
  }

  return (
    <div className="max-w-2xl space-y-8">
      {/* Phase 1a — Trivia Results */}
      <TriviaResultsForm
        accessToken={accessToken}
        draftPartId={draftPartId}
        complete={triviaComplete}
      />

      {/* Phase 1b — Draft Positions (revealed after trivia submitted) */}
      {triviaComplete && (
        <DraftPositionsForm
          accessToken={accessToken}
          draftPartId={draftPartId}
          allAssigned={allPositionsAssigned}
        />
      )}

      {/* Start Draft button — visible once all positions assigned */}
      {allPositionsAssigned && (
        <button
          onClick={() => {/* UI-only transition — page re-renders via picks.length check */}}
          className="px-6 py-3 bg-sd-red text-white font-oswald tracking-widest text-sm hover:bg-sd-red/80 transition-colors"
        >
          START DRAFT
        </button>
      )}
    </div>
  );
}

// ── Trivia Results Form ───────────────────────────────────────────────────────

function TriviaResultsForm({
  accessToken,
  draftPartId,
  complete,
}: {
  accessToken: string;
  draftPartId: string;
  complete: boolean;
}) {
  const { gameplay, refetch } = useLiveDraft();
  const [scores, setScores] = useState<Record<string, { position: number; questionsWon: number }>>(
    () =>
      Object.fromEntries(
        (gameplay.participants ?? []).map((p, i) => [
          p.participantId,
          {
            position: gameplay.triviaResults?.find((t) => t.participantId === p.participantId)?.position ?? i + 1,
            questionsWon: gameplay.triviaResults?.find((t) => t.participantId === p.participantId)?.questionsWon ?? 0,
          },
        ]),
      ),
  );
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit() {
    setSaving(true);
    setError(null);
    try {
      await submitTriviaResults(
        accessToken,
        draftPartId,
        (gameplay.participants ?? []).map((p) => ({
          participantPublicId: p.participantId ?? '',
          participantKind: p.participantKind ?? 0,
          position: scores[p.participantId ?? '']?.position ?? 1,
          questionsWon: scores[p.participantId ?? '']?.questionsWon ?? 0,
        })),
      );
      await refetch();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to save trivia results.');
    } finally {
      setSaving(false);
    }
  }

  return (
    <section>
      <h2 className="font-oswald text-lg tracking-widest text-sd-paper uppercase mb-4">
        Trivia Results
      </h2>

      {complete ? (
        <div className="space-y-1">
          {gameplay.triviaResults?.
            sort((a, b) => (a.position ?? 0) - (b.position ?? 0))
            .map((t) => (
              <div key={t.participantId} className="flex gap-4 text-sm text-white/60 font-mono">
                <span className="text-sd-red font-bold w-4">{t.position}</span>
                <span>{t.participantName}</span>
                <span className="text-white/30">— {t.questionsWon} correct</span>
              </div>
            ))}
          <p className="text-xs text-white/30 mt-2 italic">Trivia results recorded.</p>
        </div>
      ) : (
        <div className="space-y-3">
          {gameplay.participants?.map((p) => (
            <div key={p.participantId} className="flex items-center gap-4">
              <span className="font-oswald text-sm text-sd-paper w-40 truncate">
                {p.participantName}
              </span>
              <label className="text-xs text-white/40 font-mono">
                Position
                <input
                  type="number"
                  min={1}
                  max={gameplay.participants?.length}
                  value={p.participantId ? scores[p.participantId]?.position ?? 1 : 1}
                  onChange={(e) => {
                    if (p.participantId) {
                      setScores((prev) => ({
                        ...prev,
                        [p.participantId!]: {
                          ...prev[p.participantId!],
                          position: parseInt(e.target.value, 10) || 1,
                        },
                      }));
                    }
                  }}
                  className="ml-2 w-16 bg-white/10 border border-white/20 text-sd-paper px-2 py-1 text-sm font-mono"
                />
              </label>
              <label className="text-xs text-white/40 font-mono">
                Questions Won
                <input
                  type="number"
                  min={0}
                  value={p.participantId ? scores[p.participantId]?.questionsWon ?? 0 : 0}
                  onChange={(e) => {
                    if (p.participantId) {
                      setScores((prev) => ({
                        ...prev,
                        [p.participantId!]: {
                          ...prev[p.participantId!],
                          questionsWon: parseInt(e.target.value, 10) || 0,
                        },
                      }));
                    }
                  }}
                  className="ml-2 w-16 bg-white/10 border border-white/20 text-sd-paper px-2 py-1 text-sm font-mono"
                />
              </label>
            </div>
          ))}

          {error && <p className="text-sd-red text-xs font-mono">{error}</p>}

          <button
            onClick={handleSubmit}
            disabled={saving}
            className="px-5 py-2 bg-sd-blue text-white font-oswald text-sm tracking-widest hover:bg-sd-blue/80 disabled:opacity-50 transition-colors"
          >
            {saving ? 'SAVING…' : 'SUBMIT TRIVIA RESULTS'}
          </button>
        </div>
      )}
    </section>
  );
}

// ── Draft Positions Form ──────────────────────────────────────────────────────

function DraftPositionsForm({
  accessToken,
  draftPartId,
  allAssigned,
}: {
  accessToken: string;
  draftPartId: string;
  allAssigned: boolean;
}) {
  const { gameplay, draftPositions, refetch } = useLiveDraft();
  const [saving, setSaving] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function handleAssign(positionPublicId: string, participantId: string) {
    if (!participantId) return;
    const participant = gameplay.participants?.find((p) => p.participantId === participantId);
    if (!participant) return;

    setSaving(positionPublicId);
    setError(null);
    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/draft-parts/${draftPartId}/positions/${positionPublicId}/assign`,
        {
          method: 'PUT',
          headers: {
            Authorization: `Bearer ${accessToken}`,
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            participantPublicId: participantId,
            participantKind: participant.participantKind,
          }),
        },
      );
      if (!res.ok) throw new Error(`Assign failed: ${res.status}`);
      await refetch();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to assign position.');
    } finally {
      setSaving(null);
    }
  }

  async function handleClear(positionPublicId: string) {
    setSaving(positionPublicId);
    setError(null);
    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/draft-parts/${draftPartId}/positions/${positionPublicId}/assign`,
        {
          method: 'DELETE',
          headers: { Authorization: `Bearer ${accessToken}` },
        },
      );
      if (!res.ok) throw new Error(`Clear failed: ${res.status}`);
      await refetch();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to clear position.');
    } finally {
      setSaving(null);
    }
  }

  return (
    <section>
      <h2 className="font-oswald text-lg tracking-widest text-sd-paper uppercase mb-4">
        Draft Positions
      </h2>

      <div className="space-y-3">
        {draftPositions.map((pos) => (
          <div key={pos.positionPublicId} className="flex items-center gap-4">
            <div className="w-28 shrink-0">
              <span className="font-oswald text-sd-red font-bold">{pos.positionName}</span>
              <span className="block text-[11px] text-white/30 font-mono">
                Picks: {(pos.ownedBoardSlots ?? []).sort((a, b) => b - a).join(', ')}
              </span>
              {pos.hasBonusVeto && (
                <span className="text-[10px] text-light-blue font-mono">+1 veto</span>
              )}
              {pos.hasBonusVetoOverride && (
                <span className="text-[10px] text-light-blue font-mono ml-1">+1 override</span>
              )}
            </div>

            {pos.assignedParticipantId ? (
              <div className="flex items-center gap-3">
                <span className="font-oswald text-sd-paper text-sm">
                  {pos.assignedParticipantName}
                </span>
                <button
                  onClick={() => pos.positionPublicId && handleClear(pos.positionPublicId)}
                  disabled={saving === pos.positionPublicId}
                  className="text-xs text-white/30 hover:text-sd-red font-mono transition-colors"
                >
                  {saving === pos.positionPublicId ? '…' : 'clear'}
                </button>
              </div>
            ) : (
              <select
                defaultValue=""
                onChange={(e) => pos.positionPublicId && e.target.value && handleAssign(pos.positionPublicId, e.target.value)}
                disabled={saving === pos.positionPublicId}
                className="bg-white/10 border border-white/20 text-sd-paper text-sm font-oswald px-3 py-1.5 min-w-[180px]"
              >
                <option value="" disabled>
                  Assign participant…
                </option>
                {(gameplay.participants ?? []).map((p) => (
                  <option key={p.participantId} value={p.participantId}>
                    {p.participantName}
                  </option>
                ))}
              </select>
            )}
          </div>
        ))}
      </div>

      {error && <p className="text-sd-red text-xs font-mono mt-2">{error}</p>}

      {allAssigned && (
        <p className="text-xs text-light-blue font-mono mt-3">
          All positions assigned. Ready to start.
        </p>
      )}
    </section>
  );
}

// ── Gameplay View (Phase 2) ───────────────────────────────────────────────────

function GameplayView({
  accessToken,
  draftPartId,
}: {
  accessToken: string;
  draftPartId: string;
}) {
  const { gameplay, draftPositions, picks, refetch } = useLiveDraft();
  const [completing, setCompleting] = useState(false);
  const [confirmComplete, setConfirmComplete] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Count total expected picks from all position slots
  const totalSlots = draftPositions.flatMap((p) => p.ownedBoardSlots).length;
  const landedPicks = picks.filter((p) => !p.wasVetoed || p.wasVetoOverridden).length;
  const draftComplete = landedPicks === totalSlots;

  async function handleComplete() {
    setCompleting(true);
    setError(null);
    try {
      await completeDraftPart(accessToken, draftPartId);
      await refetch();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to complete draft part.');
    } finally {
      setCompleting(false);
      setConfirmComplete(false);
    }
  }

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
      {/* Left — Draft Board */}
      <div>
        <h2 className="font-oswald text-sm tracking-widest text-white/50 uppercase mb-3">
          Draft Board
        </h2>
        <DraftBoard />

        {/* Position summary */}
        <div className="mt-4 flex flex-wrap gap-4">
          {draftPositions.map((pos) => (
            <div key={pos.positionPublicId} className="text-xs font-mono text-white/50">
              <span className="text-sd-red font-bold">{pos.positionName}</span>{' '}
              {pos.assignedParticipantName ?? '—'}
            </div>
          ))}
        </div>

        {/* Complete button */}
        <div className="mt-6">
          {error && <p className="text-sd-red text-xs font-mono mb-2">{error}</p>}
          {confirmComplete ? (
            <div className="flex gap-3">
              <button
                onClick={handleComplete}
                disabled={completing}
                className="px-5 py-2 bg-sd-red text-white font-oswald text-sm tracking-wider hover:bg-sd-red/80 disabled:opacity-50"
              >
                {completing ? 'COMPLETING…' : 'CONFIRM'}
              </button>
              <button
                onClick={() => setConfirmComplete(false)}
                className="px-5 py-2 border border-white/20 text-white/60 font-oswald text-sm tracking-wider"
              >
                CANCEL
              </button>
            </div>
          ) : (
            <button
              onClick={() => setConfirmComplete(true)}
              disabled={!draftComplete}
              className="px-5 py-2 bg-sd-blue text-white font-oswald text-sm tracking-widest hover:bg-sd-blue/80 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
            >
              {gameplay.isFinalPart ? 'COMPLETE DRAFT' : 'COMPLETE PART'}
            </button>
          )}
          {!draftComplete && (
            <p className="text-xs text-white/30 font-mono mt-1">
              {totalSlots - landedPicks} pick{totalSlots - landedPicks !== 1 ? 's' : ''} remaining
            </p>
          )}
        </div>

        {/* Zoom placeholder */}
        <div className="mt-6 h-32 border border-dashed border-white/10 flex items-center justify-center">
          <span className="text-white/20 text-xs font-mono">Zoom — coming soon</span>
        </div>
      </div>

      {/* Right — Pick list */}
      <div>
        <h2 className="font-oswald text-sm tracking-widest text-white/50 uppercase mb-3">
          Pick List
        </h2>
        <DraftPickList />
      </div>
    </div>
  );
}