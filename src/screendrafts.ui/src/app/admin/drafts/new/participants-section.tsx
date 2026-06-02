"use client";

import { useState, useEffect, useRef, useCallback } from "react";
import { searchDrafters, searchDrafterTeams } from "@/services/admin/fetch-admin-drafts";

const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";
const SECTION_HEADING =
  "font-oswald font-bold text-[18px] tracking-wide uppercase text-sd-ink mb-4 pb-2 border-b border-sd-ink/10";

interface DrafterResult {
  publicId: string;
  displayName: string;
  isRetired: boolean;
}

interface TeamResult {
  publicId: string;
  name: string;
  numberOfDrafters: number;
}

export interface InitialParticipant {
  publicId: string;
  displayName: string;
  kind: "drafter" | "team";
}

interface Props {
  accessToken: string;
  selectedDrafterIds: Set<string>;
  selectedTeamIds: Set<string>;
  onToggleDrafter: (id: string) => void;
  onToggleTeam: (id: string) => void;
  initialParticipants?: InitialParticipant[];
}

export function ParticipantsSection({
  accessToken,
  selectedDrafterIds,
  selectedTeamIds,
  onToggleDrafter,
  onToggleTeam,
  initialParticipants = [],
}: Props) {
  type Tab = "drafter" | "team";
  const [tab, setTab] = useState<Tab>("drafter");

  // Drafter search state
  const [drafterQuery, setDrafterQuery] = useState("");
  const [drafterResults, setDrafterResults] = useState<DrafterResult[]>([]);
  const [drafterLoading, setDrafterLoading] = useState(false);

  // Team search state
  const [teamQuery, setTeamQuery] = useState("");
  const [teamResults, setTeamResults] = useState<TeamResult[]>([]);
  const [teamLoading, setTeamLoading] = useState(false);

  const [displayNames, setDisplayNames] = useState<Map<string, string>>(() => {
    const m = new Map<string, string>();
    for (const p of initialParticipants) m.set(p.publicId, p.displayName);
    return m;
  });

  const drafterDebounce = useRef<ReturnType<typeof setTimeout> | null>(null);
  const teamDebounce = useRef<ReturnType<typeof setTimeout> | null>(null);

  // Initial load — fetch first page with no query so the list isn't empty on open
  useEffect(() => {
    fetchDrafters("");
    fetchTeams("");
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const fetchDrafters = useCallback(
    async (q: string) => {
      setDrafterLoading(true);
      try {
        const results = await searchDrafters(accessToken, q || undefined, false, 1, 50);
        setDrafterResults(results);
      } finally {
        setDrafterLoading(false);
      }
    },
    [accessToken]
  );

  const fetchTeams = useCallback(
    async (q: string) => {
      setTeamLoading(true);
      try {
        const results = await searchDrafterTeams(accessToken, q || undefined, 1, 50);
        setTeamResults(results);
      } finally {
        setTeamLoading(false);
      }
    },
    [accessToken]
  );

  function handleDrafterQueryChange(value: string) {
    setDrafterQuery(value);
    if (drafterDebounce.current) clearTimeout(drafterDebounce.current);
    drafterDebounce.current = setTimeout(() => fetchDrafters(value), 300);
  }

  function handleTeamQueryChange(value: string) {
    setTeamQuery(value);
    if (teamDebounce.current) clearTimeout(teamDebounce.current);
    teamDebounce.current = setTimeout(() => fetchTeams(value), 300);
  }

  function handleToggleDrafter(id: string, displayName: string) {
    setDisplayNames((prev) => {
      const next = new Map(prev);
      next.set(id, displayName);
      return next;
    });
    onToggleDrafter(id);
  }

  function handleToggleTeam(id: string, name: string) {
    setDisplayNames((prev) => {
      const next = new Map(prev);
      next.set(id, name);
      return next;
    });
    onToggleTeam(id);
  }

  const totalSelected = selectedDrafterIds.size + selectedTeamIds.size;

  return (
    <section>
      <h2 className={SECTION_HEADING}>Participants (Optional)</h2>

      {/* Selected summary chips */}
      {totalSelected > 0 && (
        <div className="flex flex-wrap gap-1.5 mb-4">
          {[...selectedDrafterIds].map((id) => (
            <span
              key={id}
              className="inline-flex items-center gap-1 px-2 py-0.5 bg-sd-ink text-white text-[11px] font-mono rounded"
            >
              {displayNames.get(id) ?? id}
              <button
                type="button"
                onClick={() => onToggleDrafter(id)}
                className="ml-0.5 hover:text-sd-red leading-none"
                aria-label="Remove"
              >
                ×
              </button>
            </span>
          ))}
          {[...selectedTeamIds].map((id) => (
            <span
              key={id}
              className="inline-flex items-center gap-1 px-2 py-0.5 bg-sd-blue text-white text-[11px] font-mono rounded"
            >
              {displayNames.get(id) ?? id}
              <button
                type="button"
                onClick={() => onToggleTeam(id)}
                className="ml-0.5 hover:text-sd-red leading-none"
                aria-label="Remove"
              >
                ×
              </button>
            </span>
          ))}
        </div>
      )}

      {/* Tab toggle */}
      <div className="flex gap-0 mb-4 border border-sd-ink/20 rounded overflow-hidden w-fit">
        <button
          type="button"
          onClick={() => setTab("drafter")}
          className={`px-4 py-2 text-sm font-mono tracking-wide uppercase transition-colors ${
            tab === "drafter"
              ? "bg-sd-ink text-white"
              : "bg-white text-sd-ink/60 hover:bg-sd-ink/5"
          }`}
        >
          Individual
        </button>
        <button
          type="button"
          onClick={() => setTab("team")}
          className={`px-4 py-2 text-sm font-mono tracking-wide uppercase transition-colors border-l border-sd-ink/20 ${
            tab === "team"
              ? "bg-sd-ink text-white"
              : "bg-white text-sd-ink/60 hover:bg-sd-ink/5"
          }`}
        >
          Team
        </button>
      </div>

      <div className="border border-sd-ink/10 rounded p-4 bg-white">
        {tab === "drafter" ? (
          <>
            <input
              type="text"
              placeholder="Search drafters…"
              className={`${INPUT} mb-3`}
              value={drafterQuery}
              onChange={(e) => handleDrafterQueryChange(e.target.value)}
            />
            <div className="max-h-48 overflow-y-auto space-y-1">
              {drafterLoading ? (
                <p className="text-sm text-sd-ink/40 font-mono px-1">Loading…</p>
              ) : drafterResults.length === 0 ? (
                <p className="text-sm text-sd-ink/40 font-mono px-1">No drafters found.</p>
              ) : (
                drafterResults.map((d) => (
                  <label
                    key={d.publicId}
                    className="flex items-center gap-2 px-3 py-1.5 text-sm text-sd-ink hover:bg-sd-ink/5 rounded cursor-pointer"
                  >
                    <input
                      type="checkbox"
                      checked={selectedDrafterIds.has(d.publicId)}
                      onChange={() => handleToggleDrafter(d.publicId, d.displayName)}
                      className="accent-sd-red"
                    />
                    <span className={d.isRetired ? "text-sd-ink/40 line-through" : ""}>
                      {d.displayName}
                    </span>
                    {d.isRetired && (
                      <span className="text-[10px] font-mono text-sd-ink/40 uppercase tracking-wide">
                        retired
                      </span>
                    )}
                  </label>
                ))
              )}
            </div>
          </>
        ) : (
          <>
            <input
              type="text"
              placeholder="Search teams…"
              className={`${INPUT} mb-3`}
              value={teamQuery}
              onChange={(e) => handleTeamQueryChange(e.target.value)}
            />
            <div className="max-h-48 overflow-y-auto space-y-1">
              {teamLoading ? (
                <p className="text-sm text-sd-ink/40 font-mono px-1">Loading…</p>
              ) : teamResults.length === 0 ? (
                <p className="text-sm text-sd-ink/40 font-mono px-1">No teams found.</p>
              ) : (
                teamResults.map((t) => (
                  <label
                    key={t.publicId}
                    className="flex items-center gap-2 px-3 py-1.5 text-sm text-sd-ink hover:bg-sd-ink/5 rounded cursor-pointer"
                  >
                    <input
                      type="checkbox"
                      checked={selectedTeamIds.has(t.publicId)}
                      onChange={() => handleToggleTeam(t.publicId, t.name)}
                      className="accent-sd-red"
                    />
                    <span>{t.name}</span>
                    <span className="text-[11px] font-mono text-sd-ink/40">
                      {t.numberOfDrafters} member{t.numberOfDrafters !== 1 ? "s" : ""}
                    </span>
                  </label>
                ))
              )}
            </div>
          </>
        )}
      </div>
    </section>
  );
}