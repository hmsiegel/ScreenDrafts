// app/guest-drafts/upcoming-guest-drafts-list.tsx
'use client';

import type { GuestDraftSummaryResponse } from '@/lib/dto';
import { guestDraftTypeLabel } from './guest-draft-type-labels';
import { UpcomingGuestDraftActions } from './upcoming-guest-draft-actions';

interface Props {
  drafts: GuestDraftSummaryResponse[];
}

// Status "Created" — GuestDraftStatus's SmartEnum name, confirmed via
// GetGuestDraftGameplayResponse.Status using the same string convention.
// Filtered client-side off one fetched array, same pattern as canonical's
// upcoming-drafts-list.tsx / in-progress-drafts-list.tsx split.
export function UpcomingGuestDraftsList({ drafts }: Props) {
  const upcoming = drafts.filter((d) => d.status === 'Created');

  if (upcoming.length === 0) {
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
          {upcoming.map((d) => (
            <tr
              key={d.publicId}
              className="border-b border-sd-ink/5 hover:bg-sd-paper/60 transition-colors"
            >
              <td className="py-3 pr-4 font-medium text-sd-ink">{d.title}</td>
              <td className="py-3 pr-4 text-sd-ink/70">{guestDraftTypeLabel(d.type)}</td>
              <td className="py-3 pr-4 text-sd-ink/70">
                {d.draftDate
                  ? d.draftDate instanceof Date
                    ? d.draftDate.toLocaleDateString()
                    : d.draftDate
                  : '—'}
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