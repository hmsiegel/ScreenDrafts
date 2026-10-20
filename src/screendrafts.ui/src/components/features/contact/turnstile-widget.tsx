// src/components/features/contact/turnstile-widget.tsx
'use client';

import Script from "next/script";
import { useEffect, useRef, useState } from "react";

interface TurnstileApi {
  render: (
    container: HTMLElement,
    options: {
      sitekey: string;
      size?: "normal" | "compact";
      theme?: "light" | "dark" | "auto";
      callback: (token: string) => void;
      "expired-callback": () => void;
      "error-callback": () => void;
    }
  ) => string;
  reset: (widgetId: string) => void;
  remove: (widgetId: string) => void;
}

declare global {
  interface Window {
    turnstile?: TurnstileApi;
  }
}

const SCRIPT_SRC = "https://challenges.cloudflare.com/turnstile/v0/api.js?render=explicit";

// The normal widget is 300px wide. Below this viewport width the form card is narrower
// than that, so use the compact variant.
const COMPACT_BELOW_PX = 380;

interface TurnstileWidgetProps {
  siteKey: string;
  /** Called with a token when the challenge passes, and with null when it expires or errors. */
  onToken: (token: string | null) => void;
  /** Increment to discard the current token and run a fresh challenge. */
  resetSignal: number;
}

export default function TurnstileWidget({ siteKey, onToken, resetSignal }: TurnstileWidgetProps) {
  const containerRef = useRef<HTMLDivElement>(null);
  const widgetIdRef = useRef<string | null>(null);
  const onTokenRef = useRef(onToken);
  const [scriptReady, setScriptReady] = useState(false);

  useEffect(() => {
    onTokenRef.current = onToken;
  }, [onToken]);

  useEffect(() => {
    const container = containerRef.current;
    if (!scriptReady || !container || !window.turnstile) return;

    const widgetId = window.turnstile.render(container, {
      sitekey: siteKey,
      size: window.innerWidth < COMPACT_BELOW_PX ? "compact" : "normal",
      theme: "light",
      callback: (token) => onTokenRef.current(token),
      "expired-callback": () => onTokenRef.current(null),
      "error-callback": () => onTokenRef.current(null),
    });
    widgetIdRef.current = widgetId;

    return () => {
      window.turnstile?.remove(widgetId);
      widgetIdRef.current = null;
    };
  }, [scriptReady, siteKey]);

  useEffect(() => {
    if (resetSignal > 0 && widgetIdRef.current) {
      window.turnstile?.reset(widgetIdRef.current);
    }
  }, [resetSignal]);

  return (
    <>
      <Script src={SCRIPT_SRC} strategy="afterInteractive" onReady={() => setScriptReady(true)} />
      <div ref={containerRef} className="min-h-[65px]" />
    </>
  );
}
