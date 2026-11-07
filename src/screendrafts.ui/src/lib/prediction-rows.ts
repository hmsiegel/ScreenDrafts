// lib/prediction-rows.ts

/**
 * Row layout for prediction results (draft page and draft summary).
 *
 * Ranked predictions (the caller says so, from the part's prediction mode):
 * one row per list slot, highest slot number first (7 → 1), like a Screen
 * Drafts countdown. Unranked entries can still carry an orderIndex, so its
 * presence alone proves nothing.
 *
 * Unranked predictions, top to bottom:
 *   1. Films several contestants predicted, and were right.   (same row)
 *   2. Films only the top scorer got right, each paired with a miss
 *      no one else predicted.
 *   3. Same for the next scorer down.
 *   4. Films several contestants predicted, and were wrong.   (same row)
 *   5. Remaining solo misses, paired row by row.
 *
 * "Top scorer" = most correct entries; ties go to Ryan, then input order.
 * A film every contestant picked always shares a row.
 */

export interface ArrangeableEntry {
  tmdbId?: number | null;
  mediaTitle: string;
  orderIndex?: number | null;
  isCorrect?: boolean | null;
}

export interface ArrangeableSet<E extends ArrangeableEntry> {
  name: string;
  entries: E[];
}

export interface PredictionRow<E> {
  /** List slot for ranked rows; null for unranked rows. */
  rank: number | null;
  /** One cell per input set, in input order. Null when that contestant has no entry on this row. */
  cells: (E | null)[];
}

export interface ArrangedPredictions<E> {
  ranked: boolean;
  rows: PredictionRow<E>[];
}

interface Film<E> {
  order: number;
  byContestant: Map<number, E>;
  correct: boolean;
}

const isRyan = (name: string) => /^ryan\b/i.test(name.trim());

function filmKey(e: ArrangeableEntry): string {
  return e.tmdbId != null ? `id:${e.tmdbId}` : `t:${e.mediaTitle.trim().toLowerCase()}`;
}

export function arrangePredictionRows<E extends ArrangeableEntry>(
  sets: ArrangeableSet<E>[],
  rankedMode: boolean
): ArrangedPredictions<E> {
  const all = sets.flatMap((s) => s.entries);
  if (all.length === 0) return { ranked: false, rows: [] };

  const ranked = rankedMode && all.every((e) => e.orderIndex != null);
  return ranked ? arrangeRanked(sets) : { ranked: false, rows: arrangeUnranked(sets) };
}

// ── Ranked ───────────────────────────────────────────────────────────────────

function arrangeRanked<E extends ArrangeableEntry>(
  sets: ArrangeableSet<E>[]
): ArrangedPredictions<E> {
  const slots = [...new Set(sets.flatMap((s) => s.entries.map((e) => e.orderIndex as number)))].sort(
    (a, b) => b - a
  );

  const rows = slots.map<PredictionRow<E>>((slot) => ({
    rank: slot,
    cells: sets.map((s) => s.entries.find((e) => e.orderIndex === slot) ?? null),
  }));

  return { ranked: true, rows };
}

// ── Unranked ─────────────────────────────────────────────────────────────────

function arrangeUnranked<E extends ArrangeableEntry>(sets: ArrangeableSet<E>[]): PredictionRow<E>[] {
  // Merge entries into films so one title shares one row across contestants.
  const films = new Map<string, Film<E>>();
  sets.forEach((set, c) => {
    set.entries.forEach((entry) => {
      const key = filmKey(entry);
      const film = films.get(key) ?? { order: films.size, byContestant: new Map(), correct: false };
      if (!film.byContestant.has(c)) film.byContestant.set(c, entry);
      if (entry.isCorrect === true) film.correct = true;
      films.set(key, film);
    });
  });

  // Top scorer first; ties go to Ryan, then input order.
  const correctCount = sets.map((s) => s.entries.filter((e) => e.isCorrect === true).length);
  const rankOrder = sets
    .map((_, i) => i)
    .sort(
      (a, b) =>
        correctCount[b] - correctCount[a] ||
        Number(isRyan(sets[b].name)) - Number(isRyan(sets[a].name)) ||
        a - b
    );
  const position = (c: number) => rankOrder.indexOf(c);

  const shared = [...films.values()]
    .filter((f) => f.byContestant.size >= 2)
    .sort(
      (a, b) =>
        b.byContestant.size - a.byContestant.size ||
        Math.min(...[...a.byContestant.keys()].map(position)) -
          Math.min(...[...b.byContestant.keys()].map(position)) ||
        a.order - b.order
    );

  const solos = [...films.values()].filter((f) => f.byContestant.size === 1).sort((a, b) => a.order - b.order);
  const soloCorrect = sets.map((_, c) => solos.filter((f) => f.correct && f.byContestant.has(c)).map(entryOf(c)));
  const soloWrong = sets.map((_, c) => solos.filter((f) => !f.correct && f.byContestant.has(c)).map(entryOf(c)));

  const blank = (): (E | null)[] => sets.map(() => null);
  const rows: PredictionRow<E>[] = [];
  const pushShared = (film: Film<E>) => {
    const cells = blank();
    film.byContestant.forEach((entry, c) => (cells[c] = entry));
    rows.push({ rank: null, cells });
  };

  // 1. Shared hits.
  shared.filter((f) => f.correct).forEach(pushShared);

  // 2–3. Solo hits, top scorer first, each paired with other contestants' solo misses.
  for (const c of rankOrder) {
    while (soloCorrect[c].length > 0) {
      const cells = blank();
      cells[c] = soloCorrect[c].shift()!;
      for (const o of rankOrder) {
        if (o !== c && soloWrong[o].length > 0) cells[o] = soloWrong[o].shift()!;
      }
      rows.push({ rank: null, cells });
    }
  }

  // 4. Shared misses.
  shared.filter((f) => !f.correct).forEach(pushShared);

  // 5. Leftover solo misses, paired.
  while (soloWrong.some((q) => q.length > 0)) {
    rows.push({ rank: null, cells: soloWrong.map((q) => q.shift() ?? null) });
  }

  return rows;
}

function entryOf<E>(c: number) {
  return (f: Film<E>) => f.byContestant.get(c)!;
}
