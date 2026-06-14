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
import { GameplayDraftPositionResponse, GameplayParticipantResponse, GameplayPickResponse, GetDraftPartGameplayResponse } from '@/lib/dto';

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
  | { kind: 'VetoApplied'; payload: VetoAppliedPayload }
  | { kind: 'VetoOverrideApplied'; payload: VetoOverrideAppliedPayload }
  | { kind: 'VetoUndone'; payload: VetoUndonePayload }
  | { kind: 'PickUndone'; payload: PickUndonePayload };

// ── Context shape ─────────────────────────────────────────────────────────────

interface LiveDraftContextValue {
  // Current gameplay state
  gameplay: GetDraftPartGameplayResponse;
  participants: GameplayParticipantResponse[];
  picks: GameplayPickResponse[];
  draftPositions: GameplayDraftPositionResponse[];
  nextExpectedParticipantId: string | null;

  // Connection state
  connectionState: signalR.HubConnectionState;
  reconnecting: boolean;

  // Notification modal queue (primary host + co-host)
  notification: GameplayNotification | null;
  dismissNotification: () => void;

  // Countdown (drafter tab)
  countdownTarget: string | null; // participantId being counted down

  // Actions
  refetch: () => Promise<void>;
  sendCountdown: (targetParticipantId: string) => Promise<void>;
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
  const [draftPositions, setDraftPositions] = useState(initialGameplay.draftPositions ?? []);
  const [nextExpectedParticipantId, setNextExpectedParticipantId] = useState(
    initialGameplay.nextExpectedParticipantId ?? null,
  );
  const [connectionState, setConnectionState] = useState<signalR.HubConnectionState>(
    signalR.HubConnectionState.Disconnected,
  );
  const [reconnecting, setReconnecting] = useState(false);
  const [notification, setNotification] = useState<GameplayNotification | null>(null);
  const [countdownTarget, setCountdownTarget] = useState<string | null>(null);
  const notificationQueue = useRef<GameplayNotification[]>([]);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  // ── Token update helper ───────────────────────────────────────────────────

  const applyTokenUpdates = useCallback((updates: TokenUpdate[]) => {
    setParticipants((prev) =>
      prev?.map((p) => {
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
      }) ?? [],
    );
  }, []);

  // ── Notification queue management ─────────────────────────────────────────

  const enqueueNotification = useCallback((n: GameplayNotification) => {
    notificationQueue.current.push(n);
    setNotification((current) => current ?? notificationQueue.current.shift() ?? null);
  }, []);

  const dismissNotification = useCallback(() => {
    const next = notificationQueue.current.shift() ?? null;
    setNotification(next);
  }, []);

  // ── Refetch full state (on reconnect) ─────────────────────────────────────

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

  // ── SignalR connection ─────────────────────────────────────────────────────

  useEffect(() => {
    const hubUrl = `${process.env.NEXT_PUBLIC_API_URL}/drafts/hub`;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => accessToken,
      })
      .withAutomaticReconnect()
      .build();

    connectionRef.current = connection;

    // ── Event handlers ──────────────────────────────────────────────────────

    connection.on('PickAdded', (payload: PickAddedPayload) => {
      const newPick: GameplayPickResponse = {
        playOrder: payload.playOrder,
        boardPosition: payload.boardPosition,
        movieTitle: payload.movieTitle,
        movieYear: undefined,
        tmdbId: payload.tmdbId,
        playedById: payload.participantId,
        playedByKind: payload.participantKind,
        playedByName:
          participants?.find((p) => p.participantId === payload.participantId)
            ?.participantName ?? '',
        wasVetoed: false,
        wasVetoOverridden: false,
        wasCommissionerOverride: false,
      };
      setPicks((prev) => [...(prev ?? []), newPick]);
      applyTokenUpdates(payload.participants);
      enqueueNotification({ kind: 'PickAdded', payload });
    });

    connection.on('VetoApplied', (payload: VetoAppliedPayload) => {
      setPicks((prev) =>
        prev?.map((p) =>
          p.playOrder === payload.playOrder ? { ...p, wasVetoed: true } : p,
        ) ?? [],
      );
      applyTokenUpdates(payload.participants);
      enqueueNotification({ kind: 'VetoApplied', payload });
    });

    connection.on('VetoOverrideApplied', (payload: VetoOverrideAppliedPayload) => {
      setPicks((prev) =>
        prev?.map((p) =>
          p.playOrder === payload.playOrder
            ? { ...p, wasVetoed: false, wasVetoOverridden: true }
            : p,
        ) ?? [],
      );
      applyTokenUpdates(payload.participants);
      enqueueNotification({ kind: 'VetoOverrideApplied', payload });
    });

    connection.on('VetoUndone', (payload: VetoUndonePayload) => {
      setPicks((prev) =>
        prev?.map((p) =>
          p.playOrder === payload.playOrder
            ? { ...p, wasVetoed: false, wasVetoOverridden: false }
            : p,
        ) ?? [],
      );
      applyTokenUpdates(payload.participants);
      enqueueNotification({ kind: 'VetoUndone', payload });
    });

    connection.on('PickUndone', (payload: PickUndonePayload) => {
      setPicks((prev) => prev?.filter((p) => p.playOrder !== payload.playOrder) ?? []);
      applyTokenUpdates(payload.participants);
      enqueueNotification({ kind: 'PickUndone', payload });
    });

    connection.on('PositionsSet', () => {
      // Re-fetch to get assigned participant names on positions
      void refetch();
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

    // ── Connection lifecycle ────────────────────────────────────────────────

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

    async function start() {
      try {
        await connection.start();
        await connection.invoke('JoinDraftPart', draftPartId);
        setConnectionState(signalR.HubConnectionState.Connected);
      } catch {
        setConnectionState(signalR.HubConnectionState.Disconnected);
      }
    }

    void start();

    return () => {
      void connection.stop();
    };
    // accessToken is stable for the lifetime of the page (server-rendered once)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [draftPartId]);

  // ── sendCountdown (co-host action) ────────────────────────────────────────

  const sendCountdown = useCallback(async (targetParticipantId: string) => {
    const connection = connectionRef.current;
    if (!connection || connection.state !== signalR.HubConnectionState.Connected) return;
    await connection.invoke('StartCountdown', draftPartId, targetParticipantId);
  }, [draftPartId]);

  return (
    <LiveDraftContext.Provider
      value={{
        gameplay,
        participants,
        picks,
        draftPositions,
        nextExpectedParticipantId,
        connectionState,
        reconnecting,
        notification,
        dismissNotification,
        countdownTarget,
        refetch,
        sendCountdown,
      }}
    >
      {children}
    </LiveDraftContext.Provider>
  );
}