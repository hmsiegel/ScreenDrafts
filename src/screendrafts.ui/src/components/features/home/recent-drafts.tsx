// components/features/home/recent-drafts.tsx
import Link from "next/link";

interface RecentDraft {
  publicId: string;
  number: number;
  title: string;
  drafters: string;
  date: string;
}

// Below sm the DATE column folds under the drafters line; the fixed 60px + 100px
// columns left the title about 140px on a phone.
const COLS = "grid-cols-[52px_minmax(0,1fr)] sm:grid-cols-[60px_minmax(0,1fr)_100px]";

export default function RecentDrafts({ drafts }: { drafts: RecentDraft[] }) {
  return (
    <div className="bg-white border-2 border-sd-ink rounded-sm min-w-0">
      <div className="bg-sd-ink text-white px-5 py-3.5 flex flex-wrap justify-between items-center gap-x-4 gap-y-1">
        <div className="font-oswald font-bold text-[19px] sm:text-[22px] tracking-[0.06em]">MOST RECENT DRAFTS</div>
        <Link href="/drafts" className="text-[11px] tracking-[0.22em] text-light-blue hover:text-white transition-colors">
          VIEW ARCHIVE →
        </Link>
      </div>

      {drafts.length === 0 ? (
        <div className="px-5 py-6 text-center text-[13px] text-gray-500">
          Recent drafts couldn&rsquo;t be loaded. The full list is in the{" "}
          <Link href="/drafts" className="text-sd-blue underline">archive</Link>.
        </div>
      ) : (
        <>
          {/* Column headers */}
          <div className={`grid ${COLS} px-5 py-2.5 text-[10px] tracking-[0.18em] text-sd-blue font-bold border-b-2 border-sd-ink`}>
            <div>NO.</div>
            <div>EPISODE</div>
            <div className="hidden sm:block">DATE</div>
          </div>

          {drafts.map((draft, i) => (
            <Link
              key={draft.publicId || draft.number}
              href={`/drafts/${draft.publicId}`}
              className={`grid ${COLS} px-5 py-3.5 items-center cursor-pointer hover:bg-gray-50 transition-colors ${i < drafts.length - 1 ? 'border-b border-gray-100' : ''}`}
            >
              <div className="font-oswald font-bold text-[24px] sm:text-[26px] text-sd-red leading-none">{draft.number}</div>
              <div className="min-w-0">
                <div className="font-semibold text-base leading-tight [overflow-wrap:anywhere]">{draft.title}</div>
                <div className="text-xs text-gray-500 mt-0.5 truncate">{draft.drafters}</div>
                <div className="sm:hidden font-mono text-[11px] text-gray-600 mt-1">{draft.date}</div>
              </div>
              <div className="hidden sm:block font-mono text-xs text-gray-600">{draft.date}</div>
            </Link>
          ))}
        </>
      )}
    </div>
  );
}