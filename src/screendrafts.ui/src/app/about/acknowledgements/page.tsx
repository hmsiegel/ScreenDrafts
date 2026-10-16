// app/about/acknowledgements/page.tsx
import type { Metadata } from "next";
import { Callout } from "@/components/features/about/about-section";
import { CreditLogo } from "@/components/features/about/credit-logo";
import { CREDIT_GROUPS } from "../credits";

export const metadata: Metadata = {
  title: "Acknowledgements",
  description: "The open source projects and services behind this site.",
};

const WIKI_URL = process.env.NEXT_PUBLIC_WIKI_URL;

export default function AboutAcknowledgementsPage() {
  return (
    <div className="space-y-10">
      <div className="space-y-3 text-sd-ink/70 leading-relaxed text-[15px]">
        <p className="text-[10px] tracking-[0.28em] font-bold text-sd-red">THANK YOU</p>
        <p>
          Fan-built software stands on other people&apos;s work. These projects and services
          make this site run.
        </p>
      </div>

      {CREDIT_GROUPS.map((group) => (
        <section key={group.title}>
          <h2 className="font-mono text-[10px] tracking-widest font-bold text-sd-red mb-3">
            {group.title}
          </h2>
          <ul className="grid grid-cols-1 sm:grid-cols-2 gap-x-8">
            {group.credits.map((credit) => (
              <li
                key={credit.name}
                className="flex items-center gap-3 border-b border-sd-ink/10 py-2.5"
              >
                <CreditLogo name={credit.name} logo={credit.logo} />
                <a
                  href={credit.href}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="font-oswald font-bold tracking-[0.03em] text-sd-blue hover:text-sd-red transition-colors"
                >
                  {credit.name}
                </a>
                <span className="ml-auto text-[13px] text-sd-ink/50 text-right">{credit.role}</span>
              </li>
            ))}
          </ul>
        </section>
      ))}

      <Callout>This product uses the TMDb API but is not endorsed or certified by TMDB.</Callout>

      {WIKI_URL && (
        <p className="text-sd-ink/70 leading-relaxed text-[15px]">
          Record keeping leans on the{" "}
          <a
            href={WIKI_URL}
            target="_blank"
            rel="noopener noreferrer"
            className="text-sd-blue hover:text-sd-red transition-colors underline underline-offset-2"
          >
            Screen Drafts wiki
          </a>
          , maintained by fans.
        </p>
      )}
    </div>
  );
}
