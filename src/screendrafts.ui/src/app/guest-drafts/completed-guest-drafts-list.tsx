// app/guest-drafts/completed-guest-drafts-list.tsx
'use client';

import Link from 'next/link';
import type { GuestDraftSummaryResponse } from '@/lib/dto';
import { guestDraftTypeLabel } from './guest-draft-type-labels';

interface Props {
  drafts: GuestDraftSummaryResponse[];
}

export function CompletedGuestDraftsList({ drafts }: Props) {
  const completed = drafts.filter((d) => d.status === 'Completed');

  if (completed.length === 0) {
    return <p className="text-sd-ink/50 text-sm font-mono">Nothing completed yet.</p>;
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-sd-ink/10">
            {['Title', 'Type', 'Date', ''].map((col) => (
              <th
                key={col}
                className="text-left font-mono text-[11px] tracking-widest uppercase text-sd-ink/50 pb-3 pr-4"
              >
                {col}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {completed.map((d) => (
            <tr key={d.publicId} className="border-b border-sd-ink/5 hover:bg-sd-paper/60 transition-colors">
              <td className="py-3 pr-4 font-medium text-sd-ink">{d.title}</td>
              <td className="py-3 pr-4 text-sd-ink/70">{guestDraftTypeLabel(d.type)}</td>
              <td className="py-3 pr-4 text-sd-ink/70">
                {d.draftDate ? new Date(d.draftDate).toLocaleDateString() : '—'}
              </td>
              <td className="py-3">
                <div className="flex items-center justify-end">
                  <Link
                    href={`/guest-drafts/${d.publicId}/summary`}
                    className="text-sd-blue text-xs font-mono uppercase tracking-wide hover:underline"
                  >
                    View
                  </Link>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}