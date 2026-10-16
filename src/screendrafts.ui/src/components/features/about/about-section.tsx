// components/features/about/about-section.tsx
interface SectionProps {
  number: string;
  subtitle: string;
  title: string;
  children: React.ReactNode;
}

export function Section({ number, subtitle, title, children }: SectionProps) {
  return (
    <section className="border-t border-sd-ink/10 pt-10">
      <div className="flex flex-col gap-1 sm:flex-row sm:items-start sm:gap-6">
        <span className="font-oswald font-bold text-[40px] sm:text-[56px] leading-none text-sd-ink/10 select-none sm:w-14 shrink-0">
          {number}
        </span>
        <div className="flex-1 min-w-0">
          <p className="text-[10px] tracking-[0.28em] font-bold text-sd-red mb-1">{subtitle}</p>
          <h2 className="font-oswald font-bold text-2xl sm:text-3xl tracking-[0.03em] text-sd-ink mb-4">
            {title}
          </h2>
          <div className="space-y-3 text-sd-ink/70 leading-relaxed text-[15px]">{children}</div>
        </div>
      </div>
    </section>
  );
}

export function Callout({ children }: { children: React.ReactNode }) {
  return (
    <div className="border-l-2 border-sd-red pl-4 py-0.5 text-sd-ink/60 text-sm leading-relaxed">
      {children}
    </div>
  );
}
