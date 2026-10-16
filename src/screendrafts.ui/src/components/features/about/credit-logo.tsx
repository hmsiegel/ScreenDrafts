// components/features/about/credit-logo.tsx
import { artifactUrl } from "@/lib/cdn";

interface CreditLogoProps {
  name: string;
  logo?: string;
}

// Logos live under artifacts/logos/ with the banners and badges.
export function CreditLogo({ name, logo }: CreditLogoProps) {
  if (!logo) {
    return (
      <span
        aria-hidden="true"
        className="shrink-0 size-6 grid place-items-center rounded-sm border border-sd-ink/25 font-oswald font-bold text-[12px] leading-none text-sd-ink/45"
      >
        {name.replace(/^\./, "").charAt(0).toUpperCase()}
      </span>
    );
  }

  return (
    // eslint-disable-next-line @next/next/no-img-element -- small static SVGs; nothing for next/image to optimize
    <img
      src={artifactUrl(`logos/${logo}.svg`)}
      alt=""
      aria-hidden="true"
      width={24}
      height={24}
      loading="lazy"
      className="shrink-0 size-6 object-contain opacity-70"
    />
  );
}