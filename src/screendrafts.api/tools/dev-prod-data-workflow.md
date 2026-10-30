# DEV / PROD data workflow

PROD is canonical. DEV is a disposable copy of it.

- Data flows PROD -> DEV, always, through `tools\Sync-ProdToDev.ps1`.
- Changes flow DEV -> PROD only as migrator scripts, deployed through a PR.
- Anything that emails users (seeding a real episode) happens in PROD.
- DBeaver writes to DEV only. Against PROD it is read-only.

## Fixing data

A data fix is a script in `tools\ScreenDrafts.Tools.DbMigrator\Scripts\<Module>\`, numbered
like any migration and named `NNNN_ef_fix_<what>.sql` (the `_ef_` pass is the one your
existing data fixes, 0054-0056, use). The migrator runs it once per database and records it in the
module's `schema_versions` journal.

1. **Refresh DEV.** `.\tools\Sync-ProdToDev.ps1 -SshTarget <user@host> -AdminEmail <you>`.
   DEV is now PROD as of the dump.
2. **Write the script** in the module's Scripts folder. Next number = highest existing + 1.
3. **Run the migrator against DEV** (the sync script does this at its end; or run it from VS).
   Because DEV holds the PROD journal, only your new script is pending. Read the `RAISE NOTICE`
   counts, open the affected drafts, rerun the audit queries.
4. **Open the PR.** The reviewer reads the SQL. Merge to `main`.
5. **Deploy runs it.** The migrator container runs before the API starts. If the script raises, the
   transaction rolls back, the migrator exits non-zero, the new API never starts, and the old one
   keeps running.

Take a backup first: `.\tools\Sync-ProdToDev.ps1 -SshTarget <user@host> -DumpOnly`.

## Rules for a fix script

- One `START TRANSACTION; ... COMMIT;` around everything (the `_ef_` pass runs without one).
- Idempotent: write only rows that differ, so a second run changes nothing.
- Safe on an empty database. The migrator also builds fresh databases (new dev setups, tests).
- Guard with `RAISE EXCEPTION`: cap how many rows it may change, and assert that each hand-made
  correction matched exactly the rows you meant.
- Pin it to the data that exists when you write it (e.g. `created_at_utc < '<date>'`), so it
  never rewrites episodes seeded later.
- Touches one module's schema only. If it must read or write across schemas, name it
  `NNNN_crossschema_<what>.sql` in `Scripts\CrossSchema` (runs as postgres).
- Never run a fix by hand in PROD. Never edit a script after it has been deployed; add a new one.

## What a refresh does not copy

MongoDB, Redis, RabbitMQ, the Keycloak realm and users, and R2 images. Only your own
account is re-linked to DEV Keycloak. Other users exist as data with no DEV logins.

## Requirements

- DEV Postgres on the same major version as PROD (`postgres:18`). The sync tells you if not.
- Key-based SSH to the VPS. The sync only runs `pg_dump` there.
- A user with your email in DEV Keycloak.
- The dump in `.containers\sync\` holds PROD data. Delete old ones.
