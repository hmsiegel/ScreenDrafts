"use client";

import { useState, useRef, useCallback } from "react";
import { env } from "@/lib/env";

// ── Types ────────────────────────────────────────────────────────────────────

export type CommunityFilmRuleKind = "BoostersVeto" | "BoostersPick";

export interface CommunityFilmRuleState {
  /** Client-only stable identity for list rendering */
  localId: string;
  /** Set once persisted; null for pending rules */
  publicId: string | null;
  ruleKind: CommunityFilmRuleKind;
  /** Required when ruleKind === "BoostersPick" */
  targetSlot: number | null;
  tmdbId: number | null;
  title: string | null;
  status: "persisted" | "pending" | "removing";
}

export interface CommunityConfig {
  enabled: boolean;
  maxCommunityPicks: number;
  maxCommunityVetoes: number;
  filmRules: CommunityFilmRuleState[];
}

export function defaultCommunityConfig(): CommunityConfig {
  return {
    enabled: false,
    maxCommunityPicks: 0,
    maxCommunityVetoes: 0,
    filmRules: [],
  };
}

// ── Media search ─────────────────────────────────────────────────────────────

interface MediaSearchResult {
  tmdbId: number;
  title: string;
  year: string | null;
}

async function searchMedia(query: string): Promise<MediaSearchResult[]> {
  if (!query.trim()) return [];
  try {
    const url = new URL(`${env.apiUrl}/media/search`);
    url.searchParams.set("query", query);
    url.searchParams.set("pageSize", "10");
    const res = await fetch(url.toString(), { cache: "no-store" });
    if (!res.ok) return [];
    const data = (await res.json()) as {
      results?: { items?: { tmdbId?: number | null; title?: string; year?: string | null }[] };
    };
    return (data.results?.items ?? [])
      .filter((r) => r.tmdbId != null)
      .map((r) => ({ tmdbId: r.tmdbId!, title: r.title ?? "", year: r.year ?? null }));
  } catch {
    return [];
  }
}

// ── Styles ───────────────────────────────────────────────────────────────────

const LABEL = "block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1";
const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";

// ── Component ─────────────────────────────────────────────────────────────────

interface Props {
  config: CommunityConfig;
  onChange: (next: CommunityConfig) => void;
}

