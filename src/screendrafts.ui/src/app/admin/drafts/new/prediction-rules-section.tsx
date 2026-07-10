"use client";

import { useState, useRef, useCallback } from "react";
import { env } from "@/lib/env";
import { searchAllHosts, AdminHostOption } from "@/services/admin/fetch-admin-drafts";

// ── Types ────────────────────────────────────────────────────────────────────

export type PredictionMode = "UnorderedAll" | "UnorderedTopN" | "OrderedTopN" | "OrderedAll";

export const PREDICTION_MODE_VALUES: Record<PredictionMode, number> = {
  UnorderedAll: 0,
  UnorderedTopN: 1,
  OrderedTopN: 2,
  OrderedAll: 3,
};

/** Reverse of PREDICTION_MODE_VALUES, derived so the two can't drift apart. */
export const PREDICTION_MODE_NAMES: PredictionMode[] = (
  Object.entries(PREDICTION_MODE_VALUES) as [PredictionMode, number][]
)
  .sort((a, b) => a[1] - b[1])
  .map(([name]) => name);

/**
 * Converts a UTC ISO string (as returned by the API) to the
 * "YYYY-MM-DDTHH:mm" shape <input type="datetime-local"> requires,
 * expressed in the browser's local time zone. Inverse of toUtcIso
 * in fetch-admin-drafts.ts.
 */
export function isoToDatetimeLocal(iso: string): string {
  const utc = new Date(iso);
  const local = new Date(utc.getTime() - utc.getTimezoneOffset() * 60000);
  return local.toISOString().slice(0, 16);
}

export interface PredictorState {
  contestantPublicId: string;
  contestantDisplayName: string;
  allowedSubmitterPersonPublicId: string | null;
  allowedSubmitterDisplayName: string | null;
}

export interface PredictionConfig {
  enabled: boolean;
  mode: PredictionMode;
  requiredCount: number;
  topN: number | null;
  deadlineUtc: string | null;
  predictors: PredictorState[];
}

export function defaultPredictionConfig(): PredictionConfig {
  return {
    enabled: false,
    mode: "UnorderedAll",
    requiredCount: 7,
    topN: null,
    deadlineUtc: null,
    predictors: [],
  };
}

// ── Contestant search ────────────────────────────────────────────────────────

interface ContestantSearchResult {
  publicId: string;
  displayName: string;
}

// TODO: confirm this endpoint exists — mirrors /hosts/search, /drafters/search
async function searchContestants(
  accessToken: string,
  query: string
): Promise<ContestantSearchResult[]> {
  try {
    const url = new URL(`${env.apiUrl}/prediction-contestants/search`);
    if (query.trim()) url.searchParams.set("name", query);
    url.searchParams.set("pageSize", "20");
    const res = await fetch(url.toString(), {
      headers: { Authorization: `Bearer ${accessToken}` },
      cache: "no-store",
    });
    if (!res.ok) return [];
    const data = (await res.json()) as { items?: ContestantSearchResult[] };
    return data.items ?? [];
  } catch {
    return [];
  }
}

// ── Styles ───────────────────────────────────────────────────────────────────

const LABEL = "block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1";
const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";

// ── Component ────────────────────────────────────────────────────────────────

interface Props {
  config: PredictionConfig;
  onChange: (next: PredictionConfig) => void;
  accessToken: string;
}

