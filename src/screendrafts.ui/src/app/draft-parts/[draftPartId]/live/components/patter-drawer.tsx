// app/draft-parts/[draftPartId]/live/components/patter-drawer.tsx
'use client';

import { GetDraftPartGameplayResponse } from '@/lib/dto';
import { useState } from 'react';

interface Props {
  gameplay: GetDraftPartGameplayResponse;
}

function buildGameRulesPatter(gameplay: GetDraftPartGameplayResponse): string {
  const isStandard = gameplay.draftType === 'Standard';
  // Position SHAPE (name + slot numbers) is fixed by draft structure and is
  // available before assignment. Who actually FILLS each position is
  // decided by trivia, which happens after the patter is read — so
  // draftPositions and participants must be treated as two independent
  // facts here, never matched against each other. Doing so previously was
  // the bug: it silently produced empty/placeholder output because no
  // position has an assigned participant yet at patter-time.
  const positions = gameplay.draftPositions ?? [];
  const participants = gameplay.participants ?? [];
  const coHost = gameplay.hosts?.find((h) => !h.isPrimary)?.hostName ?? '[Co-Host]';

  const drafterNames = participants
    .map((p) => p.participantName)
    .filter((name): name is string => !!name);
  const namesIntro =
    drafterNames.length > 1
      ? `${drafterNames.slice(0, -1).join(', ')}, and ${drafterNames[drafterNames.length - 1]}`
      : (drafterNames[0] ?? 'our guest GMs');

    // Rollover vetoes/overrides are a fact about THIS participant entering
  // the part with a carried-in token — separate from VetoTokensRemaining /
  // OverrideTokensRemaining (net totals that also include this part's flat
  // starting allotment, post-trivia awards, and anything already used).
  // At patter-time nothing's been awarded or used yet, so these are the
  // only fields that can honestly answer "is anyone rolling in a token."
  const vetoRollers = participants.filter((p) => (p.vetoesRollingIn ?? 0) > 0);
  const overrideRollers = participants.filter((p) => (p.vetoOverridesRollingIn ?? 0) > 0);

  function listNames(people: typeof participants): string {
    const names = people.map((p) => p.participantName).filter((n): n is string => !!n);
    if (names.length === 0) return '';
    if (names.length === 1) return names[0];
    return `${names.slice(0, -1).join(', ')} and ${names[names.length - 1]}`;
  }

  const rolloverVetoSentence =
    vetoRollers.length > 0
      ? `${listNames(vetoRollers)} ${vetoRollers.length === 1 ? 'is' : 'are'} rolling in a veto from a previous episode.`
      : '';

  const rolloverOverrideSentence =
    overrideRollers.length > 0
      ? `${listNames(overrideRollers)} ${overrideRollers.length === 1 ? 'is' : 'are'} rolling in a veto override from a previous episode.`
      : '';

  if (isStandard && positions.length === 2) {
    const a = positions.find((p) => p.positionName === 'Drafter A');
    const b = positions.find((p) => p.positionName === 'Drafter B');
    const slotsA = (a?.ownedBoardSlots ?? []).sort((x, y) => y - x).join(', ');
    const slotsB = (b?.ownedBoardSlots ?? []).sort((x, y) => y - x).join(', ');
    const countA = (a?.ownedBoardSlots ?? []).length;
    const countB = (b?.ownedBoardSlots ?? []).length;

    // Each array entry is ONE paragraph as the listener hears it. Breaking
    // a long paragraph across several source lines here (string
    // concatenation, or just continuing the literal on the next line) is
    // purely for editor readability — it does NOT insert a line break in
    // the rendered patter, because there's no literal \n inside any single
    // entry. Only the '\n\n' from .join below produces visible paragraph
    // breaks, and that only happens BETWEEN entries.
    const paragraphs = [
      `Screen Drafts is a serpentine-style draft where our guest GMs, ${namesIntro}, ` +
        `are going to be alternating placing picks on the single definitive Best-Of ` +
        `list for today's topic which is ${gameplay.draftTitle}.`,

      `If you are Drafter A, you will be placing picks ${slotsA} — ${countA} picks ` +
        `overall. If you are Drafter B, you will be placing picks ${slotsB} — which ` +
        `is fewer picks overall, but they are weighted higher on the list and you ` +
        `get the number 1 pick.`,

      `Pick placement is determined by a trivia round prepared by ${coHost}. ` +
        `Whoever wins trivia decides whether to be Drafter A or Drafter B.`,

      `There are vetoes in this game! How the veto works is if the other guest GM ` +
        `plays a title you disagree with, either it's inclusion on the list at all ` +
        `or just at that particular spot, you can veto it. It knocks the title out ` +
        `of that spot but not out of the game. It goes back into the pick ether to ` +
        `be played by either player higher up the list. If you do not use your ` +
        `veto you can roll it over to the next time you are on the show, but you ` +
        `can only ever rollover one veto.` +
        (rolloverVetoSentence ? ` ${rolloverVetoSentence}` : ''),
    ];

    return paragraphs.join('\n\n');
  }

  // Extended draft (3+ positions) — same narrative voice and structure as
  // the standard patter above, generalized across however many positions
  // exist, plus a dedicated veto override paragraph (a mechanic standard
  // 2-person drafts don't use).
  const sortedPositions = [...positions].sort((x, y) =>
    (x.positionName ?? '').localeCompare(y.positionName ?? ''),
  );

  const positionBreakdown = sortedPositions
    .map((p) => {
      const slots = (p.ownedBoardSlots ?? []).sort((x, y) => y - x).join(', ');
      const count = (p.ownedBoardSlots ?? []).length;
      return `If you are Drafter ${p.positionName}, you will be placing picks ${slots} — ${count} pick${count === 1 ? '' : 's'} overall.`;
    })
    .join(' ');

  const vetoPositions = sortedPositions.filter((p) => p.hasBonusVeto);
  const vetoNote =
    vetoPositions.length > 0
      ? ` ${vetoPositions
          .map((p) => p.positionName)
          .join(' and ')} ${vetoPositions.length === 1 ? 'is' : 'are'} starting with an extra veto.`
      : '';

  const overridePositions = sortedPositions.filter((p) => p.hasBonusVetoOverride);
  const overrideNote =
    overridePositions.length > 0
      ? ` ${overridePositions
          .map((p) => p.positionName)
          .join(' and ')} ${overridePositions.length === 1 ? 'is' : 'are'} starting with an extra veto override.`
      : '';

  const extendedParagraphs = [
    `Screen Drafts is a serpentine-style draft where our guest GMs, ${namesIntro}, ` +
      `are going to be alternating placing picks on the single, definitive Best-Of ` +
      `list for today's topic which is ${gameplay.draftTitle}.`,

    `${positionBreakdown}`,

    `Pick placement is determined by a trivia round prepared by ${coHost}. Trivia ` +
      `performance determines draft order, from first position down to last.`,

    `There are vetoes in this game! How the veto works is if another guest GM ` +
      `plays a title you disagree with, either it's inclusion on the list at all ` +
      `or just at that particular spot, you can veto it. It knocks the title out ` +
      `of that spot but not out of the game. It goes back into the pick ether to ` +
      `be played by anyone higher up the list. If you do not use your veto you ` +
      `can roll it over to the next time you are on the show, but you can only ` +
      `ever rollover one veto.` +
      (rolloverVetoSentence ? ` ${rolloverVetoSentence}` : '') +
      ` Drafter${vetoNote}`,

    `In addition to the veto, in expanded drafts, there is also the veto override. ` +
      `How the veto override works is if somebody plays a pick and is vetoed ` +
      `and you disagree with that person you can override the veto. That obliterates ` +
      `the veto, locks the pick in, and ends the round. A veto override can only be used ` +
      `to protect someone elses pick. You can not use it to protect your own. ` +
      `Like the veto, the veto override also rolls over, but you can only role over one veto override.` +
      (rolloverOverrideSentence ? ` ${rolloverOverrideSentence}` : '') +
      ` Drafter${overrideNote}`,

  ];

  return extendedParagraphs.join('\n\n');
}

