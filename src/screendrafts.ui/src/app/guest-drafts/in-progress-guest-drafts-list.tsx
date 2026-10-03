// app/guest-drafts/in-progress-guest-drafts-list.tsx
'use client';

import Link from 'next/link';
import type { MyGuestDraftSummary } from '@/lib/dto';
import { guestDraftTypeLabel } from './guest-draft-type-labels';

interface Props {
  drafts: MyGuestDraftSummary[];
}

export function InProgressGuestDraftsList({ drafts }: Props) {
  if (drafts.length === 0) {
    return <p className="text-sd-ink/50 text-sm font-mono">Nothing in progress right now.</p>;
  }

  return (
    // sm+: a table. Below sm each row is a card — title on its own line, type/date/role
    // as a wrapping meta line, actions underneath. Same markup; the elements switch
    // display types at sm (block/flex ↔ table, table-row, table-cell).
    <div className="sm:overflow-x-auto">
      <table className="block sm:table w-full text-sm">
        <thead className="hidden sm:table-header-group">
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
        <tbody className="block sm:table-row-group">
          {drafts.map((d) => (
            <tr
              key={d.publicId}
              className="flex flex-wrap items-center gap-x-3 gap-y-1 py-3 sm:table-row sm:py-0 border-b border-sd-ink/5 last:border-b-0 sm:last:border-b hover:bg-sd-paper/60 transition-colors"
            >
              <td className="w-full sm:w-auto sm:table-cell sm:py-3 sm:pr-4 font-medium text-sd-ink [overflow-wrap:anywhere]">{d.title}</td>
              <td className="sm:table-cell sm:py-3 sm:pr-4 text-xs sm:text-sm text-sd-ink/70">{guestDraftTypeLabel(d.type)}</td>
              <td className="sm:table-cell sm:py-3 sm:pr-4">
                <span className="font-mono text-[10px] tracking-widest uppercase text-sd-ink/50">
                  {d.isOwner ? 'Owner' : 'Participant'}
                </span>
              </td>
              <td className="w-full sm:w-auto sm:table-cell sm:py-3 pt-2">
                <div className="flex items-center sm:justify-end">
                  <Link
                    href={`/guest-drafts/${d.publicId}/live`}
                    className="inline-flex items-center min-h-9 sm:min-h-0 bg-sd-red text-white font-oswald font-medium uppercase tracking-wide text-xs px-3 py-1.5 hover:bg-sd-red/90"
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