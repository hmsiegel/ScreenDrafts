"use client";

import { useState, type ReactNode } from "react";

interface Props {
  /** URLs to try in order. Moves to the next one each time the browser fails to load. */
  sources: string[];
  alt: string;
  className?: string;
  /** Rendered once every source has failed (or there were none). */
  fallback?: ReactNode;
}

/**
 * An <img> that walks a list of candidate URLs. Keyed on the list, so a new set
 * of sources (for example after an upload) starts again from the first one.
 */
export default function FallbackImage(props: Props) {
  return <FallbackImageInner key={props.sources.join("|")} {...props} />;
}

function FallbackImageInner({ sources, alt, className, fallback = null }: Props) {
  const [index, setIndex] = useState(0);

  if (index >= sources.length) return <>{fallback}</>;

  return (
    <img
      src={sources[index]}
      alt={alt}
      className={className}
      onError={() => setIndex((i) => i + 1)}
    />
  );
}