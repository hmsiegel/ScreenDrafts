// components/features/stats/query-builder.tsx
"use client";

import { useState } from "react";
import { runStatsQuery } from "@/app/stats/actions";
import { formatDraftType } from "@/lib/draft-type-display";
import type {
  StatsOptionsView,
  StatsQueryInput,
  StatsQueryResultView,
} from "@/services/stats/stats-types";
import { QueryResults } from "./query-results";

const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";
const LABEL = "block font-mono text-[10px] tracking-widest text-[#5a6075] mb-1";
const LIMITS = [10, 25, 50, 100];

interface FormState {
  metric: string;
  groupBy: string;
  series: string[];
  draftTypes: string[];
  episodeFrom: string;
  episodeTo: string;
  minAppearances: string;
  ascending: boolean;
  limit: number;
  includeAll: boolean;
}

interface Preset {
  label: string;
  input: StatsQueryInput;
}

const PRESETS: Preset[] = [
  { label: "Most vetoes used", input: { metric: "vetoesUsed", groupBy: "drafter" } },
  {
    label: "Fewest vetoes per draft, 10+ drafts",
    input: { metric: "avgVetoesPerDraft", groupBy: "drafter", minAppearances: 10, ascending: true },
  },
  { label: "Picks vetoed by series", input: { metric: "picksVetoed", groupBy: "series" } },
  { label: "Most copacetic drafts", input: { metric: "copaceticDrafts", groupBy: "drafter" } },
];

function toggle(list: string[], value: string): string[] {
  return list.includes(value) ? list.filter((v) => v !== value) : [...list, value];
}

function parseNumber(value: string): number | undefined {
  if (value.trim() === "") return undefined;
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : undefined;
}

function toInput(form: FormState): StatsQueryInput {
  return {
    metric: form.metric,
    groupBy: form.groupBy,
    series: form.series.length > 0 ? form.series : undefined,
    draftTypes: form.draftTypes.length > 0 ? form.draftTypes : undefined,
    episodeFrom: parseNumber(form.episodeFrom),
    episodeTo: parseNumber(form.episodeTo),
    minAppearances: form.groupBy === "drafter" ? parseNumber(form.minAppearances) : undefined,
    ascending: form.ascending,
    limit: form.limit,
    includeAll: form.includeAll ? true : undefined,
  };
}

function fromInput(input: StatsQueryInput, includeAll: boolean): FormState {
  return {
    metric: input.metric,
    groupBy: input.groupBy,
    series: input.series ?? [],
    draftTypes: input.draftTypes ?? [],
    episodeFrom: input.episodeFrom?.toString() ?? "",
    episodeTo: input.episodeTo?.toString() ?? "",
    minAppearances: input.minAppearances?.toString() ?? "",
    ascending: input.ascending ?? false,
    limit: input.limit ?? 25,
    includeAll,
  };
}

function Chip({ label, active, onClick }: { label: string; active: boolean; onClick: () => void }) {
  return (
    <button
      type="button"
      aria-pressed={active}
      onClick={onClick}
      className={`min-h-8 px-3 border font-mono text-[11px] transition-colors ${
        active
          ? "bg-sd-ink text-white border-sd-ink"
          : "bg-white text-sd-ink border-sd-ink/30 hover:border-sd-ink"
      }`}
    >
      {label}
    </button>
  );
}

