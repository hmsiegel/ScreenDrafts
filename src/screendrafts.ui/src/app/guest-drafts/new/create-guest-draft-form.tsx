// app/guest-drafts/new/create-guest-draft-form.tsx
'use client';

import { useState, useEffect, useRef, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import {
  createGuestDraft,
  addGuestDraftParticipant,
  searchGuestDrafters,
} from '../[guestDraftId]/live/gameplay-fetchers';
import type { GuestDrafterSummaryResponse, GuestDraftPositionInput } from '@/lib/dto';
// Reused directly, not ported — this is generic PositionConfig/validation UI
// with no DraftParts-specific API calls inside it, and GuestDraftPositionInput
// matches PositionConfig field-for-field. ASSUMPTION: `@/` maps to `src/`, per
// the file's own header comment (src/app/admin/drafts/new/positions-editor.tsx) —
// adjust the import if that alias resolves differently.
import {
  PositionsEditor,
  getDefaultPositions,
  validatePositions,
  type PositionConfig,
} from '@/app/admin/drafts/new/positions-editor';
import { GUEST_DRAFT_TYPE_LABELS } from '../guest-draft-type-labels';

const INPUT =
  'border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full';
const LABEL = 'block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1';
const SECTION_HEADING =
  'font-oswald font-bold text-[18px] tracking-wide uppercase text-sd-ink mb-4 pb-2 border-b border-sd-ink/10';

// GuestDraftType.cs's 5 SmartEnum values. Standard and MiniSuper are fixed
// (GuestDraftBoardTemplates.cs: Standard = A[7,6,4,2]/B[5,3,1], MiniSuper =
// A[5,3,1]/B[4,2] — the backend already knows these; CreateGuestDraftCommandHandler
// ignores Positions entirely for these two types and pulls the template
// itself). MiniMega, Super, and Mega are owner-defined.
const GUEST_DRAFT_TYPES = ['Standard', 'MiniMega', 'Mega', 'Super', 'MiniSuper'] as const;

const FIXED_TYPES = new Set<string>(['Standard', 'MiniSuper']);

interface Props {
  accessToken: string;
}

export function CreateGuestDraftForm({ accessToken }: Props) {
  const router = useRouter();

  const [title, setTitle] = useState('');
  const [type, setType] = useState<(typeof GUEST_DRAFT_TYPES)[number]>('Standard');
  const [draftDate, setDraftDate] = useState('');

  // Positions are shown at all times, same as canonical — read-only summary
  // for fixed types (positions-editor.tsx's own FixedSummary branch), fully
  // editable for MiniMega/Super/Mega. State always reflects the current
  // type's real layout so the read-only view isn't ever stale.
  const [positions, setPositions] = useState<PositionConfig[]>(getDefaultPositions('Standard'));
  const [totalPicks, setTotalPicks] = useState(
    getDefaultPositions('Standard').flatMap((p) => p.picks).length,
  );

  const [selectedDrafterIds, setSelectedDrafterIds] = useState<Set<string>>(new Set());

  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);
  const [participantWarnings, setParticipantWarnings] = useState<string[]>([]);
  const [createdDraftId, setCreatedDraftId] = useState<string | null>(null);

  const isFixed = FIXED_TYPES.has(type);
  const positionErrors = isFixed ? [] : validatePositions(positions, totalPicks);
  const canSubmit = title.trim() !== '' && (isFixed || positionErrors.length === 0);

  function handleTypeChange(next: (typeof GUEST_DRAFT_TYPES)[number]) {
    setType(next);
    const defaults = getDefaultPositions(next);
    setPositions(defaults);
    if (FIXED_TYPES.has(next)) {
      setTotalPicks(defaults.flatMap((p) => p.picks).length);
    }
  }

  async function handleCreate() {
    if (!canSubmit || creating) return;
    setCreating(true);
    setCreateError(null);
    setParticipantWarnings([]);
    try {
      const body: GuestDraftPositionInput[] = isFixed
        ? []
        : positions.map((p) => ({
            name: p.name,
            picks: p.picks,
            hasBonusVeto: p.hasBonusVeto,
            hasBonusVetoOverride: p.hasBonusVetoOverride,
            hasBonusFungibleToken: p.hasBonusFungibleToken,
          }));

      // CreateGuestDraftCommandHandler's NumberOfPicks < 1 check runs
      // unconditionally, before the fixed-vs-custom branch — fixed types
      // ignore this value for board-building purposes, but it still has to
      // be a real positive number. Derive it from the fixed template rather
      // than sending 0.
      const numberOfPicks = isFixed
        ? getDefaultPositions(type).flatMap((p) => p.picks).length
        : totalPicks;

      const created = await createGuestDraft(accessToken, {
        title: title.trim(),
        type,
        draftDate: draftDate || null,
        numberOfPicks,
        positions: body,
      });

      setCreatedDraftId(created.publicId);

      // AddParticipant is a separate call per drafter — CreateGuestDraftCommandHandler
      // adds none, not even the owner (see its own remarks). Sequential, not
      // Promise.all: these all mutate the same GuestDraft aggregate, and I'd
      // rather take the small latency hit than risk a concurrent-update
      // conflict on the very first thing this draft does.
      const warnings: string[] = [];
      for (const guestDrafterPublicId of selectedDrafterIds) {
        try {
          await addGuestDraftParticipant(accessToken, created.publicId, guestDrafterPublicId);
        } catch (e) {
          warnings.push(
            e instanceof Error
              ? `Failed to add a participant: ${e.message}`
              : 'Failed to add a participant.',
          );
        }
      }

      if (warnings.length > 0) {
        // Don't auto-navigate past a partial failure — the draft exists and
        // is fine, but silently landing on setup with fewer participants
        // than the owner picked would be confusing. Let them see it and
        // decide (setup page can retry the add same as this one does).
        setParticipantWarnings(warnings);
      } else {
        router.push(`/guest-drafts/${created.publicId}/setup`);
      }
    } catch (e) {
      setCreateError(e instanceof Error ? e.message : 'Failed to create guest draft.');
    } finally {
      setCreating(false);
    }
  }

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
          </div>
        </div>
      </section>

      <section>
        <h2 className={SECTION_HEADING}>
          Positions — {GUEST_DRAFT_TYPE_LABELS[type] ?? type}
        </h2>
        {!isFixed && (
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
        )}
        <PositionsEditor
          positions={positions}
          onChange={setPositions}
          totalPicks={totalPicks}
          readonly={isFixed}
        />
      </section>

      <ParticipantsSection
        accessToken={accessToken}
        selectedDrafterIds={selectedDrafterIds}
        onToggle={(publicId) =>
          setSelectedDrafterIds((prev) => {
            const next = new Set(prev);
            if (next.has(publicId)) next.delete(publicId);
            else next.add(publicId);
            return next;
          })
        }
      />

      {createError && <p className="text-sd-red text-sm font-mono">{createError}</p>}

      {participantWarnings.length > 0 && createdDraftId && (
        <div className="border border-yellow-400/40 bg-yellow-400/5 p-4 space-y-2">
          {participantWarnings.map((w, i) => (
            <p key={i} className="text-yellow-700 text-xs font-mono">
              {w}
            </p>
          ))}
          <button
            type="button"
            onClick={() => router.push(`/guest-drafts/${createdDraftId}/setup`)}
            className="text-sd-blue text-xs font-mono uppercase tracking-wide hover:underline"
          >
            Continue to setup anyway →
          </button>
        </div>
      )}

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

// ── Participant multi-select — checkboxes, applied after creation ──────────
// Visually the same pattern as canonical's participants-section.tsx (search +
// checkbox list + selected chips), but nothing here calls the backend except
// the search itself — selections are just local state until handleCreate
// applies them one AddParticipant call at a time, since the draft doesn't
// exist yet while this form is open. No team tab — GuestDrafts has no
// team-participant support yet (AddParticipant only accepts a
// GuestDrafterPublicId).

function ParticipantsSection({
  accessToken,
  selectedDrafterIds,
  onToggle,
}: {
  accessToken: string;
  selectedDrafterIds: Set<string>;
  onToggle: (guestDrafterPublicId: string) => void;
}) {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<GuestDrafterSummaryResponse[]>([]);
  const [searching, setSearching] = useState(false);
  const [displayNames, setDisplayNames] = useState<Map<string, string>>(new Map());
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  // Guards against out-of-order responses: typing "cl" then quickly "clay"
  // fires two calls, and if "cl"'s response resolves after "clay"'s, it would
  // otherwise overwrite the newer, more specific results with stale ones.
  const requestIdRef = useRef(0);

  const runSearch = useCallback(
    async (q: string) => {
      const requestId = ++requestIdRef.current;
      setSearching(true);
      try {
        const found = await searchGuestDrafters(accessToken, q || undefined);
        if (requestId !== requestIdRef.current) return; // a newer search has since started — drop this one
        setResults(found);
        setDisplayNames((prev) => {
          const next = new Map(prev);
          for (const d of found) next.set(d.publicId, d.displayName);
          return next;
        });
      } finally {
        if (requestId === requestIdRef.current) setSearching(false);
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

  return (
    <section>
      <h2 className={SECTION_HEADING}>Participants (Optional)</h2>

      {selectedDrafterIds.size > 0 && (
        <div className="flex flex-wrap gap-1.5 mb-4">
          {[...selectedDrafterIds].map((id) => (
            <span
              key={id}
              className="inline-flex items-center gap-1 px-2 py-0.5 bg-sd-ink text-white text-[11px] font-mono rounded"
            >
              {displayNames.get(id) ?? id}
              <button
                type="button"
                onClick={() => onToggle(id)}
                className="ml-0.5 hover:text-sd-red leading-none"
                aria-label="Remove"
              >
                ×
              </button>
            </span>
          ))}
        </div>
      )}

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
            results.map((d) => (
              <label
                key={d.publicId}
                className="flex items-center gap-2 px-3 py-1.5 text-sm text-sd-ink hover:bg-sd-ink/5 rounded cursor-pointer"
              >
                <input
                  type="checkbox"
                  checked={selectedDrafterIds.has(d.publicId)}
                  onChange={() => onToggle(d.publicId)}
                  className="accent-sd-red"
                />
                {d.displayName}
              </label>
            ))
          )}
        </div>
      </div>
    </section>
  );
}