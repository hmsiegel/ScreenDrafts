// components/features/stats/record-row.tsx
import type { RecordTierView, RecordView } from "@/services/stats/stats-types";
import { formatStatValue } from "./format-stat-value";
import { HolderList } from "./holder-list";

/** The value and holders of one record, for one tier. Shared by the plain and the tiered row. */
export function RecordBody({ record, tier }: { record: RecordView; tier: RecordTierView }) {
  return (
    <>
      <div className="flex items-baseline justify-between gap-3">
        <span className="font-mono text-[11px] tracking-wide text-[#5a6075]">
          {record.label.toUpperCase()}
        </span>
        <span className="font-oswald font-bold text-[22px] text-sd-ink leading-none shrink-0">
          {formatStatValue(tier.value, record.format)}
        </span>
      </div>
      <HolderList holders={tier.holders} />
    </>
  );
}

/** A record with no minimum-drafts qualifier. */
export function RecordRow({ record }: { record: RecordView }) {
  const tier = record.tiers[0];
  if (!tier) return null;

  return (
    <li className="py-3 border-t border-sd-ink/10 first:border-t-0 first:pt-0">
      <RecordBody record={record} tier={tier} />
    </li>
  );
}
