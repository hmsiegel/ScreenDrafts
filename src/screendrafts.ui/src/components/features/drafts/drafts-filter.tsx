// components/features/drafts/drafts-filter.tsx
'use client';

import { useRouter, useSearchParams } from "next/navigation";
import { useState, useEffect, useId, useRef } from "react";

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
        aria-expanded={open}
        className="w-full border border-sd-ink/30 rounded px-3 py-2 text-sm text-sd-ink bg-white focus:outline-none focus:border-sd-blue text-left flex items-center justify-between gap-2"
      >
        <span className={`truncate ${selected.size === 0 ? "text-sd-ink/40" : ""}`}>{label}</span>
        <span className="text-sd-ink/40 text-xs">{open ? "▲" : "▼"}</span>
      </button>

      {open && (
        <div className="absolute z-20 top-full left-0 mt-1 w-full min-w-[200px] bg-white border border-sd-ink/20 shadow-lg max-h-60 overflow-y-auto overscroll-contain">
          {categories.length === 0 && (
            <p className="px-3 py-2 font-mono text-[11px] text-sd-ink/40">No categories.</p>
          )}
          {categories.map(c => (
            <label
              key={c.publicId}
              className="flex items-center gap-2.5 px-3 py-2.5 lg:py-2 cursor-pointer hover:bg-sd-paper/60 select-none"
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
                className="w-full text-left px-3 py-2.5 lg:py-2 font-mono text-[10px] tracking-widest text-sd-red hover:bg-red-50"
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

// Layout by width:
//   < lg      Search plus a FILTERS toggle; the other fields fold into a panel
//             (1 column on phones, 2 from sm).
//   lg – 2xl  Always open, 4-column grid: search + dates, then the four selects,
//             then the button on its own row, right-aligned.
//   >= 2xl    The original single row. Below 1536 the two native date inputs
//             (~125px each) don't fit in one row beside everything else.
export default function DraftsFilter({ campaigns, categories }: DraftsFilterProps) {
  const router = useRouter();
  const params = useSearchParams();
  const panelId = useId();

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
  const [panelOpen, setPanelOpen] = useState(false);

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

  // Counts what's applied in the URL, not unsaved edits, so the badge matches the results shown.
  const appliedFilterCount =
    (params.get("fromDate") || params.get("toDate") ? 1 : 0) +
    (params.get("draftType") ? 1 : 0) +
    (params.get("campaignPublicId") ? 1 : 0) +
    (params.getAll("categoryPublicIds").length > 0 ? 1 : 0) +
    (params.get("minDrafters") ? 1 : 0);

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
    setPanelOpen(false);
    router.push(`?${qs.toString()}`);
  };

  const labelCls = "block font-mono text-[9px] tracking-widest text-sd-blue font-bold mb-1.5 uppercase";
  const inputCls =
    "w-full min-w-0 border border-sd-ink/30 rounded px-3 py-2 text-sm text-sd-ink focus:outline-none focus:border-sd-blue";
  const selectCls =
    "w-full min-w-0 border border-sd-ink/30 rounded px-3 py-2 text-sm text-sd-ink focus:outline-none focus:border-sd-blue bg-white";

  return (
    <div className="bg-white border-b-[1.5px] border-sd-ink/20 page-x py-4 lg:py-6">
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-4 lg:items-end lg:gap-x-6 2xl:grid-cols-[minmax(0,2fr)_minmax(0,2fr)_repeat(4,minmax(0,1fr))_auto]">
        {/* Search — always visible. Below lg the FILTERS toggle rides beside it. */}
        <div className="lg:col-span-2 2xl:col-span-1">
          <label className={labelCls}>SEARCH THE ARCHIVE</label>
          <div className="flex gap-2">
            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && apply()}
              placeholder="Search by title, drafter, or film…"
              enterKeyHint="search"
              className={inputCls}
            />
            <button
              type="button"
              onClick={() => setPanelOpen(v => !v)}
              aria-expanded={panelOpen}
              aria-controls={panelId}
              className={`lg:hidden shrink-0 rounded border px-3 font-oswald text-xs tracking-widest transition-colors ${
                panelOpen || appliedFilterCount > 0
                  ? "border-sd-ink bg-sd-ink text-white"
                  : "border-sd-ink/30 text-sd-ink"
              }`}
            >
              FILTERS{appliedFilterCount > 0 ? ` (${appliedFilterCount})` : ""}
            </button>
          </div>
        </div>

        {/* Collapsible below lg. From lg, `contents` dissolves this wrapper so its
            children drop straight into the outer grid. */}
        <div
          id={panelId}
          className={`${panelOpen ? "grid" : "hidden"} grid-cols-1 sm:grid-cols-2 gap-4 lg:contents`}
        >
          {/* Date range */}
          <div className="sm:col-span-2 2xl:col-span-1">
            <label className={labelCls}>DATE RANGE</label>
            <div className="grid grid-cols-2 gap-2">
              <input
                type="date"
                value={fromDate}
                onChange={(e) => setFromDate(e.target.value)}
                className={inputCls}
                title="From"
                aria-label="From date"
              />
              <input
                type="date"
                value={toDate}
                onChange={(e) => setToDate(e.target.value)}
                className={inputCls}
                title="To"
                aria-label="To date"
              />
            </div>
          </div>

          {/* Draft type */}
          <div>
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
          <div>
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
          <div className="min-w-0">
            <label className={labelCls}>CATEGORIES</label>
            <CategoryDropdown
              categories={categories}
              selected={selectedCategories}
              onChange={setSelectedCategories}
            />
          </div>

          {/* Drafter count */}
          <div>
            <label className={labelCls}>DRAFTER COUNT</label>
            <select value={minDrafters} onChange={(e) => setMinDrafters(e.target.value)} className={selectCls}>
              {DRAFTER_COUNT_OPTIONS.map((o) => (
                <option key={o.value} value={o.value}>
                  {o.label}
                </option>
              ))}
            </select>
          </div>

          {/* Apply — full width in the panel, right-aligned on its own row at lg, inline at 2xl. */}
          <button
            onClick={apply}
            className="min-h-11 lg:min-h-0 sm:col-span-2 lg:col-span-4 lg:justify-self-end 2xl:col-span-1 bg-sd-red text-white font-oswald text-xs tracking-wide px-5 py-2.5 hover:bg-red-700 transition-colors"
          >
            FILTER →
          </button>
        </div>
      </div>
    </div>
  );
}