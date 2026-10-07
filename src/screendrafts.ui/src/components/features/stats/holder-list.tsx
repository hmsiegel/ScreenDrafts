// components/features/stats/holder-list.tsx
import Link from "next/link";
import type { RecordHolderView } from "@/services/stats/stats-types";
import { statHref } from "./format-stat-value";

const MAX_SHOWN = 8;

/** Everyone sharing a record. A tie lists every holder, each with the draft or count it was set in. */
export function HolderList({ holders }: { holders: RecordHolderView[] }) {
  if (holders.length === 0) return null;

  const shown = holders.slice(0, MAX_SHOWN);
  const hidden = holders.length - shown.length;

  return (
    <ul className="mt-1.5 flex flex-col gap-0.5">
      {shown.map((holder) => {
        const href = statHref(holder.kind, holder.publicId);
        const key = `${holder.kind}:${holder.publicId ?? holder.name}:${holder.context ?? ""}`;

        return (
          <li key={key} className="flex flex-wrap items-baseline gap-x-2 leading-snug">
            {href ? (
              <Link
                href={href}
                className="font-oswald font-bold text-[15px] text-sd-blue hover:text-sd-red transition-colors [overflow-wrap:anywhere]"
              >
                {holder.name}
              </Link>
            ) : (
              <span className="font-oswald font-bold text-[15px] text-sd-ink [overflow-wrap:anywhere]">
                {holder.name}
              </span>
            )}
            {holder.context && (
              <span className="font-mono text-[11px] text-[#5a6075] [overflow-wrap:anywhere]">
                {holder.context}
              </span>
            )}
          </li>
        );
      })}
      {hidden > 0 && (
        <li className="font-mono text-[11px] text-[#5a6075]">and {hidden} more</li>
      )}
    </ul>
  );
}
