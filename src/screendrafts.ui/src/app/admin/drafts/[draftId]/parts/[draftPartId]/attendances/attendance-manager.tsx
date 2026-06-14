// src/app/admin/drafts/[draftId]/parts/[draftPartId]/attendances/attendance-manager.tsx
'use client';

import { useState } from 'react';
import type { AttendanceItem } from '@/services/admin/fetch-attendances';
import {
  confirmAttendance,
  withdrawAttendance,
  reinstateAttendance,
} from '@/services/admin/fetch-attendances';

const STATUS = { Pending: 0, Confirmed: 1, Joined: 2, Withdrawn: 3 } as const;

function StatusBadge({ status, statusName }: { status: number; statusName: string }) {
  const colours: Record<number, string> = {
    [STATUS.Pending]:   'bg-sd-ink/10 text-sd-ink/50 border-sd-ink/20',
    [STATUS.Confirmed]: 'bg-sd-blue/10 text-sd-blue border-sd-blue/20',
    [STATUS.Joined]:    'bg-green-100 text-green-800 border-green-200',
    [STATUS.Withdrawn]: 'bg-sd-red/10 text-sd-red border-sd-red/20',
  };
  return (
    <span className={`text-[10px] font-oswald tracking-widest px-2 py-0.5 border ${colours[status] ?? 'bg-sd-ink/5 text-sd-ink/30 border-sd-ink/10'}`}>
      {statusName.toUpperCase()}
    </span>
  );
}

interface Props {
  initialItems: AttendanceItem[];
  accessToken: string;
  draftPartId: string;
}

export function AttendanceManager({ initialItems, accessToken, draftPartId }: Props) {
  const [items, setItems] = useState(initialItems);
  const [busy, setBusy] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function act(
    fn: () => Promise<void>,
    personPublicId: string,
    optimistic: (prev: AttendanceItem[]) => AttendanceItem[],
  ) {
    setBusy(personPublicId);
    setError(null);
    setItems((prev) => optimistic(prev));
    try {
      await fn();
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/draft-parts/${draftPartId}/attendances`,
        { headers: { Authorization: `Bearer ${accessToken}` }, cache: 'no-store' },
      );
      if (res.ok) {
        const data = await res.json();
        setItems(data.items ?? []);
      }
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Action failed.');
      setItems(initialItems);
    } finally {
      setBusy(null);
    }
  }

  function handleConfirm(item: AttendanceItem) {
    act(
      () => confirmAttendance(accessToken, draftPartId, item.personPublicId),
      item.personPublicId,
      (prev) => prev.map((i) =>
        i.personPublicId === item.personPublicId
          ? { ...i, status: STATUS.Confirmed, statusName: 'Confirmed' }
          : i,
      ),
    );
  }

  function handleWithdraw(item: AttendanceItem) {
    act(
      () => withdrawAttendance(accessToken, draftPartId, item.personPublicId),
      item.personPublicId,
      (prev) => prev.map((i) =>
        i.personPublicId === item.personPublicId
          ? { ...i, status: STATUS.Withdrawn, statusName: 'Withdrawn' }
          : i,
      ),
    );
  }

  function handleReinstate(item: AttendanceItem) {
    act(
      () => reinstateAttendance(accessToken, draftPartId, item.personPublicId),
      item.personPublicId,
      (prev) => prev.map((i) =>
        i.personPublicId === item.personPublicId
          ? { ...i, status: STATUS.Pending, statusName: 'Pending' }
          : i,
      ),
    );
  }

  if (items.length === 0) {
    return (
      <p className="text-sd-ink/40 font-mono text-sm italic">
        No attendance records. Participants are auto-enrolled when added to the draft part.
      </p>
    );
  }

  return (
    <div className="space-y-2">
      {error && (
        <p className="text-sd-red text-xs font-mono px-4 py-2 bg-sd-red/10 border border-sd-red/20">
          {error}
        </p>
      )}

      <div className="divide-y divide-sd-ink/10 border border-sd-ink/10">
        {items.map((item) => {
          const isBusy = busy === item.personPublicId;
          return (
            <div
              key={item.publicId}
              className="flex items-center gap-4 px-4 py-3 bg-white hover:bg-slate-400 transition-colors"
            >
              {/* Name */}
              <span className="font-oswald text-sm text-sd-ink flex-1 truncate">
                {item.personName ?? item.personPublicId}
              </span>

              {/* Status */}
              <StatusBadge status={item.status} statusName={item.statusName} />

              {/* Actions */}
              <div className="flex gap-2 shrink-0">
                {item.status === STATUS.Pending && (
                  <button
                    onClick={() => handleConfirm(item)}
                    disabled={isBusy}
                    className="px-3 py-1 bg-sd-blue text-white font-oswald text-xs tracking-wider hover:bg-sd-blue/80 disabled:opacity-40 transition-colors"
                  >
                    {isBusy ? '…' : 'CONFIRM'}
                  </button>
                )}
                {(item.status === STATUS.Confirmed || item.status === STATUS.Joined) && (
                  <button
                    onClick={() => handleWithdraw(item)}
                    disabled={isBusy}
                    className="px-3 py-1 border border-sd-red/40 text-sd-red font-oswald text-xs tracking-wider hover:border-sd-red disabled:opacity-40 transition-colors"
                  >
                    {isBusy ? '…' : 'WITHDRAW'}
                  </button>
                )}
                {item.status === STATUS.Withdrawn && (
                  <button
                    onClick={() => handleReinstate(item)}
                    disabled={isBusy}
                    className="px-3 py-1 border border-sd-ink/20 text-sd-ink/50 font-oswald text-xs tracking-wider hover:border-sd-ink/40 hover:text-sd-ink/70 disabled:opacity-40 transition-colors"
                  >
                    {isBusy ? '…' : 'REINSTATE'}
                  </button>
                )}
              </div>
            </div>
          );
        })}
      </div>

      <p className="text-sd-ink/30 font-mono text-[11px] pt-1">
        Confirmed status required before a participant can join.
      </p>
    </div>
  );
}