'use client';

import { useRouter, useSearchParams } from "next/navigation";
import { useState, useEffect, useRef } from "react";

interface CampaignOption {
  publicId: string;
  name: string;
}

interface CategoryOption {
  publicId: string;
  name: string;
}

interface DraftsFilterProps {
  campaigns: CampaignOption[];
  categories: CategoryOption[];
}

const DRAFT_TYPE_OPTIONS = [
  { id: 0, label: "Standard" },
  { id: 2, label: "Mega" },
  { id: 3, label: "Super" },
  { id: 1, label: "Mini-Mega" },
  { id: 4, label: "Mini-Super" },
  { id: 5, label: "Speed Draft" },
];

const DRAFTER_COUNT_OPTIONS = [
  { value: "", label: "Any" },
  { value: "2", label: "2" },
  { value: "3", label: "3" },
  { value: "4", label: "4" },
];

const SORT_OPTIONS = [
  { value: "date-desc", label: "Air Date (Newest First)" },
  { value: "date-asc", label: "Air Date (Oldest First)" },
  { value: "episodenumber-desc", label: "Episode No. (High → Low)" },
  { value: "episodenumber-asc", label: "Episode No. (Low → High)" },
  { value: "title-asc", label: "Title (A → Z)" },
  { value: "title-desc", label: "Title (Z → A)" },
];

// ── Category dropdown ──────────────────────────────────────────────────────

interface CategoryDropdownProps {
  categories: CategoryOption[];
  selected: Set<string>;
  onChange: (next: Set<string>) => void;
}

function CategoryDropdown({ categories, selected, onChange }: CategoryDropdownProps) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function handler(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    }
    document.addEventListener("mousedown", handler);
    return () => document.removeEventListener("mousedown", handler);
  }, []);

  function toggle(publicId: string) {
    const next = new Set(selected);
    next.has(publicId) ? next.delete(publicId) : next.add(publicId);
    onChange(next);
  }

  const label = selected.size === 0
    ? "All categories"
    : selected.size === 1
      ? (categories.find(c => selected.has(c.publicId))?.name ?? "1 selected")
      : `${selected.size} selected`;

  return (
    <div ref={ref} className="relative">
      <button
        type="button"
        onClick={() => setOpen(v => !v)}
        className="w-full border border-sd-ink/30 rounded px-3 py-2 text-sm text-sd-ink bg-white focus:outline-none focus:border-sd-blue text-left flex items-center justify-between gap-2"
      >
        <span className={selected.size === 0 ? "text-sd-ink/40" : ""}>{label}</span>
        <span className="text-sd-ink/40 text-xs">{open ? "▲" : "▼"}</span>
      </button>

      {open && (
        <div className="absolute z-20 top-full left-0 mt-1 w-full min-w-[200px] bg-white border border-sd-ink/20 shadow-lg max-h-60 overflow-y-auto">
          {categories.length === 0 && (
            <p className="px-3 py-2 font-mono text-[11px] text-sd-ink/40">No categories.</p>
          )}
          {categories.map(c => (
            <label
              key={c.publicId}
              className="flex items-center gap-2.5 px-3 py-2 cursor-pointer hover:bg-sd-paper/60 select-none"
            >
              <input
                type="checkbox"
                checked={selected.has(c.publicId)}
                onChange={() => toggle(c.publicId)}
                className="accent-sd-blue"
              />
              <span className="text-sm text-sd-ink">{c.name}</span>
            </label>
          ))}
          {selected.size > 0 && (
            <>
              <div className="border-t border-sd-ink/10 mx-2" />
              <button
                type="button"
                onClick={() => onChange(new Set())}
                className="w-full text-left px-3 py-2 font-mono text-[10px] tracking-widest text-sd-red hover:bg-red-50"
              >
                CLEAR
              </button>
            </>
          )}
        </div>
      )}
    </div>
  );
}

