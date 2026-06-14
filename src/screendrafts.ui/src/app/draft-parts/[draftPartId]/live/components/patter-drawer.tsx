// app/draft-parts/[draftPartId]/live/components/patter-drawer.tsx
'use client';

import { GetDraftPartGameplayResponse } from '@/lib/dto';
import { useState } from 'react';

interface Props {
  gameplay: GetDraftPartGameplayResponse;
}

function buildGameRulesPatter(gameplay: GetDraftPartGameplayResponse): string {
  const isStandard = gameplay.draftType === 'Standard';
  const positions = gameplay.draftPositions ?? [];
  const coHost = gameplay.hosts?.find((h) => !h.isPrimary)?.hostName ?? '[Co-Host]';

  if (isStandard && positions.length === 2) {
    const a = positions.find((p) => p.positionName === 'A');
    const b = positions.find((p) => p.positionName === 'B');
    const nameA = a?.assignedParticipantName ?? 'Drafter A';
    const nameB = b?.assignedParticipantName ?? 'Drafter B';
    const slotsA = (a?.ownedBoardSlots ?? []).sort((x, y) => y - x).join(', ');
    const slotsB = (b?.ownedBoardSlots ?? []).sort((x, y) => y - x).join(', ');
    const countA = (a?.ownedBoardSlots ?? []).length;
    const countB = (b?.ownedBoardSlots ?? []).length;

    return `Screen Drafts is a draft in a unique style. Our guest GMs, ${nameA} and ${nameB}, are going to be alternating placing picks on the single definitive Best-Of list for today's topic which is ${gameplay.draftTitle}.

If you are Drafter A, you will be placing picks ${slotsA} — ${countA} picks overall. If you are Drafter B, you will be placing picks ${slotsB} — fewer picks overall, but they are weighted higher on the list and you get the number 1 pick.

Pick placement is determined by a trivia round prepared by ${coHost}. Whoever wins trivia decides whether to be Drafter A or Drafter B.

There are vetoes! If the other guest GM plays a title you disagree with, you can veto it. It knocks the title out of that spot but not out of the game — it goes back into the pick ether to be played by either player higher up the list. Unused vetoes roll over to your next appearance.`;
  }

  // Extended draft (3–4 positions)
  const positionLines = positions
    .map((p) => {
      const slots = (p.ownedBoardSlots ?? []).sort((a, b) => b - a).join(', ');
      return `Position ${p.positionName}: picks ${slots}`;
    })
    .join('\n');

  return `Screen Drafts is a draft in a unique style for today's topic: ${gameplay.draftTitle}.

${positionLines}

Pick placement is determined by a trivia round. There are vetoes and veto overrides in this game. See the veto status bar for each participant's current token counts.`;
}

export function PatterDrawer({ gameplay }: Props) {
  const [open, setOpen] = useState(false);
  const [section, setSection] = useState<'opening' | 'rules' | 'trivia'>('opening');

  const opening =
    'Welcome to Screen Drafts, where experts and enthusiasts competitively collaborate on the creation of screen-centric best-of lists.';

  const rules = buildGameRulesPatter(gameplay);

  return (
    <>
      <button
        onClick={() => setOpen(true)}
        className="px-4 py-2 border border-white/20 font-oswald text-xs tracking-widest text-white/60 hover:border-white/40 hover:text-white/80 transition-colors"
      >
        PATTER
      </button>

      {open && (
        <div className="fixed inset-y-0 right-0 z-30 flex">
          {/* Backdrop */}
          <div
            className="fixed inset-0 bg-black/40"
            onClick={() => setOpen(false)}
          />

          {/* Drawer */}
          <div className="relative ml-auto w-full max-w-md bg-sd-ink border-l border-white/10 flex flex-col h-full">
            <div className="flex items-center justify-between px-6 py-4 border-b border-white/10">
              <h2 className="font-oswald text-lg tracking-widest text-sd-paper uppercase">
                Patter
              </h2>
              <button
                onClick={() => setOpen(false)}
                className="text-white/40 hover:text-white/70 font-mono text-xs"
              >
                CLOSE
              </button>
            </div>

            {/* Section tabs */}
            <div className="flex border-b border-white/10">
              {(['opening', 'rules', 'trivia'] as const).map((s) => (
                <button
                  key={s}
                  onClick={() => setSection(s)}
                  className={`flex-1 py-2 font-oswald text-xs tracking-widest transition-colors ${
                    section === s
                      ? 'text-sd-paper border-b-2 border-sd-red'
                      : 'text-white/40 hover:text-white/60'
                  }`}
                >
                  {s === 'opening' ? 'OPENING' : s === 'rules' ? 'GAME RULES' : 'TRIVIA RULES'}
                </button>
              ))}
            </div>

            <div className="flex-1 overflow-y-auto px-6 py-6">
              {section === 'opening' && (
                <p className="text-sd-paper leading-relaxed text-sm">{opening}</p>
              )}
              {section === 'rules' && (
                <pre className="whitespace-pre-wrap text-sd-paper text-sm leading-relaxed font-sans">
                  {rules}
                </pre>
              )}
              {section === 'trivia' && (
                <p className="text-white/40 italic text-sm">
                  Trivia rules patter — to be added.
                </p>
              )}
            </div>
          </div>
        </div>
      )}
    </>
  );
}