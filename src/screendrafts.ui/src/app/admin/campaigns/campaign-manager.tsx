'use client';

import { useCallback, useEffect, useRef, useState } from "react";
import { CampaignListItem } from "@/services/admin/fetch-admin-campaigns";

interface Props {
  initialData: CampaignListItem[];
  accessToken: string | undefined;
}

interface SlideOverProps {
  mode: "create" | "edit";
  item?: CampaignListItem;
  onClose: () => void;
  onSaved: () => void;
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
  const [description, setDescription] = useState("");
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
        const res = await fetch(`${apiBase}/campaigns`, {
          method: "POST",
          headers,
          body: JSON.stringify({ name: name.trim(), slug: name.trim().toLowerCase().replace(/\s+/g, "-") }),
        });
        if (!res.ok) {
          const err = await res.json().catch(() => ({}));
          setError(err?.detail ?? err?.errors?.[0] ?? "Save failed.");
          return;
        }
      } else {
        const res = await fetch(`${apiBase}/campaigns/${item!.publicId}`, {
          method: "PATCH",
          headers,
          body: JSON.stringify({ name: name.trim() }),
        });
        if (!res.ok) {
          const err = await res.json().catch(() => ({}));
          setError(err?.detail ?? err?.errors?.[0] ?? "Save failed.");
          return;
        }
      }
      // Re-fetch rather than construct the row locally — this is the actual
      // fix for "CreateCampaign not refreshing the list": the old code built
      // a fake row from `created.publicId ?? created.id ?? ""` with
      // slug hardcoded to "", which could silently produce a bad or
      // duplicate key instead of the real server row.
      onSaved();
    } finally {
      setSaving(false);
    }
  }

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black/30 z-40"
        onClick={onClose}
        aria-hidden="true"
      />
      {/* Panel */}
      <div
        role="dialog"
        aria-modal="true"
        aria-label={mode === "create" ? "New Campaign" : "Edit Campaign"}
        className="fixed right-0 top-0 h-full w-[400px] bg-sd-paper border-l border-sd-ink z-50 flex flex-col shadow-xl"
      >
        <div className="px-6 py-5 border-b border-sd-ink/10 flex items-center justify-between">
          <h2 className="font-oswald font-bold text-[20px] text-sd-ink uppercase tracking-wide">
            {mode === "create" ? "New Campaign" : "Edit Campaign"}
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
              value={description}
              onChange={e => setDescription(e.target.value)}
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

export default function CampaignManager({ initialData, accessToken }: Props) {
  const [items, setItems] = useState(initialData);
  const [showRetired, setShowRetired] = useState(false);
  const [slideOver, setSlideOver] = useState<{ mode: "create" | "edit"; item?: CampaignListItem } | null>(null);
  const [confirming, setConfirming] = useState<string | null>(null);
  const [toast, setToast] = useState<string | null>(null);
  const [refreshing, setRefreshing] = useState(false);

  function showToast(msg: string) {
    setToast(msg);
    setTimeout(() => setToast(null), 2000);
  }

  const refresh = useCallback(async () => {
    setRefreshing(true);
    try {
      const apiBase = process.env.NEXT_PUBLIC_API_URL;
      const headers: HeadersInit = accessToken ? { Authorization: `Bearer ${accessToken}` } : {};

      const url = new URL(`${apiBase}/campaigns`);
      url.searchParams.set("includeDeleted", "true");

      let res = await fetch(url.toString(), { headers, cache: "no-store" });

      if (res.status === 403) {
        url.searchParams.delete("includeDeleted");
        res = await fetch(url.toString(), { headers, cache: "no-store" });
      }

      if (!res.ok) return;
      const data = await res.json();
      setItems(data.items ?? data ?? []);
    } catch (err) {
      console.error("[refresh]", err);
    } finally {
      setRefreshing(false);
    }
  }, [accessToken]);

  const handleSaved = useCallback(async () => {
    const mode = slideOver?.mode;
    setSlideOver(null);
    await refresh();
    showToast(mode === "create" ? "Campaign created." : "Campaign updated.");
  }, [refresh, slideOver?.mode]);

  async function handleRetire(publicId: string) {
    setConfirming(null);
    try {
      const apiBase = process.env.NEXT_PUBLIC_API_URL;
      const res = await fetch(`${apiBase}/campaigns/${publicId}`, {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
          ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
        },
        body: JSON.stringify({}),
      });
      if (!res.ok) throw new Error();
      await refresh();
      showToast("Campaign retired.");
    } catch {
      showToast("Failed to retire campaign.");
    }
  }

  async function handleRestore(publicId: string) {
    try {
      const apiBase = process.env.NEXT_PUBLIC_API_URL;
      const res = await fetch(`${apiBase}/campaigns/${publicId}/restore`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
        },
        body: JSON.stringify({}),
      });
      if (!res.ok) throw new Error();
      await refresh();
      showToast("Campaign restored.");
    } catch {
      showToast("Failed to restore campaign.");
    }
  }

  const visible = showRetired ? items : items.filter(i => !i.isDeleted);

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
          {refreshing && <span className="text-sd-ink/30 normal-case">refreshing…</span>}
        </label>
        <button
          onClick={() => setSlideOver({ mode: "create" })}
          className="bg-sd-red text-white font-oswald font-medium tracking-wide uppercase px-4 py-2 text-sm hover:bg-sd-red/90 transition-colors"
        >
          + New Campaign
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
                  No campaigns found.
                </td>
              </tr>
            )}
            {visible.map(item => (
              <tr
                key={item.publicId}
                className={`border-b border-sd-ink/5 transition-colors ${item.isDeleted ? "opacity-50" : "hover:bg-sd-paper/60"}`}
              >
                <td className="px-4 py-3 font-medium text-sd-ink">{item.name}</td>
                <td className="px-4 py-3 text-sd-ink/60">—</td>
                <td className="px-4 py-3">
                  {item.isDeleted && (
                    <span className="font-mono text-[10px] tracking-widest text-sd-red uppercase">
                      Retired
                    </span>
                  )}
                </td>
                <td className="px-4 py-3 text-right whitespace-nowrap">
                  {!item.isDeleted ? (
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
                            onClick={() => handleRetire(item.publicId)}
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
                          onClick={() => setConfirming(item.publicId)}
                          className="text-sd-ink/50 text-sm hover:text-sd-red hover:underline"
                        >
                          Retire
                        </button>
                      )}
                    </span>
                  ) : (
                    <button
                      onClick={() => handleRestore(item.publicId)}
                      className="text-sd-blue text-sm font-medium hover:underline"
                    >
                      Restore
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  );
}