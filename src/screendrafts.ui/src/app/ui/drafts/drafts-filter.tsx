'use client';

import { DRAFT_TYPES } from "@/app/lib/types/draft-types";
import { useRouter, useSearchParams } from "next/navigation";
import { useEffect, useState } from "react";

export default function DraftsFilter() {
  const router = useRouter();
  const params = useSearchParams();

  const [fromDate, setFromDate] = useState(params.get("fromDate") ?? "");
  const [toDate, setToDate] = useState(params.get("toDate") ?? "");
  const [minDrafters, setMinDrafters] = useState(params.get("minDrafters") ?? "");
  const [maxDrafters, setMaxDrafters] = useState(params.get("maxDrafters") ?? "");
  const [minPicks, setMinPicks] = useState(params.get("minPicks") ?? "");
  const [maxPicks, setMaxPicks] = useState(params.get("maxPicks") ?? "");

  const initialTypes = params.getAll("draftType").map(Number);
  const [draftType, setDraftType] = useState<Set<number>>(new Set(initialTypes));

  const [open, setOpen] = useState(false);


  useEffect(() => {
    setDraftType(new Set(params.getAll("draftType").map(Number)));
  }, [params]);

  const toggleDraftType = (id: number) =>
    setDraftType(prev => {
      const newSet = new Set(prev);
      newSet.has(id) ? newSet.delete(id) : newSet.add(id);
      return newSet;
    });

  const apply = () => {
    const qs = new URLSearchParams();

    if (fromDate) qs.set("fromDate", fromDate);
    if (toDate) qs.set("toDate", toDate);
    if (minDrafters) qs.set("minDrafters", minDrafters);
    if (maxDrafters) qs.set("maxDrafters", maxDrafters);
    if (minPicks) qs.set("minPicks", minPicks);
    if (maxPicks) qs.set("maxPicks", maxPicks);

    draftType.forEach(type => qs.append("draftType", String(type)));

    router.push(`?${qs.toString()}`);
  };

  return (
    <div className="mb-6">
      {/* toggle button */}
      <button
        onClick={() => setOpen(o => !o)}
        className="btn btn-outline mb-2"
      >
        {open ? "Hide Filters ▲" : "Show Filters ▼"}
      </button>

      {/* collapsible panel */}
      {open && (
        <>
          <div className="grid md:grid-cols-3 gap-6 border border-slate-300 bg-slate-50 p-4 rounded-lg">
            <div className="flex flex-col gap-4">
              {/* date range */}
              <DateInput label="From" value={fromDate} onChange={setFromDate} />
              <NumInput label="Min Drafters" value={minDrafters} onChange={setMinDrafters} />
              <NumInput label="Min Picks" value={minPicks} onChange={setMinPicks} />
            </div>
            <div className="flex flex-col gap-4">
              <DateInput label="To" value={toDate} onChange={setToDate} />
              <NumInput label="Max Drafters" value={maxDrafters} onChange={setMaxDrafters} />
              <NumInput label="Max Picks" value={maxPicks} onChange={setMaxPicks} />
            </div>

            {/* draft-type checkboxes */}
            <fieldset className="flex flex-col gap-2">
              <legend className="font-medium text-sm mb-1">Draft Type</legend>
              {DRAFT_TYPES.map(dt => (
                <label key={dt.id} className="inline-flex items-center gap-2">
                  <input
                    type="checkbox"
                    className="checkbox checkbox-sm"
                    checked={draftType.has(dt.id)}
                    onChange={() => toggleDraftType(dt.id)}
                  />
                  <span>{dt.label}</span>
                </label>
              ))}
            </fieldset>
            {/* apply button */}
            <div className="col-span-3 flex justify-center">
              <button onClick={apply} className="btn btn-primary bg-sd-blue items-center rounded-lg text-lg px-4 text-white h-10">
                Apply
              </button>
            </div>
          </div>
        </>
      )}
    </div>
  );
}

/* --- tiny input helpers ---------------------------------------- */
function DateInput({
  label,
  value,
  onChange,
}: {
  label: string;
  value: string;
  onChange: (v: string) => void;
}) {
  return (
    <div className="flex flex-col">
      <label className="text-sm">{label}</label>
      <input
        type="date"
        value={value}
        onChange={e => onChange(e.target.value)}
        className="input input-bordered"
      />
    </div>
  );
}

function NumInput({
  label,
  value,
  onChange,
}: {
  label: string;
  value: string;
  onChange: (v: string) => void;
}) {
  return (
    <div className="flex flex-col">
      <label className="text-sm">{label}</label>
      <input
        type="number"
        min={0}
        value={value}
        onChange={e => onChange(e.target.value)}
        className="input input-bordered"
      />
    </div>
  );
}