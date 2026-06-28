// app/draft-parts/[draftPartId]/live/live-draft-context.tsx
'use client';

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
} from 'react';
import * as signalR from '@microsoft/signalr';
import { fetchGameplay } from './gameplay-fetchers';
import {
  GameplayDraftPositionResponse,
  GameplayParticipantResponse,
  GameplayPickResponse,
  GetDraftPartGameplayResponse,
} from '@/lib/dto';

// ── SignalR payload shapes ────────────────────────────────────────────────────

interface TokenUpdate {
  participantIdValue: string;
  participantKindValue: number;
  vetoTokensRemaining: number;
  overrideTokensRemaining: number;
}

export interface PickAddedPayload {
  draftPartPublicId: string;
  playOrder: number;
  boardPosition: number;
  movieTitle: string;
  tmdbId: number;
  participantId: string;
  participantKind: number;
  participants: TokenUpdate[];
}

// Pick submitted but not yet revealed — host-only, awaiting ANNOUNCE/REVEAL.
export interface PickSubmittedPayload {
  draftPartPublicId: string;
  playOrder: number;
  boardPosition: number;
  moviePublicId: string;
  movieTitle: string;
  tmdbId: number;
  participantId: string;
  participantKind: number;
}

// Pick has been revealed — this is the moment it becomes visible to everyone.
export interface PickRevealedPayload {
  draftPartPublicId: string;
  playOrder: number;
  boardPosition: number;
  movieTitle: string;
  tmdbId: number;
  participantId: string;
  participantKind: number;
  participants: TokenUpdate[];
}

export interface VetoAppliedPayload {
  draftPartPublicId: string;
  playOrder: number;
  movieTitle: string;
  tmdbId: number;
  vetoedByParticipantId: string;
  vetoedByParticipantKind: number;
  playedByParticipantId: string;
  playedByParticipantKind: number;
  participants: TokenUpdate[];
}

export interface VetoOverrideAppliedPayload {
  draftPartPublicId: string;
  playOrder: number;
  movieTitle: string;
  tmdbId: number;
  overriddenByParticipantId: string;
  overriddenByParticipantKind: number;
  participants: TokenUpdate[];
}

export interface CommissionerOverrideAppliedPayload {
  draftPartPublicId: string;
  tmdbId: number;
  movieTitle: string;
  participantId: string;
  participantKind: number;
  boardPosition: number;
  participants: TokenUpdate[];
}

export interface VetoUndonePayload {
  draftPartPublicId: string;
  playOrder: number;
  movieTitle: string;
  tmdbId: number;
  participants: TokenUpdate[];
}

export interface PickUndonePayload {
  draftPartPublicId: string;
  playOrder: number;
  boardPosition: number;
  movieTitle: string;
  tmdbId: number;
  participants: TokenUpdate[];
}

export interface MovieHonorificChangedPayload {
  draftPartPublicId: string;
  moviePublicId: string;
  movieTitle: string;
  previousAppearanceHonorificValue: number;
  newAppearanceHonorificValue: number;
  previousPositionHonorificValue: number;
  newPositionHonorificValue: number;
  appearanceCount: number;
}

export interface DrafterHonorificChangedPayload {
  draftPartPublicId: string;
  drafterIdValue: string;
  previousHonorificValue: number;
  newHonorificValue: number;
  appearanceCount: number;
}

export interface CommunityRuleAppliedPayload {
  draftPartPublicId: string;
  tmdbId: number;
  movieTitle?: string;
  playOrder: number;
  boardPosition: number;
  ruleKind: number; // 0 = BoostersVeto, 1 = BoostersPick
  targetSlot: number;
}

export interface CompletedPickSummary {
  position: number;
  mediaPublicId: string;
  mediaTitle: string;
}

export interface PickHonorificSummary {
  mediaPublicId: string;
  mediaTitle: string;
  boardPosition: number;
  priorCount: number;
  newCount: number;
}

export interface DrafterHonorificSummary {
  drafterIdValue: string;
  priorCount: number;
  newCount: number;
}

