'use client';

import { useCallback, useEffect, useRef, useState } from "react";
import { SeriesListItem } from "@/services/admin/fetch-admin-series";

interface Props {
  initialData: SeriesListItem[];
  accessToken: string | undefined;
}

interface SlideOverProps {
  mode: "create" | "edit";
  item?: SeriesListItem;
  onClose: () => void;
  onSaved: (item: SeriesListItem, mode: "create" | "edit") => void;
  accessToken: string | undefined;
}

function Toast({ message }: { message: string }) {
  return (
    <div className="fixed bottom-6 right-6 z-[100] bg-sd-ink text-white font-mono text-[12px] tracking-wide px-5 py-3 rounded-full shadow-lg">
      {message}
    </div>
  );
}

function SlideOver({ mode, item, onClose, onSaved, accessToken }: SlideOverProps) {
  const [name, setName] = useState(item?.name ?? "");
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const firstRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    firstRef.current?.focus();
    function onKey(e: KeyboardEvent) {
      if (e.key === "Escape") onClose();
    }
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [onClose]);

  async function handleSave() {
    if (!name.trim()) { setError("Name is required."); return; }
    setSaving(true);
    setError(null);
    try {
      const apiBase = process.env.NEXT_PUBLIC_API_URL;
      const headers: HeadersInit = {
        "Content-Type": "application/json",
        ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
      };

      if (mode === "create") {
        const res = await fetch(`${apiBase}/series`, {
          method: "POST",
          headers,
          body: JSON.stringify({ name: name.trim() }),
        });
        if (!res.ok) {
          const err = await res.json().catch(() => ({}));
          setError(err?.detail ?? err?.errors?.[0] ?? "Save failed.");
          return;
        }
        const created = await res.json();
        onSaved({ publicId: created.publicId ?? created.id ?? "", name: name.trim() }, "create");
      } else {
        const res = await fetch(`${apiBase}/series/${item!.publicId}`, {
          method: "PATCH",
          headers,
          body: JSON.stringify({ name: name.trim() }),
        });
        if (!res.ok) {
          const err = await res.json().catch(() => ({}));
          setError(err?.detail ?? err?.errors?.[0] ?? "Save failed.");
          return;
        }
        onSaved({ ...item!, name: name.trim() }, "edit");
      }
    } finally {
      setSaving(false);
    }
  }

  return (
    <>
      <div className="fixed inset-0 bg-black/30 z-40" onClick={onClose} aria-hidden="true" />
      <div
        role="dialog"
        aria-modal="true"
        aria-label={mode === "create" ? "New Series" : "Edit Series"}
        className="fixed right-0 top-0 h-full w-[400px] bg-sd-paper border-l border-sd-ink z-50 flex flex-col shadow-xl"
      >
        <div className="px-6 py-5 border-b border-sd-ink/10 flex items-center justify-between">
          <h2 className="font-oswald font-bold text-[20px] text-sd-ink uppercase tracking-wide">
            {mode === "create" ? "New Series" : "Edit Series"}
          </h2>
          <button onClick={onClose} className="text-sd-ink/50 hover:text-sd-ink text-xl leading-none">&times;</button>
        </div>

        <div className="flex-1 overflow-y-auto px-6 py-6 space-y-5">
          {error && (
            <div className="bg-red-50 border border-sd-red text-sd-red font-mono text-[12px] px-4 py-3">
              {error}
            </div>
          )}
          <div>
            <label className="block font-mono text-[11px] tracking-widest text-sd-ink/60 mb-1.5 uppercase">
              Name <span className="text-sd-red">*</span>
            </label>
            <input
              ref={firstRef}
              type="text"
              value={name}
              onChange={e => setName(e.target.value)}
              className="w-full border border-sd-ink/20 px-3 py-2 text-sm text-sd-ink bg-white focus:outline-none focus:border-sd-blue"
            />
          </div>
          <div>
            <label className="block font-mono text-[11px] tracking-widest text-sd-ink/60 mb-1.5 uppercase">
              Description
            </label>
            <textarea
              rows={4}
              className="w-full border border-sd-ink/20 px-3 py-2 text-sm text-sd-ink bg-white focus:outline-none focus:border-sd-blue resize-none"
            />
          </div>
        </div>

        <div className="px-6 py-4 border-t border-sd-ink/10 flex gap-3">
          <button
            onClick={handleSave}
            disabled={saving}
            className="flex-1 bg-sd-ink text-white font-oswald font-medium tracking-wide uppercase py-2.5 hover:bg-sd-ink/80 transition-colors disabled:opacity-50"
          >
            {saving ? "Saving…" : "Save"}
          </button>
          <button
            onClick={onClose}
            className="px-5 py-2.5 border border-sd-ink/20 text-sd-ink font-oswald font-medium tracking-wide uppercase hover:bg-sd-ink/5 transition-colors"
          >
            Cancel
          </button>
        </div>
      </div>
    </>
  );
}