export function PredictionRulesSection({ config, onChange, accessToken }: Props) {
  const [contestantSearch, setContestantSearch] = useState("");
  const [contestantResults, setContestantResults] = useState<ContestantSearchResult[]>([]);
  const [contestantLoading, setContestantLoading] = useState(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const [surrogateModeBySlot, setSurrogateModeBySlot] = useState<Record<string, boolean>>(() => {
    const initial: Record<string, boolean> = {};
    config.predictors.forEach((p) => {
      if (p.allowedSubmitterPersonPublicId) initial[p.contestantPublicId] = true;
    });
    return initial;
  });
  const [hostSearchBySlot, setHostSearchBySlot] = useState<Record<string, string>>({});
  const [hostResultsBySlot, setHostResultsBySlot] = useState<Record<string, AdminHostOption[]>>({});

  function set(patch: Partial<PredictionConfig>) {
    onChange({ ...config, ...patch });
  }

  function setPredictors(predictors: PredictorState[]) {
    set({ predictors });
  }

  const runContestantSearch = useCallback(
    async (q: string) => {
      setContestantLoading(true);
      try {
        setContestantResults(await searchContestants(accessToken, q));
      } finally {
        setContestantLoading(false);
      }
    },
    [accessToken]
  );

  function handleContestantSearchChange(value: string) {
    setContestantSearch(value);
    if (debounceRef.current) clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(() => runContestantSearch(value), 300);
  }

  function addPredictor(contestant: ContestantSearchResult) {
    if (config.predictors.some((p) => p.contestantPublicId === contestant.publicId)) return;
    setPredictors([
      ...config.predictors,
      {
        contestantPublicId: contestant.publicId,
        contestantDisplayName: contestant.displayName,
        allowedSubmitterPersonPublicId: null,
        allowedSubmitterDisplayName: null,
      },
    ]);
    setContestantSearch("");
    setContestantResults([]);
  }

  function removePredictor(contestantPublicId: string) {
    setPredictors(config.predictors.filter((p) => p.contestantPublicId !== contestantPublicId));
  }

  function setSurrogate(contestantPublicId: string, host: AdminHostOption | null) {
    setPredictors(
      config.predictors.map((p) =>
        p.contestantPublicId === contestantPublicId
          ? {
              ...p,
              allowedSubmitterPersonPublicId: host?.personPublicId ?? null,
              allowedSubmitterDisplayName: host?.displayName ?? null,
            }
          : p
      )
    );
  }

  function toggleSurrogateMode(contestantPublicId: string, checked: boolean) {
    setSurrogateModeBySlot((prev) => ({ ...prev, [contestantPublicId]: checked }));
    if (!checked) {
      setSurrogate(contestantPublicId, null);
    }
  }

  async function handleHostSearchChange(contestantPublicId: string, value: string) {
    setHostSearchBySlot((prev) => ({ ...prev, [contestantPublicId]: value }));
    if (!value.trim()) {
      setHostResultsBySlot((prev) => ({ ...prev, [contestantPublicId]: [] }));
      return;
    }
    const results = await searchAllHosts(accessToken, value);
    setHostResultsBySlot((prev) => ({ ...prev, [contestantPublicId]: results }));
  }

  const isTopNMode = config.mode === "UnorderedTopN" || config.mode === "OrderedTopN";

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
          Commissioner Predictions
        </span>
      </label>

      {config.enabled && (
        <div className="mt-4 space-y-5">
          {/* ── Rules ── */}
          <div className="border border-sd-ink/10 rounded p-4 bg-white">
            <p className={`${LABEL} mb-3`}>Rules</p>
            <div className="grid grid-cols-2 gap-4 max-w-lg">
              <div>
                <label className={LABEL}>Mode</label>
                <select
                  className={INPUT}
                  value={config.mode}
                  onChange={(e) => {
                    const mode = e.target.value as PredictionMode;
                    const nextIsTopN = mode === "UnorderedTopN" || mode === "OrderedTopN";
                    set({ mode, topN: nextIsTopN ? (config.topN ?? config.requiredCount) : null });
                  }}
                >
                  <option value="UnorderedAll">Unordered — All</option>
                  <option value="UnorderedTopN">Unordered — Top N</option>
                  <option value="OrderedTopN">Ordered — Top N</option>
                  <option value="OrderedAll">Ordered — All</option>
                </select>
              </div>
              <div>
                <label className={LABEL}>Required Count</label>
                <input
                  type="number"
                  min={1}
                  className={INPUT}
                  value={config.requiredCount}
                  onChange={(e) =>
                    set({ requiredCount: Math.max(1, parseInt(e.target.value, 10) || 1) })
                  }
                />
              </div>
              {isTopNMode && (
                <div>
                  <label className={LABEL}>Top N</label>
                  <input
                    type="number"
                    min={1}
                    className={INPUT}
                    value={config.topN ?? ""}
                    onChange={(e) => {
                      const v = parseInt(e.target.value, 10);
                      set({ topN: isNaN(v) ? null : Math.max(1, v) });
                    }}
                  />
                </div>
              )}
              <div>
                <label className={LABEL}>Deadline (optional)</label>
                <input
                  type="datetime-local"
                  className={INPUT}
                  value={config.deadlineUtc ?? ""}
                  onChange={(e) => set({ deadlineUtc: e.target.value || null })}
                />
              </div>
            </div>
          </div>

          {/* ── Predictors ── */}
          <div className="border border-sd-ink/10 rounded p-4 bg-white">
            <p className={`${LABEL} mb-3`}>Predictors</p>
            <p className="text-[11px] font-mono text-sd-ink/50 mb-4">
              Every slot here gets scored. Toggle a surrogate when the contestant themself
              isn&apos;t the one submitting.
            </p>

            {config.predictors.length > 0 && (
              <div className="space-y-3 mb-4">
                {config.predictors.map((p) => (
                  <div
                    key={p.contestantPublicId}
                    className="border border-sd-ink/10 rounded p-3 bg-sd-paper space-y-2"
                  >
                    <div className="flex items-center justify-between">
                      <span className="text-sm text-sd-ink font-medium">
                        {p.contestantDisplayName}
                      </span>
                      <button
                        type="button"
                        onClick={() => removePredictor(p.contestantPublicId)}
                        className="text-sd-ink/40 hover:text-sd-red text-lg leading-none"
                        aria-label="Remove predictor"
                      >
                        ×
                      </button>
                    </div>

                    <label className="flex items-center gap-2 text-[11px] font-mono text-sd-ink/60 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={surrogateModeBySlot[p.contestantPublicId] ?? false}
                        onChange={(e) =>
                          toggleSurrogateMode(p.contestantPublicId, e.target.checked)
                        }
                        className="accent-sd-red"
                      />
                      Using a surrogate
                    </label>

                    {surrogateModeBySlot[p.contestantPublicId] && (
                      <div>
                        {p.allowedSubmitterDisplayName ? (
                          <div className="flex items-center gap-2">
                            <span className="text-sm text-sd-ink">
                              {p.allowedSubmitterDisplayName}
                            </span>
                            <button
                              type="button"
                              onClick={() => setSurrogate(p.contestantPublicId, null)}
                              className="text-[11px] font-mono text-sd-ink/40 hover:text-sd-red"
                            >
                              change
                            </button>
                          </div>
                        ) : (
                          <>
                            <input
                              type="text"
                              placeholder="Search hosts…"
                              className={INPUT}
                              value={hostSearchBySlot[p.contestantPublicId] ?? ""}
                              onChange={(e) =>
                                handleHostSearchChange(p.contestantPublicId, e.target.value)
                              }
                            />
                            {(hostResultsBySlot[p.contestantPublicId] ?? []).length > 0 && (
                              <div className="border border-sd-ink/10 rounded overflow-hidden mt-2">
                                {(hostResultsBySlot[p.contestantPublicId] ?? []).map((host) => (
                                  <button
                                    key={host.publicId}
                                    type="button"
                                    onClick={() => setSurrogate(p.contestantPublicId, host)}
                                    className="w-full text-left px-3 py-2 text-sm text-sd-ink hover:bg-sd-ink/5 border-b border-sd-ink/5 last:border-0"
                                  >
                                    {host.displayName}
                                  </button>
                                ))}
                              </div>
                            )}
                          </>
                        )}
                      </div>
                    )}
                  </div>
                ))}
              </div>
            )}

            <div>
              <label className={LABEL}>Add Predictor</label>
              <input
                type="text"
                placeholder="Search contestants…"
                className={INPUT}
                value={contestantSearch}
                onChange={(e) => handleContestantSearchChange(e.target.value)}
                onFocus={() => runContestantSearch(contestantSearch)}
              />
              {contestantLoading && (
                <p className="text-sm text-sd-ink/40 font-mono px-1 mt-1">Searching…</p>
              )}
              {!contestantLoading && contestantResults.length > 0 && (
                <div className="border border-sd-ink/10 rounded overflow-hidden mt-2">
                  {contestantResults
                    .filter(
                      (c) => !config.predictors.some((p) => p.contestantPublicId === c.publicId)
                    )
                    .map((c) => (
                      <button
                        key={c.publicId}
                        type="button"
                        onClick={() => addPredictor(c)}
                        className="w-full text-left px-3 py-2 text-sm text-sd-ink hover:bg-sd-ink/5 border-b border-sd-ink/5 last:border-0"
                      >
                        + {c.displayName}
                      </button>
                    ))}
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}