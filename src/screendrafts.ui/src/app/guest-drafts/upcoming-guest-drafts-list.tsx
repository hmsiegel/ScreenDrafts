// app/guest-drafts/upcoming-guest-drafts-list.tsx
'use client';

import type { MyGuestDraftSummary } from '@/lib/dto';
import { guestDraftTypeLabel } from './guest-draft-type-labels';
import { UpcomingGuestDraftActions } from './upcoming-guest-draft-actions';

interface Props {
  drafts: MyGuestDraftSummary[];
}

// No more client-side status filter — GetMyGuestDrafts buckets Created and
// Paused into this list server-side already (mirrors canonical's own
// Draft-level bucketing).
export function UpcomingGuestDraftsList({ drafts }: Props) {
  if (drafts.length === 0) {
    return <p className="text-sd-ink/50 text-sm font-mono">Nothing upcoming right now.</p>;
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-sd-ink/10">
            {['Title', 'Type', 'Date', 'Role', ''].map((col) => (
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
          {drafts.map((d) => (
            <tr
              key={d.publicId}
              className="border-b border-sd-ink/5 hover:bg-sd-paper/60 transition-colors"
            >
              <td className="py-3 pr-4 font-medium text-sd-ink">{d.title}</td>
              <td className="py-3 pr-4 text-sd-ink/70">{guestDraftTypeLabel(d.type)}</td>
              <td className="py-3 pr-4 text-sd-ink/70">
                {d.scheduledForUtc ? new Date(d.scheduledForUtc).toLocaleString() : '—'}
              </td>
              <td className="py-3 pr-4">
                <span className="font-mono text-[10px] tracking-widest uppercase text-sd-ink/50">
                  {d.isOwner ? 'Owner' : 'Participant'}
                </span>
              </td>
              <td className="py-3">
                <UpcomingGuestDraftActions draft={d} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}