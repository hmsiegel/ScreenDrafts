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
} from "@/services/admin/fetch-admin-drafts";
import { importAndResolve, type ResolvedMovie } from "@/lib/movie-resolve";
import { useMovieSearch } from "@/lib/use-movie-search";
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
  // Only populated on hydration from /gameplay — picks vetoed/overridden
  // within the current session already show this via the issuer picker's
  // own selection, these fields exist mainly so a resumed session (after a
  // refresh) still shows who did it, not just that it happened.
  vetoedByName: string | null;
  savedByName: string | null;
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

  // Next-pick form state
  const [position, setPosition] = useState<number | "">("");
  const [participantIdValue, setParticipantIdValue] = useState(participants[0]?.participantIdValue ?? "");
  const [revealedByHostId, setRevealedByHostId] = useState(primaryHost?.hostPublicId ?? "");
  const [query, setQuery] = useState("");
  const [selectedMovie, setSelectedMovie] = useState<ResolvedMovie | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [importing, setImporting] = useState(false);
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

  const { results, searching } = useMovieSearch(query, accessToken);

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

  // Hydrates from the server on mount rather than starting from an empty
  // list — this component previously tracked picks in local state only, so
  // navigating away mid-entry and coming back showed an empty board with no
  // veto/override actions available, even though every pick already
  // submitted was safely recorded server-side.
  useEffect(() => {
    (async () => {
      const gameplay = await getDraftPartGameplay(accessToken, draft.draftPartPublicId);
      if (gameplay) {
        setPicks(
          gameplay.picks
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
            }))
        );
      }
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

  async function handleSubmitPick() {
    if (!selectedMovie || position === "" || !participantIdValue || !revealedByHostId || submitting) {
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

      await seedRevealPick(accessToken, {
        draftPartId: draft.draftPartPublicId,
        playOrder: nextPlayOrder,
        actedByPublicId: revealedByHostId,
      });

      const newPick: LocalPick = {
        playOrder: nextPlayOrder,
        position: Number(position),
        movieTitle: selectedMovie.title,
        tmdbId: selectedMovie.tmdbId,
        participantIdValue,
        participantDisplayName: participant.displayName ?? "—",
        status: "landed",
        vetoedByName: null,
        savedByName: null,
      };
      setPicks((prev) => [...prev, newPick]);

      const newLandedCount = landedCount + 1;
      if (totalPicks != null && newLandedCount >= totalPicks) {
        onAllPositionsFilled();
      }

      setSelectedMovie(null);
      setQuery("");
      setPosition("");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to record pick.");
    } finally {
      setSubmitting(false);
    }
  }

  async function handleVetoLast(issuerIdValue: string) {
    if (!lastPick || lastPick.status !== "landed" || submitting) return;
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
      setPicks((prev) =>
        prev.map((p) =>
          p.playOrder === lastPick.playOrder
            ? { ...p, status: "vetoed", vetoedByName: issuer.displayName ?? null }
            : p
        )
      );
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
    setSubmitting(true);
    setError(null);
    try {
      await applyVetoOverride(accessToken, {
        draftPartId: draft.draftPartPublicId,
        playOrder: pick.playOrder,
        participantIdValue: overrider.participantIdValue,
        participantKind: overrider.participantKindValue.value,
      });
      setPicks((prev) =>
        prev.map((p) =>
          p.playOrder === pick.playOrder
            ? { ...p, status: "vetoOverridden", savedByName: overrider.displayName ?? null }
            : p
        )
      );
      setPendingAction(null);
      const newLandedCount = landedCount + 1;
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
      setPicks((prev) =>
        prev.map((p) =>
          p.playOrder === pick.playOrder ? { ...p, status: "commissionerOverridden" } : p
        )
      );
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
                  </div>
                  {!showingPicker && (
                    <div className="flex items-center gap-2 shrink-0">
                      {/* Veto and Commissioner Override are alternatives on
                          the same landed pick, not a veto-then-override
                          sequence — Veto stays restricted to the most
                          recent pick (the domain enforces this),
                          Commissioner Override doesn't have that
                          restriction so it's offered on any landed pick. */}
                      {p.status === "landed" && p.playOrder === lastPick?.playOrder && (
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
                      {p.status === "landed" && (
                        <button
                          type="button"
                          onClick={() => handleCommissionerOverride(p)}
                          disabled={submitting}
                          className={BTN_SECONDARY}
                        >
                          Commissioner Override
                        </button>
                      )}
                      {p.status === "vetoed" && allowsOverride && (
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
                      {participants.map((participant) => (
                        <option key={participant.participantIdValue} value={participant.participantIdValue}>
                          {participant.displayName ?? participant.participantIdValue}
                          {participant.participantIdValue === p.participantIdValue ? " (self)" : ""}
                        </option>
                      ))}
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
              <input
                type="text"
                className={INPUT}
                placeholder="Search movies…"
                value={query}
                onChange={(e) => setQuery(e.target.value)}
              />
              {searching && <p className="text-[11px] font-mono text-sd-ink/40 mt-1">Searching…</p>}
              {importing && <p className="text-[11px] font-mono text-sd-ink/40 mt-1">Importing…</p>}
              {results.length > 0 && (
                <div className="border border-sd-ink/10 rounded mt-2 max-h-48 overflow-y-auto">
                  {results.map((m) => {
                    const barred = barredTmdbIds.has(m.tmdbId);
                    return (
                      <button
                        key={`${m.tmdbId}-${m.mediaPublicId}`}
                        type="button"
                        onClick={() => handlePickMovie(m)}
                        disabled={barred}
                        className={`w-full text-left px-3 py-2 text-sm border-b border-sd-ink/5 last:border-0 ${
                          barred
                            ? "text-sd-ink/30 cursor-not-allowed"
                            : "hover:bg-sd-paper/60"
                        }`}
                      >
                        {m.title} {m.year ? `(${m.year})` : ""}
                        {barred && (
                          <span className="ml-2 text-[10px] font-mono text-sd-red uppercase">
                            commissioner overridden
                          </span>
                        )}
                        {!barred && !m.mediaPublicId && (
                          <span className="ml-2 text-[10px] font-mono text-sd-blue uppercase">
                            will import
                          </span>
                        )}
                      </button>
                    );
                  })}
                </div>
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

        {error && (
          <div className="border border-red-300 bg-red-50 text-red-800 text-sm px-4 py-3 rounded">
            {error}
          </div>
        )}

        <button
          type="button"
          onClick={handleSubmitPick}
          disabled={!selectedMovie || position === "" || !participantIdValue || !revealedByHostId || submitting}
          className={BTN_PRIMARY}
        >
          {submitting ? "Recording…" : "Record Pick"}
        </button>
      </div>
    </div>
  );
}