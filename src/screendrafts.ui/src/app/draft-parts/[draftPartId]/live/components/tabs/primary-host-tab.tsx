// app/draft-parts/[draftPartId]/live/components/tabs/primary-host-tab.tsx
'use client';

import { useState } from 'react';
import { resolveParticipantName, useLiveDraft } from '../../live-draft-context';
import { DraftBoard } from '../draft-board';
import { DraftPickList } from '../draft-pick-list';
import { submitTriviaResults } from '../../gameplay-fetchers';
import { completeDraftPart } from '@/services/drafts/fetch-my-drafts';
import * as signalR from '@microsoft/signalr';

interface Props {
  accessToken: string;
  draftPartId: string;
  isCommissioner: boolean;
}

export function PrimaryHostTab({ accessToken, draftPartId, isCommissioner: _isCommissioner }: Props) {
  const { gameplay, draftPositions } = useLiveDraft();
  const allPositionsAssigned = draftPositions.every((p) => p.assignedParticipantId !== null);
  const triviaComplete = (gameplay.triviaResults?.length ?? 0) > 0;

  if (allPositionsAssigned && triviaComplete) {
    return <GameplayView accessToken={accessToken} draftPartId={draftPartId} />;
  }

  return (
    <div className="max-w-2xl space-y-8">
      <TriviaResultsForm
        accessToken={accessToken}
        draftPartId={draftPartId}
        complete={triviaComplete}
      />

      {triviaComplete && (
        <DraftPositionsForm
          accessToken={accessToken}
          draftPartId={draftPartId}
          allAssigned={allPositionsAssigned}
        />
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
            position:
              gameplay.triviaResults?.find((t) => t.participantId === p.participantId)?.position ??
              i + 1,
            questionsWon:
              gameplay.triviaResults?.find((t) => t.participantId === p.participantId)
                ?.questionsWon ?? 0,
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
          participantPublicId: p.participantPublicId ?? '',
          kind: p.participantKind ?? 0,
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
          {gameplay.triviaResults
            ?.sort((a, b) => (a.position ?? 0) - (b.position ?? 0))
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
                  value={p.participantId ? (scores[p.participantId]?.position ?? 1) : 1}
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
                  value={p.participantId ? (scores[p.participantId]?.questionsWon ?? 0) : 0}
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
    const participant = gameplay.participants?.find((p) => p.participantPublicId === participantId);
    if (!participant) return;

    setSaving(positionPublicId);
    setError(null);
    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/draft-parts/${draftPartId}/positions/${positionPublicId}/participant`,
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
        `${process.env.NEXT_PUBLIC_API_URL}/draft-parts/${draftPartId}/positions/${positionPublicId}/participant`,
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
        {draftPositions
          .filter((pos) => !pos.isCommunitPosition)
          .map((pos) => (
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
                  onChange={(e) =>
                    pos.positionPublicId && e.target.value &&
                    handleAssign(pos.positionPublicId, e.target.value)
                  }
                  disabled={saving === pos.positionPublicId}
                  className="bg-white/10 border border-white/20 text-sd-paper text-sm font-oswald px-3 py-1.5 min-w-[180px] [&>option]:text-sd-ink [&>option]:bg-white"
                >
                  <option value="" disabled>
                    Assign participant…
                  </option>
                  {(gameplay.participants ?? [])
                    .filter((p) =>
                      // Show only unassigned participants (those not assigned to any position)
                      !draftPositions.some(
                        (otherPos) =>
                          otherPos.positionPublicId !== pos.positionPublicId &&
                          otherPos.assignedParticipantId?.toString() === p.participantId?.toString()
                      )
                    )
                    .map((p) => (
                      <option key={p.participantPublicId} value={p.participantPublicId}>
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
  const { gameplay, draftPositions, picks, pendingPicks, refetch, revealPick, sendCountdown, nextExpectedParticipantId, connectionState } = useLiveDraft();
  const [completing, setCompleting] = useState(false);
  const [confirmComplete, setConfirmComplete] = useState(false);
  const [acting, setActing] = useState<string | null>(null); // action key in flight
  const [error, setError] = useState<string | null>(null);
  const [counting, setCounting] = useState(false);

  const totalSlots = draftPositions.flatMap((p) => p.ownedBoardSlots).length;
  const landedPicks = picks.filter((p) => !p.wasCommissionerOverride && (!p.wasVetoed || p.wasVetoOverridden)).length;
  const draftComplete = landedPicks === totalSlots;

  const mostRecentPick = picks.reduce<(typeof picks)[0] | null>(
    (acc, p) => (!acc || (p.playOrder ?? 0) > (acc.playOrder ?? 0) ? p : acc),
    null,
  );

  const communityParticipantId = 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee';
  const isCommunityTurn = nextExpectedParticipantId === communityParticipantId;


  const communityPickRule = isCommunityTurn
    ? gameplay.communityFilmRules?.find(
      (r) =>
        r.ruleKind === 1 && // BoostersPick
        r.targetSlot != null &&
        !picks.some((p) => p.boardPosition === r.targetSlot && !p.wasVetoed)
    )
    : null;

  async function handleCountdown() {
    if (!nextExpectedParticipantId || counting) return;
    setCounting(true);
    await sendCountdown(nextExpectedParticipantId);
    setTimeout(() => setCounting(false), 7000);
  }

  async function handleComplete() {
    setCompleting(true);
    setError(null);
    try {
      await completeDraftPart(accessToken, gameplay.draftId ?? "", gameplay.partIndex ?? 0);
      await refetch();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to complete draft part.');
    } finally {
      setCompleting(false);
      setConfirmComplete(false);
    }
  }

  async function handleUndoPick(playOrder: number) {
    setActing('undo-pick');
    setError(null);
    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/draft-parts/${draftPartId}/picks/${playOrder}`,
        { method: 'DELETE', headers: { Authorization: `Bearer ${accessToken}` } },
      );
      if (!res.ok) throw new Error(`Undo pick failed: ${res.status}`);
      await refetch();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to undo pick.');
    } finally {
      setActing(null);
    }
  }

  async function handleUndoVeto(playOrder: number) {
    setActing('undo-veto');
    setError(null);
    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/draft-parts/${draftPartId}/picks/${playOrder}/undo-veto`,
        { method: 'POST', headers: { Authorization: `Bearer ${accessToken}` } },
      );
      if (!res.ok) throw new Error(`Undo veto failed: ${res.status}`);
      await refetch();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to undo veto.');
    } finally {
      setActing(null);
    }
  }

  async function handleCommissionerOverride(playOrder: number) {
    setActing('commissioner-override');
    setError(null);
    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/draft-parts/${draftPartId}/commissioner-override/${playOrder}`,
        {
          method: 'POST',
          headers: {
            Authorization: `Bearer ${accessToken}`,
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({}),
        },
      );
      if (!res.ok) throw new Error(`Commissioner override failed: ${res.status}`);
      await refetch();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to apply commissioner override.');
    } finally {
      setActing(null);
    }
  }

  async function handlePlayCommunityPick(rule: { tmdbId?: number; targetSlot?: number }) {
    if (!rule.tmdbId || !rule.targetSlot) return;
    setActing('community-pick');
    setError(null);
    try {
      // PlayOrder = next available play order
      const nextPlayOrder = (picks.reduce((max, p) => Math.max(max, p.playOrder ?? 0), 0)) + 1;

      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/draft-parts/${draftPartId}/picks`,
        {
          method: 'POST',
          headers: {
            Authorization: `Bearer ${accessToken}`,
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            tmdbId: rule.tmdbId,
            position: rule.targetSlot,
            playOrder: nextPlayOrder,
            participantPublicId: null,
            participantKind: 2,       // Community
          }),
        },
      );
      if (!res.ok) {
        const text = await res.text().catch(() => String(res.status));
        setError(`Community pick failed: ${text}`);
      }
      await refetch();
    } finally {
      setActing(null);
    }
  }

  // Primary host inline actions on the most recent pick: undo the pick
  // entirely, undo a veto (if the most recent pick was vetoed), or apply
  // a commissioner override (remove without counting as veto or save).
  function renderBoardActions(pick: import('@/lib/dto').GameplayPickResponse) {
    if (pick.playOrder !== mostRecentPick?.playOrder) return null;
    const playOrder = pick.playOrder!;
    const busy = acting !== null;

    return (
      <div className="flex gap-2">
        {pick.wasVetoed && !pick.wasVetoOverridden && (
          <button
            onClick={() => handleUndoVeto(playOrder)}
            disabled={busy}
            className="px-3 py-1 border border-light-blue text-light-blue font-oswald text-xs tracking-widest hover:bg-light-blue hover:text-sd-ink disabled:opacity-40 transition-colors"
          >
            {acting === 'undo-veto' ? '…' : 'UNDO VETO'}
          </button>
        )}
        <button
          onClick={() => handleCommissionerOverride(playOrder)}
          disabled={busy}
          className="px-3 py-1 border border-white/30 text-white/70 font-oswald text-xs tracking-widest hover:border-white hover:text-white disabled:opacity-40 transition-colors"
        >
          {acting === 'commissioner-override' ? '…' : 'OVERRIDE'}
        </button>
        <button
          onClick={() => handleUndoPick(playOrder)}
          disabled={busy}
          className="px-3 py-1 border border-sd-red text-sd-red font-oswald text-xs tracking-widest hover:bg-sd-red hover:text-white disabled:opacity-40 transition-colors"
        >
          {acting === 'undo-pick' ? '…' : 'UNDO'}
        </button>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
      <div>
        {/* On the clock — shown only while waiting for the next pick. A submitted
            drafter pick (→ pending/ANNOUNCE banner) or a community turn (→ Community
            Pick banner) replaces it. */}
        {!isCommunityTurn &&
          !pendingPicks.some((pp) => pp.participantKind !== 2) &&
          nextExpectedParticipantId && (
            <div className="mb-4 px-4 py-3 bg-white/5 border border-white/10">
              <p className="font-oswald text-white text-sm">
                <span className="text-white/50 text-xs tracking-widest uppercase mr-2">
                  On the clock
                </span>
                <span className="font-bold">
                  {resolveParticipantName(gameplay.participants ?? [], nextExpectedParticipantId)}
                </span>
              </p>
            </div>
          )}
        {isCommunityTurn && communityPickRule && (
          <div className="mb-4 flex items-center justify-between gap-4 px-4 py-3 bg-sd-red/20 border border-sd-red/40">
            <div className="min-w-0">
              <p className="font-oswald text-white text-xs tracking-widest uppercase mb-1">
                Community Pick
              </p>
              <p className="font-oswald text-white text-sm">
                <span className="text-sd-red font-bold">
                  At No. {communityPickRule.targetSlot}
                </span>
                {', the Patreon community selects '}
                <span className="font-bold">
                  {communityPickRule.title ?? `TMDb #${communityPickRule.tmdbId}`}
                </span>
              </p>
            </div>
            <button
              onClick={() => handlePlayCommunityPick(communityPickRule)}
              className="shrink-0 px-4 py-2 bg-sd-red text-white font-oswald text-xs tracking-widest hover:bg-sd-red/80 transition-colors"
            >
              ANNOUNCE
            </button>
          </div>
        )}

        {pendingPicks.some((pp) => pp.participantKind !== 2) && (
          <div className="mb-4 space-y-2">
            {pendingPicks
              .filter((pp) => pp.participantKind !== 1)
              .map((pp) => {
                const participantName =
                  gameplay.participants?.find((p) => p.participantId === pp.participantId)
                    ?.participantName ?? pp.participantId;

                return (
                  <div
                    key={pp.playOrder}
                    className="flex items-center justify-between gap-4 px-4 py-3 bg-sd-blue/20 border border-sd-blue/40"
                  >
                    <p className="font-oswald text-sd-paper text-sm min-w-0">
                      <span className="text-sd-red font-bold">At No. {pp.boardPosition}</span>
                      {', '}
                      <span className="text-white/70">{participantName}</span>
                      {' selects '}
                      <a
                        href={`https://www.themoviedb.org/movie/${pp.tmdbId}`}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="font-bold hover:underline underline-offset-2"
                      >
                        {pp.movieTitle}
                      </a>
                    </p>
                    <button
                      onClick={() => revealPick(pp.playOrder)}
                      className="shrink-0 px-4 py-2 bg-sd-blue text-white font-oswald text-xs tracking-widest hover:bg-sd-blue/80 transition-colors"
                    >
                      ANNOUNCE
                    </button>
                  </div>
                );
              })}
          </div>
        )}

        <h2 className="font-oswald text-sm tracking-widest text-white/50 uppercase mb-3">
          Draft Board
        </h2>
        <DraftBoard renderActions={renderBoardActions} />

        {error && <p className="text-sd-red text-xs font-mono mt-2">{error}</p>}

        <div className="mt-4 space-y-1">
          {draftPositions
            .filter((pos) => !pos.isCommunityPosition)
            .map((pos) => (
              <div key={pos.positionPublicId} className="text-xs font-mono text-white/50">
                <span className="text-sd-red font-bold tracking-wider">
                  {pos.positionName}
                </span>
                {pos.assignedParticipantName && (
                  <>
                    <span className="text-white/70 ml-2">{pos.assignedParticipantName}</span>
                    <span className="text-white/30 ml-2">
                      {(pos.ownedBoardSlots ?? [])
                        .slice()
                        .sort((a, b) => b - a)
                        .join(', ')}
                    </span>
                  </>
                )}
                {!pos.assignedParticipantName && (
                  <span className="text-white/20 ml-2">unassigned</span>
                )}
              </div>
            ))}
        </div>

        <div className="mt-6 space-y-2">
          <div className="flex items-center gap-3">
            {confirmComplete ? (
              <>
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
              </>
            ) : (
              <button
                onClick={() => setConfirmComplete(true)}
                disabled={!draftComplete}
                className="px-5 py-2 bg-sd-blue text-white font-oswald text-sm tracking-widest hover:bg-sd-blue/80 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
              >
                {gameplay.isFinalPart ? 'COMPLETE DRAFT' : 'COMPLETE PART'}
              </button>
            )}
            <button
              onClick={handleCountdown}
              disabled={
                !nextExpectedParticipantId ||
                counting ||
                connectionState !== signalR.HubConnectionState.Connected
              }
              className="px-4 py-2 border border-sd-red/50 text-sd-red font-oswald text-xs tracking-widest hover:border-sd-red disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
            >
              {counting ? 'COUNTING DOWN…' : 'START COUNTDOWN'}
            </button>
          </div>
          {!draftComplete && (
            <p className="text-xs text-white/30 font-mono">
              {totalSlots - landedPicks} pick{totalSlots - landedPicks !== 1 ? 's' : ''} remaining
            </p>
          )}
        </div>

        <div className="mt-6 h-32 border border-dashed border-white/10 flex items-center justify-center">
          <span className="text-white/20 text-xs font-mono">Zoom — coming soon</span>
        </div>
      </div>

      <div>
        <h2 className="font-oswald text-sm tracking-widest text-white/50 uppercase mb-3">
          Pick List
        </h2>
        <DraftPickList />
      </div>
    </div>
  );
}