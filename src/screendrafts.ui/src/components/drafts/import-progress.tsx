// components/drafts/import-progress.tsx
"use client";

import { useEffect, useState } from "react";

interface ImportProgressProps {
  /** Title being imported. Omit when it isn't known yet (e.g. manual TMDb ID entry). */
  label?: string | null;
  /** "light" for admin and site surfaces, "dark" for the live draft panels. */
  theme?: "light" | "dark";
  /** Seconds before the "taking longer than usual" line appears. */
  slowAfterSeconds?: number;
  className?: string;
}

const THEMES = {
  light: {
    track: "bg-sd-ink/10",
    bar: "bg-sd-blue",
    text: "text-sd-ink/60",
    hint: "text-sd-ink/40",
  },
  dark: {
    track: "bg-white/10",
    bar: "bg-sd-red",
    text: "text-white/60",
    hint: "text-white/40",
  },
} as const;

/**
 * Indeterminate "import in flight" indicator. The import endpoint returns 202
 * and the UI polls until the media row lands, so there is no real percentage
 * to show — a sliding bar says "still working" without faking one.
 *
 * Render it conditionally (`{importing && <ImportProgress />}`). It mounts when
 * an import starts and unmounts when it ends, so the slow-import timer
 * restarts for every import with no reset logic.
 */
export function ImportProgress({
  label,
  theme = "light",
  slowAfterSeconds = 5,
  className = "",
}: ImportProgressProps) {
  const [slow, setSlow] = useState(false);

  useEffect(() => {
    const timer = setTimeout(() => setSlow(true), slowAfterSeconds * 1000);
    return () => clearTimeout(timer);
  }, [slowAfterSeconds]);

  const t = THEMES[theme];

  return (
    <div role="status" aria-live="polite" className={className}>
      <style>{`
        @keyframes sd-import-progress-slide {
          from { transform: translateX(0); }
          to { transform: translateX(150%); }
        }
        @keyframes sd-import-progress-pulse {
          from { opacity: 0.25; }
          to { opacity: 0.9; }
        }
        .sd-import-progress-bar {
          animation: sd-import-progress-slide 1s ease-in-out infinite alternate;
        }
        @media (prefers-reduced-motion: reduce) {
          .sd-import-progress-bar {
            width: 100%;
            animation: sd-import-progress-pulse 1.2s ease-in-out infinite alternate;
          }
        }
      `}</style>

      <p className={`text-[11px] font-mono ${t.text}`}>
        {label ? `Importing “${label}” from TMDb…` : "Importing from TMDb…"}
      </p>

      <div
        className={`relative mt-1.5 h-1 w-full overflow-hidden rounded-full ${t.track}`}
        aria-hidden="true"
      >
        <div
          className={`sd-import-progress-bar absolute inset-y-0 left-0 w-2/5 rounded-full ${t.bar}`}
        />
      </div>

      {slow && (
        <p className={`mt-1.5 text-[11px] font-mono ${t.hint}`}>
          Still working. New titles can take a few seconds.
        </p>
      )}
    </div>
  );
}