import { auth } from "@/auth";
import { redirect } from "next/navigation";
import { getDraftPart, getDraftPool } from "@/services/drafts/fetch-draft-parts";
import PositionSlotsGrid from "@/components/gameplay/position-slots-grid";
import ParticipantStatusStrip from "@/components/gameplay/participant-status-strip";
import DraftPoolPanel from "@/components/gameplay/draft-pool-panel";
import DraftBoardPanel from "@/components/gameplay/draft-board-panel";
import { Metadata } from "next";

// TODO: connect SignalR hub here — DraftHub
// Hub needs OnMessageReceived config to read JWT from query string for WebSocket handshake

export const dynamic = "force-dynamic";

export async function generateMetadata(): Promise<Metadata> {
  return { title: "Gameplay" };
}

const SUPER_DRAFT_TYPE = 3;

const STATUS_LABELS: Record<number, string> = {
  0: "Created",
  1: "In Progress",
  2: "Active",
  3: "Paused",
  4: "Completed",
  5: "Cancelled",
};

export default async function GameplayPage({ params }: { params: Promise<{ draftPartId: string }> }) {
  const session = await auth();
  if (!session?.accessToken) redirect("/sign-in");

  const { draftPartId } = await params;
  const part = await getDraftPart(session.accessToken, draftPartId);

  if (!part) redirect("/my-drafts");

  // TODO: check isJoined for this draftPartId once backend returns that field
  // if (!part.isJoined) redirect("/my-drafts");

  const isSuper = part.draftType?.value === SUPER_DRAFT_TYPE;
  const pool = isSuper ? await getDraftPool(session.accessToken, part.draftId) : null;

  const isHost = false; // TODO: derive from session roles + part.participants once backend returns this
  const isJoinedDrafter = !isHost;

  // For Super drafts, resolve pool movie metadata
  // TODO: replace with real metadata lookup once endpoint is confirmed
  const poolMovies = (pool?.tmdbIds ?? []).map((tmdbId) => ({
    tmdbId,
    title: `Movie ${tmdbId}`,
    year: 0,
    posterUrl: undefined as string | undefined,
  }));

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="px-6 md:px-10 py-10 max-w-[1400px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">
          / GAMEPLAY / {part.draftName?.toUpperCase()}
        </p>

        {/* SignalR placeholder banner */}
        <div className="mb-6 px-4 py-2 border border-sd-ink/10 bg-sd-paper font-mono text-xs text-sd-ink/60">
          Live updates coming soon — refresh to see latest picks.
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-[1fr_360px] gap-8">
          {/* Left column — Live Board */}
          <div className="space-y-6">
            {/* Header */}
            <div className="bg-white border border-sd-ink/10">
              <div className="flex items-center gap-3 px-6 py-4 bg-sd-ink">
                <div className="w-1 h-6 bg-sd-red shrink-0" />
                <div>
                  <h1 className="font-oswald font-bold text-2xl uppercase leading-none text-white">
                    {part.draftName}
                  </h1>
                  <p className="font-mono text-xs text-white/60 mt-0.5">
                    PART {part.partNumber} —{" "}
                    <span>{STATUS_LABELS[part.status?.value ?? -1] ?? "Unknown"}</span>
                  </p>
                </div>
              </div>
            </div>

            {/* Position slots */}
            {part.positions.length > 0 ? (
              <PositionSlotsGrid positions={part.positions} />
            ) : (
              <p className="font-mono text-sm text-sd-ink/40">No positions configured.</p>
            )}

            {/* Participant status */}
            <ParticipantStatusStrip participants={part.participants} />
          </div>

          {/* Right column — Movie Source */}
          <div>
            {isSuper && pool ? (
              <DraftPoolPanel movies={poolMovies} />
            ) : (
              <DraftBoardPanel
                isHost={isHost}
                accessToken={session.accessToken}
                draftPartId={draftPartId}
                board={[]}
                candidateList={[]}
              />
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
