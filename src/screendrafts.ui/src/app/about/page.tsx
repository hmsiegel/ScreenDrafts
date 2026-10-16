// app/about/page.tsx
import type { Metadata } from "next";
import Link from "next/link";
import { Callout, Section } from "@/components/features/about/about-section";

export const metadata: Metadata = {
  title: "The Rules",
  description: "How a Screen Drafts draft works: picks, vetoes, overrides and predictions.",
};

export default function AboutRulesPage() {
  return (
    <div className="space-y-12">
      <Section number="01" subtitle="THE GAME" title="HOW A DRAFT WORKS">
        <p>
          A Standard Draft has seven picks. Drafter A makes picks 7, 6, 4 and 2. Drafter B
          makes picks 5, 3 and 1. While Drafter B has fewer picks, they are
          more concentrated near the top of the list, including the No. 1 pick.
          Trivia, usually prepared by the episode&apos;s co-host, sets the draft order.
        </p>
        <p>
          Drafters send each pick to the host, who announces it. The selector
          then summarizes the film and defends the pick and its placement. The other Guest
          G.M.s may comment and consider a veto. Once the next pick is selected, the
          previous pick is locked in.
        </p>
      </Section>

      <Section number="02" subtitle="THE WEAPON" title="VETOES">
        <p>
          A drafter who disagrees with a pick can deploy a veto. The veto removes the title
          from that position on the list, but not from the game. Drafters veto to remove a title
          for good, or to move it higher on the list. In a Super Draft, every eligible title
          gets drafted, so vetoes only move a pick higher.
        </p>
        <p>
          Guest G.M.s are permitted to self-veto.
        </p>
        <p>
          If a drafter does not use their veto, they can roll it over to another draft.
          Only one veto can roll over at a time. In expanded drafts, trivia order sets the veto order:
          the earliest position must decide first on each film, and so risks using vetoes sooner.
        </p>
        <Callout>Vetoes are non-fungible. No trading, gifting or bartering.</Callout>
      </Section>

      <Section number="03" subtitle="THE COUNTER" title="VETO OVERRIDES">
        <p>
          Expanded drafts, including Mini-Mega, Super and Mega Drafts, may offer a veto
          override. Typically one drafter holds it. It erases an attempted veto from the
          game. A drafter cannot use it to protect their own pick.
        </p>
        <p>Like vetoes, overrides can also roll over, and also only one at a time.</p>
      </Section>

      <Section number="04" subtitle="THE REVIEW" title="COMMISSIONER OVERRIDE">
        <p>
          When a title strays too far from the episode&apos;s topic, either commissioner can call
          for a review. They discuss the title&apos;s merits. If both agree it strays too
          far, the title leaves the game without penalty and the drafter selects a
          replacement.
        </p>
      </Section>

      <Section number="05" subtitle="THE AGREEMENT" title="THE MOOCH RULE">
        <p>
          The Mooch Rule eliminates Made for TV movies from the list. The rule was created when <Link href="/drafters/pe_8yf2GyHrPaDVljw">Grant Moniger</Link>
          played the Made for TV movie <Link href="/media/m_sRZu05YIml4rRmU">Mooch Goes to Hollywood</Link> during the <Link href="">Dog Movies</Link> draft.
          The rule is designed to limit the pool of eligible films and to ensure people have access to the eligible titles played.
          The rule can be suspended if both Guest G.M.s feel it is necessary to complete the list.
        </p>
      </Section>

      <Section number="06" subtitle="THE SIDE GAME" title="COMMISSIONER PREDICTIONS">
        <p>
          Before each draft, the two commissioners predict which titles will land on the
          list. Each correct title earns a point. The game runs to 100, then resets.
        </p>
      </Section>

      <Section number="07" subtitle="THE TOOLS" title="DRAFT GUIDE">
        <p>Draft pools, boards and candidate lists have their own page.</p>
        <Link
          href="/draft-guide"
          className="inline-block font-oswald font-bold tracking-[0.12em] text-sm text-sd-blue hover:text-sd-red transition-colors"
        >
          → READ THE DRAFT GUIDE
        </Link>
      </Section>
    </div>
  );
}
