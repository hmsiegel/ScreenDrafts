// app/about/layout.tsx
import { AboutTabs } from "@/components/features/about/about-tabs";

export default function AboutLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen bg-light-blue font-sans">
      <div className="max-w-2xl mx-auto page-x pt-8 lg:pt-12">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-3">ABOUT</p>
        <h1 className="font-oswald font-bold text-[36px] sm:text-[52px] leading-none tracking-[-0.01em] text-sd-ink mb-4">
          ABOUT SCREEN DRAFTS
        </h1>
        <p className="text-sd-ink/55 leading-relaxed text-[15px] max-w-lg mb-6">
          Screen Drafts is a serpentine-style draft. Guest G.M.s alternate placing picks on the single, definitive
          &ldquo;Best Of&rdquo; list for the episode&apos;s topic. This site archives every list.
        </p>
        <AboutTabs />
      </div>
      <div className="max-w-2xl mx-auto page-x py-8 lg:py-10">{children}</div>
    </div>
  );
}
