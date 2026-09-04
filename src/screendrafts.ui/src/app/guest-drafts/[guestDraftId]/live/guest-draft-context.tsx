// app/guest-drafts/[guestDraftId]/live/guest-draft-context.tsx
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
import { fetchGuestDraftGameplay, fetchMediaByPublicId } from './gameplay-fetchers';
import {
  GameplayParticipantResponse,
  GameplayPickResponse,
  GameplayPositionResponse,
  GetGuestDraftGameplayResponse,
} from '@/lib/dto';

// ── Position shape normalization ────────────────────────────────────────────
// GuestDrafts' GameplayPositionResponse has different field names than
// canonical's GameplayDraftPositionResponse (name vs positionName, picks vs
// ownedBoardSlots, assignedParticipantPublicId vs assignedParticipantId) and
// no participant-kind field at all — GuestDrafts has no Team-kind concept.
// Normalizing once here, at the context boundary, is what lets draft-board.tsx
// and veto-status-bar.tsx port over with only their import line changed.
export interface GuestDraftPositionView {
  positionPublicId?: string;
  positionName?: string;
  ownedBoardSlots?: number[];
  hasBonusVeto?: boolean;
  hasBonusVetoOverride?: boolean;
  hasBonusFungibleToken?: boolean;
  assignedParticipantId?: string;
  assignedParticipantName?: string;
}

function toPositionView(p: GameplayPositionResponse): GuestDraftPositionView {
  return {
    positionPublicId: p.positionPublicId,
    positionName: p.name,
    ownedBoardSlots: p.picks,
    hasBonusVeto: p.hasBonusVeto,
    hasBonusVetoOverride: p.hasBonusVetoOverride,
    hasBonusFungibleToken: p.hasBonusFungibleToken,
    assignedParticipantId: p.assignedParticipantPublicId,
    assignedParticipantName: p.assignedParticipantDisplayName,
  };
}

// ── Pending reveal ───────────────────────────────────────────────────────────
// GuestDraftPickSubmittedIntegrationEventConsumer.cs only sends PickSubmitted
// to the revealer's own participant group (guest-draft:{id}:participant:{id}),
// never the flat group — so if this connection receives it, it IS the
// designated revealer, no client-side filtering needed. Positional args match
// the consumer's SendCoreAsync call exactly:
//   [GuestDraftPublicId, PlayOrder, BoardPosition, MoviePublicId, PlayedByParticipantId]
// No movie title in the payload (only moviePublicId) — resolve it via
// GET /media/{publicId} when rendering the reveal prompt.
export interface GuestDraftPendingReveal {
  guestDraftPublicId: string;
  playOrder: number;
  boardPosition: number;
  moviePublicId: string;
  playedByParticipantId: string;
}

// ── Completion summary ───────────────────────────────────────────────────────
// Matches "just board + counts" — no honorifics/predictions/standings for
// GuestDrafts. DraftCompleted's payload gives the two counts directly; the
// final board is whatever's already in `picks` by the time this fires.
export interface GuestDraftCompletionSummary {
  guestDraftPublicId: string;
  totalPicks: number;
  vetoCount: number;
}

// ── Context shape ─────────────────────────────────────────────────────────────

interface GuestDraftLiveContextValue {
  gameplay: GetGuestDraftGameplayResponse;
  participants: GameplayParticipantResponse[];
  picks: GameplayPickResponse[];
  draftPositions: GuestDraftPositionView[];
  isOwner: boolean;
  isParticipant: boolean;
  // The caller's own participant public id (from callerContext.participantPublicId),
  // null if the caller isn't a participant (e.g. an owner who didn't also join).
  // ASSUMPTION, flagged for confirmation: GuestDraft participant matching keys
  // off participantPublicId end-to-end — GameplayPositionResponse.assignedParticipantPublicId,
  // callerContext.participantPublicId, and GameplayParticipantResponse.participantPublicId
  // all share that name, so that's the field this context matches on. Canonical
  // used participantId (a different field) for the same purpose — verify picks'
  // playedById is actually populated with participantPublicId for GuestDrafts
  // before trusting turn-taking/ownership comparisons built on this.
  callerParticipantId: string | null;
  pendingReveal: GuestDraftPendingReveal | null;
  completionSummary: GuestDraftCompletionSummary | null;
  connectionState: signalR.HubConnectionState;
  reconnecting: boolean;
  refetch: () => Promise<void>;
}

const GuestDraftLiveContext = createContext<GuestDraftLiveContextValue | null>(null);

