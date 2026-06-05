'use client';

import { useCallback, useEffect, useRef, useState } from "react";
import { SeriesResponse } from "@/lib/dto";

// ── Enum constants ─────────────────────────────────────────────────────────

const SERIES_KIND_OPTIONS = [
  { label: "Regular",                              value: 0 },
  { label: "Franchise Mini Super Draft",           value: 1 },
  { label: "In Memoriam",                          value: 2 },
  { label: "Legends Mega Draft",                   value: 3 },
  { label: "Best Picture Nominee mini-Super Draft", value: 4 },
  { label: "Patreon Vs. Draft",                    value: 5 },
  { label: "Live Draft",                           value: 6 },
  { label: "Special Draft",                        value: 7 },
  { label: "Speed Drafts",                         value: 8 },
  { label: "Legends Super Draft",                  value: 9 },
] as const;

const CANONICAL_POLICY_OPTIONS = [
  { label: "Always",       value: 0, hint: "Every draft in this series is canon immediately on completion." },
  { label: "Never",        value: 1, hint: "Drafts in this series never contribute to honorifics." },
  { label: "On Main Feed", value: 2, hint: "Patreon-first drafts become canon once released on the main feed." },
] as const;

const CONTINUITY_SCOPE_OPTIONS = [
  { label: "None",         value: 0, hint: "No veto rollover between drafts." },
  { label: "Series",       value: 1, hint: "Vetoes roll over within this series only." },
  { label: "Global",       value: 2, hint: "Vetoes roll over across all drafts on the same release channel." },
  { label: "Speed Drafts", value: 3, hint: "Vetoes roll over between sub-drafts within a single Speed Draft episode." },
] as const;

const CONTINUITY_DATE_RULE_OPTIONS = [
  { label: "Any Channel First Release", value: 0, hint: "Eligibility starts from the earliest release on any channel." },
  { label: "Main Feed First Release",   value: 1, hint: "Eligibility starts from the main feed release date only." },
] as const;

const DRAFT_TYPE_OPTIONS = [
  { label: "Standard",    value: 0, bit: 1  },
  { label: "Mini-Mega",   value: 1, bit: 2  },
  { label: "Mega",        value: 2, bit: 4  },
  { label: "Super",       value: 3, bit: 8  },
  { label: "Mini-Super",  value: 4, bit: 16 },
  { label: "Speed Draft", value: 5, bit: 32 },
] as const;

// ── Internal type ──────────────────────────────────────────────────────────
// All SmartEnum fields normalised to plain numbers so state stays homogeneous.

interface SeriesItem {
  publicId: string;
  name: string;
  description: string;
  kind: number;
  canonicalPolicy: number;
  continuityScope: number;
  continuityDateRule: number;
  allowedDraftTypesMask: number;
  defaultDraftType: number | null;
  isDeleted: boolean;
}

function normalize(s: SeriesResponse): SeriesItem {
  return {
    publicId:          s.publicId ?? "",
    name:              s.name ?? "",
    description:       s.description ?? "",
    kind:              s.kind?.value ?? 0,
    canonicalPolicy:   s.canonicalPolicy?.value ?? 0,
    continuityScope:   s.continuityScope?.value ?? 0,
    continuityDateRule: s.continuityDateRule?.value ?? 0,
    allowedDraftTypesMask: s.allowedDraftTypesMask ?? 0,
    defaultDraftType:  s.defaultDraftType?.value ?? null,
    isDeleted:         false,
  };
}

// ── Helpers ────────────────────────────────────────────────────────────────

function maskToBits(mask: number): Set<number> {
  return new Set(DRAFT_TYPE_OPTIONS.filter(o => (mask & o.bit) !== 0).map(o => o.value));
}

function bitsToMask(checked: Set<number>): number {
  return DRAFT_TYPE_OPTIONS.filter(o => checked.has(o.value)).reduce((acc, o) => acc | o.bit, 0);
}

// ── Sub-components ─────────────────────────────────────────────────────────

interface Props {
  initialData: SeriesResponse[];
  accessToken: string | undefined;
}

interface SlideOverProps {
  mode: "create" | "edit";
  item?: SeriesItem;
  onClose: () => void;
  onSaved: (item: SeriesItem, mode: "create" | "edit") => void;
  accessToken: string | undefined;
}

function Toast({ message }: { message: string }) {
  return (
    <div className="fixed bottom-6 right-6 z-[100] bg-sd-ink text-white font-mono text-[12px] tracking-wide px-5 py-3 rounded-full shadow-lg">
      {message}
    </div>
  );
}

const labelCls = "block font-mono text-[11px] tracking-widest text-sd-ink/60 mb-1.5 uppercase";
const inputCls = "w-full border border-sd-ink/20 px-3 py-2 text-sm text-sd-ink bg-white focus:outline-none focus:border-sd-blue";
const selectCls = "w-full border border-sd-ink/20 px-3 py-2 text-sm text-sd-ink bg-white focus:outline-none focus:border-sd-blue";
const hintCls = "mt-1 font-mono text-[10px] text-sd-ink/40 leading-relaxed";

