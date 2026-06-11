// app/draft-guide/page.tsx
import type { Metadata } from 'next';
import Link from 'next/link';

export const metadata: Metadata = { title: 'Draft Guide' };

interface SectionProps {
  number: string;
  title: string;
  subtitle: string;
  children: React.ReactNode;
}

function Section({ number, title, subtitle, children }: SectionProps) {
  return (
    <section className="border-t border-sd-ink/10 pt-10">
      <div className="flex items-start gap-6">
        <span className="font-oswald font-bold text-[56px] leading-none text-sd-ink/10 select-none w-14 shrink-0">
          {number}
        </span>
        <div className="flex-1">
          <p className="text-[10px] tracking-[0.28em] font-bold text-sd-red mb-1">{subtitle}</p>
          <h2 className="font-oswald font-bold text-3xl tracking-[0.03em] text-sd-ink mb-4">
            {title}
          </h2>
          <div className="space-y-3 text-sd-ink/70 leading-relaxed text-[15px]">
            {children}
          </div>
        </div>
      </div>
    </section>
  );
}

function Callout({ children }: { children: React.ReactNode }) {
  return (
    <div className="border-l-2 border-sd-red pl-4 py-0.5 text-sd-ink/60 text-sm leading-relaxed">
      {children}
    </div>
  );
}

export default function DraftGuidePage() {
  return (
    <div className="min-h-screen bg-light-blue">
      <div className="max-w-2xl mx-auto px-6 py-12">

        {/* Header */}
        <div className="mb-12">
          <p className="text-[10px] tracking-[0.28em] font-bold text-sd-ink/35 mb-2">
            <Link href="/" className="hover:text-sd-ink transition-colors">HOME</Link>
            {' / '}DRAFT GUIDE
          </p>
          <h1 className="font-oswald font-bold text-[52px] leading-none tracking-[-0.01em] text-sd-ink mb-4">
            DRAFT GUIDE
          </h1>
          <p className="text-sd-ink/55 leading-relaxed text-[15px] max-w-lg">
            Every Screen Drafts episode is a competitive draft — drafters take turns selecting
            films until a final ranked list is built. Here&apos;s how the tools on this site
            support that process.
          </p>
        </div>

        {/* Rule that applies to all */}
        <div className="bg-sd-ink text-white px-6 py-5 mb-12">
          <p className="text-[10px] tracking-[0.28em] font-bold text-sd-red mb-2">UNIVERSAL RULE</p>
          <p className="font-oswald font-bold text-lg tracking-[0.03em] leading-snug">
            Once a title has been played in a draft, it cannot be played again.
          </p>
          <p className="text-white/60 text-sm mt-1.5 leading-relaxed">
            This applies across all draft types and all lists — pool, board, and candidate list alike.
          </p>
        </div>

        <div className="space-y-12">
          <Section number="01" subtitle="SUPER DRAFTS" title="DRAFT POOL">
            <p>
              A Draft Pool is used for Super Drafts; episodes where the entire filmography of a
              topic (a director, actor, or era) is on the table. The pool is the master list of
              every eligible title for that draft.
            </p>
            <p>
              The pool is created and managed by the host or an admin before the draft begins.
              Drafters cannot add to or remove from the pool themselves, it represents the
              agreed-upon universe of films that can be selected.
            </p>
            <Callout>
              Think of the pool as the deck of cards. Every drafter draws from the same deck,
              and once a card is played it&apos;s gone.
            </Callout>
            <p>
              Not all draft types use a pool. Standard, mini-Mega and Mega drafts are open; any film
              fitting the topic can be played without a predefined list.
            </p>
          </Section>

          <Section number="02" subtitle="ALL DRAFT TYPES" title="DRAFT BOARD">
            <p>
              A Draft Board is personal to each drafter. It&apos;s where a drafter builds and
              organizes their research ahead of the episode; a private ranked list of films
              they&apos;re considering selecting.
            </p>
            <p>
              Only the drafter can see and manage their own board. You can add titles, reorder
              them by priority, and add notes. The board is your strategy document: the films
              at the top are your first picks, the ones lower down are your fallbacks if
              someone else grabs your top choices first.
            </p>
            <Callout>
              Your board is your playbook. It doesn&apos;t affect the draft directly; it&apos;s
              purely for your own preparation and is only visible to you.
            </Callout>
          </Section>

          <Section number="03" subtitle="FOCUSED DRAFTS" title="CANDIDATE LIST">
            <p>
              A Candidate List is used when a draft has a narrower topic where only a specific
              set of titles qualifies — for example, films from a particular year, or within a
              narrow genre. Rather than every drafter independently researching what&apos;s
              eligible, the candidate list is a shared, public record of all qualifying titles.
            </p>
            <p>
              Drafters contribute to and update the candidate list collaboratively. Hosts and
              admins can also manage it. Because the list is public and shared, every
              participant can see what&apos;s available; making it part research tool,
              part transparency mechanism.
            </p>
            <Callout>
              The candidate list is the opposite of the draft board: where the board is
              private and personal, the candidate list is public and collaborative.
            </Callout>
            <p>
              Not every draft has a candidate list. Like the pool, it&apos;s only relevant when
              the topic has defined boundaries.
            </p>
          </Section>
        </div>

        {/* Footer nav */}
        <div className="border-t border-sd-ink/10 mt-12 pt-8">
          <Link href="/my-drafts"
            className="font-oswald font-bold tracking-[0.12em] text-sm text-sd-blue hover:text-sd-red transition-colors">
            → GO TO MY DRAFTS
          </Link>
        </div>

      </div>
    </div>
  );
}