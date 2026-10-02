// src/components/features/wiki-export/wiki-export.tsx
'use client';

import { createContext, useCallback, useContext, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
// ExportWikiResponse appears in dto.ts after the next NSwag run.
import type { ExportWikiResponse } from '@/lib/dto';

const API_BASE = process.env.NEXT_PUBLIC_API_URL;

// Mirrors WikiText.MaxSelection on the API.
const MAX_SELECTION = 50;

export type WikiExportKind = 'drafts' | 'drafters';

interface WikiExportContextValue {
  kind: WikiExportKind;
  selected: ReadonlySet<string>;
  exporting: boolean;
  error: string | null;
  notice: string | null;
  toggle: (id: string) => void;
  setMany: (ids: string[], checked: boolean) => void;
  clear: () => void;
  exportSelected: () => Promise<void>;
}

const WikiExportContext = createContext<WikiExportContextValue | null>(null);

function useWikiExport(): WikiExportContextValue {
  const ctx = useContext(WikiExportContext);
  if (!ctx) throw new Error('Wiki export components must render inside <WikiExportScope>.');
  return ctx;
}

function downloadText(fileName: string, content: string) {
  const blob = new Blob([content], { type: 'text/plain;charset=utf-8' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}

// ── Scope ─────────────────────────────────────────────────────────────────

interface WikiExportScopeProps {
  enabled: boolean;
  kind: WikiExportKind;
  accessToken: string;
  children: ReactNode;
}

// Renders children untouched for non-admins; wraps them in selection state plus the
// export bar for admins. Selection is per page — the lists paginate with full page loads.
export function WikiExportScope({ enabled, kind, accessToken, children }: WikiExportScopeProps) {
  if (!enabled) return <>{children}</>;

  return (
    <WikiExportProvider kind={kind} accessToken={accessToken}>
      {children}
      <WikiExportBar />
    </WikiExportProvider>
  );
}

function WikiExportProvider({
  kind,
  accessToken,
  children,
}: Omit<WikiExportScopeProps, 'enabled'>) {
  const [selected, setSelected] = useState<ReadonlySet<string>>(new Set());
  const [exporting, setExporting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(null);

  const toggle = useCallback((id: string) => {
    setSelected(prev => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }, []);

  const setMany = useCallback((ids: string[], checked: boolean) => {
    setSelected(prev => {
      const next = new Set(prev);
      ids.forEach(id => (checked ? next.add(id) : next.delete(id)));
      return next;
    });
  }, []);

  const clear = useCallback(() => {
    setSelected(new Set());
    setError(null);
    setNotice(null);
  }, []);

  const exportSelected = useCallback(async () => {
    if (selected.size === 0 || selected.size > MAX_SELECTION) return;

    setExporting(true);
    setError(null);
    setNotice(null);

    try {
      const ids = Array.from(selected);
      const body = kind === 'drafts' ? { draftPublicIds: ids } : { drafterPublicIds: ids };

      const res = await fetch(`${API_BASE}/wiki-exports/${kind}`, {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${accessToken}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(body),
      });

      if (!res.ok) throw new Error(`Wiki export failed: ${res.status}`);

      const data = (await res.json()) as ExportWikiResponse;
      downloadText(data.fileName, data.content);

      // Patreon (non-main-feed) drafts are never exported.
      const skipped = ids.length - data.pageCount;
      if (skipped > 0) {
        setNotice(`${skipped} skipped — only main-feed drafts are exported.`);
      }
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Wiki export failed.');
    } finally {
      setExporting(false);
    }
  }, [accessToken, kind, selected]);

  const value = useMemo<WikiExportContextValue>(
    () => ({ kind, selected, exporting, error, notice, toggle, setMany, clear, exportSelected }),
    [kind, selected, exporting, error, notice, toggle, setMany, clear, exportSelected]
  );

  return <WikiExportContext.Provider value={value}>{children}</WikiExportContext.Provider>;
}

// ── Checkboxes ────────────────────────────────────────────────────────────

export function WikiSelectCheckbox({ id }: { id: string }) {
  const { selected, toggle } = useWikiExport();

  return (
    <input
      type="checkbox"
      checked={selected.has(id)}
      onChange={() => toggle(id)}
      aria-label="Select for wiki export"
      className="h-4 w-4 cursor-pointer accent-sd-red"
    />
  );
}

export function WikiSelectAll({ ids }: { ids: string[] }) {
  const { selected, setMany } = useWikiExport();
  const allSelected = ids.length > 0 && ids.every(id => selected.has(id));

  return (
    <input
      type="checkbox"
      checked={allSelected}
      onChange={() => setMany(ids, !allSelected)}
      aria-label="Select all on this page for wiki export"
      className="h-4 w-4 cursor-pointer accent-sd-red"
    />
  );
}

// ── Bar ───────────────────────────────────────────────────────────────────

function WikiExportBar() {
  const { kind, selected, exporting, error, notice, clear, exportSelected } = useWikiExport();

  if (selected.size === 0) return null;

  const tooMany = selected.size > MAX_SELECTION;
  const noun = kind === 'drafts' ? 'DRAFT' : 'DRAFTER';

  return (
    <div className="fixed bottom-6 left-1/2 z-50 -translate-x-1/2">
      <div className="flex items-center gap-4 border-2 border-sd-ink bg-sd-ink px-5 py-3 font-mono text-[11px] tracking-widest text-white shadow-lg">
        <span>
          {selected.size} {noun}
          {selected.size === 1 ? '' : 'S'} SELECTED
        </span>

        <button
          type="button"
          onClick={exportSelected}
          disabled={exporting || tooMany}
          className="bg-sd-red px-4 py-1.5 font-oswald font-bold tracking-[0.14em] text-xs text-white transition-colors hover:bg-sd-red/80 disabled:cursor-not-allowed disabled:opacity-40"
        >
          {exporting ? 'EXPORTING…' : 'EXPORT TO WIKI'}
        </button>

        <button
          type="button"
          onClick={clear}
          disabled={exporting}
          className="text-white/60 transition-colors hover:text-white disabled:opacity-40"
        >
          CLEAR
        </button>
      </div>

      {tooMany && (
        <p className="mt-2 text-center text-xs text-sd-red">
          Select at most {MAX_SELECTION} at a time.
        </p>
      )}
      {error && <p className="mt-2 text-center text-xs text-sd-red">{error}</p>}
      {notice && <p className="mt-2 text-center text-xs text-white/70">{notice}</p>}
    </div>
  );
}