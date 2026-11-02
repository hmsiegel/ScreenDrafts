// components/ui/back-to-top.tsx
'use client';

// A button pinned to the bottom-right corner that scrolls the page to the top.
// It stays hidden until the reader has scrolled `threshold` pixels, so short pages
// never show it.

import { useEffect, useState } from "react";

export default function BackToTop({ threshold = 800 }: { threshold?: number }) {
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    const onScroll = () => setVisible(window.scrollY > threshold);
    onScroll();
    window.addEventListener("scroll", onScroll, { passive: true });
    return () => window.removeEventListener("scroll", onScroll);
  }, [threshold]);

  const scrollToTop = () => {
    const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    window.scrollTo({ top: 0, behavior: reduceMotion ? "auto" : "smooth" });
  };

  if (!visible) return null;

  return (
    <button
      type="button"
      onClick={scrollToTop}
      aria-label="Back to top"
      className="fixed bottom-4 right-4 sm:bottom-6 sm:right-6 z-40 px-4 py-3 bg-sd-ink text-white border-2 border-sd-ink font-mono text-[11px] tracking-widest font-bold shadow-lg hover:bg-sd-red hover:border-sd-red transition-colors"
    >
      ↑ BACK TO TOP
    </button>
  );
}
