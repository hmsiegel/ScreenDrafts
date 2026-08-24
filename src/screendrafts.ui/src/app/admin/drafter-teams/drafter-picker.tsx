"use client";

import { useState, useEffect, useRef, useCallback } from "react";
import { searchDrafters } from "@/services/admin/fetch-admin-drafts";

const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";

interface DrafterResult {
  publicId: string;
  displayName: string;
  isRetired: boolean;
}

interface Props {
  accessToken: string;
  // Drafters already on the team (or already selected locally, for the create form) —
  // hidden from the results so the same person can't be added twice.
  excludeIds: Set<string>;
  onSelect: (drafter: { publicId: string; displayName: string }) => void;
  disabled?: boolean;
}

export function DrafterPicker({ accessToken, excludeIds, onSelect, disabled }: Props) {
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<DrafterResult[]>([]);
  const [loading, setLoading] = useState(false);
  const debounce = useRef<ReturnType<typeof setTimeout> | null>(null);

  const fetchDrafters = useCallback(
    async (q: string) => {
      setLoading(true);
      try {
        const found = await searchDrafters(accessToken, q || undefined, false, 1, 50);
        setResults(found);
      } finally {
        setLoading(false);
      }
    },
    [accessToken]
  );

  // Initial load with no query so the list isn't empty on open, same as ParticipantsSection.
  useEffect(() => {
    fetchDrafters("");
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  function handleQueryChange(value: string) {
    setQuery(value);
    if (debounce.current) clearTimeout(debounce.current);
    debounce.current = setTimeout(() => fetchDrafters(value), 300);
  }

  const visible = results.filter((d) => !excludeIds.has(d.publicId));

  return (
    <div className="border border-sd-ink/10 rounded p-4 bg-white">
      <input
        type="text"
        placeholder="Search drafters…"
        className={`${INPUT} mb-3`}
        value={query}
        onChange={(e) => handleQueryChange(e.target.value)}
        disabled={disabled}
      />
      <div className="max-h-48 overflow-y-auto space-y-1">
        {loading ? (
          <p className="text-sm text-sd-ink/40 font-mono px-1">Loading…</p>
        ) : visible.length === 0 ? (
          <p className="text-sm text-sd-ink/40 font-mono px-1">
            {results.length === 0 ? "No drafters found." : "All matches are already members."}
          </p>
        ) : (
          visible.map((d) => (
            <button
              type="button"
              key={d.publicId}
              onClick={() => onSelect({ publicId: d.publicId, displayName: d.displayName })}
              disabled={disabled}
              className="flex items-center gap-2 w-full text-left px-3 py-1.5 text-sm text-sd-ink hover:bg-sd-ink/5 rounded disabled:opacity-40"
            >
              <span className={d.isRetired ? "text-sd-ink/40 line-through" : ""}>
                {d.displayName}
              </span>
              {d.isRetired && (
                <span className="text-[10px] font-mono text-sd-ink/40 uppercase tracking-wide">
                  retired
                </span>
              )}
              <span className="ml-auto text-sd-blue text-lg leading-none">+</span>
            </button>
          ))
        )}
      </div>
    </div>
  );
}