// app/guest-drafts/[guestDraftId]/live/components/guest-draft-summary-content.tsx
import { StaticDraftPickList, type StaticListPick } from './static-draft-pick-list';

interface Props {
  title: string;
  totalPicks: number;
  vetoCount: number;
  picks: StaticListPick[];
  action: React.ReactNode;
}

// Shared between GuestDraftCompletionModal (the live, just-happened moment —
// fed from useGuestDraftLive() context at its own call site) and the
// standalone /summary page (fed from the new GetGuestDraftDetails fetch, no
// context, no SignalR connection).
//
// Flat pick list, not a board grid — matches canonical's /drafts/[id] page
// ("THE FINAL LIST": every pick ever played, sorted by play order, vetoed
// ones struck through, overridden ones shown as saved and still on the
// board). A board-grid view (DraftBoard/StaticDraftBoard) only shows one
// pick per position slot by design, which is right for "what does the board
// look like right now" but wrong here — it was silently hiding every
// vetoed pick, which is the opposite of what a summary should do.
export function GuestDraftSummaryContent({ title, totalPicks, vetoCount, picks, action }: Props) {
  return (
    <div className="w-full max-w-3xl mx-auto">
      <div className="text-center mb-8">
        <p className="font-oswald text-sd-red text-xs tracking-[0.3em] uppercase mb-2">
          Draft Complete
        </p>
        <h1 className="font-oswald text-4xl text-sd-paper uppercase tracking-wide leading-tight">
          {title}
        </h1>
        <p className="font-mono text-white/40 text-sm mt-2">
          {totalPicks} pick{totalPicks !== 1 ? 's' : ''}
          {vetoCount > 0 && ` · ${vetoCount} veto${vetoCount !== 1 ? 'es' : ''}`}
        </p>
      </div>

      <section className="mb-8">
        <p className="font-oswald text-xs tracking-[0.25em] text-white/40 uppercase mb-3">
          Final Picks
        </p>
        <div className="border border-white/10 px-4">
          <StaticDraftPickList picks={picks} />
        </div>
      </section>

      <div className="text-center">{action}</div>
    </div>
  );
}