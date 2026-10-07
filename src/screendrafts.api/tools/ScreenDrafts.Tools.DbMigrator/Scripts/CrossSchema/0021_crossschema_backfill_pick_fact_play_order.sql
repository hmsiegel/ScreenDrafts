-- Fills reporting.pick_facts.play_order from drafts.picks.
-- play_order is the order picks were played within a draft part (per sub-draft for a Speed Draft). The title honorific
-- pages use it to order titles that join an honorific in the same episode, as the wiki numbers them.
-- Runs after the Reporting migration that adds the column (default 0) and after the 0020 facts backfill.
-- Idempotent. No explicit transaction: the DbMigrator runner wraps the script.

UPDATE reporting.pick_facts pf
SET play_order = p.play_order
FROM drafts.picks p
WHERE p.id = pf.id
  AND pf.play_order IS DISTINCT FROM p.play_order;