// components/features/stats/record-section.tsx
import type { RecordSectionView } from "@/services/stats/stats-types";
import { RecordRow } from "./record-row";
import { TieredRecordRow } from "./tiered-record-row";

// One card per group. Phones: one column. lg: two, so a long group does not stretch a short one's card.
export function RecordSection({ section }: { section: RecordSectionView }) {
  const groups = section.groups.filter((g) => g.records.length > 0);
  if (groups.length === 0) return null;

  return (
    <section aria-labelledby={`record-section-${section.key}`} className="mt-10 first:mt-0">
      <h2
        id={`record-section-${section.key}`}
        className="font-oswald font-bold text-[24px] sm:text-[28px] leading-none text-sd-ink mb-5 uppercase"
      >
        {section.title}
      </h2>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5 items-start">
        {groups.map((group) => (
          <div key={group.key} className="bg-white border-2 border-sd-ink p-5 sm:p-6 min-w-0">
            <h3 className="font-oswald font-bold text-[13px] tracking-widest text-sd-red mb-4 uppercase">
              {group.title}
            </h3>
            <ul>
              {group.records.map((record) =>
                record.tiers.length > 1 ? (
                  <TieredRecordRow key={record.key} record={record} />
                ) : (
                  <RecordRow key={record.key} record={record} />
                )
              )}
            </ul>
          </div>
        ))}
      </div>
    </section>
  );
}