export interface DraftCompletionSummary {
  draftPartPublicId: string;
  draftId: string;
  draftPublicId: string;
  title: string;
  draftType: string;
  partIndex: number;
  totalParts: number;
  totalPicks: number;
  vetoCount: number;
  isPatreon: boolean;
  picks: CompletedPickSummary[];
  movieHonorifics: PickHonorificSummary[];
  drafterHonorifics: DrafterHonorificSummary[];
}

export interface CountdownStartedPayload {
  draftPartPublicId: string;
  targetParticipantId: string;
}

// ── Notification queue ────────────────────────────────────────────────────────

export type GameplayNotification =
  | { kind: 'PickAdded'; payload: PickAddedPayload }
  | { kind: 'PickRevealed'; payload: PickRevealedPayload }
  | { kind: 'VetoApplied'; payload: VetoAppliedPayload }
  | { kind: 'VetoOverrideApplied'; payload: VetoOverrideAppliedPayload }
  | { kind: 'CommissionerOverrideApplied'; payload: CommissionerOverrideAppliedPayload }
  | { kind: 'VetoUndone'; payload: VetoUndonePayload }
  | { kind: 'PickUndone'; payload: PickUndonePayload }
  | { kind: 'MovieHonorificChanged'; payload: MovieHonorificChangedPayload }
  | { kind: 'DrafterHonorificChanged'; payload: DrafterHonorificChangedPayload }
  | { kind: 'CommunityRuleApplied'; payload: CommunityRuleAppliedPayload };

// ── Context shape ─────────────────────────────────────────────────────────────

interface LiveDraftContextValue {
  gameplay: GetDraftPartGameplayResponse;
  participants: GameplayParticipantResponse[];
  picks: GameplayPickResponse[];
  // Picks submitted but not yet revealed. Only ever populated for the
  // primary host (only they join the host SignalR group that receives
  // PickSubmitted). Empty array for everyone else.
  pendingPicks: PickSubmittedPayload[];
  draftPositions: GameplayDraftPositionResponse[];
  nextExpectedParticipantId: string | null;
  // The caller's drafter participant ID (GUID string), null if not a participant.
  callerParticipantId: string | null;
  connectionState: signalR.HubConnectionState;
  reconnecting: boolean;
  notification: GameplayNotification | null;
  dismissNotification: () => void;
  countdownTarget: string | null;
  dismissCountdown: () => void;
  refetch: () => Promise<void>;
  sendCountdown: (targetParticipantId: string) => Promise<void>;
  // Primary-host action: announce/reveal a pending pick to everyone.
  revealPick: (playOrder: number) => Promise<void>;
  completionSummary: DraftCompletionSummary | null;
  dismissCompletionSummary: () => void;
}

const LiveDraftContext = createContext<LiveDraftContextValue | null>(null);

export function useLiveDraft() {
  const ctx = useContext(LiveDraftContext);
  if (!ctx) throw new Error('useLiveDraft must be used within LiveDraftProvider');
  return ctx;
}

// ── Provider ──────────────────────────────────────────────────────────────────

interface LiveDraftProviderProps {
  draftPartId: string;
  accessToken: string;
  initialGameplay: GetDraftPartGameplayResponse;
  children: React.ReactNode;
}

