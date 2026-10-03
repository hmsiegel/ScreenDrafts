// components/ui/large-screen-gate.tsx
'use client';

// Keeps the live draft screens off phones. The children (which mount the
// SignalR provider) are not rendered at all below the threshold, so a phone
// never opens a hub connection or receives gameplay events for a screen it
// can't use. A CSS-only hide would still mount the provider.
//
// Threshold, by device:
//   - width >= 700px admits an iPad mini in portrait (744px wide), which a
//     plain `md` (768px) cutoff would lock out.
//   - height >= 500px keeps phones out in landscape — a large phone is ~930px
//     wide but only ~430px tall that way round.
//
// Once the screen has fit, it stays admitted for the rest of the visit. Without
// that latch, narrowing a desktop window mid-draft would unmount the provider and
// drop a host's live connection.

import { useEffect, useState, useSyncExternalStore } from 'react';
import type { ReactNode } from 'react';
import Link from 'next/link';

const QUERY = '(min-width: 700px) and (min-height: 500px)';

function subscribe(onChange: () => void) {
  const mq = window.matchMedia(QUERY);
  mq.addEventListener('change', onChange);
  return () => mq.removeEventListener('change', onChange);
}

const getSnapshot = () => window.matchMedia(QUERY).matches;
// Unknown during server render and hydration. Rendering a neutral placeholder
// then avoids both a hydration mismatch and a flash of the "too small" message
// on desktop.
const getServerSnapshot = () => null;

interface Props {
  children: ReactNode;
  /** Where the "too small" screen sends people. */
  backHref: string;
  backLabel: string;
}

export function LargeScreenGate({ children, backHref, backLabel }: Props) {
  const fits = useSyncExternalStore<boolean | null>(subscribe, getSnapshot, getServerSnapshot);
  const [admitted, setAdmitted] = useState(false);

  useEffect(() => {
    if (fits) setAdmitted(true);
  }, [fits]);

  if (admitted || fits) return <>{children}</>;
  if (fits === null) return <div className="min-h-screen bg-sd-ink" />;

  return (
    <div className="min-h-[70vh] bg-sd-ink text-sd-paper page-x py-16 flex items-center justify-center">
      <div className="max-w-sm text-center">
        <h1 className="font-oswald font-bold text-[28px] leading-tight tracking-wide">
          LIVE DRAFTS NEED A BIGGER SCREEN
        </h1>
        <p className="mt-4 text-[15px] leading-relaxed text-white/70">
          The board and pick controls don&rsquo;t fit on a phone. Open this draft on a tablet or
          a computer — your seat in the draft is waiting there.
        </p>
        <Link
          href={backHref}
          className="mt-8 inline-flex items-center min-h-11 border-2 border-white/80 px-5 font-oswald font-semibold tracking-[0.14em] text-sm hover:bg-white hover:text-sd-ink transition-colors"
        >
          ← BACK TO {backLabel.toUpperCase()}
        </Link>
      </div>
    </div>
  );
}