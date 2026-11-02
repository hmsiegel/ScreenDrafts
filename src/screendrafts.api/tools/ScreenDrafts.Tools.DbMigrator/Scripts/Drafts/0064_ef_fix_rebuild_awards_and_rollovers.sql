-- One-time data fix: award counters and rollovers for every drafter, from the game board.
-- Run after 0059, 0062 and 0063.
--
-- Awards. awarded_vetoes / awarded_veto_overrides on a solo drafter row now equal the number of
-- game-board positions assigned to that drafter that carry has_bonus_veto / has_bonus_veto_override.
-- Many historical rows still carry an old award with no flagged position behind it (the earlier repair
-- only touched drafters who hold a flagged position). The board matches the wiki for Maureen Lee Lenker
-- and Drew McWeeny, so the board is the source. Rows of 'Robert Altman Super Draft' are left as they are
-- (manual awards, including Ryan Marker's restitution). Team and community rows are not touched.
--
-- Rollovers. vetoes_rolling_in and veto_overrides_rolling_in are rebuilt down each drafter's chain of
-- completed solo parts in Global-scope series, or with no series, in release order, with the rule of
-- SetDraftPartStatusCommandHandler.ApplyRolloversAsync: a part rolls 1 out when
-- (starting +) rolling_in + awarded - used >= 1 (overrides: without starting). The first part of a chain
-- keeps its stored value. "Used" is the larger of the counter and the rows the participant issued
-- (vetoes paid from the fungible pool do not count). vetoes_used and starting_vetoes are never changed.
--
-- Idempotent: a second run changes nothing. Raises, and rolls back, if a board count would raise an award
-- (a stored award below its flagged count needs a look first) or a rebuilt value is not 0 or 1.
-- Drafts schema only; Reporting read models are not touched. Run on prod and dev.

START TRANSACTION;

DO $$
DECLARE
  v_award_rows int;
  v_roll_rows  int;
  v_roll_v     int;
  v_roll_o     int;
  v_people     int;
BEGIN
  CREATE TEMP TABLE _award_fix ON COMMIT DROP AS
  SELECT dpp.id AS dpp_id,
       dpp.awarded_vetoes         AS old_v, COALESCE(f.flagged_vetoes, 0)    AS new_v,
       dpp.awarded_veto_overrides AS old_o, COALESCE(f.flagged_overrides, 0) AS new_o
FROM drafts.draft_part_participants dpp
JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
JOIN drafts.drafts d       ON d.id = dp.draft_id
LEFT JOIN (
  SELECT gb.draft_part_id AS part_id, pos.assigned_to_id,
         COUNT(*) FILTER (WHERE pos.has_bonus_veto)          AS flagged_vetoes,
         COUNT(*) FILTER (WHERE pos.has_bonus_veto_override) AS flagged_overrides
  FROM drafts.draft_positions pos
  JOIN drafts.game_boards gb ON gb.id = pos.game_board_id
  WHERE pos.assigned_to_kind = 0 AND pos.assigned_to_id IS NOT NULL
  GROUP BY gb.draft_part_id, pos.assigned_to_id
) f ON f.part_id = dpp.draft_part_id AND f.assigned_to_id = dpp.participant_id_value
WHERE dpp.participant_kind_value = 0
  AND d.title <> 'Robert Altman Super Draft';

  IF EXISTS (SELECT 1 FROM _award_fix WHERE new_v > old_v OR new_o > old_o) THEN
    RAISE EXCEPTION 'Award rebuild: a stored award is below its flagged board count on % row(s); review before running',
      (SELECT COUNT(*) FROM _award_fix WHERE new_v > old_v OR new_o > old_o);
  END IF;

  CREATE TEMP TABLE _chain ON COMMIT DROP AS
  WITH RECURSIVE parts AS (
    SELECT dpp.participant_id_value AS drafter_id, dpp.id AS dpp_id,
           MIN(r.release_date) AS rel, dp.created_at_utc,
           dpp.starting_vetoes AS st,
           dpp.vetoes_rolling_in AS stored_in_v, dpp.veto_overrides_rolling_in AS stored_in_o,
           COALESCE(a.new_v, dpp.awarded_vetoes)         AS aw_v,
           COALESCE(a.new_o, dpp.awarded_veto_overrides) AS aw_o,
           GREATEST(dpp.vetoes_used,
                    (SELECT COUNT(*)::int FROM drafts.vetoes v
                     WHERE v.issued_by_participant_id = dpp.id AND NOT v.spent_from_fungible_pool)) AS used_v,
           GREATEST(dpp.veto_overrides_used,
                    (SELECT COUNT(*)::int FROM drafts.veto_overrides vo
                     WHERE vo.issued_by_participant_id = dpp.id)) AS used_o
    FROM drafts.draft_part_participants dpp
    JOIN drafts.draft_parts dp ON dp.id = dpp.draft_part_id
    JOIN drafts.drafts d       ON d.id = dp.draft_id
    LEFT JOIN drafts.series s  ON s.id = d.series_id
    LEFT JOIN drafts.draft_releases r ON r.part_id = dp.id
    LEFT JOIN _award_fix a ON a.dpp_id = dpp.id
    WHERE dpp.participant_kind_value = 0
      AND dp.status = 3
      AND (s.id IS NULL OR s.continuity_scope = 2) -- 2 = ContinuityScope.Global
    GROUP BY dpp.participant_id_value, dpp.id, dp.created_at_utc, dpp.starting_vetoes,
             dpp.vetoes_rolling_in, dpp.veto_overrides_rolling_in, a.new_v, a.new_o,
             dpp.awarded_vetoes, dpp.awarded_veto_overrides, dpp.vetoes_used, dpp.veto_overrides_used
  ),
  numbered AS (
    SELECT *, ROW_NUMBER() OVER (PARTITION BY drafter_id ORDER BY rel ASC NULLS LAST, created_at_utc ASC) AS rn
    FROM parts
  ),
  chain AS (
    SELECT n.*, n.stored_in_v AS new_in_v, n.stored_in_o AS new_in_o
    FROM numbered n WHERE n.rn = 1
    UNION ALL
    SELECT n.*,
           CASE WHEN c.st + c.new_in_v + c.aw_v - c.used_v >= 1 THEN 1 ELSE 0 END,
           CASE WHEN c.new_in_o + c.aw_o - c.used_o >= 1 THEN 1 ELSE 0 END
    FROM chain c JOIN numbered n ON n.drafter_id = c.drafter_id AND n.rn = c.rn + 1
  )
  SELECT * FROM chain;

  IF NOT EXISTS (SELECT 1 FROM _chain) THEN
    RAISE EXCEPTION 'Award rebuild: no completed Global-scope solo parts found';
  END IF;
  IF EXISTS (SELECT 1 FROM _chain WHERE new_in_v NOT IN (0, 1) OR new_in_o NOT IN (0, 1)) THEN
    RAISE EXCEPTION 'Award rebuild: a rebuilt rolling-in value is outside 0/1';
  END IF;

  UPDATE drafts.draft_part_participants dpp
  SET awarded_vetoes = a.new_v, awarded_veto_overrides = a.new_o
  FROM _award_fix a
  WHERE dpp.id = a.dpp_id AND (dpp.awarded_vetoes <> a.new_v OR dpp.awarded_veto_overrides <> a.new_o);
  GET DIAGNOSTICS v_award_rows = ROW_COUNT;

  UPDATE drafts.draft_part_participants dpp
  SET vetoes_rolling_in = c.new_in_v, veto_overrides_rolling_in = c.new_in_o
  FROM _chain c
  WHERE dpp.id = c.dpp_id
    AND (dpp.vetoes_rolling_in <> c.new_in_v OR dpp.veto_overrides_rolling_in <> c.new_in_o);
  GET DIAGNOSTICS v_roll_rows = ROW_COUNT;

  SELECT COUNT(*) FILTER (WHERE stored_in_v <> new_in_v), COUNT(*) FILTER (WHERE stored_in_o <> new_in_o),
         COUNT(DISTINCT drafter_id) FILTER (WHERE stored_in_v <> new_in_v OR stored_in_o <> new_in_o)
  INTO v_roll_v, v_roll_o, v_people FROM _chain;

  RAISE NOTICE 'Award rebuild: % award row(s) corrected; % rolling-in row(s) updated (veto changes %, override changes %, % drafters)',
    v_award_rows, v_roll_rows, v_roll_v, v_roll_o, v_people;
END $$;

COMMIT;
