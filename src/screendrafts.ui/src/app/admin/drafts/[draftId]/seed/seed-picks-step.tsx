// app/admin/drafts/[draftId]/seed/seed-picks-step.tsx
"use client";

import { useEffect, useState } from "react";
import {
  listDraftPositions,
  getDraftPartGameplay,
  playPick,
  seedRevealPick,
  applyVeto,
  applyVetoOverride,
  applyCommissionerOverride,
  type DraftPartParticipant,
  type DraftPartHost,
  type GameplayPick,
  type GameplayVetoHistoryEntry,
} from "@/services/admin/fetch-admin-drafts";
import { importAndResolve, type ResolvedMovie } from "@/lib/movie-resolve";
import { importAndResolveEpisode, MEDIA_TYPE_TV_EPISODE } from "@/lib/tv-episode-resolve";
import { MediaPicker, type SelectedMedia } from "@/components/drafts/media-picker";
import type { SeedDraftState } from "./seed-draft-wizard";

const LABEL = "block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1";
const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";
const BTN_PRIMARY =
  "bg-sd-red text-white font-oswald font-medium tracking-wide uppercase px-5 py-2.5 hover:bg-sd-red/90 disabled:opacity-50 transition-colors";
const BTN_SECONDARY =
  "border border-sd-ink/20 text-sd-ink font-mono text-[11px] tracking-widest uppercase px-3 py-1.5 hover:bg-sd-ink/5 disabled:opacity-40 transition-colors";

interface LocalPick {
  playOrder: number;
  position: number;
  movieTitle: string;
  tmdbId: number | null;
  participantIdValue: string;
  participantDisplayName: string;
  status: "landed" | "vetoed" | "vetoOverridden" | "commissionerOverridden";
  vetoedByName: string | null;
  savedByName: string | null;
  // Whether the pick's current veto/override was paid for by a fungible token
  // rather than a normal one. Only meaningful alongside the matching status.
  // Always sourced fresh from the server (see refreshFromServer) — the
  // frontend has no way to know client-side which pool a spend drew from,
  // that's decided server-side by whichever budget was actually available.
  vetoWasFungible: boolean;
  vetoOverrideWasFungible: boolean;
  // Only set on a hostless part — the participant this pick was "sent to,"
  // assigned server-side at play time. Used to auto-resolve who reveals it
  // instead of asking the seeder to pick a host that doesn't exist.
  revealAuthorizedParticipantId: string | null;
  revealAuthorizedByName: string | null;
  // Full veto history for this pick — see GameplayVetoHistoryEntry. Normally holds at
  // most one entry; a second only appears after a veto → override → re-veto sequence.
  vetoHistory: GameplayVetoHistoryEntry[];
}

interface Props {
  draft: SeedDraftState;
  participants: DraftPartParticipant[];
  primaryHost: DraftPartHost | null;
  coHosts: DraftPartHost[];
  // MiniMega / Mega / Super / MiniSuper allow overrides; Standard and
  // SpeedDraft don't — ApplyVetoOverride refuses both at the domain level.
  allowsOverride: boolean;
  accessToken: string;
  onAllPositionsFilled: () => void;
}