export default function SeriesManager({ initialData, accessToken }: Props) {
  const [items, setItems] = useState(initialData);
  const [showRetired, setShowRetired] = useState(false);
  const [slideOver, setSlideOver] = useState<{ mode: "create" | "edit"; item?: SeriesListItem } | null>(null);
  const [confirming, setConfirming] = useState<string | null>(null);
  const [toast, setToast] = useState<string | null>(null);

  function showToast(msg: string) {
    setToast(msg);
    setTimeout(() => setToast(null), 2000);
  }

  const handleSaved = useCallback((saved: SeriesListItem, mode: "create" | "edit") => {
    setItems(prev =>
      mode === "create"
        ? [saved, ...prev]
        : prev.map(i => (i.publicId === saved.publicId ? saved : i))
    );
    setSlideOver(null);
    showToast(mode === "create" ? "Series created." : "Series updated.");
  }, []);

  async function handleRetire(publicId: string) {
    const apiBase = process.env.NEXT_PUBLIC_API_URL;
    setItems(prev => prev.map(i => i.publicId === publicId ? { ...i, isDeleted: true } : i));
    setConfirming(null);
    try {
      const res = await fetch(`${apiBase}/series/${publicId}`, {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
          ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
        },
        body: JSON.stringify({}),
      });
      if (!res.ok) throw new Error();
      showToast("Series retired.");
    } catch {
      setItems(prev => prev.map(i => i.publicId === publicId ? { ...i, isDeleted: false } : i));
      showToast("Failed to retire series.");
    }
  }

  async function handleRestore(publicId: string) {
    const apiBase = process.env.NEXT_PUBLIC_API_URL;
    setItems(prev => prev.map(i => i.publicId === publicId ? { ...i, isDeleted: false } : i));
    try {
      const res = await fetch(`${apiBase}/series/${publicId}/restore`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
        },
        body: JSON.stringify({}),
      });
      if (!res.ok) throw new Error();
      showToast("Series restored.");
    } catch {
      setItems(prev => prev.map(i => i.publicId === publicId ? { ...i, isDeleted: true } : i));
      showToast("Failed to restore series.");
    }
  }

  // SeriesResponse does not have isDeleted — filter on local state only
  const visible = showRetired ? items : items.filter(i => !(i as SeriesListItem & { isDeleted?: boolean }).isDeleted);

  return (
    <>
      {toast && <Toast message={toast} />}
      {slideOver && (
        <SlideOver
          mode={slideOver.mode}
          item={slideOver.item}
          onClose={() => setSlideOver(null)}
          onSaved={handleSaved}
          accessToken={accessToken}
        />
      )}

      <div className="flex items-center justify-between mb-5">
        <label className="flex items-center gap-2 font-mono text-[11px] tracking-widest text-sd-ink/60 cursor-pointer select-none">
          <input
            type="checkbox"
            checked={showRetired}
            onChange={e => setShowRetired(e.target.checked)}
            className="accent-sd-red"
          />
          SHOW RETIRED
        </label>
        <button
          onClick={() => setSlideOver({ mode: "create" })}
          className="bg-sd-red text-white font-oswald font-medium tracking-wide uppercase px-4 py-2 text-sm hover:bg-sd-red/90 transition-colors"
        >
          + New Series
        </button>
      </div>

      <div className="bg-white border border-sd-ink/10 overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-sd-ink/10 bg-sd-ink">
              {["Name", "Description", "Status", ""].map(col => (
                <th
                  key={col}
                  className="text-left font-mono text-[10px] tracking-widest uppercase text-white/60 px-4 py-3"
                >
                  {col}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {visible.length === 0 && (
              <tr>
                <td colSpan={4} className="px-4 py-8 text-center font-mono text-[12px] text-sd-ink/40">
                  No series found.
                </td>
              </tr>
            )}
            {visible.map(item => {
              const isDeleted = !!(item as SeriesListItem & { isDeleted?: boolean }).isDeleted;
              return (
                <tr
                  key={item.publicId}
                  className={`border-b border-sd-ink/5 transition-colors ${isDeleted ? "opacity-50" : "hover:bg-sd-paper/60"}`}
                >
                  <td className="px-4 py-3 font-medium text-sd-ink">{item.name}</td>
                  <td className="px-4 py-3 text-sd-ink/60">—</td>
                  <td className="px-4 py-3">
                    {isDeleted && (
                      <span className="font-mono text-[10px] tracking-widest text-sd-red uppercase">
                        Retired
                      </span>
                    )}
                  </td>
                  <td className="px-4 py-3 text-right whitespace-nowrap">
                    {!isDeleted ? (
                      <span className="flex items-center justify-end gap-3">
                        <button
                          onClick={() => setSlideOver({ mode: "edit", item })}
                          className="text-sd-blue text-sm font-medium hover:underline"
                        >
                          Edit
                        </button>
                        {confirming === item.publicId ? (
                          <span className="flex items-center gap-2 font-mono text-[11px]">
                            <span className="text-sd-ink/70">Retire {item.name}?</span>
                            <button
                              onClick={() => handleRetire(item.publicId!)}
                              className="text-sd-red font-medium hover:underline"
                            >
                              Confirm
                            </button>
                            <button
                              onClick={() => setConfirming(null)}
                              className="text-sd-ink/50 hover:underline"
                            >
                              Cancel
                            </button>
                          </span>
                        ) : (
                          <button
                            onClick={() => setConfirming(item.publicId!)}
                            className="text-sd-ink/50 text-sm hover:text-sd-red hover:underline"
                          >
                            Retire
                          </button>
                        )}
                      </span>
                    ) : (
                      <button
                        onClick={() => handleRestore(item.publicId!)}
                        className="text-sd-blue text-sm font-medium hover:underline"
                      >
                        Restore
                      </button>
                    )}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </>
  );
}
