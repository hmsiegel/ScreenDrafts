"use client";

export interface PositionConfig {
  name: string;
  picks: number[];
  hasBonusVeto: boolean;
  hasBonusVetoOverride: boolean;
}

const POSITION_NAMES = ["A", "B", "C", "D"];

// Fixed position layouts — derived automatically, not editable.
export function getDefaultPositions(draftTypeName: string): PositionConfig[] {
  switch (draftTypeName) {
    case "Standard":
      return [
        { name: "A", picks: [7, 6, 4, 2], hasBonusVeto: false, hasBonusVetoOverride: false },
        { name: "B", picks: [5, 3, 1],    hasBonusVeto: false, hasBonusVetoOverride: false },
      ];
    case "MiniSuper":
      return [
        { name: "A", picks: [5, 3, 1], hasBonusVeto: false, hasBonusVetoOverride: false },
        { name: "B", picks: [4, 2],    hasBonusVeto: false, hasBonusVetoOverride: false },
      ];
    case "SpeedDraft":
      return [
        { name: "A", picks: [7, 5, 3, 1], hasBonusVeto: false, hasBonusVetoOverride: false },
        { name: "B", picks: [6, 4, 2],    hasBonusVeto: false, hasBonusVetoOverride: false },
      ];
    default:
      // Variable — start with two empty positions
      return [
        { name: "A", picks: [], hasBonusVeto: false, hasBonusVetoOverride: false },
        { name: "B", picks: [], hasBonusVeto: false, hasBonusVetoOverride: false },
      ];
  }
}

export function isFixedPositionType(draftTypeName: string | undefined): boolean {
  return ["Standard", "MiniSuper", "SpeedDraft"].includes(draftTypeName ?? "");
}

// ── Validation ────────────────────────────────────────────────────────────

export function validatePositions(
  positions: PositionConfig[],
  totalPicks: number
): string[] {
  const errors: string[] = [];

  for (const pos of positions) {
    if (pos.picks.length === 0) {
      errors.push(`Position ${pos.name}: at least one pick is required.`);
    }
  }

  const allPicks = positions.flatMap((p) => p.picks);
  const seen = new Set<number>();
  const dupes = new Set<number>();
  for (const n of allPicks) {
    if (seen.has(n)) dupes.add(n);
    seen.add(n);
  }
  if (dupes.size > 0) {
    errors.push(`Pick numbers appear in more than one position: ${[...dupes].sort((a, b) => a - b).join(", ")}.`);
  }

  if (allPicks.length !== totalPicks) {
    errors.push(
      `Total picks across all positions is ${allPicks.length}, but max positions is ${totalPicks}.`
    );
  }

  return errors;
}

// ── Fixed summary ─────────────────────────────────────────────────────────

function FixedSummary({ positions }: { positions: PositionConfig[] }) {
  return (
    <div className="text-[11px] font-mono text-sd-ink/50 space-y-0.5">
      {positions.map((p) => (
        <div key={p.name}>
          <span className="text-sd-ink/70 font-bold">{p.name}:</span>{" "}
          picks {p.picks.join(", ")}
        </div>
      ))}
    </div>
  );
}

// ── Editor ────────────────────────────────────────────────────────────────

interface Props {
  positions: PositionConfig[];
  onChange: (positions: PositionConfig[]) => void;
  totalPicks: number;
  readonly?: boolean;
}

const LABEL = "block text-[11px] font-mono tracking-widest text-sd-ink/60 uppercase mb-1";
const INPUT =
  "border border-sd-ink/20 bg-sd-paper px-3 py-2 text-sd-ink font-sans text-sm focus:outline-none focus:ring-2 focus:ring-sd-blue rounded w-full";

export function PositionsEditor({ positions, onChange, totalPicks, readonly }: Props) {
  const errors = validatePositions(positions, totalPicks);

  function updatePicks(idx: number, raw: string) {
    const picks = raw
      .split(",")
      .map((s) => parseInt(s.trim(), 10))
      .filter((n) => !isNaN(n) && n > 0);
    onChange(positions.map((p, i) => (i === idx ? { ...p, picks } : p)));
  }

  function updateBonusVeto(idx: number, value: boolean) {
    onChange(positions.map((p, i) => (i === idx ? { ...p, hasBonusVeto: value } : p)));
  }

  function updateBonusVetoOverride(idx: number, value: boolean) {
    onChange(positions.map((p, i) => (i === idx ? { ...p, hasBonusVetoOverride: value } : p)));
  }

  function addPosition() {
    if (positions.length >= 4) return;
    const name = POSITION_NAMES[positions.length];
    onChange([
      ...positions,
      { name, picks: [], hasBonusVeto: false, hasBonusVetoOverride: false },
    ]);
  }

  function removePosition(idx: number) {
    if (positions.length <= 1) return;
    // Reassign names after removal
    const next = positions
      .filter((_, i) => i !== idx)
      .map((p, i) => ({ ...p, name: POSITION_NAMES[i] }));
    onChange(next);
  }

  if (readonly) {
    return <FixedSummary positions={positions} />;
  }

  return (
    <div className="space-y-3">
      {positions.map((pos, idx) => (
        <div
          key={idx}
          className="border border-sd-ink/10 rounded p-3 bg-white grid grid-cols-[28px_1fr_auto_auto_auto] items-start gap-3"
        >
          {/* Position name */}
          <div className="pt-2 font-oswald font-bold text-[15px] text-sd-ink">
            {pos.name}
          </div>

          {/* Picks */}
          <div>
            <label className={LABEL}>Picks</label>
            <input
              type="text"
              className={INPUT}
              value={pos.picks.join(", ")}
              onChange={(e) => updatePicks(idx, e.target.value)}
              placeholder="e.g. 7, 5, 3, 1"
            />
          </div>

          {/* Bonus veto */}
          <div className="pt-1">
            <label className="flex flex-col items-center gap-1 cursor-pointer select-none">
              <span className="font-mono text-[9px] tracking-widest text-sd-ink/50 uppercase">
                Veto
              </span>
              <input
                type="checkbox"
                checked={pos.hasBonusVeto}
                onChange={(e) => updateBonusVeto(idx, e.target.checked)}
                className="accent-sd-red w-4 h-4"
              />
            </label>
          </div>

          {/* Bonus veto override */}
          <div className="pt-1">
            <label className="flex flex-col items-center gap-1 cursor-pointer select-none">
              <span className="font-mono text-[9px] tracking-widest text-sd-ink/50 uppercase whitespace-nowrap">
                Override
              </span>
              <input
                type="checkbox"
                checked={pos.hasBonusVetoOverride}
                onChange={(e) => updateBonusVetoOverride(idx, e.target.checked)}
                className="accent-sd-blue w-4 h-4"
              />
            </label>
          </div>

          {/* Remove */}
          <div className="pt-1">
            <button
              type="button"
              onClick={() => removePosition(idx)}
              disabled={positions.length <= 1}
              className="text-sd-ink/30 hover:text-sd-red disabled:opacity-0 text-xl leading-none mt-5"
              aria-label={`Remove position ${pos.name}`}
            >
              ×
            </button>
          </div>
        </div>
      ))}

      {positions.length < 4 && (
        <button
          type="button"
          onClick={addPosition}
          className="text-[11px] font-mono tracking-widest text-sd-blue hover:text-sd-ink uppercase"
        >
          + Add Position
        </button>
      )}

      {errors.length > 0 && (
        <div className="space-y-1">
          {errors.map((e, i) => (
            <p key={i} className="text-[11px] font-mono text-sd-red">
              {e}
            </p>
          ))}
        </div>
      )}
    </div>
  );
}