export function SeedPicksStep({
  draft,
  participants,
  primaryHost,
  coHosts,
  allowsOverride,
  accessToken,
  onAllPositionsFilled,
}: Props) {
  const [totalPicks, setTotalPicks] = useState<number | null>(null);
  const [picks, setPicks] = useState<LocalPick[]>([]);
  const [hydrated, setHydrated] = useState(false);

  // Fungible token (BUV/Rabbit's Foot-style formats) — null for the
  // overwhelming majority of drafts that don't use one. Remaining balances
  // are keyed by participantIdValue, refreshed alongside picks after every
  // mutation so they never drift from what the server actually has.
  const [fungibleTokenName, setFungibleTokenName] = useState<string | null>(null);
  const [fungibleRemaining, setFungibleRemaining] = useState<Map<string, number>>(new Map());

  // Hostless parts have no primary host to reveal picks — reveal authority
  // instead belongs to whichever participant the just-played pick was "sent
  // to" (Pick.RevealAuthorizedParticipant, assigned server-side at play
  // time). See handleSubmitPick, which branches on this instead of asking
  // for a host selection that can never be filled in.
  const [isHostless, setIsHostless] = useState(false);

  // Next-pick form state
  const [position, setPosition] = useState<number | "">("");
  const [participantIdValue, setParticipantIdValue] = useState(participants[0]?.participantIdValue ?? "");
  const [revealedByHostId, setRevealedByHostId] = useState(primaryHost?.hostPublicId ?? "");
  const [selectedMovie, setSelectedMovie] = useState<ResolvedMovie | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [importing, setImporting] = useState(false);
  // Fallback for titles TMDb's own title search can't find — e.g. "$"
  // (TMDb 31644), which the API appears to treat as an empty/punctuation-only
  // query and returns zero matches for, independent of any length gate on
  // this app's side. Bypasses search entirely: goes straight through
  // handlePickMovie's existing import path with a known TMDb ID.
  const [manualEntryOpen, setManualEntryOpen] = useState(false);
  const [manualTmdbId, setManualTmdbId] = useState("");
  // Which pick is currently showing an inline "who's doing this" picker,
  // and what's been selected so far. Veto and Veto Override both need an
  // explicit issuer — self-veto is a real case (the same drafter who
  // played the pick can also veto it), and on drafts with more than two
  // participants there's no single "the other one" to infer at all.
  const [pendingAction, setPendingAction] = useState<{
    playOrder: number;
    type: "veto" | "vetoOverride";
    issuerIdValue: string;
  } | null>(null);

  const allHosts = primaryHost ? [primaryHost, ...coHosts] : coHosts;
  const nextPlayOrder = picks.length + 1;
  const lastPick = picks[picks.length - 1] ?? null;
  // commissionerOverridden deliberately excluded — that pick never lands,
  // per handleCommissionerOverride's own comment below. Previously included
  // here by mistake, which let this counter overstate board completion on
  // any part with a commissioner override.
  const landedCount = picks.filter(
    (p) => p.status === "landed" || p.status === "vetoOverridden"
  ).length;

  // A commissioner-overridden film is barred from this draft part
  // entirely, not just from the slot it was removed from — the slot
  // itself can still take a different pick. Tracked by tmdbId rather than
  // title text, since title matching risks both false blocks (formatting
  // differences on the same film) and false negatives (two different
  // films sharing a title).
  const barredTmdbIds = new Set(
    picks.filter((p) => p.status === "commissionerOverridden" && p.tmdbId != null).map((p) => p.tmdbId)
  );

  function statusFromGameplayPick(p: GameplayPick): LocalPick["status"] {
    if (p.wasCommissionerOverride) return "commissionerOverridden";
    if (p.wasVetoOverridden) return "vetoOverridden";
    if (p.wasVetoed) return "vetoed";
    return "landed";
  }

  // Single source of truth for picks + participant token state, called on
  // mount and after every mutation. Replaces the old pattern of optimistic
  // local patches after each action — those can't be correct here, since
  // whether a spend drew from a participant's normal pool or their fungible
  // token is decided server-side, not something the frontend can compute.
  // Returns the fresh picks so callers can check board-completion state
  // immediately, since setPicks itself won't be visible in this closure yet.
  async function refreshFromServer(): Promise<LocalPick[]> {
    const gameplay = await getDraftPartGameplay(accessToken, draft.draftPartPublicId);
    if (!gameplay) return picks;

    setFungibleTokenName(gameplay.fungibleTokenName);
    setIsHostless(gameplay.isHostless);
    setFungibleRemaining(
      new Map(gameplay.participants.map((p) => [p.participantId, p.fungibleTokensRemaining]))
    );

    const nextPicks: LocalPick[] = gameplay.picks
      .slice()
      .sort((a, b) => a.playOrder - b.playOrder)
      .map((p) => ({
        playOrder: p.playOrder,
        position: p.boardPosition,
        movieTitle: p.movieTitle,
        tmdbId: p.tmdbId,
        participantIdValue: p.playedById,
        participantDisplayName: p.playedByName,
        status: statusFromGameplayPick(p),
        vetoedByName: p.vetoedByName,
        savedByName: p.savedByName,
        vetoWasFungible: p.wasVetoFungible,
        vetoOverrideWasFungible: p.wasVetoOverrideFungible,
        revealAuthorizedParticipantId: p.revealAuthorizedParticipantId,
        revealAuthorizedByName: p.revealAuthorizedByName,
        vetoHistory: p.vetoHistory ?? [],
      }));

    setPicks(nextPicks);
    return nextPicks;
  }

  // Hydrates from the server on mount rather than starting from an empty
  // list — this component previously tracked picks in local state only, so
  // navigating away mid-entry and coming back showed an empty board with no
  // veto/override actions available, even though every pick already
  // submitted was safely recorded server-side.
  useEffect(() => {
    (async () => {
      await refreshFromServer();
      setHydrated(true);
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [accessToken, draft.draftPartPublicId]);

  useEffect(() => {
    (async () => {
      const positions = await listDraftPositions(accessToken, draft.draftPartPublicId);
      setTotalPicks(positions.reduce((sum, p) => sum + p.picks.length, 0));
    })();
  }, [accessToken, draft.draftPartPublicId]);

  // If re-entering this step finds the board already full from a prior
  // session, advance immediately rather than showing a full board with
  // nothing left to do.
  useEffect(() => {
    if (!hydrated || totalPicks == null) return;
    if (landedCount >= totalPicks) onAllPositionsFilled();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [hydrated, totalPicks]);

  async function handlePickMovie(movie: ResolvedMovie) {
    if (barredTmdbIds.has(movie.tmdbId)) {
      setError(`${movie.title} was commissioner-overridden earlier in this part and can't be picked again.`);
      return;
    }
    if (movie.mediaPublicId) {
      setSelectedMovie(movie);
      return;
    }
    // Not in the local media database yet — import from TMDb and wait for
    // it to land, same as the live pick-source-panel does.
    setImporting(true);
    setError(null);
    try {
      const imported = await importAndResolve(movie.tmdbId, accessToken);
      if (!imported) {
        setError("Movie could not be imported in time — try selecting it again in a moment.");
        return;
      }
      setSelectedMovie(imported);
    } finally {
      setImporting(false);
    }
  }

  // Entry point for MediaPicker's onSelect — branches on mediaType since
  // movies and TV episodes resolve differently (see tv-episode-resolve.ts).
  // The movie branch delegates straight to handlePickMovie, unchanged.
  async function handleMediaSelect(media: SelectedMedia) {
    if (media.mediaType !== MEDIA_TYPE_TV_EPISODE) {
      await handlePickMovie({
        tmdbId: media.tmdbId,
        mediaPublicId: "",
        title: media.title,
        year: media.year,
        posterUrl: null,
      });
      return;
    }

    if (barredTmdbIds.has(media.tmdbId)) {
      setError(`${media.title} was commissioner-overridden earlier in this part and can't be picked again.`);
      return;
    }
    if (!media.tvSeriesTmdbId || !media.seasonNumber || !media.episodeNumber) {
      setError("Missing episode identity — try selecting again.");
      return;
    }
    setImporting(true);
    setError(null);
    try {
      const publicId = await importAndResolveEpisode(
        media.tmdbId,
        media.tvSeriesTmdbId,
        media.seasonNumber,
        media.episodeNumber,
        accessToken,
      );
      if (!publicId) {
        setError("Episode could not be imported in time — try selecting it again in a moment.");
        return;
      }
      setSelectedMovie({
        mediaPublicId: publicId,
        tmdbId: media.tmdbId,
        title: media.title,
        year: media.year,
        posterUrl: null,
      });
    } finally {
      setImporting(false);
    }
  }

  async function handleManualTmdbSubmit() {
    const parsed = Number.parseInt(manualTmdbId.trim(), 10);
    if (!Number.isFinite(parsed) || parsed <= 0) {
      setError("Enter a valid TMDb ID.");
      return;
    }
    // mediaPublicId/title/year/posterUrl left blank on purpose — handlePickMovie
    // only trusts mediaPublicId to decide whether an import is needed, and
    // importAndResolve fills in the real title/year from TMDb once it lands.
    await handlePickMovie({
      tmdbId: parsed,
      mediaPublicId: "",
      title: "",
      year: null,
      posterUrl: null,
    });
    setManualTmdbId("");
    setManualEntryOpen(false);
  }

  async function handleSubmitPick() {
    const hasRevealer = isHostless || !!revealedByHostId;
    if (!selectedMovie || position === "" || !participantIdValue || !hasRevealer || submitting) {
      return;
    }
    const participant = participants.find((p) => p.participantIdValue === participantIdValue);
    if (!participant || participant.participantKindValue.value == null) {
      setError("Selected participant is missing a kind — try re-selecting them.");
      return;
    }
    const participantKind = participant.participantKindValue.value;

    setSubmitting(true);
    setError(null);
    try {
      await playPick(accessToken, {
        draftPartId: draft.draftPartPublicId,
        position: Number(position),
        playOrder: nextPlayOrder,
        participantPublicId: participant.participantPublicId,
        participantKind,
        moviePublicId: selectedMovie.mediaPublicId,
      });

      // The pick is now landed on the server regardless of what happens below — clear
      // the movie/slot selection immediately so a reveal failure doesn't leave the form
      // stuck displaying a pick that's already been recorded.
      setSelectedMovie(null);
      setPosition("");

      // Hostless parts have no host to reveal picks — refetch immediately to
      // pick up Pick.RevealAuthorizedParticipant, which the server assigned
      // at play time, and reveal as that participant instead.
      let actedByPublicId = revealedByHostId;
      if (isHostless) {
        const freshPicks = await refreshFromServer();
        const justPlayed = freshPicks.find((p) => p.playOrder === nextPlayOrder);
        const recipient = participants.find(
          (p) => p.participantIdValue === justPlayed?.revealAuthorizedParticipantId
        );
        if (!recipient?.participantPublicId) {
          throw new Error(
            "Could not determine who this pick was sent to — refresh and try revealing manually."
          );
        }
        actedByPublicId = recipient.participantPublicId;
      }

      await seedRevealPick(accessToken, {
        draftPartId: draft.draftPartPublicId,
        playOrder: nextPlayOrder,
        actedByPublicId,
      });

      const nextPicks = await refreshFromServer();

      const newLandedCount = nextPicks.filter(
        (p) => p.status === "landed" || p.status === "vetoOverridden"
      ).length;
      if (totalPicks != null && newLandedCount >= totalPicks) {
        onAllPositionsFilled();
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to record pick.");
    } finally {
      setSubmitting(false);
    }
  }

  async function handleVetoLast(issuerIdValue: string) {
    if (
      !lastPick ||
      (lastPick.status !== "landed" && lastPick.status !== "vetoOverridden") ||
      submitting
    ) {
      return;
    }
    const issuer = participants.find((p) => p.participantIdValue === issuerIdValue);
    if (!issuer || issuer.participantKindValue.value == null) {
      setError("Select who's issuing this veto.");
      return;
    }
    setSubmitting(true);
    setError(null);
    try {
      await applyVeto(accessToken, {
        draftPartId: draft.draftPartPublicId,
        playOrder: lastPick.playOrder,
        participantPublicId: issuer.participantPublicId,
        participantKind: issuer.participantKindValue.value,
      });
      await refreshFromServer();
      setPendingAction(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to apply veto.");
    } finally {
      setSubmitting(false);
    }
  }

  async function handleVetoOverride(pick: LocalPick, issuerIdValue: string) {
    if (submitting) return;
    const overrider = participants.find((p) => p.participantIdValue === issuerIdValue);
    if (!overrider || overrider.participantKindValue.value == null) {
      setError("Select who's overriding this veto.");
      return;
    }
    if (!overrider.participantPublicId) {
      setError("Selected overrider has no public ID on record — try re-selecting them.");
      return;
    }
    setSubmitting(true);
    setError(null);
    try {
      await applyVetoOverride(accessToken, {
        draftPartId: draft.draftPartPublicId,
        playOrder: pick.playOrder,
        participantIdValue: overrider.participantPublicId,
        participantKind: overrider.participantKindValue.value,
      });
      const nextPicks = await refreshFromServer();
      setPendingAction(null);
      const newLandedCount = nextPicks.filter(
        (p) => p.status === "landed" || p.status === "vetoOverridden"
      ).length;
      if (totalPicks != null && newLandedCount >= totalPicks) onAllPositionsFilled();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to apply veto override.");
    } finally {
      setSubmitting(false);
    }
  }

  async function handleCommissionerOverride(pick: LocalPick) {
    if (submitting) return;
    setSubmitting(true);
    setError(null);
    try {
      await applyCommissionerOverride(accessToken, {
        draftPartId: draft.draftPartPublicId,
        playOrder: pick.playOrder,
      });
      await refreshFromServer();
      // Commissioner override does NOT land the pick on the board — it just
      // removes it from contention. Doesn't move landedCount, so no
      // onAllPositionsFilled check here.
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to apply commissioner override.");
    } finally {
      setSubmitting(false);
    }
  }

  if (!hydrated) {
    return <p className="text-sm text-sd-ink/50 font-mono">Loading picks…</p>;
  }

  return (
    <div className="space-y-6 max-w-2xl">
      <div className="bg-white border border-sd-ink/10 rounded p-4 flex items-center justify-between">
        <p className="text-sm text-sd-ink/70">
          {totalPicks == null
            ? "Loading board…"
            : `${landedCount} of ${totalPicks} positions landed`}
        </p>
        <p className="font-mono text-[11px] text-sd-ink/40 uppercase tracking-widest">
          Next: Play Order {nextPlayOrder}
        </p>
      </div>

      {/* Fungible token balances — only shown for drafts that use one */}
      {fungibleTokenName && (
        <div className="bg-white border border-sd-ink/10 rounded p-3">
          <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 uppercase mb-2">
            {fungibleTokenName} Remaining
          </p>
          <div className="flex flex-wrap gap-x-4 gap-y-1 text-sm text-sd-ink/70">
            {participants.map((p) => (
              <span key={p.participantIdValue}>
                {p.displayName ?? p.participantIdValue}:{" "}
                {fungibleRemaining.get(p.participantIdValue) ?? 0}
              </span>
            ))}
          </div>
        </div>
      )}

      {/* Picks so far */}
      {picks.length > 0 && (
        <div className="bg-white border border-sd-ink/10 rounded divide-y divide-sd-ink/5">
          {picks.map((p) => {
            const showingPicker = pendingAction?.playOrder === p.playOrder;
            return (
              <div key={p.playOrder} className="px-4 py-2.5 text-sm">
                <div className="flex items-center justify-between">
                  <div className="min-w-0">
                    <span className="font-mono text-[11px] text-sd-ink/40 mr-2">#{p.playOrder}</span>
                    <span className="font-medium text-sd-ink">{p.movieTitle}</span>
                    <span className="text-sd-ink/50"> — slot {p.position}, {p.participantDisplayName}</span>
                    {p.status !== "landed" && (
                      <span className="ml-2 text-[10px] font-mono uppercase tracking-widest text-sd-red">
                        {p.status === "vetoed"
                          ? `vetoed${p.vetoedByName ? ` by ${p.vetoedByName}` : ""} — slot open`
                          : p.status === "vetoOverridden"
                            ? `veto overridden${p.savedByName ? ` by ${p.savedByName}` : ""}`
                            : "commissioner override — film barred, slot still open"}
                      </span>
                    )}
                    {p.vetoHistory.length > 1 && (
                      <div className="mt-1 flex flex-wrap items-center gap-x-1 gap-y-0.5 text-[10px] font-mono text-sd-ink/40">
                        <span className="uppercase tracking-widest text-sd-ink/30">Full history:</span>
                        {p.vetoHistory.map((v) => (
                          <span key={v.sequence}>
                            {`vetoed by ${v.vetoedByName}`}
                            {v.isOverridden && ` → overridden by ${v.overriddenByName ?? "?"} → `}
                          </span>
                        ))}
                      </div>
                    )}
                  </div>
                  {!showingPicker && (
                    <div className="flex items-center gap-2 shrink-0">
                      {/* All three actions — Veto, Commissioner Override, and Veto
                          Override — are only offered on the most recent play. Once a
                          later pick has been made, an earlier slot is settled and none
                          of these should still be actionable from this form. A veto that
                          was overridden can still be vetoed again (the override itself
                          gets overridden), which is why "vetoOverridden" is included
                          alongside "landed" for the Veto button below. */}
                      {(p.status === "landed" || p.status === "vetoOverridden") &&
                        p.playOrder === lastPick?.playOrder && (
                          <button
                            type="button"
                            onClick={() =>
                              setPendingAction({ playOrder: p.playOrder, type: "veto", issuerIdValue: "" })
                            }
                            disabled={submitting}
                            className={BTN_SECONDARY}
                          >
                            Veto
                          </button>
                        )}
                      {p.status === "landed" && p.playOrder === lastPick?.playOrder && (
                        <button
                          type="button"
                          onClick={() => handleCommissionerOverride(p)}
                          disabled={submitting}
                          className={BTN_SECONDARY}
                        >
                          Commissioner Override
                        </button>
                      )}
                      {p.status === "vetoed" && allowsOverride && p.playOrder === lastPick?.playOrder && (
                        <button
                          type="button"
                          onClick={() =>
                            setPendingAction({
                              playOrder: p.playOrder,
                              type: "vetoOverride",
                              issuerIdValue: "",
                            })
                          }
                          disabled={submitting}
                          className={BTN_SECONDARY}
                        >
                          Veto Override
                        </button>
                      )}
                    </div>
                  )}
                </div>

                {showingPicker && pendingAction && (
                  <div className="flex items-center gap-2 mt-2 pt-2 border-t border-sd-ink/5">
                    <label className="text-[11px] font-mono text-sd-ink/50 uppercase tracking-widest">
                      {pendingAction.type === "veto" ? "Vetoed by" : "Overridden by"}
                    </label>
                    <select
                      className="border border-sd-ink/20 bg-sd-paper px-2 py-1 text-sm rounded flex-1"
                      value={pendingAction.issuerIdValue}
                      onChange={(e) =>
                        setPendingAction({ ...pendingAction, issuerIdValue: e.target.value })
                      }
                    >
                      <option value="">Select…</option>
                      {participants.map((participant) => {
                        const remaining = fungibleRemaining.get(participant.participantIdValue);
                        return (
                          <option key={participant.participantIdValue} value={participant.participantIdValue}>
                            {participant.displayName ?? participant.participantIdValue}
                            {participant.participantIdValue === p.participantIdValue ? " (self)" : ""}
                            {fungibleTokenName && remaining != null
                              ? ` — ${remaining} ${fungibleTokenName} left`
                              : ""}
                          </option>
                        );
                      })}
                    </select>
                    <button
                      type="button"
                      onClick={() =>
                        pendingAction.type === "veto"
                          ? handleVetoLast(pendingAction.issuerIdValue)
                          : handleVetoOverride(p, pendingAction.issuerIdValue)
                      }
                      disabled={!pendingAction.issuerIdValue || submitting}
                      className={BTN_SECONDARY}
                    >
                      Confirm
                    </button>
                    <button
                      type="button"
                      onClick={() => setPendingAction(null)}
                      className="text-[11px] font-mono text-sd-ink/40 uppercase tracking-widest hover:underline"
                    >
                      Cancel
                    </button>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}

      {/* Next pick form */}
      <div className="bg-white border border-sd-ink/10 rounded p-6 space-y-4">
        <div>
          <label className={LABEL}>Movie</label>
          {selectedMovie ? (
            <div className="flex items-center justify-between border border-sd-ink/20 rounded px-3 py-2">
              <span className="text-sm text-sd-ink">
                {selectedMovie.title} {selectedMovie.year ? `(${selectedMovie.year})` : ""}
              </span>
              <button
                type="button"
                onClick={() => setSelectedMovie(null)}
                className="text-sd-ink/40 hover:text-sd-red text-sm"
              >
                Change
              </button>
            </div>
          ) : (
            <>
              <MediaPicker
                accessToken={accessToken}
                onSelect={handleMediaSelect}
                disabled={importing}
                fixedSeriesTmdbId={draft.restrictedTvSeriesTmdbId ?? undefined}
              />
              {importing && <p className="text-[11px] font-mono text-sd-ink/40 mt-1">Importing…</p>}

              {/* Manual TMDb-ID fallback — movie-only. There's no episode
                  equivalent: TMDb has no title search over episodes, so the
                  browse-a-season flow above is already the "can't find it"
                  answer for TV. Kept unconditionally visible (previously
                  gated on the now-removed free-text query) rather than tied
                  to which MediaPicker mode is active, since MediaPicker
                  doesn't expose that upward. */}
              {manualEntryOpen ? (
                <div className="flex items-center gap-2 mt-2">
                  <input
                    type="number"
                    placeholder="TMDb ID"
                    className={`${INPUT} max-w-[140px]`}
                    value={manualTmdbId}
                    onChange={(e) => setManualTmdbId(e.target.value)}
                  />
                  <button
                    type="button"
                    onClick={handleManualTmdbSubmit}
                    disabled={importing}
                    className="text-[11px] font-mono text-sd-blue uppercase tracking-wide hover:underline disabled:opacity-40"
                  >
                    Add
                  </button>
                  <button
                    type="button"
                    onClick={() => {
                      setManualEntryOpen(false);
                      setManualTmdbId("");
                    }}
                    className="text-[11px] font-mono text-sd-ink/40 uppercase tracking-wide hover:underline"
                  >
                    Cancel
                  </button>
                </div>
              ) : (
                <button
                  type="button"
                  onClick={() => setManualEntryOpen(true)}
                  className="text-[11px] font-mono text-sd-ink/40 uppercase tracking-wide hover:underline mt-2"
                >
                  Movie not showing up? Enter TMDb ID directly →
                </button>
              )}
            </>
          )}
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className={LABEL}>Board Slot</label>
            <input
              type="number"
              min={1}
              className={INPUT}
              value={position}
              onChange={(e) => setPosition(e.target.value === "" ? "" : parseInt(e.target.value, 10))}
            />
          </div>
          <div>
            <label className={LABEL}>Played By</label>
            <select
              className={INPUT}
              value={participantIdValue}
              onChange={(e) => setParticipantIdValue(e.target.value)}
            >
              {participants.map((p) => (
                <option key={p.participantIdValue} value={p.participantIdValue}>
                  {p.displayName ?? p.participantIdValue}
                </option>
              ))}
            </select>
          </div>
        </div>

        {isHostless ? (
          <p className="text-[11px] font-mono text-sd-ink/40 uppercase tracking-widest">
            Hostless draft — reveal recipient is assigned automatically at play time.
          </p>
        ) : (
          <div>
            <label className={LABEL}>Revealed By (Host)</label>
            <select
              className={INPUT}
              value={revealedByHostId}
              onChange={(e) => setRevealedByHostId(e.target.value)}
            >
              {allHosts.map((h) => (
                <option key={h.hostPublicId} value={h.hostPublicId}>
                  {h.displayName}
                </option>
              ))}
            </select>
          </div>
        )}

        {error && (
          <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
            {error}
          </div>
        )}

        <button
          type="button"
          onClick={handleSubmitPick}
          disabled={
            !selectedMovie ||
            position === "" ||
            !participantIdValue ||
            (!isHostless && !revealedByHostId) ||
            submitting
          }
          className={BTN_PRIMARY}
        >
          {submitting ? "Recording…" : "Record Pick"}
        </button>
      </div>
    </div>
  );
}