export function QueryBuilder({ options }: { options: StatsOptionsView }) {
  const firstMetric = options.metrics[0];
  const [form, setForm] = useState<FormState>(
    fromInput(
      {
        metric: firstMetric?.code ?? "",
        groupBy: firstMetric?.groupBys[0] ?? "drafter",
      },
      false
    )
  );
  const [result, setResult] = useState<StatsQueryResultView | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const metric = options.metrics.find((m) => m.code === form.metric);
  const allowedGroupBys = options.groupBys.filter((g) => metric?.groupBys.includes(g.code));

  function update(patch: Partial<FormState>) {
    setForm((current) => ({ ...current, ...patch }));
  }

  function onMetricChange(code: string) {
    const next = options.metrics.find((m) => m.code === code);
    const groupBy = next?.groupBys.includes(form.groupBy) ? form.groupBy : (next?.groupBys[0] ?? form.groupBy);
    update({ metric: code, groupBy });
  }

  async function run(input: StatsQueryInput) {
    setLoading(true);
    setError(null);

    const outcome = await runStatsQuery(input);

    setLoading(false);

    if (outcome.ok) {
      setResult(outcome.data);
    } else {
      setResult(null);
      setError(outcome.message);
    }
  }

  function applyPreset(preset: Preset) {
    const next = fromInput(preset.input, form.includeAll);
    setForm(next);
    void run(toInput(next));
  }

  function onSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    void run(toInput(form));
  }

  return (
    <div className="flex flex-col gap-6">
      <div>
        <p className="font-mono text-[10px] tracking-widest text-[#5a6075] mb-2">QUICK QUERIES</p>
        <div className="flex flex-wrap gap-2">
          {PRESETS.map((preset) => (
            <Chip key={preset.label} label={preset.label} active={false} onClick={() => applyPreset(preset)} />
          ))}
        </div>
      </div>

      <form onSubmit={onSubmit} className="bg-white border-2 border-sd-ink p-5 sm:p-6 flex flex-col gap-5">
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label htmlFor="stats-metric" className={LABEL}>MEASURE</label>
            <select
              id="stats-metric"
              className={INPUT}
              value={form.metric}
              onChange={(e) => onMetricChange(e.target.value)}
            >
              {options.metrics.map((m) => (
                <option key={m.code} value={m.code}>{m.label}</option>
              ))}
            </select>
          </div>

          <div>
            <label htmlFor="stats-group-by" className={LABEL}>GROUP BY</label>
            <select
              id="stats-group-by"
              className={INPUT}
              value={form.groupBy}
              onChange={(e) => update({ groupBy: e.target.value })}
            >
              {allowedGroupBys.map((g) => (
                <option key={g.code} value={g.code}>{g.label}</option>
              ))}
            </select>
          </div>
        </div>

        {metric?.description && (
          <p className="text-[13px] text-sd-ink/60 -mt-2">{metric.description}</p>
        )}

        {options.series.length > 0 && (
          <fieldset>
            <legend className={LABEL}>SERIES (ANY IF NONE SELECTED)</legend>
            <div className="flex flex-wrap gap-2">
              {options.series.map((name) => (
                <Chip
                  key={name}
                  label={name}
                  active={form.series.includes(name)}
                  onClick={() => update({ series: toggle(form.series, name) })}
                />
              ))}
            </div>
          </fieldset>
        )}

        {options.draftTypes.length > 0 && (
          <fieldset>
            <legend className={LABEL}>DRAFT TYPE (ANY IF NONE SELECTED)</legend>
            <div className="flex flex-wrap gap-2">
              {options.draftTypes.map((name) => (
                <Chip
                  key={name}
                  label={formatDraftType(name)}
                  active={form.draftTypes.includes(name)}
                  onClick={() => update({ draftTypes: toggle(form.draftTypes, name) })}
                />
              ))}
            </div>
          </fieldset>
        )}

        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
          <div>
            <label htmlFor="stats-episode-from" className={LABEL}>
              EPISODE FROM{options.minEpisode !== null ? ` (${options.minEpisode}+)` : ""}
            </label>
            <input
              id="stats-episode-from"
              type="number"
              inputMode="numeric"
              min={options.minEpisode ?? undefined}
              max={options.maxEpisode ?? undefined}
              className={INPUT}
              value={form.episodeFrom}
              onChange={(e) => update({ episodeFrom: e.target.value })}
            />
          </div>

          <div>
            <label htmlFor="stats-episode-to" className={LABEL}>
              EPISODE TO{options.maxEpisode !== null ? ` (${options.maxEpisode})` : ""}
            </label>
            <input
              id="stats-episode-to"
              type="number"
              inputMode="numeric"
              min={options.minEpisode ?? undefined}
              max={options.maxEpisode ?? undefined}
              className={INPUT}
              value={form.episodeTo}
              onChange={(e) => update({ episodeTo: e.target.value })}
            />
          </div>

          {form.groupBy === "drafter" && (
            <div>
              <label htmlFor="stats-min-appearances" className={LABEL}>MIN. DRAFTS</label>
              <input
                id="stats-min-appearances"
                type="number"
                inputMode="numeric"
                min={1}
                className={INPUT}
                value={form.minAppearances}
                onChange={(e) => update({ minAppearances: e.target.value })}
              />
            </div>
          )}

          <div>
            <label htmlFor="stats-sort" className={LABEL}>SORT</label>
            <select
              id="stats-sort"
              className={INPUT}
              value={form.ascending ? "asc" : "desc"}
              onChange={(e) => update({ ascending: e.target.value === "asc" })}
            >
              <option value="desc">Highest first</option>
              <option value="asc">Lowest first</option>
            </select>
          </div>

          <div>
            <label htmlFor="stats-limit" className={LABEL}>ROWS</label>
            <select
              id="stats-limit"
              className={INPUT}
              value={form.limit}
              onChange={(e) => update({ limit: Number(e.target.value) })}
            >
              {LIMITS.map((n) => (
                <option key={n} value={n}>{n}</option>
              ))}
            </select>
          </div>
        </div>

        {options.canIncludeAll && (
          <label className="flex items-center gap-2 text-sm text-sd-ink cursor-pointer">
            <input
              type="checkbox"
              checked={form.includeAll}
              onChange={(e) => update({ includeAll: e.target.checked })}
              className="w-4 h-4 accent-sd-blue"
            />
            Include Patreon and Speed drafts
          </label>
        )}

        <div>
          <button
            type="submit"
            disabled={loading || !form.metric}
            className="min-h-11 px-6 bg-sd-blue text-white font-oswald font-medium text-sm tracking-[0.14em] rounded hover:bg-blue-700 transition-colors disabled:opacity-50"
          >
            {loading ? "RUNNING…" : "RUN QUERY"}
          </button>
        </div>
      </form>

      {error && (
        <p role="alert" className="border-2 border-sd-red bg-white px-4 py-3 text-sm text-sd-red">
          {error}
        </p>
      )}

      {result && <QueryResults result={result} />}
    </div>
  );
}