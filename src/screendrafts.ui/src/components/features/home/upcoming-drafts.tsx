interface UpcomingDraft {
  date: string;
  title: string;
  type: string;
  access: 'PUBLIC' | 'PATRON';
}

export default function UpcomingDrafts({ drafts }: { drafts: UpcomingDraft[] }) {
  return (
    <div className="bg-white border-2 border-sd-ink rounded-sm">
      <div className="bg-sd-blue text-white px-5 py-3.5">
        <div className="font-oswald font-bold text-[22px] tracking-[0.06em]">UPCOMING DRAFTS</div>
        <div className="text-[11px] tracking-[0.18em] opacity-85 mt-0.5">NEXT 14 DAYS</div>
      </div>

      <div className="divide-y divide-gray-100">
        {drafts.map((draft) => (
          <div
            key={draft.title}
            className={`px-5 py-3.5 relative ${draft.access === 'PATRON' ? 'bg-amber-50' : 'bg-white'}`}
          >
            {draft.access === 'PATRON' && (
              <span className="absolute top-3 right-3.5 bg-sd-red text-white text-[9px] px-1.5 py-0.5 tracking-widest font-bold rounded-sm">
                ★ PATRON
              </span>
            )}
            <div className="font-mono text-[11px] text-sd-blue font-bold">{draft.date}</div>
            <div className="font-semibold text-[15px] mt-1 pr-14 leading-snug">{draft.title}</div>
            <div className="text-[11px] text-gray-500 mt-0.5 tracking-[0.06em]">{draft.type.toUpperCase()}</div>
          </div>
        ))}
      </div>
    </div>
  );
}
