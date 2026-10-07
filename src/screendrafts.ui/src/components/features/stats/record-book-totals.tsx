// components/features/stats/record-book-totals.tsx
import type { RecordTotalView } from "@/services/stats/stats-types";
import { formatStatValue } from "./format-stat-value";

// Phones: 2 columns. sm: 3. lg: 6, so twelve totals make two tidy rows.
export function RecordBookTotals({ totals }: { totals: RecordTotalView[] }) {
  if (totals.length === 0) return null;

  return (
    <div className="bg-sd-ink text-white px-5 py-6 lg:px-8 grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-x-6 gap-y-6">
      {totals.map((total) => (
        <div key={total.code} className="flex flex-col min-w-0">
          <div className="font-oswald font-bold text-[26px] lg:text-[30px] text-light-blue leading-[1.1] tracking-[0.02em]">
            {formatStatValue(total.value, "count")}
          </div>
          <div className="text-[10px] tracking-[0.18em] opacity-70 mt-1 uppercase">{total.label}</div>
        </div>
      ))}
    </div>
  );
}
