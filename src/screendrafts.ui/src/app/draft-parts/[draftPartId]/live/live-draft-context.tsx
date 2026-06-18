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
  | { kind: 'VetoUndone'; payload: VetoUndonePayload }
  | { kind: 'PickUndone'; payload: PickUndonePayload };

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
  refetch: () => Promise<void>;
  sendCountdown: (targetParticipantId: string) => Promise<void>;
  // Primary-host action: announce/reveal a pending pick to everyone.
  revealPick: (playOrder: number) => Promise<void>;
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
  const notificationQueue = useRef<GameplayNotification[]>([]);
  const participantsRef = useRef(initialGameplay.participants ?? []);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

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
    connection.on('PickSubmitted', (payload: PickSubmittedPayload) => {
      setPendingPicks((prev) => {
        const without = prev.filter((p) => p.playOrder !== payload.playOrder);
        return [...without, payload];
      });
    });

    // General — the pick is now public. This is the real "pick landed on
    // the board" signal for everyone (drafters, co-hosts, and the host's
    // own board view), replacing PickAdded for every non-SpeedDraft type.
    connection.on('PickRevealed', (payload: PickRevealedPayload) => {
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
      applyTokenUpdates(payload.participants);
      enqueueNotification({ kind: 'PickRevealed', payload });
      setTimeout(() => void refetch(), 300);
    });

    connection.on('VetoApplied', (payload: VetoAppliedPayload) => {
      setPicks((prev) =>
        prev.map((p) =>
          p.playOrder === payload.playOrder ? { ...p, wasVetoed: true } : p,
        ),
      );
      applyTokenUpdates(payload.participants);
      enqueueNotification({ kind: 'VetoApplied', payload });
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
    });

    connection.on('PickUndone', (payload: PickUndonePayload) => {
      setPicks((prev) => prev.filter((p) => p.playOrder !== payload.playOrder));
      setPendingPicks((prev) => prev.filter((p) => p.playOrder !== payload.playOrder));
      applyTokenUpdates(payload.participants);
      enqueueNotification({ kind: 'PickUndone', payload });
    });

    connection.on('PositionsSet', () => {
      // Small delay to ensure the server has committed before we refetch.
      setTimeout(() => void refetch(), 500);
    });

    connection.on('CountdownStarted', (payload: CountdownStartedPayload) => {
      setCountdownTarget(payload.targetParticipantId);
      setTimeout(() => setCountdownTarget(null), 6000);
    });

    connection.on('DraftCompleted', () => {
      window.location.href = `/drafts/${initialGameplay.draftId}`;
    });

    connection.on('PartCompleted', () => {
      window.location.href = `/my-drafts/${initialGameplay.draftId}`;
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
      // Wait for negotiation/start to settle before stopping — calling
      // stop() while start() is mid-negotiation throws "connection was
      // stopped during negotiation" and logs as an unhandled error.
      void startPromise.finally(() => {
        void connection.stop();
      });
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [draftPartId, isPrimaryHost]);

  const sendCountdown = useCallback(async (targetParticipantId: string) => {
    const connection = connectionRef.current;
    if (!connection || connection.state !== signalR.HubConnectionState.Connected) return;
    await connection.invoke('StartCountdownAsync', draftPartId, targetParticipantId);
  }, [draftPartId]);

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
        refetch,
        sendCountdown,
        revealPick,
      }}
    >
      {children}
    </LiveDraftContext.Provider>
  );
}