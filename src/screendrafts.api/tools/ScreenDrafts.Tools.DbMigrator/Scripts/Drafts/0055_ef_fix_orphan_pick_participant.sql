-- One-time data fix: the MCU Super Draft pick at position 1 (pick 6ae8e1ce-c2c6-58bc-94cc-1fd904eb8a23)
-- points played_by_participant_id at a participant row in a different draft part. Its denormalized
-- player (drafter de55fe82-462b-426d-adb4-d36f0c35c220) is correct, so Reporting facts are unaffected;
-- this repairs the foreign key for the Drafts domain model.
-- Updates only when exactly one participant row for that drafter exists in the pick's own part.
-- Otherwise it changes nothing and raises a notice. Run on prod and dev.

DO $$
DECLARE
  v_pick_id    uuid := '6ae8e1ce-c2c6-58bc-94cc-1fd904eb8a23';
  v_drafter_id uuid := 'de55fe82-462b-426d-adb4-d36f0c35c220';
  v_matches    int;
  v_participant_id uuid;
BEGIN
  SELECT COUNT(*), MIN(pp.id::text)::uuid
  INTO v_matches, v_participant_id
  FROM drafts.picks p
  JOIN drafts.draft_part_participants pp
    ON pp.draft_part_id = p.draft_part_id
   AND pp.participant_id_value = v_drafter_id
   AND pp.participant_kind_value = 0
  WHERE p.id = v_pick_id;

  IF v_matches = 1 THEN
    UPDATE drafts.picks
    SET played_by_participant_id = v_participant_id
    WHERE id = v_pick_id;
    RAISE NOTICE 'Pick % repointed to participant %', v_pick_id, v_participant_id;
  ELSE
    RAISE NOTICE 'Pick % not changed: % matching participant rows in its part (expected 1)', v_pick_id, v_matches;
  END IF;
END $$;