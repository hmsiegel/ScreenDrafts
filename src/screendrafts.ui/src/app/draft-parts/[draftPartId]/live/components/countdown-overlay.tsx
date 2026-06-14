// app/draft-parts/[draftPartId]/live/components/countdown-overlay.tsx
'use client';

import { useEffect, useState } from 'react';

interface Props {
  onComplete: () => void;
}

export function CountdownOverlay({ onComplete }: Props) {
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
      </div>
    </div>
  );
}