// app/guest-drafts/new/create-guest-draft-form.tsx
'use client';

import { useState, useEffect, useRef, useCallback } from 'react';
import Link from 'next/link';
import {
  createGuestDraft,
  fetchGuestDraftGameplay,
  addGuestDraftParticipant,
  assignGuestDraftParticipantToPosition,
  searchGuestDrafters,
  setGuestDraftStatus,
} from '../[guestDraftId]/live/gameplay-fetchers';
import {
  GuestDraftLiveProvider,
  useGuestDraftLive,
} from '../[guestDraftId]/live/guest-draft-context';
import type {
  CreateGuestDraftPositionInput,
  GetGuestDraftGameplayResponse,
  GuestDrafterSummaryResponse,
} from '@/lib/dto';
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
// A[5,3,1]/B[4,2] — the backend already knows these; CreateGuestDraftCommandHandler
// ignores Positions/NumberOfPicks entirely for these two types and pulls the
// template itself). MiniMega, Super, and Mega are owner-defined: however many
// drafters, whatever picks each one gets, typically with bonus vetoes/overrides.
const GUEST_DRAFT_TYPES = ['Standard', 'MiniMega', 'Mega', 'Super', 'MiniSuper'] as const;

const FIXED_TYPES = new Set<string>(['Standard', 'MiniSuper']);

const FIXED_TYPE_PREVIEW: Record<string, string> = {
  Standard: 'Drafter A picks 7, 6, 4, 2 · Drafter B picks 5, 3, 1',
  MiniSuper: 'Drafter A picks 5, 3, 1 · Drafter B picks 4, 2',
};

// Display only — `type` itself still sends the raw SmartEnum name (e.g.
// "MiniSuper") to the backend. This just controls what the radio buttons
// (and section headings) show for it.
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
  const [draftDate, setDraftDate] = useState('');

  // Positions are only relevant/shown for MiniMega/Super/Mega — fixed types
  // never send these (the handler ignores them), but state still tracks a
  // sensible default so switching types doesn't lose in-progress edits.
  const [positions, setPositions] = useState<PositionConfig[]>(getDefaultPositions('Standard'));
  const [totalPicks, setTotalPicks] = useState(1);

  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);

  const [guestDraftId, setGuestDraftId] = useState<string | null>(null);
  const [initialGameplay, setInitialGameplay] = useState<GetGuestDraftGameplayResponse | null>(
    null,
  );

  const isFixed = FIXED_TYPES.has(type);
  const positionErrors = isFixed ? [] : validatePositions(positions, totalPicks);
  const canSubmit = title.trim() !== '' && (isFixed || positionErrors.length === 0);

  function handleTypeChange(next: (typeof GUEST_DRAFT_TYPES)[number]) {
    setType(next);
    if (!FIXED_TYPES.has(next)) {
      setPositions(getDefaultPositions(next));
    }
  }

  async function handleCreate() {
    if (!canSubmit || creating) return;
    setCreating(true);
    setCreateError(null);
    try {
      const body: CreateGuestDraftPositionInput[] = isFixed
        ? []
        : positions.map((p) => ({
            name: p.name,
            picks: p.picks,
            hasBonusVeto: p.hasBonusVeto,
            hasBonusVetoOverride: p.hasBonusVetoOverride,
            hasBonusFungibleToken: p.hasBonusFungibleToken,
          }));

      const created = await createGuestDraft(accessToken, {
        title: title.trim(),
        type,
        draftDate: draftDate || null,
        // Ignored server-side for fixed types, but CreateGuestDraftRequest.
        // NumberOfPicks is `required` — send the real picked total either way.
        numberOfPicks: isFixed ? 0 : totalPicks,
        positions: body,
      });
      const gameplay = await fetchGuestDraftGameplay(accessToken, created.publicId);
      setGuestDraftId(created.publicId);
      setInitialGameplay(gameplay);
    } catch (e) {
      setCreateError(e instanceof Error ? e.message : 'Failed to create guest draft.');
    } finally {
      setCreating(false);
    }
  }

  // ── Setup (add participants, assign positions, start) ───────────────────
  if (guestDraftId && initialGameplay) {
    return (
      <GuestDraftLiveProvider
        guestDraftId={guestDraftId}
        accessToken={accessToken}
        initialGameplay={initialGameplay}
      >
        <SetupPanel accessToken={accessToken} guestDraftId={guestDraftId} />
      </GuestDraftLiveProvider>
    );
  }

  // ── Create ────────────────────────────────────────────────────────────────
  return (
    <div className="space-y-6">
      <section>
        <h2 className={SECTION_HEADING}>Details</h2>
        <div className="space-y-4">
          <div>
            <label className={LABEL}>Title</label>
            <input
              type="text"
              className={INPUT}
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="e.g. Best Heist Movies"
            />
          </div>
          <div>
            <label className={LABEL}>Draft Date (optional)</label>
            <input
              type="date"
              className={`${INPUT} max-w-[200px]`}
              value={draftDate}
              onChange={(e) => setDraftDate(e.target.value)}
            />
          </div>
          <div>
            <label className={LABEL}>Type</label>
            <div className="flex gap-4">
              {GUEST_DRAFT_TYPES.map((t) => (
                <label key={t} className="flex items-center gap-2 text-sm text-sd-ink cursor-pointer">
                  <input
                    type="radio"
                    checked={type === t}
                    onChange={() => handleTypeChange(t)}
                    className="accent-sd-red"
                  />
                  {GUEST_DRAFT_TYPE_LABELS[t] ?? t}
                </label>
              ))}
            </div>
            <p className="text-xs text-sd-ink/50 font-mono mt-2">
              {FIXED_TYPE_PREVIEW[type] ?? 'Define positions below.'}
            </p>
          </div>
        </div>
      </section>

      {!isFixed && (
        <section>
          <h2 className={SECTION_HEADING}>
            Define Positions — {GUEST_DRAFT_TYPE_LABELS[type] ?? type}
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
      )}

      {createError && <p className="text-sd-red text-sm font-mono">{createError}</p>}

      <button
        type="button"
        onClick={handleCreate}
        disabled={!canSubmit || creating}
        className="px-6 py-2.5 bg-sd-ink text-white font-oswald text-sm tracking-widest uppercase hover:bg-sd-ink/80 disabled:opacity-40 transition-colors"
      >
        {creating ? 'CREATING…' : 'CREATE'}
      </button>
    </div>
  );
}