export function useGuestDraftLive() {
  const ctx = useContext(GuestDraftLiveContext);
  if (!ctx) throw new Error('useGuestDraftLive must be used within GuestDraftLiveProvider');
  return ctx;
}

// ── Provider ──────────────────────────────────────────────────────────────────

interface GuestDraftLiveProviderProps {
  guestDraftId: string;
  accessToken: string;
  initialGameplay: GetGuestDraftGameplayResponse;
  children: React.ReactNode;
}

export function GuestDraftLiveProvider({
  guestDraftId,
  accessToken,
  initialGameplay,
  children,
}: GuestDraftLiveProviderProps) {
  const [gameplay, setGameplay] = useState(initialGameplay);
  const [participants, setParticipants] = useState(initialGameplay.participants ?? []);
  const [picks, setPicks] = useState(initialGameplay.picks ?? []);
  const [draftPositions, setDraftPositions] = useState(
    (initialGameplay.positions ?? []).map(toPositionView),
  );

  const isOwner = initialGameplay.callerContext?.isOwner ?? false;
  const isParticipant = initialGameplay.callerContext?.isParticipant ?? false;
  const callerParticipantId = initialGameplay.callerContext?.participantPublicId ?? null;

  const [connectionState, setConnectionState] = useState<signalR.HubConnectionState>(
    signalR.HubConnectionState.Disconnected,
  );
  const [reconnecting, setReconnecting] = useState(false);
  const [pendingReveal, setPendingReveal] = useState<GuestDraftPendingReveal | null>(null);
  const [completionSummary, setCompletionSummary] = useState<GuestDraftCompletionSummary | null>(
    null,
  );

  // The SignalR effect below intentionally doesn't depend on `participants`
  // (reconnecting the hub every time a token count changes would be worse
  // than a stale read) — so handlers that need the current participant list
  // to resolve a name read it from this ref instead of the closed-over state.
  const participantsRef = useRef(participants);
  useEffect(() => {
    participantsRef.current = participants;
  }, [participants]);

  // If the pending pick shows up in `picks` (revealed, by us or otherwise),
  // the duty's done — clear it. Safety net alongside the explicit clear after
  // a successful reveal call.
  useEffect(() => {
    if (pendingReveal && picks.some((p) => p.playOrder === pendingReveal.playOrder)) {
      setPendingReveal(null);
    }
  }, [picks, pendingReveal]);

  const connectionRef = useRef<signalR.HubConnection | null>(null);
  // See live-draft-context.tsx's identical ref for why: guards against React
  // Strict Mode's dev-only mount → cleanup → remount double-joining the group.
  const previousTeardownRef = useRef<Promise<void> | null>(null);

  // ── Refetch ───────────────────────────────────────────────────────────────

  const refetch = useCallback(async () => {
    try {
      const fresh = await fetchGuestDraftGameplay(accessToken, guestDraftId);
      setGameplay(fresh);
      setParticipants(fresh.participants ?? []);
      setPicks(fresh.picks ?? []);
      setDraftPositions((fresh.positions ?? []).map(toPositionView));
    } catch {
      // silent — reconnecting banner already visible
    }
  }, [accessToken, guestDraftId]);

  // ── SignalR ───────────────────────────────────────────────────────────────
  //
  // GuestDrafts is hostless — there's no primary-host group to branch on.
  // Every connection joins the same two groups: the flat "guest-draft:{id}"
  // group everyone gets, and "guest-draft:{id}:participant:{id}" which routes
  // PickSubmitted to whoever's authorized to reveal that specific pending pick.
  // Both joins happen through one hub method, JoinGuestDraftAsync(guestDraftId,
  // participantId), confirmed from the GuestDrafts real-time pipeline summary.
  //
  // PickSubmitted's shape came from GuestDraftPickSubmittedIntegrationEventConsumer.cs.
  // All 9 broadcast events now have confirmed shapes (from their matching
  // *IntegrationEventConsumer.cs files) and are patched locally where the
  // payload actually supports it, instead of round-tripping through a
  // refetch. VetoUndone and DraftStarted still fall back to a refetch for
  // part of their work — see their handlers below for why.
  useEffect(() => {
    const hubUrl = `${API_BASE_URL()}/drafts/hub`;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => accessToken })
      .withAutomaticReconnect()
      .build();

    connectionRef.current = connection;

    const refetchOnEvent = () => {
      setTimeout(() => void refetch(), 300);
    };

    // [GuestDraftPublicId, PlayOrder, BoardPosition, MoviePublicId, PlayedByParticipantId,
    //  VetoTokensRemaining, OverrideTokensRemaining] —
    // GuestDraftCommissionerOverrideAppliedIntegrationEventConsumer.cs. A
    // commissioner override removes a pick without it landing as a veto or a
    // save (per the Landed formula: wasCommissionerOverride always excludes
    // it) — so unlike PickUndone this does NOT drop the pick from `picks`,
    // it stays and gets flagged, which is what frees the board slot for that
    // position to be picked again while still keeping the removed pick
    // visible in the pick list ("REMOVED").
    connection.on(
      'CommissionerOverrideApplied',
      (
        _eventGuestDraftPublicId: string,
        playOrder: number,
        _boardPosition: number,
        _moviePublicId: string,
        playedByParticipantId: string,
        vetoTokensRemaining: number,
        overrideTokensRemaining: number,
      ) => {
        setPicks((prev) =>
          prev.map((p) =>
            p.playOrder === playOrder ? { ...p, wasCommissionerOverride: true } : p,
          ),
        );
        setParticipants((prev) =>
          prev.map((p) =>
            p.participantPublicId === playedByParticipantId
              ? { ...p, vetoTokensRemaining, overrideTokensRemaining }
              : p,
          ),
        );
      },
    );

    // [GuestDraftPublicId, PlayOrder, BoardPosition, MoviePublicId, PlayedByParticipantId] —
    // GuestDraftPickSubmittedIntegrationEventConsumer.cs. Only reaches the
    // designated revealer's own group, so receiving this at all means it's ours.
    connection.on(
      'PickSubmitted',
      (
        eventGuestDraftPublicId: string,
        playOrder: number,
        boardPosition: number,
        moviePublicId: string,
        playedByParticipantId: string,
      ) => {
        setPendingReveal({
          guestDraftPublicId: eventGuestDraftPublicId,
          playOrder,
          boardPosition,
          moviePublicId,
          playedByParticipantId,
        });
      },
    );

    // [GuestDraftPublicId, PlayOrder, BoardPosition, MoviePublicId, PlayedByParticipantId] —
    // GuestDraftPickRevealedIntegrationEventConsumer.cs. Same shape as
    // PickSubmitted, sent to everyone this time. No title in the payload, so
    // resolve the movie before adding it to `picks`; if that lookup fails,
    // fall back to a refetch rather than show a half-built pick.
    connection.on(
      'PickRevealed',
      async (
        _eventGuestDraftPublicId: string,
        playOrder: number,
        boardPosition: number,
        moviePublicId: string,
        playedByParticipantId: string,
      ) => {
        try {
          const movie = await fetchMediaByPublicId(accessToken, moviePublicId);
          setPicks((prev) => {
            if (prev.some((p) => p.playOrder === playOrder)) return prev;
            const revealed: GameplayPickResponse = {
              playOrder,
              boardPosition,
              movieTitle: movie.title,
              movieYear: movie.year,
              tmdbId: movie.tmdbId,
              playedById: playedByParticipantId,
              playedByName: participantsRef.current.find(
                (p) => p.participantPublicId === playedByParticipantId,
              )?.participantName,
              wasVetoed: false,
              wasVetoOverridden: false,
              wasCommissionerOverride: false,
            };
            return [...prev, revealed];
          });
        } catch {
          refetchOnEvent();
        }
      },
    );

    // [GuestDraftPublicId, PlayOrder, BoardPosition, MoviePublicId] —
    // GuestDraftPickUndoneIntegrationEventConsumer.cs. Unambiguous — just
    // drop the pick with this playOrder.
    connection.on(
      'PickUndone',
      (_eventGuestDraftPublicId: string, playOrder: number) => {
        setPicks((prev) => prev.filter((p) => p.playOrder !== playOrder));
      },
    );

    // [GuestDraftPublicId, PlayOrder, MoviePublicId, VetoedByParticipantId,
    //  PlayedByParticipantId, VetoTokensRemaining, OverrideTokensRemaining] —
    // GuestDraftVetoAppliedIntegrationEventConsumer.cs. Patches the pick and
    // the vetoer's own token counts (the two numbers in this payload are
    // that participant's current totals, not a map across everyone).
    connection.on(
      'VetoApplied',
      (
        _eventGuestDraftPublicId: string,
        playOrder: number,
        _moviePublicId: string,
        vetoedByParticipantId: string,
        _playedByParticipantId: string,
        vetoTokensRemaining: number,
        overrideTokensRemaining: number,
      ) => {
        const vetoerName = participantsRef.current.find(
          (p) => p.participantPublicId === vetoedByParticipantId,
        )?.participantName;
        setPicks((prev) =>
          prev.map((p) =>
            p.playOrder === playOrder ? { ...p, wasVetoed: true, vetoedByName: vetoerName } : p,
          ),
        );
        setParticipants((prev) =>
          prev.map((p) =>
            p.participantPublicId === vetoedByParticipantId
              ? { ...p, vetoTokensRemaining, overrideTokensRemaining }
              : p,
          ),
        );
      },
    );

    // [GuestDraftPublicId, PlayOrder, MoviePublicId, OverriddenByParticipantId,
    //  VetoTokensRemaining, OverrideTokensRemaining] —
    // GuestDraftVetoOverrideAppliedIntegrationEventConsumer.cs. Same pattern
    // as VetoApplied, keyed off the overriding participant instead.
    connection.on(
      'VetoOverrideApplied',
      (
        _eventGuestDraftPublicId: string,
        playOrder: number,
        _moviePublicId: string,
        overriddenByParticipantId: string,
        vetoTokensRemaining: number,
        overrideTokensRemaining: number,
      ) => {
        const overriderName = participantsRef.current.find(
          (p) => p.participantPublicId === overriddenByParticipantId,
        )?.participantName;
        setPicks((prev) =>
          prev.map((p) =>
            p.playOrder === playOrder
              ? { ...p, wasVetoOverridden: true, savedByName: overriderName }
              : p,
          ),
        );
        setParticipants((prev) =>
          prev.map((p) =>
            p.participantPublicId === overriddenByParticipantId
              ? { ...p, vetoTokensRemaining, overrideTokensRemaining }
              : p,
          ),
        );
      },
    );

    // [GuestDraftPublicId, PlayOrder, MoviePublicId, VetoTokensRemaining,
    //  OverrideTokensRemaining] — GuestDraftVetoUndoneIntegrationEventConsumer.cs.
    // Unlike VetoApplied/VetoOverrideApplied, this payload carries no
    // participant id, so there's no safe way to know whose token counts
    // these two numbers belong to (matching on the pick's vetoedByName
    // string back to a participant would be a name-collision bug waiting to
    // happen). Patch the pick locally — that part's unambiguous, keyed by
    // playOrder — and refetch for the participant token sync.
    connection.on(
      'VetoUndone',
      (_eventGuestDraftPublicId: string, playOrder: number) => {
        setPicks((prev) =>
          prev.map((p) =>
            p.playOrder === playOrder
              ? {
                  ...p,
                  wasVetoed: false,
                  vetoedByName: undefined,
                  wasVetoOverridden: false,
                  savedByName: undefined,
                }
              : p,
          ),
        );
        refetchOnEvent();
      },
    );

    // [GuestDraftPublicId, ParticipantCount] — GuestDraftStartedIntegrationEventConsumer.cs.
    // Not enough here to patch anything meaningfully (positions/status still
    // need a real fetch), so this stays a refetch.
    connection.on('DraftStarted', refetchOnEvent);

    // [GuestDraftPublicId, TotalPicks, VetoCount] —
    // GuestDraftCompletedIntegrationEventConsumer.cs. Enough to build the
    // "board + counts" completion summary directly — the final board is
    // whatever's already in `picks`. Still refetches alongside it so
    // gameplay.status and anything else catches up.
    connection.on(
      'DraftCompleted',
      (eventGuestDraftPublicId: string, totalPicks: number, vetoCount: number) => {
        setPendingReveal(null);
        setCompletionSummary({
          guestDraftPublicId: eventGuestDraftPublicId,
          totalPicks,
          vetoCount,
        });
        refetchOnEvent();
      },
    );

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
      if (previousTeardownRef.current) {
        await previousTeardownRef.current;
      }
      if (!mounted) return;

      try {
        await connection.start();
        if (!mounted) return;
        await connection.invoke('JoinGuestDraftAsync', guestDraftId, callerParticipantId ?? '');
        setConnectionState(signalR.HubConnectionState.Connected);
      } catch {
        if (mounted) setConnectionState(signalR.HubConnectionState.Disconnected);
      }
    }

    const startPromise = start();

    return () => {
      mounted = false;
      const teardown = startPromise
        .catch(() => undefined)
        .then(() => connection.stop());
      previousTeardownRef.current = teardown;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [guestDraftId, callerParticipantId]);

  return (
    <GuestDraftLiveContext.Provider
      value={{
        gameplay,
        participants,
        picks,
        draftPositions,
        isOwner,
        isParticipant,
        callerParticipantId,
        pendingReveal,
        completionSummary,
        connectionState,
        reconnecting,
        refetch,
      }}
    >
      {children}
    </GuestDraftLiveContext.Provider>
  );
}

function API_BASE_URL() {
  return process.env.NEXT_PUBLIC_API_URL;
}