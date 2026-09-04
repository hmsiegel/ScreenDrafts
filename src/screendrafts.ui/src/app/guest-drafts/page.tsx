// app/guest-drafts/page.tsx
import { auth } from '@/auth';
import { redirect } from 'next/navigation';
import Link from 'next/link';
import { Metadata } from 'next';

export const metadata: Metadata = { title: 'My Guest Drafts' };
export const dynamic = 'force-dynamic';

// ── The real blocker on this page ────────────────────────────────────────────
// There is no list endpoint for GuestDrafts anywhere in dto.ts — only
// CreateGuestDraft (make one) and GetGameplay (fetch one you already have the
// id for). Nothing below renders real data until something like
// GET /guest-drafts/mine exists, returning an array shaped roughly like this:
interface MyGuestDraftSummary {
  guestDraftPublicId: string;
  title: string;
  type: string;
  status: string;
  isOwner: boolean;
}

// ASSUMPTION baked into the section split below, not confirmed: "Created &
// Paused" / "In Progress" show anything you own OR are invited to that isn't
// finished yet (mirrors canonical's My Drafts, which shows anything you're
// isDrafter/isHost on) — "Completed" is owner-only, per what was actually
// asked for. If Created/In Progress should be owner-only too, that's a
// one-line filter change once real data exists.
//
// Also unresolved: every link below points at /guest-drafts/{id}/live, but
// that page doesn't exist yet either (flagged previously), and canonical
// sends completed drafts to a separate recap page (/my-drafts/{id}), not the
// live page — GuestDrafts has no equivalent recap route. Worth deciding
// whether a completed guest draft needs its own view, or whether the live
// page should just handle a read-only/completed state.

function SectionCard({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="bg-white border border-sd-ink/10">
      <div className="flex items-center gap-3 px-6 py-4 border-b border-sd-ink/10 bg-sd-ink">
        <div className="w-1 h-5 bg-sd-red shrink-0" />
        <h2 className="font-oswald font-bold text-[16px] tracking-wide uppercase text-white">
          {title}
        </h2>
      </div>
      <div className="p-6">{children}</div>
    </div>
  );
}

function GuestDraftRow({ draft }: { draft: MyGuestDraftSummary }) {
  return (
    <Link
      href={`/guest-drafts/${draft.guestDraftPublicId}/live`}
      className="flex items-center gap-4 px-4 py-3 bg-white border border-sd-ink/10 hover:border-sd-ink/30 transition-colors"
    >
      <div className="w-1 h-8 bg-sd-red shrink-0" />
      <div className="flex-1 min-w-0">
        <p className="font-oswald font-bold text-[15px] uppercase tracking-wide text-sd-ink truncate">
          {draft.title}
        </p>
        <p className="font-mono text-[11px] text-sd-ink/40 mt-0.5">{draft.type}</p>
      </div>
      {draft.isOwner && (
        <span className="font-mono text-[9px] tracking-widest uppercase px-1.5 py-0.5 bg-sd-ink/10 text-sd-ink border border-sd-ink/20 shrink-0">
          Owner
        </span>
      )}
    </Link>
  );
}

function CompletedGuestDraftCard({ draft }: { draft: MyGuestDraftSummary }) {
  return (
    <Link
      href={`/guest-drafts/${draft.guestDraftPublicId}/live`}
      className="block bg-white border border-sd-ink/10 hover:border-sd-ink/30 transition-colors"
    >
      <div className="px-4 py-3">
        <span className="font-oswald font-bold text-sm uppercase tracking-wide text-sd-ink block truncate">
          {draft.title}
        </span>
        <p className="font-mono text-[11px] text-sd-ink/40 mt-1">{draft.type}</p>
      </div>
    </Link>
  );
}

export default async function GuestDraftsPage() {
  const session = await auth();
  if (!session?.accessToken) redirect('/');

  // Placeholders until the list endpoint above exists.
  const createdAndPaused: MyGuestDraftSummary[] = [];
  const inProgress: MyGuestDraftSummary[] = [];
  const completed: MyGuestDraftSummary[] = [];

  return (
    <div className="min-h-screen bg-light-blue">
      <div className="px-6 md:px-10 py-10 max-w-[1200px] mx-auto space-y-8">
        <div className="flex items-end justify-between">
          <div>
            <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-2">
              / MY DRAFTS
            </p>
            <h1 className="font-oswald font-bold text-[56px] leading-none text-sd-ink">
              GUEST DRAFTS
            </h1>
          </div>
          <Link
            href="/guest-drafts/new"
            className="bg-sd-red text-white font-oswald font-medium tracking-wide uppercase px-5 py-3 hover:bg-sd-red/90 transition-colors"
          >
            + Create
          </Link>
        </div>

        <SectionCard title="Created & Paused">
          {createdAndPaused.length === 0 ? (
            <p className="text-sd-ink/50 text-sm font-mono">No drafts here yet.</p>
          ) : (
            <div className="space-y-2">
              {createdAndPaused.map((d) => (
                <GuestDraftRow key={d.guestDraftPublicId} draft={d} />
              ))}
            </div>
          )}
        </SectionCard>

        <SectionCard title="In Progress">
          {inProgress.length === 0 ? (
            <p className="text-sd-ink/50 text-sm font-mono">Nothing in progress right now.</p>
          ) : (
            <div className="space-y-2">
              {inProgress.map((d) => (
                <GuestDraftRow key={d.guestDraftPublicId} draft={d} />
              ))}
            </div>
          )}
        </SectionCard>

        <section>
          <h2 className="font-oswald font-bold text-[28px] uppercase tracking-wide text-sd-ink mb-4">
            Completed
          </h2>
          {completed.length === 0 ? (
            <p className="font-mono text-sm text-sd-ink/40">No completed drafts.</p>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
              {completed.map((d) => (
                <CompletedGuestDraftCard key={d.guestDraftPublicId} draft={d} />
              ))}
            </div>
          )}
        </section>
      </div>
    </div>
  );
}