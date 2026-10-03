// components/features/home/stat-bar.tsx
interface Stat {
  value: string;
  label: string;
}

// Phones: 2 columns. sm: 3. lg: one row, one equal column per stat (grid-flow-col +
// auto-cols-fr), so the count can change without touching the class list.
export default function StatBar({ stats }: { stats: Stat[] }) {
  return (
    <div className="bg-sd-ink text-white page-x py-6 lg:px-8 lg:py-5 grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-none lg:grid-flow-col lg:auto-cols-fr gap-x-6 gap-y-5 items-center">
      {stats.map((stat) => (
        <div key={stat.label} className="flex flex-col min-w-0">
          <div className="font-oswald font-bold text-[26px] lg:text-[32px] text-light-blue leading-[1.1] tracking-[0.02em]">
            {stat.value}
          </div>
          <div className="text-[10px] tracking-[0.22em] opacity-70 mt-1">{stat.label}</div>
        </div>
      ))}
    </div>
  );
}