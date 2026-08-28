// src/components/features/home/upcoming-drafts.tsx
"use client";

import { useState } from "react";
import { MappedUpcomingDraft } from "@/services/home/fetch-home-data";

const PAGE_SIZE = 5;

export default function UpcomingDrafts({ drafts }: { drafts: MappedUpcomingDraft[] }) {
  const [page, setPage] = useState(1);

  const totalPages = Math.max(1, Math.ceil(drafts.length / PAGE_SIZE));
  const start = (page - 1) * PAGE_SIZE;
  const visibleDrafts = drafts.slice(start, start + PAGE_SIZE);

  return (
    <div className="bg-white border-2 border-sd-ink rounded-sm">
      <div className="bg-sd-blue text-white px-5 py-3.5">
        <div className="font-oswald font-bold text-[22px] tracking-[0.06em]">UPCOMING DRAFTS</div>
      </div>

      <div className="divide-y divide-gray-100">
        {visibleDrafts.map((draft) => (
          <div
            key={draft.draftPartPublicId || draft.title}
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

        {visibleDrafts.length === 0 && (
          <div className="px-5 py-6 text-center text-[13px] text-gray-500">
            No upcoming drafts.
          </div>
        )}
      </div>

      {totalPages > 1 && (
        <div className="flex items-center justify-center gap-1.5 px-5 py-3 border-t border-gray-100">
          {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
            <button
              key={p}
              type="button"
              onClick={() => setPage(p)}
              aria-current={p === page ? "page" : undefined}
              className={`font-mono text-[11px] font-bold w-7 h-7 rounded-sm border transition-colors ${
                p === page
                  ? "bg-sd-blue text-white border-sd-blue"
                  : "bg-white text-sd-ink border-gray-300 hover:border-sd-blue hover:text-sd-blue"
              }`}
            >
              {p}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}