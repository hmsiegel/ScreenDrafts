// app/draft-parts/[draftPartId]/live/components/honorific-overlay.tsx
'use client';

import { useEffect } from 'react';
import type { HonorificMoment } from '../lib/honorifics';

interface Props {
  // The honorific moment to announce, or null when nothing is showing.
  payload: HonorificMoment | null;
  onDismiss: () => void;
  // Auto-dismiss delay in ms — long enough to read the beat, short enough to get
  // out of the host's way before the next pick. When several moments are queued,
  // each is shown for this long before advancing to the next.
  dismissAfterMs?: number;
}

// "joins the High Five" / "earns The Cycle" — appearance honorifics read as tiers
// you join, position honorifics as feats you earn. Reverts (veto / commissioner
// override) drop the article-matched verb.
function connectorFor(moment: HonorificMoment): string {
  if (moment.axis === 'appearance') {
    return moment.direction === 'earned' ? 'joins the' : 'loses the';
  }

  return moment.direction === 'earned' ? 'earns' : 'loses';
}

// Full-screen headline moment, mirroring CountdownOverlay's treatment: a film
// reaching (or losing) a canonical honorific is the headline event of the draft,
// so it gets the same z-50 full-bleed stage rather than a corner toast.
export function HonorificOverlay({ payload, onDismiss, dismissAfterMs = 4500 }: Props) {
  useEffect(() => {
    if (!payload) {
      return;
    }

    const timer = setTimeout(onDismiss, dismissAfterMs);
    return () => clearTimeout(timer);
  }, [payload, onDismiss, dismissAfterMs]);

  if (!payload) {
    return null;
  }

  const earned = payload.direction === 'earned';

  return (
    <div
      onClick={onDismiss}
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/85 px-6 cursor-pointer"
    >
      <div className="text-center max-w-3xl">
        <p
          className={`font-oswald tracking-widest text-sm mb-6 uppercase ${
            earned ? 'text-sd-red' : 'text-white/40'
          }`}
        >
          {earned ? 'Barring a veto' : 'Vetoed'}
        </p>

        <p className="font-oswald text-white font-bold uppercase leading-none text-4xl sm:text-5xl mb-6">
          {payload.movieTitle}
        </p>

        <p className="font-oswald text-white/60 tracking-wide text-lg uppercase">
          {connectorFor(payload)}
        </p>

        <p className="font-oswald text-sd-red font-bold uppercase leading-none text-6xl sm:text-7xl mt-3">
          {payload.honorificName}
        </p>

        <p className="font-oswald text-white/30 tracking-widest text-xs mt-10 uppercase">
          Tap to dismiss
        </p>
      </div>
    </div>
  );
}