export function PatterDrawer({ gameplay }: Props) {
  const [open, setOpen] = useState(false);
  const [section, setSection] = useState<'opening' | 'rules' | 'trivia'>('opening');
  const primaryHost = gameplay.hosts?.find((h) => h.isPrimary)?.hostName;
  const coHost = gameplay.hosts?.find((h) => !h.isPrimary)?.hostName ?? 'nobody';


  const opening =
    'Welcome to Screen Drafts, where experts and enthusiasts competitively collaborate on the creation of screen-centric best-of lists. ' +
      `I'm draft commissioner ${primaryHost} and alongside me is my co-commissioner ${coHost}.`;

  const triviaRules = [
    `There are 4 rules to trivia: ` +
    `The first rule is the Alex Trebek rule. This means that you must let me finish asking the question before answering. ` +
    `Do not step on the question.`,

    `The second rule of trivia is the Lord Bullingdon rule named after Lord Bullingdon from Stanley Kubrick's epic Barry Lyndon. ` +
    `If you fire your imaginary answer pistol and you miss, you have to wait for your opponent to fire their imaginary answer pistol ` +
    `before taking another shot. A round robin of bad answers.`,

    `The third rule is the Gulager rule, name for Clu Gulager. If you and your opponent are stumped, then you can ask for a clue. ` +
    `This resets the Lord Bullingdon order.`,

    `The final rule is the John and Gena rule, named for John Carravetes and Gena Rowlands. Just like in many of their movies, ` +
    `you just shout out your answer. You do not have to state your name, buzz in, or raise your hand. Just shout out the answer.`
  ];

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
                <pre className="whitespace-pre-wrap text-sd-paper text-sm leading-relaxed font-sans">
                  {triviaRules.join('\n\n')}
                </pre>
              )}
            </div>
          </div>
        </div>
      )}
    </>
  );
}