export function CommunitySection({ config, onChange }: Props) {
  const [filmSearch, setFilmSearch] = useState("");
  const [filmResults, setFilmResults] = useState<MediaSearchResult[]>([]);
  const [filmSearchLoading, setFilmSearchLoading] = useState(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  function set(patch: Partial<CommunityConfig>) {
    onChange({ ...config, ...patch });
  }

  function setRules(rules: CommunityFilmRuleState[]) {
    set({ filmRules: rules });
  }

  // Film search
  const runSearch = useCallback(async (q: string) => {
    setFilmSearchLoading(true);
    try {
      setFilmResults(await searchMedia(q));
    } finally {
      setFilmSearchLoading(false);
    }
  }, []);

  function handleFilmSearchChange(value: string) {
    setFilmSearch(value);
    if (debounceRef.current) clearTimeout(debounceRef.current);
    if (!value.trim()) { setFilmResults([]); return; }
    debounceRef.current = setTimeout(() => runSearch(value), 300);
  }

  function addRule(ruleKind: CommunityFilmRuleKind, film?: MediaSearchResult) {
    const newRule: CommunityFilmRuleState = {
      localId: crypto.randomUUID(),
      publicId: null,
      ruleKind,
      targetSlot: ruleKind === "BoostersPick" ? 1 : null,
      tmdbId: film?.tmdbId ?? null,
      title: film?.title ?? null,
      status: "pending",
    };
    setRules([...config.filmRules.filter((r) => r.status !== "removing"), newRule]);
    setFilmSearch("");
    setFilmResults([]);
  }

  function updateRule(localId: string, patch: Partial<CommunityFilmRuleState>) {
    setRules(config.filmRules.map((r) => r.localId === localId ? { ...r, ...patch } : r));
  }

  function removeRule(localId: string) {
    setRules(
      config.filmRules.flatMap((r) => {
        if (r.localId !== localId) return [r];
        // Pending rules are discarded entirely; persisted rules are marked for removal
        return r.status === "pending" ? [] : [{ ...r, status: "removing" as const }];
      })
    );
  }

  const visibleRules = config.filmRules.filter((r) => r.status !== "removing");

  return (
    <div>
      {/* Toggle */}
      <label className="flex items-center gap-2.5 cursor-pointer select-none group w-fit">
        <input
          type="checkbox"
          checked={config.enabled}
          onChange={(e) => set({ enabled: e.target.checked })}
          className="accent-sd-red w-4 h-4"
        />
        <span className="font-mono text-[11px] tracking-widest uppercase text-sd-ink/70 group-hover:text-sd-ink transition-colors">
          Community Participation (Patreon / Boosters)
        </span>
      </label>

      {config.enabled && (
        <div className="mt-4 space-y-5">
          {/* ── Limits ── */}
          <div className="border border-sd-ink/10 rounded p-4 bg-white">
            <p className={`${LABEL} mb-3`}>Limits</p>
            <div className="grid grid-cols-2 gap-4 max-w-xs">
              <div>
                <label className={LABEL}>Max Picks</label>
                <input
                  type="number"
                  min={0}
                  className={INPUT}
                  value={config.maxCommunityPicks}
                  onChange={(e) =>
                    set({ maxCommunityPicks: Math.max(0, parseInt(e.target.value, 10) || 0) })
                  }
                />
              </div>
              <div>
                <label className={LABEL}>Max Vetoes</label>
                <input
                  type="number"
                  min={0}
                  className={INPUT}
                  value={config.maxCommunityVetoes}
                  onChange={(e) =>
                    set({ maxCommunityVetoes: Math.max(0, parseInt(e.target.value, 10) || 0) })
                  }
                />
              </div>
            </div>
          </div>

          {/* ── Film Rules ── */}
          <div className="border border-sd-ink/10 rounded p-4 bg-white">
            <p className={`${LABEL} mb-3`}>Film Rules (Optional)</p>
            <p className="text-[11px] font-mono text-sd-ink/50 mb-4">
              Set the rule first, then assign a film once the community has voted.
            </p>

            {/* Existing rules */}
            {visibleRules.length > 0 && (
              <div className="space-y-3 mb-4">
                {visibleRules.map((rule) => (
                  <div
                    key={rule.localId}
                    className="border border-sd-ink/10 rounded p-3 bg-sd-paper space-y-3"
                  >
                    {/* Rule kind */}
                    <div className="flex items-start gap-4 flex-wrap">
                      <label className="flex items-center gap-1.5 text-sm text-sd-ink cursor-pointer">
                        <input
                          type="radio"
                          name={`rule-kind-${rule.localId}`}
                          checked={rule.ruleKind === "BoostersVeto"}
                          onChange={() =>
                            updateRule(rule.localId, { ruleKind: "BoostersVeto", targetSlot: null })
                          }
                          className="accent-sd-red"
                        />
                        <span>
                          <strong className="font-medium">Boosters Veto</strong>
                          <span className="text-sd-ink/50 text-[11px] ml-1.5">
                            — veto fires if played at all
                          </span>
                        </span>
                      </label>
                      <label className="flex items-center gap-1.5 text-sm text-sd-ink cursor-pointer">
                        <input
                          type="radio"
                          name={`rule-kind-${rule.localId}`}
                          checked={rule.ruleKind === "BoostersPick"}
                          onChange={() =>
                            updateRule(rule.localId, { ruleKind: "BoostersPick", targetSlot: rule.targetSlot ?? 1 })
                          }
                          className="accent-sd-red"
                        />
                        <span>
                          <strong className="font-medium">Boosters Pick</strong>
                          <span className="text-sd-ink/50 text-[11px] ml-1.5">
                            — veto fires if not at target slot
                          </span>
                        </span>
                      </label>
                    </div>

                    {/* Target slot (BoostersPick only) */}
                    {rule.ruleKind === "BoostersPick" && (
                      <div className="max-w-[120px]">
                        <label className={LABEL}>Target Slot #</label>
                        <input
                          type="number"
                          min={1}
                          className={INPUT}
                          value={rule.targetSlot ?? ""}
                          placeholder="e.g. 1"
                          onChange={(e) => {
                            const v = parseInt(e.target.value, 10);
                            updateRule(rule.localId, { targetSlot: isNaN(v) ? null : Math.max(1, v) });
                          }}
                        />
                      </div>
                    )}

                    {/* Assigned film */}
                    <div className="flex items-center gap-3">
                      {rule.tmdbId ? (
                        <div className="flex items-center gap-2 flex-1">
                          <span className="text-sm text-sd-ink font-medium">{rule.title}</span>
                          <span className="text-[11px] font-mono text-sd-ink/40">
                            TMDb #{rule.tmdbId}
                          </span>
                          <button
                            type="button"
                            onClick={() => updateRule(rule.localId, { tmdbId: null, title: null })}
                            className="text-[11px] font-mono text-sd-ink/40 hover:text-sd-red ml-1"
                          >
                            clear film
                          </button>
                        </div>
                      ) : (
                        <span className="text-[11px] font-mono text-sd-ink/40 italic flex-1">
                          No film assigned yet — search below to assign
                        </span>
                      )}
                      <button
                        type="button"
                        onClick={() => removeRule(rule.localId)}
                        className="text-sd-ink/40 hover:text-sd-red text-lg leading-none ml-auto"
                        aria-label="Remove rule"
                      >
                        ×
                      </button>
                    </div>

                    {rule.status === "persisted" && (
                      <p className="text-[10px] font-mono text-sd-ink/30">saved</p>
                    )}
                  </div>
                ))}
              </div>
            )}

            {/* Add new rule (blank) */}
            <div className="flex gap-2 mb-4">
              <button
                type="button"
                onClick={() => addRule("BoostersVeto")}
                className="border border-sd-ink/20 text-sd-ink/70 font-mono text-[11px] tracking-wide uppercase px-3 py-1.5 hover:bg-sd-ink/5 rounded transition-colors"
              >
                + Boosters Veto Rule
              </button>
              <button
                type="button"
                onClick={() => addRule("BoostersPick")}
                className="border border-sd-ink/20 text-sd-ink/70 font-mono text-[11px] tracking-wide uppercase px-3 py-1.5 hover:bg-sd-ink/5 rounded transition-colors"
              >
                + Boosters Pick Rule
              </button>
            </div>

            {/* Film search — assigns to any unassigned rule */}
            {visibleRules.some((r) => !r.tmdbId) && (
              <div>
                <p className={`${LABEL} mb-2`}>Assign Film to Rule</p>
                <input
                  type="text"
                  placeholder="Search films…"
                  className={`${INPUT} mb-2`}
                  value={filmSearch}
                  onChange={(e) => handleFilmSearchChange(e.target.value)}
                />
                {filmSearchLoading && (
                  <p className="text-sm text-sd-ink/40 font-mono px-1">Searching…</p>
                )}
                {!filmSearchLoading && filmResults.length > 0 && (
                  <div className="border border-sd-ink/10 rounded overflow-hidden">
                    {filmResults.map((film) => (
                      <div
                        key={film.tmdbId}
                        className="flex items-center justify-between px-3 py-2 text-sm text-sd-ink hover:bg-sd-ink/5 border-b border-sd-ink/5 last:border-0"
                      >
                        <span>
                          {film.title}
                          {film.year && (
                            <span className="text-sd-ink/50 ml-1.5 text-[11px] font-mono">
                              ({film.year})
                            </span>
                          )}
                        </span>
                        <div className="flex gap-1.5 ml-4 shrink-0">
                          {visibleRules.filter((r) => !r.tmdbId).map((rule) => (
                            <button
                              key={rule.localId}
                              type="button"
                              onClick={() => {
                                updateRule(rule.localId, { tmdbId: film.tmdbId, title: film.title });
                                setFilmSearch("");
                                setFilmResults([]);
                              }}
                              className="text-[11px] font-mono border border-sd-ink/20 px-2 py-0.5 rounded hover:bg-sd-ink/5 text-sd-ink/70"
                            >
                              {visibleRules.filter((r) => !r.tmdbId).length > 1
                                ? `Assign to Rule ${visibleRules.indexOf(rule) + 1}`
                                : "Assign"}
                            </button>
                          ))}
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}