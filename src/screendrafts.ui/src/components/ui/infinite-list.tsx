// components/ui/infinite-list.tsx
'use client';

// Infinite scroll for the paged lists, below lg only.
//
// The page renders page N on the server as usual. This hook keeps those items and
// appends later pages as a sentinel near the bottom scrolls into view. The sentinel
// sits inside an `lg:hidden` block: at lg and up it is display:none, an
// IntersectionObserver never reports it as visible, and nothing extra loads. Desktop
// keeps its paginator without any JS breakpoint check on the loading path.
//
// Back navigation: the list component unmounts when you open a draft, so pressing
// Back would drop every appended page and strand you at the top. Loaded items and
// the scroll position go to sessionStorage and come back on a Back/Forward return.

import { useCallback, useEffect, useLayoutEffect, useRef, useState } from "react";
import type { ReactNode, RefObject } from "react";
import type { ListPage } from "@/lib/list-queries";

const MOBILE_QUERY = "(max-width: 1023px)";
const STORAGE_PREFIX = "infinite-list:";

// ── Back/Forward detection ────────────────────────────────────────────────
// App Router navigations never reload the document, so the Navigation Timing API only
// describes the first load. A popstate a moment before mount means Back/Forward.
// The listener registers when this module first loads. By the time anyone can press
// Back to a list, they have already visited it, so the module is loaded.
let lastPopstateAt = 0;
let firstMountChecked = false;
if (typeof window !== "undefined") {
  window.addEventListener("popstate", () => {
    lastPopstateAt = Date.now();
  });
}

function arrivedByBackForward(): boolean {
  if (Date.now() - lastPopstateAt < 3000) return true;
  // A full reload via Back (e.g. the tab was discarded) is visible only on the first mount.
  if (!firstMountChecked) {
    firstMountChecked = true;
    const nav = performance.getEntriesByType?.("navigation")[0] as PerformanceNavigationTiming | undefined;
    return nav?.type === "back_forward";
  }
  return false;
}

// ── Storage ───────────────────────────────────────────────────────────────

interface Saved<T> {
  items: T[];
  page: number;
  total: number;
  totalPages: number;
}

function readSaved<T>(key: string): Saved<T> | null {
  try {
    const raw = sessionStorage.getItem(STORAGE_PREFIX + key);
    return raw ? (JSON.parse(raw) as Saved<T>) : null;
  } catch {
    return null;
  }
}

function writeSaved<T>(key: string, value: Saved<T>) {
  try {
    sessionStorage.setItem(STORAGE_PREFIX + key, JSON.stringify(value));
  } catch {
    // Quota or privacy mode — Back just starts from the top again.
  }
}

function readScroll(key: string): number | null {
  try {
    const raw = sessionStorage.getItem(`${STORAGE_PREFIX}${key}:y`);
    return raw ? Number(raw) : null;
  } catch {
    return null;
  }
}

function writeScroll(key: string, y: number) {
  try {
    sessionStorage.setItem(`${STORAGE_PREFIX}${key}:y`, String(Math.round(y)));
  } catch {
    // ignore
  }
}

function clearSaved(key: string) {
  try {
    sessionStorage.removeItem(STORAGE_PREFIX + key);
    sessionStorage.removeItem(`${STORAGE_PREFIX}${key}:y`);
  } catch {
    // ignore
  }
}

// ── Hook ──────────────────────────────────────────────────────────────────

export type InfiniteStatus = "idle" | "loading" | "error" | "done";

interface UseInfiniteListOptions<T> {
  /** Unique per list view, page included. The component using this hook must also take it as its React key. */
  cacheKey: string;
  initialItems: T[];
  initialPage: number;
  total: number;
  totalPages: number;
  fetchPage: (page: number) => Promise<ListPage<T>>;
  /** Deduplicates rows that shift across page boundaries when items are added mid-scroll. */
  getKey: (item: T) => string;
}