export function LiveDraftProvider({
  draftPartId,
  accessToken,
  initialGameplay,
  children,
}: LiveDraftProviderProps) {
  const [gameplay, setGameplay] = useState(initialGameplay);
  const [participants, setParticipants] = useState(initialGameplay.participants ?? []);
  const [picks, setPicks] = useState(initialGameplay.picks ?? []);
  const [pendingPicks, setPendingPicks] = useState<PickSubmittedPayload[]>([]);
  const [draftPositions, setDraftPositions] = useState(initialGameplay.draftPositions ?? []);
  const [nextExpectedParticipantId, setNextExpectedParticipantId] = useState(
    initialGameplay.nextExpectedParticipantId ?? null,
  );
  // callerParticipantId is stable — it's who the user is in this draft part.
  // It does not change on refetch (the caller doesn't switch roles mid-draft).
  const callerParticipantId = initialGameplay.callerParticipantId ?? null;
  // isPrimaryHost is also stable for the duration of this draft part session.
  const isPrimaryHost = initialGameplay.currentUserRoles?.isPrimaryHost ?? false;

  const [connectionState, setConnectionState] = useState<signalR.HubConnectionState>(
    signalR.HubConnectionState.Disconnected,
  );
  const [reconnecting, setReconnecting] = useState(false);
  const [notification, setNotification] = useState<GameplayNotification | null>(null);
  const [countdownTarget, setCountdownTarget] = useState<string | null>(null);
  const [completionSummary, setCompletionSummary] = useState<DraftCompletionSummary | null>(null);
  const notificationQueue = useRef<GameplayNotification[]>([]);
  const participantsRef = useRef(initialGameplay.participants ?? []);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const countdownTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  // Holds the teardown (stop) promise from the PREVIOUS effect invocation.
  // Persists across Strict Mode's dev-only mount → cleanup → remount cycle
  // because refs survive that cycle on the same component instance. The next
  // invocation awaits this before starting a new connection, so the old
  // connection is guaranteed to have actually left the SignalR group before
  // the new one joins — otherwise both connections can be briefly live
  // members of the same group, and every server broadcast (PickSubmitted,
  // token updates, etc.) gets delivered twice to this client.
  const previousTeardownRef = useRef<Promise<void> | null>(null);

  // ── Token update helper ───────────────────────────────────────────────────

  // Keep ref in sync so SignalR handlers can read current participants without stale closure.
  useEffect(() => {
    participantsRef.current = participants;
  }, [participants]);

  const applyTokenUpdates = useCallback((updates: TokenUpdate[]) => {
    setParticipants((prev) =>
      prev.map((p) => {
        const update = updates.find(
          (u) =>
            u.participantIdValue === p.participantId &&
            u.participantKindValue === p.participantKind,
        );
        if (!update) return p;
        return {
          ...p,
          vetoTokensRemaining: update.vetoTokensRemaining,
          overrideTokensRemaining: update.overrideTokensRemaining,
        };
      }),
    );
  }, []);

  // ── Notification queue ────────────────────────────────────────────────────

  const enqueueNotification = useCallback((n: GameplayNotification) => {
    notificationQueue.current.push(n);
    setNotification((current) => current ?? notificationQueue.current.shift() ?? null);
  }, []);

  const dismissNotification = useCallback(() => {
    setNotification(notificationQueue.current.shift() ?? null);
  }, []);

  // ── Refetch ───────────────────────────────────────────────────────────────

  const refetch = useCallback(async () => {
    try {
      const fresh = await fetchGameplay(accessToken, draftPartId);
      setGameplay(fresh);
      setParticipants(fresh.participants ?? []);
      setPicks(fresh.picks ?? []);
      setDraftPositions(fresh.draftPositions ?? []);
      setNextExpectedParticipantId(fresh.nextExpectedParticipantId ?? null);
    } catch {
      // silent — reconnecting banner already visible
    }
  }, [accessToken, draftPartId]);

  // ── Reveal action (primary host only) ────────────────────────────────────

  const revealPick = useCallback(
    async (playOrder: number) => {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/draft-parts/${draftPartId}/picks/${playOrder}/reveal`,
        {
          method: 'POST',
          headers: { Authorization: `Bearer ${accessToken}` },
        },
      );
      if (!res.ok) {
        console.error('Reveal pick failed', res.status);
        return;
      }
      // Optimistically clear from pending — PickRevealed broadcast will add
      // it to `picks` for everyone (including this host) shortly after.
      setPendingPicks((prev) => prev.filter((p) => p.playOrder !== playOrder));
    },
    [accessToken, draftPartId],
  );

  // ── SignalR ───────────────────────────────────────────────────────────────

  useEffect(() => {
    const hubUrl = `${process.env.NEXT_PUBLIC_API_URL}/drafts/hub`;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => accessToken })
      .withAutomaticReconnect()
      .build();

    connectionRef.current = connection;

    // PickAdded still fires for SpeedDraft (auto-revealed at submission) and
    // is kept for backward compatibility / non-reveal-gated draft types.
    connection.on('PickAdded', (payload: PickAddedPayload) => {
      setPicks((prev) => {
        const without = prev.filter((p) => p.playOrder !== payload.playOrder);
        const newPick: GameplayPickResponse = {
          playOrder: payload.playOrder,
          boardPosition: payload.boardPosition,
          movieTitle: payload.movieTitle,
          movieYear: undefined,
          tmdbId: payload.tmdbId,
          playedById: payload.participantId,
          playedByKind: payload.participantKind,
          playedByName:
            participantsRef.current.find((p) => p.participantId === payload.participantId)
              ?.participantName ?? '',
          wasVetoed: false,
          wasVetoOverridden: false,
          wasCommissionerOverride: false,
        };
        return [...without, newPick];
      });
      applyTokenUpdates(payload.participants);
      enqueueNotification({ kind: 'PickAdded', payload });
      setTimeout(() => void refetch(), 300);
    });

    // Host-only — a pick has been submitted and is awaiting announcement.
    // Only the primary host's connection is in the host group, so this
    // never fires for drafters or co-hosts.
    connection.on(
      'PickSubmitted',
      (
        eventDraftPartId: string,
        playOrder: number,
        moviePublicId: string,
        movieTitle: string,
        tmdbId: number,
        boardPosition: number,
        participantId: string,
        participantKind: number,
      ) => {
        const payload: PickSubmittedPayload = {
          draftPartPublicId: draftPartId,
          playOrder,
          boardPosition,
          moviePublicId,
          movieTitle,
          tmdbId,
          participantId,
          participantKind,
        };
        setPendingPicks((prev) => {
          const without = prev.filter((p) => p.playOrder !== payload.playOrder);
          return [...without, payload];
        });
      },
    );

    // General — the pick is now public. This is the real "pick landed on
    // the board" signal for everyone (drafters, co-hosts, and the host's
    // own board view), replacing PickAdded for every non-SpeedDraft type.
    connection.on(
      'PickRevealed',
      (
        eventDraftPartId: string,
        playOrder: number,
        moviePublicId: string,
        movieTitle: string,
        tmdbId: number,
        boardPosition: number,
        participantId: string,
        participantKind: number,
      ) => {
        const payload: PickRevealedPayload = {
          draftPartPublicId: draftPartId,
          playOrder,
          boardPosition,
          movieTitle,
          tmdbId,
          participantId,
          participantKind,
          // Reveal does not change anyone's veto/override token counts —
          // only veto/override/undo actions do, and those broadcast their
          // own token updates separately. Kept as an empty array so
          // PickRevealedPayload's shape stays consistent for callers, but
          // applyTokenUpdates is intentionally not called below.
          participants: [],
        };
        setPendingPicks((prev) => prev.filter((p) => p.playOrder !== payload.playOrder));
        setPicks((prev) => {
          const without = prev.filter((p) => p.playOrder !== payload.playOrder);
          const newPick: GameplayPickResponse = {
            playOrder: payload.playOrder,
            boardPosition: payload.boardPosition,
            movieTitle: payload.movieTitle,
            movieYear: undefined,
            tmdbId: payload.tmdbId,
            playedById: payload.participantId,
            playedByKind: payload.participantKind,
            playedByName:
              participantsRef.current.find((p) => p.participantId === payload.participantId)
                ?.participantName ?? '',
            wasVetoed: false,
            wasVetoOverridden: false,
            wasCommissionerOverride: false,
          };
          return [...without, newPick];
        });
        enqueueNotification({ kind: 'PickRevealed', payload });
        setTimeout(() => void refetch(), 300);
      },
    );

    connection.on('VetoApplied', (payload: VetoAppliedPayload) => {
      setPicks((prev) =>
        prev.map((p) =>
          p.playOrder === payload.playOrder ? { ...p, wasVetoed: true } : p,
        ),
      );
      applyTokenUpdates(payload.participants);
      enqueueNotification({ kind: 'VetoApplied', payload });
      setTimeout(() => void refetch(), 300);
    });

    connection.on('VetoOverrideApplied', (payload: VetoOverrideAppliedPayload) => {
      setPicks((prev) =>
        prev.map((p) =>
          p.playOrder === payload.playOrder
            ? { ...p, wasVetoed: false, wasVetoOverridden: true }
            : p,
        ),
      );
      applyTokenUpdates(payload.participants);
      enqueueNotification({ kind: 'VetoOverrideApplied', payload });
      setTimeout(() => void refetch(), 300);
    });

    connection.on('CommissionerOverrideApplied', (payload: CommissionerOverrideAppliedPayload) => {
      setPicks((prev) =>
        prev.map((p) =>
          p.boardPosition === payload.boardPosition
            ? { ...p, wasCommissionerOverride: true }
            : p,
        ),
      );
      applyTokenUpdates(payload.participants);
      enqueueNotification({ kind: 'CommissionerOverrideApplied', payload });
      setTimeout(() => void refetch(), 300);
    });

    connection.on('VetoUndone', (payload: VetoUndonePayload) => {
      setPicks((prev) =>
        prev.map((p) =>
          p.playOrder === payload.playOrder
            ? { ...p, wasVetoed: false, wasVetoOverridden: false }
            : p,
        ),
      );
      applyTokenUpdates(payload.participants);
      enqueueNotification({ kind: 'VetoUndone', payload });
      setTimeout(() => void refetch(), 300);
    });

    connection.on('PickUndone', (payload: PickUndonePayload) => {
      setPicks((prev) => prev.filter((p) => p.playOrder !== payload.playOrder));
      setPendingPicks((prev) => prev.filter((p) => p.playOrder !== payload.playOrder));
      applyTokenUpdates(payload.participants);
      enqueueNotification({ kind: 'PickUndone', payload });
      setTimeout(() => void refetch(), 300);
    });

    // Fires whenever a movie's honorific changes — in either direction.
    // newAppearanceHonorificValue > previousAppearanceHonorificValue = earned.
    // newAppearanceHonorificValue < previousAppearanceHonorificValue = reverted
    // (caused by a veto or commissioner override striking a previously-locked pick).
    connection.on(
      'MovieHonorificEarned',
      (
        eventDraftPartPublicId: string,
        moviePublicId: string,
        movieTitle: string,
        previousAppearanceHonorificValue: number,
        newAppearanceHonorificValue: number,
        previousPositionHonorificValue: number,
        newPositionHonorificValue: number,
        appearanceCount: number,
      ) => {
        const payload: MovieHonorificChangedPayload = {
          draftPartPublicId: eventDraftPartPublicId,
          moviePublicId,
          movieTitle,
          previousAppearanceHonorificValue,
          newAppearanceHonorificValue,
          previousPositionHonorificValue,
          newPositionHonorificValue,
          appearanceCount,
        };
        enqueueNotification({ kind: 'MovieHonorificChanged', payload });
      },
    );

    connection.on(
      'DrafterHonorificEarned',
      (
        eventDraftPartPublicId: string,
        drafterIdValue: string,
        previousHonorificValue: number,
        newHonorificValue: number,
        appearanceCount: number,
      ) => {
        const payload: DrafterHonorificChangedPayload = {
          draftPartPublicId: eventDraftPartPublicId,
          drafterIdValue,
          previousHonorificValue,
          newHonorificValue,
          appearanceCount,
        };
        enqueueNotification({ kind: 'DrafterHonorificChanged', payload });
      },
    );

    connection.on('CommunityRuleApplied', (payload: CommunityRuleAppliedPayload) => {
      // Mark the pick as vetoed in local state
      setPicks((prev) =>
        prev.map((p) =>
          p.playOrder === payload.playOrder ? { ...p, wasVetoed: true } : p,
        ),
      );
      enqueueNotification({ kind: 'CommunityRuleApplied', payload });
      setTimeout(() => void refetch(), 300);
    });

    connection.on('PositionsSet', () => {
      setTimeout(() => void refetch(), 500);
    });

    connection.on('CountdownStarted', (payload: CountdownStartedPayload) => {
      if (countdownTimerRef.current) {
        clearTimeout(countdownTimerRef.current);
      }
      setCountdownTarget(payload.targetParticipantId);
      countdownTimerRef.current = setTimeout(() => {
        setCountdownTarget(null);
        countdownTimerRef.current = null;
      }, 6000);
    });

    connection.on('DraftCompleted', () => {
      window.location.href = `/drafts/${initialGameplay.draftId}`;
    });

    connection.on('PartCompleted', (summary: DraftCompletionSummary) => {
      setCompletionSummary(summary);
    });

    connection.onreconnecting(() => {
      setReconnecting(true);
      setConnectionState(signalR.HubConnectionState.Reconnecting);
    });

    connection.onreconnected(async () => {
      setReconnecting(false);
      setConnectionState(signalR.HubConnectionState.Connected);
      await refetch();
    });

    connection.onclose(() => {
      setConnectionState(signalR.HubConnectionState.Disconnected);
    });

    let mounted = true;

    async function start() {
      // Wait for any prior connection (from a previous effect invocation —
      // notably React Strict Mode's dev-only mount → cleanup → remount) to
      // fully stop and leave the SignalR group before this one starts and
      // joins. Without this, two connections can briefly both be members of
      // the same group, and every server broadcast gets delivered twice.
      if (previousTeardownRef.current) {
        await previousTeardownRef.current;
      }
      if (!mounted) return;

      try {
        await connection.start();
        if (!mounted) return;
        // Primary hosts join the host group too, so they receive
        // PickSubmitted (pending-pick) notifications before reveal.
        if (isPrimaryHost) {
          await connection.invoke('JoinDraftPartAsHostAsync', draftPartId);
        } else {
          await connection.invoke('JoinDraftPartAsync', draftPartId);
        }
        setConnectionState(signalR.HubConnectionState.Connected);
      } catch {
        if (mounted) setConnectionState(signalR.HubConnectionState.Disconnected);
      }
    }

    const startPromise = start();

    return () => {
      mounted = false;
      if (countdownTimerRef.current) {
        clearTimeout(countdownTimerRef.current);
        countdownTimerRef.current = null;
      }
      // Build this invocation's teardown promise and publish it to the ref
      // BEFORE awaiting it here, so the next effect invocation's start()
      // (which may already be queued via Strict Mode's synchronous remount)
      // can see and await it immediately rather than racing against it.
      const teardown = startPromise
        .catch(() => undefined)
        .then(() => connection.stop());
      previousTeardownRef.current = teardown;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [draftPartId, isPrimaryHost]);

  const sendCountdown = useCallback(async (targetParticipantId: string) => {
    const connection = connectionRef.current;
    if (!connection || connection.state !== signalR.HubConnectionState.Connected) return;
    await connection.invoke('StartCountdownAsync', draftPartId, targetParticipantId);
  }, [draftPartId]);

  const dismissCompletionSummary = useCallback(() => {
    setCompletionSummary(null);
    window.location.href = `/my-drafts/${initialGameplay.draftId}`;
  }, [initialGameplay.draftId]);

  const dismissCountdown = useCallback(() => {
    if (countdownTimerRef.current) {
      clearTimeout(countdownTimerRef.current);
      countdownTimerRef.current = null;
    }
    setCountdownTarget(null);
  }, []);

  return (
    <LiveDraftContext.Provider
      value={{
        gameplay,
        participants,
        picks,
        pendingPicks,
        draftPositions,
        nextExpectedParticipantId,
        callerParticipantId,
        connectionState,
        reconnecting,
        notification,
        dismissNotification,
        countdownTarget,
        dismissCountdown,
        refetch,
        sendCountdown,
        revealPick,
        completionSummary,
        dismissCompletionSummary,
      }}
    >
      {children}
    </LiveDraftContext.Provider>
  );
}