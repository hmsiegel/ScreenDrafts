'use client';

// components/ui/info-tooltip.tsx
// Accessible hover/focus tooltip triggered by an ⓘ icon button.
// Usage:
//   <InfoTooltip>Your explanation text here.</InfoTooltip>

import { useEffect, useRef, useState } from 'react';

interface InfoTooltipProps {
  children: React.ReactNode;
  /** Positioning hint — defaults to 'top'. Use 'bottom' when the tab bar is near the top of the viewport. */
  position?: 'top' | 'bottom';
}

export default function InfoTooltip({ children, position = 'top' }: InfoTooltipProps) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  // Close on outside click
  useEffect(() => {
    if (!open) return;
    function handler(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, [open]);

  // Close on Escape
  useEffect(() => {
    if (!open) return;
    function handler(e: KeyboardEvent) {
      if (e.key === 'Escape') setOpen(false);
    }
    document.addEventListener('keydown', handler);
    return () => document.removeEventListener('keydown', handler);
  }, [open]);

  const popoverPosition =
    position === 'bottom'
      ? 'top-full mt-2'
      : 'bottom-full mb-2';

  return (
    <div ref={ref} className="relative inline-flex items-center">
      <span
        role="button"
        tabIndex={0}
        aria-label="More information"
        aria-expanded={open}
        onClick={() => setOpen(v => !v)}
        onKeyDown={e => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); setOpen(v => !v); } }}
        className="text-sd-ink/30 hover:text-sd-blue transition-colors focus:outline-none focus-visible:ring-1 focus-visible:ring-sd-blue rounded-full cursor-pointer"
      >
        <svg
          xmlns="http://www.w3.org/2000/svg"
          viewBox="0 0 20 20"
          fill="currentColor"
          className="w-3.5 h-3.5"
          aria-hidden="true"
        >
          <path
            fillRule="evenodd"
            d="M18 10a8 8 0 1 1-16 0 8 8 0 0 1 16 0Zm-7-4a1 1 0 1 1-2 0 1 1 0 0 1 2 0ZM9 9a.75.75 0 0 0 0 1.5h.253a.25.25 0 0 1 .244.304l-.459 2.066A1.75 1.75 0 0 0 10.747 15H11a.75.75 0 0 0 0-1.5h-.253a.25.25 0 0 1-.244-.304l.459-2.066A1.75 1.75 0 0 0 9.253 9H9Z"
            clipRule="evenodd"
          />
        </svg>
      </span>

      {open && (
        <div
          role="tooltip"
          className={`absolute left-1/2 -translate-x-1/2 ${popoverPosition} z-50 w-64 bg-sd-ink text-white text-xs leading-relaxed px-3.5 py-3 shadow-lg pointer-events-none`}
        >
          {/* Arrow */}
          <span
            className={`absolute left-1/2 -translate-x-1/2 w-2 h-2 bg-sd-ink rotate-45 ${
              position === 'bottom' ? '-top-1' : '-bottom-1'
            }`}
            aria-hidden="true"
          />
          {children}
        </div>
      )}
    </div>
  );
}