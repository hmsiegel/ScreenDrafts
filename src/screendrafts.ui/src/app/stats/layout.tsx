// app/stats/layout.tsx
import { StatsTabs } from "@/components/features/stats/stats-tabs";

export default function StatsLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen bg-light-blue font-sans">
      <div className="page-x pt-8 lg:pt-10 max-w-[1200px] mx-auto">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-3">STATS</p>
        <h1 className="font-oswald font-bold text-[30px] sm:text-[40px] leading-none text-sd-ink mb-6 [overflow-wrap:anywhere]">
          RECORDS &amp; STATS
        </h1>
        <StatsTabs />
      </div>
      <div className="page-x py-8 lg:py-10 max-w-[1200px] mx-auto">{children}</div>
    </div>
  );
}
