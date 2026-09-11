// app/guest-drafts/[guestDraftId]/setup/setup-panel.tsx
'use client';

import { useState, useEffect, useRef, useCallback } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useGuestDraftLive } from '../live/guest-draft-context';
import {
  addGuestDraftParticipant,
  assignGuestDraftParticipantToPosition,
  searchGuestDrafters,
  setGuestDraftStatus,
} from '../live/gameplay-fetchers';
import type { GuestDrafterSummaryResponse } from '@/lib/dto';
import { GUEST_DRAFT_STATUS_ACTION } from '../../guest-draft-status-actions';


const INPUT =
  'border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full';
const SECTION_HEADING =
  'font-oswald font-bold text-[18px] tracking-wide uppercase text-sd-ink mb-4 pb-2 border-b border-sd-ink/10';

interface Props {
  accessToken: string;
  guestDraftId: string;
}

// This page is reachable any time the draft is still in Created status —
// creation no longer forces the owner through this in one sitting. They can
// add a couple participants, leave, come back later and add more, assign
// positions whenever, and hit Start whenever they're actually ready.
export function SetupPanel({ accessToken, guestDraftId }: Props) {
  const router = useRouter();
  const { gameplay, participants, draftPositions, refetch } = useGuestDraftLive();

  const [assigningPosition, setAssigningPosition] = useState<string | null>(null);
  const [selectedParticipant, setSelectedParticipant] = useState<Record<string, string>>({});
  const [assignError, setAssignError] = useState<string | null>(null);

  const [starting, setStarting] = useState(false);
  const [startError, setStartError] = useState<string | null>(null);

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
      router.push(`/guest-drafts/${guestDraftId}/live`);
    } catch (e) {
      setStartError(e instanceof Error ? e.message : 'Failed to start draft.');
      setStarting(false);
    }
  }

  return (
    <div className="space-y-8">
      <div className="p-3 bg-sd-ink/5 border border-sd-ink/10 flex items-center justify-between">
        <p className="text-xs text-sd-ink/50 font-mono">
          {gameplay.type} · {gameplay.status}
        </p>
        <Link
          href="/guest-drafts"
          className="text-sd-blue text-xs font-mono uppercase tracking-wide hover:underline"
        >
          ← My Guest Drafts
        </Link>
      </div>

      <AddParticipantsSection accessToken={accessToken} guestDraftId={guestDraftId} />

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

      <section>
        <h2 className={SECTION_HEADING}>Start</h2>
        {!canStart && (
          <p className="text-xs text-sd-ink/50 font-mono mb-3">
            Needs at least 2 participants and every position assigned before starting. Nothing
            here has to happen right now — come back whenever you're ready.
          </p>
        )}
        {startError && <p className="text-sd-red text-xs font-mono mb-3">{startError}</p>}
        <button
          type="button"
          onClick={handleStart}
          disabled={starting || !canStart}
          className="px-6 py-2.5 bg-sd-ink text-white font-oswald text-sm tracking-widest uppercase hover:bg-sd-ink/80 disabled:opacity-40 transition-colors"
        >
          {starting ? 'STARTING…' : 'START DRAFT'}
        </button>
      </section>
    </div>
  );
}

// ── Add participants — immediate add, unlike the create form's batched
// checkboxes. The draft already exists here, so each pick fires
// AddParticipant right away rather than collecting a batch for later. ─────

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
  // Same out-of-order-response guard as create-guest-draft-form.tsx's search.
  const requestIdRef = useRef(0);

  const alreadyAdded = new Set(participants.map((p) => p.participantPublicId));

  const runSearch = useCallback(
    async (q: string) => {
      const requestId = ++requestIdRef.current;
      setSearching(true);
      try {
        const found = await searchGuestDrafters(accessToken, q || undefined);
        if (requestId !== requestIdRef.current) return;
        setResults(found);
      } finally {
        if (requestId === requestIdRef.current) setSearching(false);
      }
    },
    [accessToken],
  );

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