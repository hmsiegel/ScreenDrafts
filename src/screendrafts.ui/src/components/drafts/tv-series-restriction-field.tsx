"use client";

import { useState } from "react";
import { TvSeriesSearchPicker } from "@/components/drafts/tv-series-search-picker";
import type { TvShowSearchResult } from "@/lib/tv-show-resolve";

const LABEL = "block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1";

export interface TvSeriesRestrictionValue {
  tvSeriesTmdbId: number | null;
  tvSeriesTitle: string | null;
}

interface Props {
  accessToken: string;
  value: TvSeriesRestrictionValue;
  onChange: (next: TvSeriesRestrictionValue) => void;
  /**
   * True once the draft is no longer in the Created status. The backend
   * enforces this too (SetTvSeriesRestrictionCommandHandler rejects the
   * change once DraftStatus isn't Created) — this is the UI half, so the
   * person sees why before they hit an error, not instead of the backend
   * check.
   */
  locked?: boolean;
}

/**
 * Shared by create-draft-form.tsx and edit-draft-form.tsx. Checkbox-gated
 * exactly like Use Fungible Token in both of those forms — hidden by
 * default so the common case (an ordinary movie draft) doesn't have to look
 * at a field it'll never use; checking the box reveals the picker the same
 * way checking "Use Fungible Token" reveals the Token Name input. Unchecking
 * clears whatever was selected, mirroring handleToggleFungibleToken's
 * `if (!checked) setFungibleTokenName("")`.
 *
 * The checkbox's initial state is derived once from whether a restriction
 * already exists (checked if editing a draft that already has one, same as
 * useFungibleToken's own `useState(!!draft.fungibleTokenName)` in
 * edit-draft-form.tsx) — not kept in sync with `value` on every render,
 * same tradeoff that pattern already makes elsewhere in this codebase.
 */
export function TvSeriesRestrictionField({ accessToken, value, onChange, locked }: Props) {
  const [enabled, setEnabled] = useState(!!value.tvSeriesTmdbId);

  function handleToggle(checked: boolean) {
    setEnabled(checked);
    if (!checked) {
      onChange({ tvSeriesTmdbId: null, tvSeriesTitle: null });
    }
  }

  function handleSelect(show: TvShowSearchResult) {
    onChange({ tvSeriesTmdbId: show.tmdbId, tvSeriesTitle: show.title });
  }

  function handleClear() {
    onChange({ tvSeriesTmdbId: null, tvSeriesTitle: null });
  }

  return (
    <div>
      <label className="flex items-center gap-2 cursor-pointer select-none mb-2">
        <input
          type="checkbox"
          checked={enabled}
          onChange={(e) => handleToggle(e.target.checked)}
          disabled={locked}
          className="accent-sd-red w-4 h-4"
        />
        <span className="text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase">
          Restrict to TV Series
        </span>
      </label>

      {enabled ? (
        <>
          {locked && (
            <p className="text-[11px] font-mono text-amber-700 bg-amber-50 border border-amber-200 rounded px-3 py-2 mb-2">
              This can only be changed while the draft hasn&apos;t started yet.
            </p>
          )}

          {value.tvSeriesTmdbId ? (
            <div className="flex items-center gap-2 border border-sd-ink/20 bg-sd-paper px-3 py-2">
              <span className="text-sm text-sd-ink flex-1 truncate">
                {value.tvSeriesTitle ?? `TMDb #${value.tvSeriesTmdbId}`}
              </span>
              {!locked && (
                <button
                  type="button"
                  onClick={handleClear}
                  className="text-[11px] font-mono text-sd-blue hover:text-sd-ink uppercase shrink-0"
                >
                  Clear
                </button>
              )}
            </div>
          ) : locked ? (
            <div className="px-3 py-2 bg-sd-ink/5 rounded text-sd-ink/50 text-sm font-mono">
              No restriction
            </div>
          ) : (
            <TvSeriesSearchPicker accessToken={accessToken} onSelect={handleSelect} />
          )}

          <p className="text-[11px] font-mono text-sd-ink/50 mt-1">
            This draft&apos;s pool, boards, and candidate lists will only accept
            episodes of this particular series.
          </p>
        </>
      ) : (
        locked && (
          <p className="text-[11px] text-sd-ink/40 mt-1 font-mono">
            Locked — a part has already started.
          </p>
        )
      )}
    </div>
  );
}