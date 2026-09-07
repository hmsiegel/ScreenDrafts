// app/guest-drafts/new/create-guest-draft-form.tsx
'use client';

import { useState } from 'react';
import Link from 'next/link';
import {
  createGuestDraft,
  setFixedGuestDraftBoardLayout,
  setCustomGuestDraftPositions,
  fetchGuestDraftGameplay,
  inviteGuestDraftParticipant,
  assignGuestDraftParticipantToPosition,
  setGuestDraftStatus,
} from '../[guestDraftId]/live/gameplay-fetchers';
import {
  GuestDraftLiveProvider,
  useGuestDraftLive,
} from '../[guestDraftId]/live/guest-draft-context';
import type { GetGuestDraftGameplayResponse, PositionInput } from '@/lib/dto';
// Reused directly, not ported — this is generic PositionConfig/validation UI
// with no DraftParts-specific API calls inside it, and PositionInput matches
// PositionConfig field-for-field. ASSUMPTION: `@/` maps to `src/`, per the
// file's own header comment (src/app/admin/drafts/new/positions-editor.tsx) —
// adjust the import if that alias resolves differently.
import {
  PositionsEditor,
  getDefaultPositions,
  validatePositions,
  type PositionConfig,
} from '@/app/admin/drafts/new/positions-editor';

// GuestDraftStatusAction.cs's raw SmartEnum ints.
const GUEST_DRAFT_STATUS_ACTION = {
  Start: 1,
  Complete: 2,
} as const;

const INPUT =
  'border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full';
const LABEL = 'block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1';
const SECTION_HEADING =
  'font-oswald font-bold text-[18px] tracking-wide uppercase text-sd-ink mb-4 pb-2 border-b border-sd-ink/10';

// GuestDraftType.cs's 5 SmartEnum values. Standard and MiniSuper are fixed
// (GuestDraftBoardTemplates.cs: Standard = A[7,6,4,2]/B[5,3,1], MiniSuper =
// A[5,3,1]/B[4,2] — the backend already knows these, no client-side template
// needed). MiniMega, Super, and Mega are owner-defined: however many drafters,
// whatever picks each one gets, typically with bonus vetoes/overrides.
const GUEST_DRAFT_TYPES = ['Standard', 'MiniMega', 'Mega', 'Super', 'MiniSuper'] as const;

const FIXED_TYPES = new Set<string>(['Standard', 'MiniSuper']);

const FIXED_TYPE_PREVIEW: Record<string, string> = {
  Standard: 'Drafter A picks 7, 6, 4, 2 · Drafter B picks 5, 3, 1',
  MiniSuper: 'Drafter A picks 5, 3, 1 · Drafter B picks 4, 2',
};

// Display only — `type` itself still sends the raw SmartEnum name (e.g.
// "MiniSuper") to the backend. This just controls what the radio buttons
// (and the "Define Positions" heading) show for it.
const GUEST_DRAFT_TYPE_LABELS: Record<string, string> = {
  Standard: 'Standard',
  MiniMega: 'Mini-Mega',
  Mega: 'Mega',
  Super: 'Super',
  MiniSuper: 'Mini-Super',
};

interface Props {
  accessToken: string;
}

