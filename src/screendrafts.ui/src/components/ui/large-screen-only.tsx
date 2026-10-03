// components/ui/large-screen-only.tsx
//
// Shows `children` only on screens big enough for a live draft, and `fallback`
// everywhere else. Used for actions that drop the user straight into a live
// draft — e.g. a host's START — so they can't be taken from a phone.
//
// Pure CSS, so it works in server components and never flashes: both branches
// render, and the media query picks one. The query must match QUERY in
// large-screen-gate.tsx, which guards the live pages themselves; Tailwind needs
// the literal class names, so it can't be imported from there.
//
// This only hides the control. The API still accepts the request from any device.

import type { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

export function LargeScreenOnly({ children, fallback }: Props) {
  return (
    <>
      <span className="hidden [@media(min-width:700px)_and_(min-height:500px)]:contents">
        {children}
      </span>
      {fallback && (
        <span className="contents [@media(min-width:700px)_and_(min-height:500px)]:hidden">
          {fallback}
        </span>
      )}
    </>
  );
}

/** Standard fallback note for a hidden START button. */
export function StartOnLargeScreenNote({ className = '' }: { className?: string }) {
  return (
    <span className={`font-mono text-[10px] tracking-widest uppercase text-sd-ink/50 ${className}`}>
      Start on a tablet or computer
    </span>
  );
}