export default function DraftsFilter({ campaigns, categories }: DraftsFilterProps) {
  const router = useRouter();
  const params = useSearchParams();

  const [search, setSearch] = useState(params.get("q") ?? "");
  const [fromDate, setFromDate] = useState(params.get("fromDate") ?? "");
  const [toDate, setToDate] = useState(params.get("toDate") ?? "");
  const [draftType, setDraftType] = useState(params.get("draftType") ?? "");
  const [minDrafters, setMinDrafters] = useState(params.get("minDrafters") ?? "");
  const [campaign, setCampaign] = useState(params.get("campaignPublicId") ?? "");
  const [selectedCategories, setSelectedCategories] = useState<Set<string>>(() => {
    const raw = params.getAll("categoryPublicIds");
    return new Set(raw);
  });
  const [sort, setSort] = useState(() => {
    const s = params.get("sort") ?? "date";
    const d = params.get("dir") ?? "desc";
    return `${s}-${d}`;
  });

  useEffect(() => {
    setSearch(params.get("q") ?? "");
    setFromDate(params.get("fromDate") ?? "");
    setToDate(params.get("toDate") ?? "");
    setDraftType(params.get("draftType") ?? "");
    setMinDrafters(params.get("minDrafters") ?? "");
    setCampaign(params.get("campaignPublicId") ?? "");
    setSelectedCategories(new Set(params.getAll("categoryPublicIds")));
    const s = params.get("sort") ?? "date";
    const d = params.get("dir") ?? "desc";
    setSort(`${s}-${d}`);
  }, [params]);

  const apply = () => {
    const qs = new URLSearchParams();
    if (search) qs.set("q", search);
    if (fromDate) qs.set("fromDate", fromDate);
    if (toDate) qs.set("toDate", toDate);
    if (draftType) qs.set("draftType", draftType);
    if (minDrafters) qs.set("minDrafters", minDrafters);
    if (campaign) qs.set("campaignPublicId", campaign);
    selectedCategories.forEach(id => qs.append("categoryPublicIds", id));
    const [sortField, sortDir] = sort.split("-") as [string, string];
    if (sortField) qs.set("sort", sortField);
    if (sortDir) qs.set("dir", sortDir);
    qs.set("page", "1");
    router.push(`?${qs.toString()}`);
  };

  const labelCls = "block font-mono text-[9px] tracking-widest text-sd-blue font-bold mb-1.5 uppercase";
  const inputCls =
    "w-full border border-sd-ink/30 rounded px-3 py-2 text-sm text-sd-ink focus:outline-none focus:border-sd-blue";
  const selectCls =
    "w-full border border-sd-ink/30 rounded px-3 py-2 text-sm text-sd-ink focus:outline-none focus:border-sd-blue bg-white";

  return (
    <div
      className="bg-white border-b border-sd-ink/20 px-10 py-6 flex items-end gap-6"
      style={{ borderBottomWidth: "1.5px" }}
    >
      {/* Search */}
      <div className="flex-[2]">
        <label className={labelCls}>SEARCH THE ARCHIVE</label>
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && apply()}
          placeholder="Search by title, drafter, or film…"
          className={inputCls}
        />
      </div>

      {/* Date range */}
      <div className="flex-1">
        <label className={labelCls}>DATE RANGE</label>
        <div className="flex gap-2">
          <input
            type="date"
            value={fromDate}
            onChange={(e) => setFromDate(e.target.value)}
            className={inputCls}
            title="From"
          />
          <input
            type="date"
            value={toDate}
            onChange={(e) => setToDate(e.target.value)}
            className={inputCls}
            title="To"
          />
        </div>
      </div>

      {/* Draft type */}
      <div className="flex-1">
        <label className={labelCls}>DRAFT TYPE</label>
        <select value={draftType} onChange={(e) => setDraftType(e.target.value)} className={selectCls}>
          <option value="">All types</option>
          {DRAFT_TYPE_OPTIONS.map((o) => (
            <option key={o.id} value={String(o.id)}>
              {o.label}
            </option>
          ))}
        </select>
      </div>

      {/* Campaign */}
      <div className="flex-1">
        <label className={labelCls}>CAMPAIGN</label>
        <select value={campaign} onChange={(e) => setCampaign(e.target.value)} className={selectCls}>
          <option value="">All campaigns</option>
          {campaigns.map((c) => (
            <option key={c.publicId} value={c.publicId}>
              {c.name}
            </option>
          ))}
        </select>
      </div>

      {/* Category multi-select */}
      <div className="flex-1 min-w-[160px]">
        <label className={labelCls}>CATEGORIES</label>
        <CategoryDropdown
          categories={categories}
          selected={selectedCategories}
          onChange={setSelectedCategories}
        />
      </div>

      {/* Drafter count */}
      <div className="flex-1">
        <label className={labelCls}>DRAFTER COUNT</label>
        <select value={minDrafters} onChange={(e) => setMinDrafters(e.target.value)} className={selectCls}>
          {DRAFTER_COUNT_OPTIONS.map((o) => (
            <option key={o.value} value={o.value}>
              {o.label}
            </option>
          ))}
        </select>
      </div>

      {/* Apply button */}
      <button
        onClick={apply}
        className="shrink-0 bg-sd-red text-white font-oswald text-xs tracking-wide px-5 py-2.5 hover:bg-red-700 transition-colors"
      >
        FILTER →
      </button>
    </div>
  );
}
