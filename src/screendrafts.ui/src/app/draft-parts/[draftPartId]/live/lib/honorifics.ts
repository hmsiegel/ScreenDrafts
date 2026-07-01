// app/draft-parts/[draftPartId]/live/lib/honorifics.ts
//
// Single source of truth for movie honorific names and for turning a
// MovieHonorificEarned payload into the discrete "moments" worth announcing.
//
// Two honorific axes ride the same wire event, independently:
//   • APPEARANCE — a tier (Marquee → Hat Trick → Grand Slam → High Five), a single
//     SmartEnum value 1-4. At most one step per event (up = earned, down = reverted).
//   • POSITION   — a [Flags] bitmask (Unified No. 1, The Cycle). Any number of flags
//     may newly set or clear in one event, so it can yield multiple moments.
//
// A film earns its first named appearance honorific on its 2nd CANONICAL appearance
// (main-feed only — Patreon picks aren't canon and never reach here).

// ── Appearance honorifics ─────────────────────────────────────────────────────

export const MOVIE_APPEARANCE_HONORIFIC_NAMES: Record<number, string> = {
  1: 'Marquee of Fame',
  2: 'Hat Trick',
  3: 'Grand Slam',
  4: 'High Five',
};

const MAX_APPEARANCE_HONORIFIC_VALUE = 4;

/** Name for an appearance honorific by its value (1-4); null below the first tier. */
export function movieAppearanceHonorificNameByValue(value: number): string | null {
  return MOVIE_APPEARANCE_HONORIFIC_NAMES[value] ?? null;
}

/**
 * Name for an appearance honorific by canonical appearance COUNT.
 * Count 2 → Marquee of Fame (value 1) … count 5+ → High Five (value 4).
 * Returns null below 2 appearances (no named honorific yet).
 */
export function movieAppearanceHonorificNameByCount(count: number): string | null {
  if (count < 2) {
    return null;
  }

  const value = Math.min(count - 1, MAX_APPEARANCE_HONORIFIC_VALUE);
  return movieAppearanceHonorificNameByValue(value);
}

// ── Position honorifics ───────────────────────────────────────────────────────
// Bit values must match Reporting's [Flags] MoviePositionHonorific enum:
//   UnifiedNumber1 = 1 << 0, TheCycle = 1 << 1.

const POSITION_HONORIFIC_UNIFIED_NUMBER_1 = 1 << 0;
const POSITION_HONORIFIC_THE_CYCLE = 1 << 1;

interface PositionHonorificFlag {
  value: number;
  name: string;
}

// Display order: rarest/most-coveted last so it announces last when several land
// in one event.
const POSITION_HONORIFIC_FLAGS: readonly PositionHonorificFlag[] = [
  { value: POSITION_HONORIFIC_UNIFIED_NUMBER_1, name: 'Unified No. 1' },
  { value: POSITION_HONORIFIC_THE_CYCLE, name: 'The Cycle' },
];

/** Names of every position honorific currently set in a bitmask (for summaries). */
export function positionHonorificNamesFromMask(mask: number): string[] {
  return POSITION_HONORIFIC_FLAGS.filter((flag) => (mask & flag.value) !== 0).map(
    (flag) => flag.name,
  );
}

// ── Direction + moment derivation ─────────────────────────────────────────────

export type HonorificDirection = 'earned' | 'reverted' | 'none';

/** Direction of an appearance-honorific change from previous to new value. */
export function appearanceHonorificDirection(
  previousValue: number,
  newValue: number,
): HonorificDirection {
  if (newValue > previousValue) {
    return 'earned';
  }

  if (newValue < previousValue) {
    return 'reverted';
  }

  return 'none';
}

export type HonorificAxis = 'appearance' | 'position';

export interface HonorificMoment {
  axis: HonorificAxis;
  // 'earned' = newly achieved (barring a veto); 'reverted' = lost to a veto/override.
  direction: Exclude<HonorificDirection, 'none'>;
  honorificName: string;
  movieTitle: string;
}

// Structurally satisfied by MovieHonorificChangedPayload — kept local so this module
// has no import cycle back into the live-draft context.
export interface HonorificChangeInput {
  movieTitle: string;
  previousAppearanceHonorificValue: number;
  newAppearanceHonorificValue: number;
  previousPositionHonorificValue: number;
  newPositionHonorificValue: number;
}

/**
 * Turns one MovieHonorificEarned payload into the ordered list of moments to
 * announce. Appearance contributes at most one; position contributes one per flag
 * that newly set (earned) or newly cleared (reverted). A position-only event (e.g.
 * a 6th appearance that completes Unified No. 1) yields only the position moment,
 * since the appearance value is unchanged at its cap.
 */
export function deriveHonorificMoments(input: HonorificChangeInput): HonorificMoment[] {
  const moments: HonorificMoment[] = [];

  const appearanceDirection = appearanceHonorificDirection(
    input.previousAppearanceHonorificValue,
    input.newAppearanceHonorificValue,
  );

  if (appearanceDirection !== 'none') {
    const value =
      appearanceDirection === 'earned'
        ? input.newAppearanceHonorificValue
        : input.previousAppearanceHonorificValue;

    const name = movieAppearanceHonorificNameByValue(value);

    if (name) {
      moments.push({
        axis: 'appearance',
        direction: appearanceDirection,
        honorificName: name,
        movieTitle: input.movieTitle,
      });
    }
  }

  const newlySet =
    input.newPositionHonorificValue & ~input.previousPositionHonorificValue;
  const newlyCleared =
    input.previousPositionHonorificValue & ~input.newPositionHonorificValue;

  for (const flag of POSITION_HONORIFIC_FLAGS) {
    if ((newlySet & flag.value) !== 0) {
      moments.push({
        axis: 'position',
        direction: 'earned',
        honorificName: flag.name,
        movieTitle: input.movieTitle,
      });
    } else if ((newlyCleared & flag.value) !== 0) {
      moments.push({
        axis: 'position',
        direction: 'reverted',
        honorificName: flag.name,
        movieTitle: input.movieTitle,
      });
    }
  }

  return moments;
}