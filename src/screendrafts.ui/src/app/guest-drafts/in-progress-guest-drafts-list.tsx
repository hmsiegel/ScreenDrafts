// app/guest-drafts/in-progress-guest-drafts-list.tsx
'use client';

import Link from 'next/link';
import type { GuestDraftSummaryResponse } from '@/lib/dto';
import { guestDraftTypeLabel } from './guest-draft-type-labels';

interface Props {
  drafts: GuestDraftSummaryResponse[];
}

export function InProgressGuestDraftsList({ drafts }: Props) {
  const inProgress = drafts.filter((d) => d.status === 'InProgress');

  if (inProgress.length === 0) {
    return <p className="text-sd-ink/50 text-sm font-mono">Nothing in progress right now.</p>;
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-sd-ink/10">
            {['Title', 'Type', 'Role', ''].map((col) => (
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
          {inProgress.map((d) => (
            <tr
              key={d.publicId}
              className="border-b border-sd-ink/5 hover:bg-sd-paper/60 transition-colors"
            >
              <td className="py-3 pr-4 font-medium text-sd-ink">{d.title}</td>
              <td className="py-3 pr-4 text-sd-ink/70">{guestDraftTypeLabel(d.type)}</td>
              <td className="py-3 pr-4">
                <span className="font-mono text-[10px] tracking-widest uppercase text-sd-ink/50">
                  {d.isOwner ? 'Owner' : 'Participant'}
                </span>
              </td>
              <td className="py-3">
                <div className="flex items-center justify-end">
                  <Link
                    href={`/guest-drafts/${d.publicId}/live`}
                    className="bg-sd-red text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-red/90"
                  >
                    Play
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