function SlideOver({ mode, item, onClose, onSaved, accessToken }: SlideOverProps) {
  const [name, setName]             = useState(item?.name ?? "");
  const [description, setDescription] = useState(item?.description ?? "");
  const [kind, setKind]             = useState(item?.kind ?? 0);
  const [canonicalPolicy, setCanonicalPolicy] = useState(item?.canonicalPolicy ?? 0);
  const [continuityScope, setContinuityScope] = useState(item?.continuityScope ?? 0);
  const [continuityDateRule, setContinuityDateRule] = useState(item?.continuityDateRule ?? 0);
  const [checkedTypes, setCheckedTypes] = useState<Set<number>>(
    () => item ? maskToBits(item.allowedDraftTypesMask) : new Set([0])
  );
  const [defaultDraftType, setDefaultDraftType] = useState<number | null>(
    item?.defaultDraftType ?? null
  );
  const [error, setError]   = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const firstRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (defaultDraftType !== null && !checkedTypes.has(defaultDraftType)) {
      setDefaultDraftType(null);
    }
  }, [checkedTypes, defaultDraftType]);

  useEffect(() => {
    firstRef.current?.focus();
    function onKey(e: KeyboardEvent) { if (e.key === "Escape") onClose(); }
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [onClose]);

  function toggleType(value: number) {
    setCheckedTypes(prev => {
      const next = new Set(prev);
      next.has(value) ? next.delete(value) : next.add(value);
      return next;
    });
  }

  async function handleSave() {
    if (!name.trim()) { setError("Name is required."); return; }
    if (checkedTypes.size === 0) { setError("At least one draft type must be allowed."); return; }

    setSaving(true);
    setError(null);

    const allowedDraftTypesMask = bitsToMask(checkedTypes);

    const body = {
      name: name.trim(),
      description: description.trim() || null,
      kind,
      canonicalPolicy,
      continuityScope,
      continuityDateRule,
      allowedDraftTypes: allowedDraftTypesMask,
      defaultDraftType,
    };

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
          body: JSON.stringify(body),
        });
        if (!res.ok) {
          const err = await res.json().catch(() => ({}));
          setError(err?.detail ?? err?.errors?.[0] ?? "Save failed.");
          return;
        }
        const created = await res.json();
        onSaved({
          publicId: created.publicId ?? "",
          name: name.trim(),
          description: description.trim(),
          kind,
          canonicalPolicy,
          continuityScope,
          continuityDateRule,
          allowedDraftTypesMask,
          defaultDraftType,
          isDeleted: false,
        }, "create");
      } else {
        const res = await fetch(`${apiBase}/series/${item!.publicId}`, {
          method: "PATCH",
          headers,
          body: JSON.stringify(body),
        });
        if (!res.ok) {
          const err = await res.json().catch(() => ({}));
          setError(err?.detail ?? err?.errors?.[0] ?? "Save failed.");
          return;
        }
        onSaved({
          ...item!,
          name: name.trim(),
          description: description.trim(),
          kind,
          canonicalPolicy,
          continuityScope,
          continuityDateRule,
          allowedDraftTypesMask,
          defaultDraftType,
        }, "edit");
      }
    } finally {
      setSaving(false);
    }
  }

  const canonicalHint   = CANONICAL_POLICY_OPTIONS.find(o => o.value === canonicalPolicy)?.hint;
  const scopeHint       = CONTINUITY_SCOPE_OPTIONS.find(o => o.value === continuityScope)?.hint;
  const dateRuleHint    = CONTINUITY_DATE_RULE_OPTIONS.find(o => o.value === continuityDateRule)?.hint;
  const allowedTypeOptions = DRAFT_TYPE_OPTIONS.filter(o => checkedTypes.has(o.value));

  return (
    <>
      <div className="fixed inset-0 bg-black/30 z-40" onClick={onClose} aria-hidden="true" />
      <div
        role="dialog"
        aria-modal="true"
        aria-label={mode === "create" ? "New Series" : "Edit Series"}
        className="fixed right-0 top-0 h-full w-[440px] bg-sd-paper border-l border-sd-ink z-50 flex flex-col shadow-xl"
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
            <label className={labelCls}>Name <span className="text-sd-red">*</span></label>
            <input
              ref={firstRef}
              type="text"
              value={name}
              onChange={e => setName(e.target.value)}
              className={inputCls}
            />
          </div>

          <div>
            <label className={labelCls}>Description</label>
            <textarea
              rows={3}
              value={description}
              onChange={e => setDescription(e.target.value)}
              className={`${inputCls} resize-none`}
            />
          </div>

          <div>
            <label className={labelCls}>Series Kind</label>
            <select value={kind} onChange={e => setKind(Number(e.target.value))} className={selectCls}>
              {SERIES_KIND_OPTIONS.map(o => (
                <option key={o.value} value={o.value}>{o.label}</option>
              ))}
            </select>
          </div>

          <div>
            <label className={labelCls}>Canonical Policy</label>
            <select value={canonicalPolicy} onChange={e => setCanonicalPolicy(Number(e.target.value))} className={selectCls}>
              {CANONICAL_POLICY_OPTIONS.map(o => (
                <option key={o.value} value={o.value}>{o.label}</option>
              ))}
            </select>
            {canonicalHint && <p className={hintCls}>{canonicalHint}</p>}
          </div>

          <div>
            <label className={labelCls}>Continuity Scope</label>
            <select value={continuityScope} onChange={e => setContinuityScope(Number(e.target.value))} className={selectCls}>
              {CONTINUITY_SCOPE_OPTIONS.map(o => (
                <option key={o.value} value={o.value}>{o.label}</option>
              ))}
            </select>
            {scopeHint && <p className={hintCls}>{scopeHint}</p>}
          </div>

          <div>
            <label className={labelCls}>Continuity Date Rule</label>
            <select value={continuityDateRule} onChange={e => setContinuityDateRule(Number(e.target.value))} className={selectCls}>
              {CONTINUITY_DATE_RULE_OPTIONS.map(o => (
                <option key={o.value} value={o.value}>{o.label}</option>
              ))}
            </select>
            {dateRuleHint && <p className={hintCls}>{dateRuleHint}</p>}
          </div>

          <div>
            <label className={labelCls}>
              Allowed Draft Types <span className="text-sd-red">*</span>
            </label>
            <div className="grid grid-cols-2 gap-2 mt-1">
              {DRAFT_TYPE_OPTIONS.map(o => (
                <label key={o.value} className="flex items-center gap-2 cursor-pointer select-none">
                  <input
                    type="checkbox"
                    checked={checkedTypes.has(o.value)}
                    onChange={() => toggleType(o.value)}
                    className="accent-sd-blue"
                  />
                  <span className="font-mono text-[11px] text-sd-ink">{o.label}</span>
                </label>
              ))}
            </div>
          </div>

          <div>
            <label className={labelCls}>Default Draft Type</label>
            <select
              value={defaultDraftType ?? ""}
              onChange={e => setDefaultDraftType(e.target.value === "" ? null : Number(e.target.value))}
              className={selectCls}
            >
              <option value="">— none —</option>
              {allowedTypeOptions.map(o => (
                <option key={o.value} value={o.value}>{o.label}</option>
              ))}
            </select>
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

// ── Manager ────────────────────────────────────────────────────────────────

export default function SeriesManager({ initialData, accessToken }: Props) {
  const [items, setItems]             = useState<SeriesItem[]>(() => initialData.map(normalize));
  const [showRetired, setShowRetired] = useState(false);
  const [slideOver, setSlideOver]     = useState<{ mode: "create" | "edit"; item?: SeriesItem } | null>(null);
  const [confirming, setConfirming]   = useState<string | null>(null);
  const [toast, setToast]             = useState<string | null>(null);

  function showToast(msg: string) {
    setToast(msg);
    setTimeout(() => setToast(null), 2000);
  }

  const handleSaved = useCallback((saved: SeriesItem, mode: "create" | "edit") => {
    setItems(prev =>
      mode === "create"
        ? [saved, ...prev]
        : prev.map(i => i.publicId === saved.publicId ? saved : i)
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
        headers: { "Content-Type": "application/json", ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}) },
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
        headers: { "Content-Type": "application/json", ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}) },
        body: JSON.stringify({}),
      });
      if (!res.ok) throw new Error();
      showToast("Series restored.");
    } catch {
      setItems(prev => prev.map(i => i.publicId === publicId ? { ...i, isDeleted: true } : i));
      showToast("Failed to restore series.");
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
              {["Name", "Kind", "Description", "Status", ""].map(col => (
                <th key={col} className="text-left font-mono text-[10px] tracking-widest uppercase text-white/60 px-4 py-3">
                  {col}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {visible.length === 0 && (
              <tr>
                <td colSpan={5} className="px-4 py-8 text-center font-mono text-[12px] text-sd-ink/40">
                  No series found.
                </td>
              </tr>
            )}
            {visible.map(item => {
              const kindLabel = SERIES_KIND_OPTIONS.find(o => o.value === item.kind)?.label ?? "—";
              return (
                <tr
                  key={item.publicId}
                  className={`border-b border-sd-ink/5 transition-colors ${item.isDeleted ? "opacity-50" : "hover:bg-sd-paper/60"}`}
                >
                  <td className="px-4 py-3 font-medium text-sd-ink">{item.name}</td>
                  <td className="px-4 py-3 font-mono text-[11px] text-sd-ink/60 uppercase tracking-wide">{kindLabel}</td>
                  <td className="px-4 py-3 text-sd-ink/60 max-w-[260px] truncate">
                    {item.description || <span className="text-sd-ink/30">—</span>}
                  </td>
                  <td className="px-4 py-3">
                    {item.isDeleted && (
                      <span className="font-mono text-[10px] tracking-widest text-sd-red uppercase">Retired</span>
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
                            <button onClick={() => handleRetire(item.publicId)} className="text-sd-red font-medium hover:underline">Confirm</button>
                            <button onClick={() => setConfirming(null)} className="text-sd-ink/50 hover:underline">Cancel</button>
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
                      <button onClick={() => handleRestore(item.publicId)} className="text-sd-blue text-sm font-medium hover:underline">
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