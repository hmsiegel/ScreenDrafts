interface RecentDraft {
  number: number;
  title: string;
  drafters: string;
  date: string;
}

export default function RecentDrafts({ drafts }: { drafts: RecentDraft[] }) {
  return (
    <div className="bg-white border-2 border-sd-ink rounded-sm">
      <div className="bg-sd-ink text-white px-5 py-3.5 flex justify-between items-center">
        <div className="font-oswald font-bold text-[22px] tracking-[0.06em]">MOST RECENT DRAFTS</div>
        <div className="text-[11px] tracking-[0.22em] text-light-blue">VIEW ARCHIVE →</div>
      </div>

      {/* Column headers */}
      <div
        className="grid px-5 py-2.5 text-[10px] tracking-[0.18em] text-sd-blue font-bold border-b-2 border-sd-ink"
        style={{ gridTemplateColumns: '60px 1fr 100px' }}
      >
        <div>NO.</div>
        <div>EPISODE</div>
        <div>DATE</div>
      </div>

      {drafts.map((draft, i) => (
        <div
          key={draft.number}
          className={`grid px-5 py-3.5 items-center ${i < drafts.length - 1 ? 'border-b border-gray-100' : ''}`}
          style={{ gridTemplateColumns: '60px 1fr 100px' }}
        >
          <div className="font-oswald font-bold text-[26px] text-sd-red leading-none">{draft.number}</div>
          <div>
            <div className="font-semibold text-base leading-tight">{draft.title}</div>
            <div className="text-xs text-gray-500 mt-0.5">{draft.drafters}</div>
          </div>
          <div className="font-mono text-xs text-gray-600">{draft.date}</div>
        </div>
      ))}
    </div>
  );
}
