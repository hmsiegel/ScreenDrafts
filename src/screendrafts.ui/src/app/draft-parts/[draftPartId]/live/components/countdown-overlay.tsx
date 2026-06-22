// app/draft-parts/[draftPartId]/live/components/countdown-overlay.tsx
'use client';

import { useEffect, useState } from 'react';

interface Props {
  onComplete: () => void;
  // Optional: called when the countdown is dismissed manually before it
  // reaches zero. Distinct from onComplete — a manual dismiss is not the
  // same event as the countdown running out naturally, since callers may
  // want different behavior (e.g. natural completion triggers an
  // auto-pick-on-timeout rule; a manual dismiss should not).
  onDismiss?: () => void;
}

export function CountdownOverlay({ onComplete, onDismiss }: Props) {
  const [count, setCount] = useState(5);

  useEffect(() => {
    if (count <= 0) { onComplete(); return; }
    const t = setTimeout(() => setCount((c) => c - 1), 1000);
    return () => clearTimeout(t);
  }, [count, onComplete]);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80">
      <div className="text-center">
        <p className="font-oswald text-white/50 tracking-widest text-sm mb-4 uppercase">
          Time to pick
        </p>
        <span className="font-oswald text-sd-red font-bold text-[120px] leading-none">
          {count}
        </span>
        {onDismiss && (
          <button
            onClick={onDismiss}
            className="block mx-auto mt-6 px-6 py-2 border border-white/20 text-white/50 font-oswald text-xs tracking-widest uppercase hover:border-white hover:text-white transition-colors"
          >
            Dismiss
          </button>
        )}
      </div>
    </div>
  );
}