export function CreateGuestDraftForm({ accessToken }: Props) {
  const [title, setTitle] = useState('');
  const [type, setType] = useState<(typeof GUEST_DRAFT_TYPES)[number]>('Standard');
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);

  const [guestDraftId, setGuestDraftId] = useState<string | null>(null);
  const [initialGameplay, setInitialGameplay] = useState<GetGuestDraftGameplayResponse | null>(
    null,
  );
  const [positionsConfirmed, setPositionsConfirmed] = useState(false);

  async function handleCreate() {
    if (!title.trim() || creating) return;
    setCreating(true);
    setCreateError(null);
    try {
      const created = await createGuestDraft(accessToken, { title: title.trim(), type });
      const isFixed = FIXED_TYPES.has(type);
      if (isFixed) {
        await setFixedGuestDraftBoardLayout(accessToken, created.publicId);
      }
      const gameplay = await fetchGuestDraftGameplay(accessToken, created.publicId);
      setGuestDraftId(created.publicId);
      setInitialGameplay(gameplay);
      setPositionsConfirmed(isFixed);
    } catch (e) {
      setCreateError(e instanceof Error ? e.message : 'Failed to create guest draft.');
    } finally {
      setCreating(false);
    }
  }

  // ── Step 2: define positions (custom types only), then invite + assign ──
  if (guestDraftId && initialGameplay) {
    return (
      <GuestDraftLiveProvider
        guestDraftId={guestDraftId}
        accessToken={accessToken}
        initialGameplay={initialGameplay}
      >
        {!positionsConfirmed ? (
          <DefinePositionsStep
            accessToken={accessToken}
            guestDraftId={guestDraftId}
            onDone={() => setPositionsConfirmed(true)}
          />
        ) : (
          <SetupPanel accessToken={accessToken} guestDraftId={guestDraftId} />
        )}
      </GuestDraftLiveProvider>
    );
  }

  // ── Step 1: title + type ─────────────────────────────────────────────────
  return (
    <div className="space-y-6">
      <section>
        <h2 className={SECTION_HEADING}>Details</h2>
        <div className="space-y-4">
          <div>
            <label className={LABEL}>
              Title
            </label>
            <input
              type="text"
              className={INPUT}
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="e.g. Best Heist Movies"
            />
          </div>
          <div>
            <label className={LABEL}>
              Type
            </label>
            <div className="flex gap-4">
              {GUEST_DRAFT_TYPES.map((t) => (
                <label key={t} className="flex items-center gap-2 text-sm text-sd-ink cursor-pointer">
                  <input
                    type="radio"
                    checked={type === t}
                    onChange={() => setType(t)}
                    className="accent-sd-red"
                  />
                  {GUEST_DRAFT_TYPE_LABELS[t] ?? t}
                </label>
              ))}
            </div>
            <p className="text-xs text-sd-ink/50 font-mono mt-2">
              {FIXED_TYPE_PREVIEW[type] ?? "You'll define drafters and picks after creating."}
            </p>
          </div>
        </div>
      </section>

      {createError && <p className="text-sd-red text-sm font-mono">{createError}</p>}

      <button
        type="button"
        onClick={handleCreate}
        disabled={!title.trim() || creating}
        className="px-6 py-2.5 bg-sd-ink text-white font-oswald text-sm tracking-widest uppercase hover:bg-sd-ink/80 disabled:opacity-40 transition-colors"
      >
        {creating ? 'CREATING…' : 'CREATE'}
      </button>
    </div>
  );
}

// ── Step 2a: define positions (MiniMega / Super / Mega only) ────────────────

function DefinePositionsStep({
  accessToken,
  guestDraftId,
  onDone,
}: {
  accessToken: string;
  guestDraftId: string;
  onDone: () => void;
}) {
  const { gameplay, refetch } = useGuestDraftLive();

  // getDefaultPositions only has real templates for Standard/MiniSuper/SpeedDraft
  // (none of which reach this step) — for MiniMega/Super/Mega it falls through
  // to its default case, two empty A/B positions, which is a reasonable blank
  // starting point for the owner to build from.
  const [positions, setPositions] = useState<PositionConfig[]>(
    getDefaultPositions(gameplay.type ?? ''),
  );
  // Matches create-draft-form.tsx's "Max Positions" input exactly (same
  // labeling, same min={1}/parseInt-fallback-to-1 pattern) — canonical's
  // version has confirmed per-type defaults via getMaxPositionsConfig
  // (Standard/SpeedDraft/MiniSuper); no such defaults are confirmed for
  // MiniMega/Super/Mega, so this starts at 1 rather than guessing a number.
  const [totalPicks, setTotalPicks] = useState(1);
  const [saving, setSaving] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);

  const errors = validatePositions(positions, totalPicks);

  async function handleSave() {
    if (errors.length > 0 || saving) return;
    setSaving(true);
    setSaveError(null);
    try {
      const body: PositionInput[] = positions.map((p) => ({
        name: p.name,
        picks: p.picks,
        hasBonusVeto: p.hasBonusVeto,
        hasBonusVetoOverride: p.hasBonusVetoOverride,
        hasBonusFungibleToken: p.hasBonusFungibleToken,
      }));
      await setCustomGuestDraftPositions(accessToken, guestDraftId, body);
      await refetch();
      onDone();
    } catch (e) {
      setSaveError(e instanceof Error ? e.message : 'Failed to save positions.');
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="space-y-6">
      <section>
        <h2 className={SECTION_HEADING}>
          Define Positions — {GUEST_DRAFT_TYPE_LABELS[gameplay.type ?? ''] ?? gameplay.type}
        </h2>
        <div className="mb-4 max-w-[160px]">
          <label className={LABEL}>Max Positions</label>
          <input
            type="number"
            min={1}
            className={INPUT}
            value={totalPicks}
            onChange={(e) => setTotalPicks(parseInt(e.target.value, 10) || 1)}
          />
        </div>
        <PositionsEditor positions={positions} onChange={setPositions} totalPicks={totalPicks} />
      </section>

      {saveError && <p className="text-sd-red text-sm font-mono">{saveError}</p>}

      <button
        type="button"
        onClick={handleSave}
        disabled={errors.length > 0 || saving}
        className="px-6 py-2.5 bg-sd-ink text-white font-oswald text-sm tracking-widest uppercase hover:bg-sd-ink/80 disabled:opacity-40 transition-colors"
      >
        {saving ? 'SAVING…' : 'SAVE POSITIONS'}
      </button>
    </div>
  );
}