export function useInfiniteList<T>({
  cacheKey,
  initialItems,
  initialPage,
  total: initialTotal,
  totalPages: initialTotalPages,
  fetchPage,
  getKey,
}: UseInfiniteListOptions<T>) {
  const [items, setItems] = useState(initialItems);
  const [page, setPage] = useState(initialPage);
  const [total, setTotal] = useState(initialTotal);
  const [totalPages, setTotalPages] = useState(initialTotalPages);
  const [status, setStatus] = useState<InfiniteStatus>(initialPage >= initialTotalPages ? "done" : "idle");

  const sentinelRef = useRef<HTMLDivElement>(null);
  const loadingRef = useRef(false);
  const pendingScrollRef = useRef<number | null>(null);
  const restoredRef = useRef(false);

  // Restore after a Back/Forward return; any other arrival starts fresh.
  useEffect(() => {
    if (!window.matchMedia(MOBILE_QUERY).matches) return;
    if (!arrivedByBackForward()) {
      clearSaved(cacheKey);
      return;
    }
    const saved = readSaved<T>(cacheKey);
    if (!saved || saved.page <= initialPage) return;
    restoredRef.current = true;
    pendingScrollRef.current = readScroll(cacheKey);
    setItems(saved.items);
    setPage(saved.page);
    setTotal(saved.total);
    setTotalPages(saved.totalPages);
    setStatus(saved.page >= saved.totalPages ? "done" : "idle");
    // Mount-only by design: cacheKey changes remount the component (it is the React key).
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Scroll once the restored rows are in the DOM, before paint.
  useLayoutEffect(() => {
    if (pendingScrollRef.current === null) return;
    window.scrollTo(0, pendingScrollRef.current);
    pendingScrollRef.current = null;
  }, [items]);

  // Persist after each appended page, never for page one alone (nothing to restore).
  useEffect(() => {
    if (page > initialPage) writeSaved(cacheKey, { items, page, total, totalPages });
  }, [cacheKey, initialPage, items, page, total, totalPages]);

  // Track scroll position, throttled to one write per frame.
  useEffect(() => {
    let frame = 0;
    const onScroll = () => {
      if (frame) return;
      frame = requestAnimationFrame(() => {
        frame = 0;
        writeScroll(cacheKey, window.scrollY);
      });
    };
    window.addEventListener("scroll", onScroll, { passive: true });
    return () => {
      window.removeEventListener("scroll", onScroll);
      if (frame) cancelAnimationFrame(frame);
    };
  }, [cacheKey]);

  const loadMore = useCallback(async () => {
    if (loadingRef.current) return;
    loadingRef.current = true;
    setStatus("loading");
    try {
      const next = page + 1;
      const result = await fetchPage(next);
      setItems((prev) => {
        const seen = new Set(prev.map(getKey));
        return [...prev, ...result.items.filter((item) => !seen.has(getKey(item)))];
      });
      setPage(next);
      setTotal(result.total);
      setTotalPages(result.totalPages);
      setStatus(next >= result.totalPages || result.items.length === 0 ? "done" : "idle");
    } catch {
      setStatus("error");
    } finally {
      loadingRef.current = false;
    }
  }, [fetchPage, getKey, page]);

  // A fresh observer each time loading settles back to idle. observe() reports the
  // current state immediately, so a sentinel still on screen after a short page
  // triggers the next load without waiting for another scroll.
  useEffect(() => {
    const el = sentinelRef.current;
    if (!el || status !== "idle") return;
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((e) => e.isIntersecting)) void loadMore();
      },
      { rootMargin: "800px 0px" }
    );
    observer.observe(el);
    return () => observer.disconnect();
  }, [status, loadMore]);

  return { items, total, status, loadMore, sentinelRef };
}

// ── Footer ────────────────────────────────────────────────────────────────

interface InfiniteListFooterProps {
  shown: number;
  total: number;
  /** Plural, uppercase: "EPISODES". */
  noun: string;
  status: InfiniteStatus;
  onRetry: () => void;
  sentinelRef: RefObject<HTMLDivElement | null>;
  /** Server-rendered desktop paginator, or null for a single page. */
  paginator: ReactNode;
  /** Count shown beside the desktop paginator (that page's item count). */
  desktopShown: number;
}

export function InfiniteListFooter({
  shown,
  total,
  noun,
  status,
  onRetry,
  sentinelRef,
  paginator,
  desktopShown,
}: InfiniteListFooterProps) {
  const totalText = total.toLocaleString("en-US");

  return (
    <>
      {/* lg+: the original count and paginator */}
      <div className="hidden lg:flex items-center justify-between mt-4">
        <span className="font-mono text-[11px] text-sd-ink/60">
          SHOWING {desktopShown} OF {totalText} {noun}
        </span>
        {paginator}
      </div>

      {/* Below lg: the sentinel and a live status line */}
      <div className="lg:hidden mt-4 flex flex-col items-center gap-3">
        <div ref={sentinelRef} aria-hidden="true" className="h-px w-full" />
        <p role="status" className="font-mono text-[11px] text-sd-ink/60 text-center">
          {status === "loading" && "LOADING MORE…"}
          {status === "idle" && `SHOWING ${shown} OF ${totalText} ${noun}`}
          {status === "done" && (total > 0 ? `ALL ${totalText} ${noun} LOADED` : "")}
          {status === "error" && "COULDN’T LOAD MORE."}
        </p>
        {status === "error" && (
          <button
            type="button"
            onClick={onRetry}
            className="min-h-11 px-5 border-2 border-sd-ink font-oswald text-xs tracking-[0.14em] text-sd-ink hover:bg-sd-ink hover:text-white transition-colors"
          >
            TRY AGAIN
          </button>
        )}
      </div>
    </>
  );
}
