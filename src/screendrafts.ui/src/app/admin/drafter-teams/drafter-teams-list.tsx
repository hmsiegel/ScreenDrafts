"use client";

import { useState, useEffect, useRef, useCallback } from "react";
import { searchDrafterTeams } from "@/services/admin/fetch-admin-drafts";

const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";
const BTN_PRIMARY =
  "bg-sd-red text-white font-oswald font-medium tracking-wide uppercase px-5 py-2.5 hover:bg-sd-red/90 disabled:opacity-50 transition-colors";

interface TeamResult {
  publicId: string;
  name: string;
  numberOfDrafters: number;
}

interface Props {
  accessToken: string;
}

export function DrafterTeamsList({ accessToken }: Props) {
  const [query, setQuery] = useState("");
  const [teams, setTeams] = useState<TeamResult[]>([]);
  const [loading, setLoading] = useState(true);
  const debounce = useRef<ReturnType<typeof setTimeout> | null>(null);

  const fetchTeams = useCallback(
    async (q: string) => {
      setLoading(true);
      try {
        const found = await searchDrafterTeams(accessToken, q || undefined, 1, 100);
        setTeams(found);
      } finally {
        setLoading(false);
      }
    },
    [accessToken]
  );

  useEffect(() => {
    fetchTeams("");
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  function handleQueryChange(value: string) {
    setQuery(value);
    if (debounce.current) clearTimeout(debounce.current);
    debounce.current = setTimeout(() => fetchTeams(value), 300);
  }

  return (
    <div className="space-y-6 max-w-2xl">
      <div className="flex items-center justify-between gap-4">
        <input
          type="text"
          placeholder="Search teams…"
          className={INPUT}
          value={query}
          onChange={(e) => handleQueryChange(e.target.value)}
        />
        <a href="/admin/drafter-teams/new" className={`${BTN_PRIMARY} shrink-0 whitespace-nowrap`}>
          New Team
        </a>
      </div>

      <div className="bg-white border border-sd-ink/10 rounded divide-y divide-sd-ink/5">
        {loading ? (
          <p className="text-sm text-sd-ink/40 font-mono px-4 py-3">Loading…</p>
        ) : teams.length === 0 ? (
          <p className="text-sm text-sd-ink/40 font-mono px-4 py-3">No drafter teams found.</p>
        ) : (
          teams.map((t) => (
            <a
              key={t.publicId}
              href={`/admin/drafter-teams/${encodeURIComponent(t.publicId)}`}
              className="flex items-center justify-between px-4 py-3 text-sm hover:bg-sd-paper/60 transition-colors"
            >
              <span className="font-medium text-sd-ink">{t.name}</span>
              <span className="font-mono text-[11px] text-sd-ink/40">
                {t.numberOfDrafters} member{t.numberOfDrafters !== 1 ? "s" : ""}
              </span>
            </a>
          ))
        )}
      </div>
    </div>
  );
}