// ── Step 2b: invite + assign ─────────────────────────────────────────────────

function SetupPanel({ accessToken, guestDraftId }: { accessToken: string; guestDraftId: string }) {
  const { gameplay, participants, draftPositions, refetch } = useGuestDraftLive();

  const [inviteeUserPublicId, setInviteeUserPublicId] = useState('');
  const [inviting, setInviting] = useState(false);
  const [inviteError, setInviteError] = useState<string | null>(null);

  const [assigningPosition, setAssigningPosition] = useState<string | null>(null);
  const [selectedParticipant, setSelectedParticipant] = useState<Record<string, string>>({});
  const [assignError, setAssignError] = useState<string | null>(null);

  const [starting, setStarting] = useState(false);
  const [startError, setStartError] = useState<string | null>(null);
  // Local flag rather than checking gameplay.status against a literal —
  // I don't actually know what string value status takes once started
  // (never confirmed), so this only reflects "we successfully called
  // start from this screen," not the true server state. refetch() after
  // a successful call still keeps gameplay.status itself accurate for
  // anything else that reads it.
  const [justStarted, setJustStarted] = useState(false);

  async function handleInvite() {
    if (!inviteeUserPublicId.trim() || inviting) return;
    setInviting(true);
    setInviteError(null);
    try {
      await inviteGuestDraftParticipant(accessToken, guestDraftId, inviteeUserPublicId.trim());
      setInviteeUserPublicId('');
      await refetch();
    } catch (e) {
      setInviteError(e instanceof Error ? e.message : 'Failed to invite participant.');
    } finally {
      setInviting(false);
    }
  }

  async function handleAssign(positionPublicId: string) {
    const participantPublicId = selectedParticipant[positionPublicId];
    if (!participantPublicId) return;
    setAssigningPosition(positionPublicId);
    setAssignError(null);
    try {
      await assignGuestDraftParticipantToPosition(
        accessToken,
        guestDraftId,
        positionPublicId,
        participantPublicId,
      );
      await refetch();
    } catch (e) {
      setAssignError(e instanceof Error ? e.message : 'Failed to assign position.');
    } finally {
      setAssigningPosition(null);
    }
  }

  async function handleStart() {
    if (starting) return;
    setStarting(true);
    setStartError(null);
    try {
      await setGuestDraftStatus(accessToken, guestDraftId, GUEST_DRAFT_STATUS_ACTION.Start);
      setJustStarted(true);
      await refetch();
    } catch (e) {
      setStartError(e instanceof Error ? e.message : 'Failed to start draft.');
    } finally {
      setStarting(false);
    }
  }

  return (
    <div className="space-y-8">
      <div className="p-3 bg-sd-ink/5 border border-sd-ink/10">
        <p className="font-oswald text-sd-ink font-bold text-sm tracking-wider">
          {gameplay.title}
        </p>
        <p className="text-xs text-sd-ink/50 font-mono mt-0.5">
          {GUEST_DRAFT_TYPE_LABELS[gameplay.type ?? ''] ?? gameplay.type} · {gameplay.status} · id:{' '}
          {guestDraftId}
        </p>
      </div>

      {/* Invite — raw user public id for now. There's no username-search
          endpoint on the backend yet, so this is a paste-the-id stopgap
          until that exists and a real typeahead can replace it. */}
      <section>
        <h2 className={SECTION_HEADING}>Invite Participants</h2>
        <div className="flex gap-2 mb-3">
          <input
            type="text"
            className={INPUT}
            value={inviteeUserPublicId}
            onChange={(e) => setInviteeUserPublicId(e.target.value)}
            placeholder="User public id (no search yet — paste directly)"
          />
          <button
            type="button"
            onClick={handleInvite}
            disabled={!inviteeUserPublicId.trim() || inviting}
            className="shrink-0 px-4 py-2 bg-sd-blue text-white font-oswald text-xs tracking-widest uppercase hover:bg-sd-blue/80 disabled:opacity-40 transition-colors"
          >
            {inviting ? 'INVITING…' : 'INVITE'}
          </button>
        </div>
        {inviteError && <p className="text-sd-red text-xs font-mono mb-3">{inviteError}</p>}

        {participants.length === 0 ? (
          <p className="text-sm text-sd-ink/40 font-mono">No participants invited yet.</p>
        ) : (
          <ul className="space-y-1">
            {participants.map((p) => (
              <li
                key={p.participantPublicId ?? p.participantId}
                className="text-sm text-sd-ink font-mono px-3 py-1.5 bg-sd-ink/5"
              >
                {p.participantName}
              </li>
            ))}
          </ul>
        )}
      </section>

      {/* Assign — participants must be invited first (refetch above keeps
          this list current) before they can be picked for a position. */}
      <section>
        <h2 className={SECTION_HEADING}>Assign Positions</h2>
        {assignError && <p className="text-sd-red text-xs font-mono mb-3">{assignError}</p>}
        <div className="space-y-3">
          {draftPositions.map((pos) => (
            <div
              key={pos.positionPublicId}
              className="flex items-center gap-3 border border-sd-ink/10 p-3"
            >
              <span className="font-oswald font-bold text-sd-ink w-8 shrink-0">
                {pos.positionName}
              </span>
              <span className="text-[11px] text-sd-ink/50 font-mono shrink-0">
                picks {pos.ownedBoardSlots?.slice().sort((a, b) => b - a).join(', ')}
              </span>
              <div className="flex-1" />
              {pos.assignedParticipantName ? (
                <span className="text-sm text-sd-ink font-mono">
                  {pos.assignedParticipantName}
                </span>
              ) : (
                <>
                  <select
                    className={`${INPUT} w-auto`}
                    value={selectedParticipant[pos.positionPublicId ?? ''] ?? ''}
                    onChange={(e) =>
                      setSelectedParticipant((prev) => ({
                        ...prev,
                        [pos.positionPublicId ?? '']: e.target.value,
                      }))
                    }
                  >
                    <option value="">Select participant…</option>
                    {participants.map((p) => (
                      <option key={p.participantPublicId} value={p.participantPublicId}>
                        {p.participantName}
                      </option>
                    ))}
                  </select>
                  <button
                    type="button"
                    onClick={() => handleAssign(pos.positionPublicId ?? '')}
                    disabled={
                      !selectedParticipant[pos.positionPublicId ?? ''] ||
                      assigningPosition === pos.positionPublicId
                    }
                    className="shrink-0 px-3 py-1.5 border border-sd-red text-sd-red font-oswald text-xs tracking-widest hover:bg-sd-red hover:text-white disabled:opacity-40 transition-colors"
                  >
                    {assigningPosition === pos.positionPublicId ? '…' : 'ASSIGN'}
                  </button>
                </>
              )}
            </div>
          ))}
        </div>
      </section>

      {/* Start */}
      <section>
        <h2 className={SECTION_HEADING}>Start</h2>
        {startError && <p className="text-sd-red text-xs font-mono mb-3">{startError}</p>}
        {justStarted ? (
          <div className="space-y-3">
            <p className="text-sm text-sd-ink font-mono">Draft started.</p>
            <Link
              href={`/guest-drafts/${guestDraftId}/live`}
              className="inline-block px-6 py-2.5 bg-sd-blue text-white font-oswald text-sm tracking-widest uppercase hover:bg-sd-blue/80 transition-colors"
            >
              GO TO LIVE DRAFT
            </Link>
          </div>
        ) : (
          <button
            type="button"
            onClick={handleStart}
            disabled={starting}
            className="px-6 py-2.5 bg-sd-ink text-white font-oswald text-sm tracking-widest uppercase hover:bg-sd-ink/80 disabled:opacity-40 transition-colors"
          >
            {starting ? 'STARTING…' : 'START DRAFT'}
          </button>
        )}
      </section>
    </div>
  );
}