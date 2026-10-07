// components/features/stats/tiered-record-row.tsx
"use client";

import { useState } from "react";
import type { RecordView } from "@/services/stats/stats-types";
import { RecordBody } from "./record-row";

/** A record with a minimum-drafts qualifier. The picker switches between the 5+, 10+, 15+ and 20+ draft versions. */
export function TieredRecordRow({ record }: { record: RecordView }) {
  const [selected, setSelected] = useState(0);
  const tier = record.tiers[selected] ?? record.tiers[0];
  if (!tier) return null;

  return (
    <li className="py-3 border-t border-sd-ink/10 first:border-t-0 first:pt-0">
      <RecordBody record={record} tier={tier} />
      <div role="group" aria-label={`Minimum drafts for ${record.label}`} className="mt-2 flex flex-wrap gap-1.5">
        {record.tiers.map((t, index) => (
          <button
            key={t.tier}
            type="button"
            aria-pressed={index === selected}
            onClick={() => setSelected(index)}
            className={`min-h-8 px-2.5 border font-mono text-[10px] tracking-widest transition-colors ${
              index === selected
                ? "bg-sd-ink text-white border-sd-ink"
                : "bg-white text-sd-ink border-sd-ink/30 hover:border-sd-ink"
            }`}
          >
            {t.tier}+ DRAFTS
          </button>
        ))}
      </div>
    </li>
  );
}
