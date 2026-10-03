'use client';

// components/ui/info-tooltip.tsx
// Accessible click/focus tooltip triggered by an ⓘ icon.
// Usage:
//   <InfoTooltip>Your explanation text here.</InfoTooltip>
//
// The popover renders in a portal with fixed positioning, for two reasons:
//   1. Callers like the My Drafts tab bar scroll sideways on phones. A scroll
//      container clips in both directions, so an absolutely-positioned popover
//      inside it was cut off.
//   2. Its position is clamped to the viewport. A 256px popover centred on an
//      icon near the screen edge used to hang off the side of a phone.
// Same approach as AddToCalendarButton's menu.

import { useEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';

interface InfoTooltipProps {
  children: React.ReactNode;
  /** Positioning hint — defaults to 'top'. Use 'bottom' when the trigger is near the top of the viewport. */
  position?: 'top' | 'bottom';
}

const POPOVER_WIDTH = 256; // w-64
const EDGE_GAP = 8;        // keep this far from the viewport edges
const OFFSET = 8;          // gap between icon and popover (mt-2 / mb-2)

interface Placement {
  top: number;
  left: number;
  width: number;
  arrowLeft: number;
}

export default function InfoTooltip({ children, position = 'top' }: InfoTooltipProps) {
  const [placement, setPlacement] = useState<Placement | null>(null);
  const triggerRef = useRef<HTMLSpanElement>(null);
  const open = placement !== null;

  function place(): Placement | null {
    const rect = triggerRef.current?.getBoundingClientRect();
    if (!rect) return null;
    const width = Math.min(POPOVER_WIDTH, window.innerWidth - EDGE_GAP * 2);
    const centre = rect.left + rect.width / 2;
    const left = Math.min(Math.max(centre - width / 2, EDGE_GAP), window.innerWidth - EDGE_GAP - width);
    const top = position === 'bottom' ? rect.bottom + OFFSET : rect.top - OFFSET;
    // Arrow stays over the icon even when the popover is pushed in from an edge.
    const arrowLeft = Math.min(Math.max(centre - left, 12), width - 12);
    return { top, left, width, arrowLeft };
  }

  function toggle() {
    setPlacement((current) => (current ? null : place()));
  }

  // While open: close on outside press, Escape, resize, or any scroll —
  // a fixed popover would otherwise drift away from its icon.
  useEffect(() => {
    if (!open) return;
    const close = () => setPlacement(null);
    function onPointerDown(e: PointerEvent) {
      if (triggerRef.current?.contains(e.target as Node)) return;
      close();
    }
    function onKeyDown(e: KeyboardEvent) {
      if (e.key === 'Escape') close();
    }
    document.addEventListener('pointerdown', onPointerDown);
    document.addEventListener('keydown', onKeyDown);
    window.addEventListener('resize', close);
    window.addEventListener('scroll', close, true);
    return () => {
      document.removeEventListener('pointerdown', onPointerDown);
      document.removeEventListener('keydown', onKeyDown);
      window.removeEventListener('resize', close);
      window.removeEventListener('scroll', close, true);
    };
  }, [open]);

  return (
    <span className="relative inline-flex items-center">
      <span
        ref={triggerRef}
        role="button"
        tabIndex={0}
        aria-label="More information"
        aria-expanded={open}
        onClick={(e) => {
          // The icon often sits inside another control (a tab button). Don't let
          // opening the tooltip also trigger that control.
          e.stopPropagation();
          toggle();
        }}
        onKeyDown={(e) => {
          if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            e.stopPropagation();
            toggle();
          }
        }}
        className="text-sd-ink/30 hover:text-sd-blue transition-colors focus:outline-none focus-visible:ring-1 focus-visible:ring-sd-blue rounded-full cursor-pointer p-1 -m-1"
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

      {placement &&
        createPortal(
          <div
            role="tooltip"
            style={{
              position: 'fixed',
              top: placement.top,
              left: placement.left,
              width: placement.width,
              transform: position === 'top' ? 'translateY(-100%)' : undefined,
            }}
            className="z-50 bg-sd-ink text-white text-xs leading-relaxed px-3.5 py-3 shadow-lg pointer-events-none normal-case tracking-normal font-sans font-normal text-left"
          >
            {/* Arrow */}
            <span
              style={{ left: placement.arrowLeft }}
              className={`absolute -translate-x-1/2 w-2 h-2 bg-sd-ink rotate-45 ${
                position === 'bottom' ? '-top-1' : '-bottom-1'
              }`}
              aria-hidden="true"
            />
            {children}
          </div>,
          document.body,
        )}
    </span>
  );
}