// ── Setup: add participants, assign positions, start ────────────────────────

function SetupPanel({ accessToken, guestDraftId }: { accessToken: string; guestDraftId: string }) {
  const { gameplay, participants, draftPositions, refetch } = useGuestDraftLive();

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

  const canStart = participants.length >= 2 && draftPositions.every((p) => p.assignedParticipantId);

  async function handleAssign(positionPublicId: string) {
    const guestDrafterPublicId = selectedParticipant[positionPublicId];
    if (!guestDrafterPublicId) return;
    setAssigningPosition(positionPublicId);
    setAssignError(null);
    try {
      await assignGuestDraftParticipantToPosition(
        accessToken,
        guestDraftId,
        positionPublicId,
        guestDrafterPublicId,
      );
      await refetch();
    } catch (e) {
      setAssignError(e instanceof Error ? e.message : 'Failed to assign position.');
    } finally {
      setAssigningPosition(null);
    }
  }

  async function handleStart() {
    if (starting || !canStart) return;
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

      <AddParticipantsSection accessToken={accessToken} guestDraftId={guestDraftId} />

      {/* Assign — participants must be added first (refetch above keeps this
          list current) before they can be picked for a position. */}
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
                    {/* value is the GuestDrafter's PublicId — same field
                        AddParticipant/AssignParticipantToPosition both key
                        off, populated on GameplayParticipantResponse as
                        participantPublicId. */}
                    {participants.map((p) => (
                      <option key={p.participantPublicId} value={p.participantPublicId}>
                        {p.displayName}
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
        {!canStart && (
          <p className="text-xs text-sd-ink/50 font-mono mb-3">
            Needs at least 2 participants and every position assigned before starting.
          </p>
        )}
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
            disabled={starting || !canStart}
            className="px-6 py-2.5 bg-sd-ink text-white font-oswald text-sm tracking-widest uppercase hover:bg-sd-ink/80 disabled:opacity-40 transition-colors"
          >
            {starting ? 'STARTING…' : 'START DRAFT'}
          </button>
        )}
      </section>
    </div>
  );
}

// ── Add participants — real search now that SearchGuestDraftersQuery exists ─
// Mirrors participants-section.tsx's debounced-search-then-select pattern,
// but simpler: no team tab (GuestDrafts has no team-participant support yet —
// AddParticipantCommand only accepts a GuestDrafterPublicId), and each pick
// fires AddParticipant immediately rather than collecting a batch for one
// later submit, since this draft already exists.

function AddParticipantsSection({
  accessToken,
  guestDraftId,
}: {
  accessToken: string;
  guestDraftId: string;
}) {
  const { participants, refetch } = useGuestDraftLive();

  const [query, setQuery] = useState('');
  const [results, setResults] = useState<GuestDrafterSummaryResponse[]>([]);
  const [searching, setSearching] = useState(false);
  const [adding, setAdding] = useState<string | null>(null);
  const [addError, setAddError] = useState<string | null>(null);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const alreadyAdded = new Set(participants.map((p) => p.participantPublicId));

  const runSearch = useCallback(
    async (q: string) => {
      setSearching(true);
      try {
        const found = await searchGuestDrafters(accessToken, q || undefined);
        setResults(found);
      } finally {
        setSearching(false);
      }
    },
    [accessToken],
  );

  // Initial load — fetch first page with no query so the list isn't empty on open.
  useEffect(() => {
    void runSearch('');
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  function handleQueryChange(value: string) {
    setQuery(value);
    if (debounceRef.current) clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(() => runSearch(value), 300);
  }

  async function handleAdd(guestDrafterPublicId: string) {
    setAdding(guestDrafterPublicId);
    setAddError(null);
    try {
      await addGuestDraftParticipant(accessToken, guestDraftId, guestDrafterPublicId);
      await refetch();
    } catch (e) {
      setAddError(e instanceof Error ? e.message : 'Failed to add participant.');
    } finally {
      setAdding(null);
    }
  }

  return (
    <section>
      <h2 className={SECTION_HEADING}>Add Participants</h2>

      {participants.length > 0 && (
        <div className="flex flex-wrap gap-1.5 mb-4">
          {participants.map((p) => (
            <span
              key={p.participantPublicId}
              className="inline-flex items-center px-2 py-0.5 bg-sd-ink text-white text-[11px] font-mono rounded"
            >
              {p.displayName}
            </span>
          ))}
        </div>
      )}

      {addError && <p className="text-sd-red text-xs font-mono mb-3">{addError}</p>}

      <div className="border border-sd-ink/10 rounded p-4 bg-white">
        <input
          type="text"
          placeholder="Search drafters…"
          className={`${INPUT} mb-3`}
          value={query}
          onChange={(e) => handleQueryChange(e.target.value)}
        />
        <div className="max-h-48 overflow-y-auto space-y-1">
          {searching ? (
            <p className="text-sm text-sd-ink/40 font-mono px-1">Loading…</p>
          ) : results.length === 0 ? (
            <p className="text-sm text-sd-ink/40 font-mono px-1">No drafters found.</p>
          ) : (
            results.map((d) => {
              const isAdded = alreadyAdded.has(d.publicId);
              return (
                <div
                  key={d.publicId}
                  className="flex items-center justify-between gap-2 px-3 py-1.5 text-sm text-sd-ink hover:bg-sd-ink/5 rounded"
                >
                  <span>{d.displayName}</span>
                  <button
                    type="button"
                    onClick={() => handleAdd(d.publicId)}
                    disabled={isAdded || adding === d.publicId}
                    className="shrink-0 px-2.5 py-1 border border-sd-blue text-sd-blue font-oswald text-[11px] tracking-widest hover:bg-sd-blue hover:text-white disabled:opacity-40 transition-colors"
                  >
                    {isAdded ? 'ADDED' : adding === d.publicId ? '…' : '+ ADD'}
                  </button>
                </div>
              );
            })
          )}
        </div>
      </div>
    </section>
  );
}