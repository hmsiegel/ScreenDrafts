# ScreenDrafts Seeder Tooling

This tooling allows you to seed development data into the **ScreenDrafts** PostgreSQL database using the modular seeding apps located in this directory.

Currently, the following modules are available:
- `movies`: Seeds the database with movie data.
- `drafts`: Seeds the database with draft data.

The seeders are containerized using Docker and can be run individually with optional module targets.

---

## Quick Start

Run from the root of the project:

```bash
./tools/seed.sh movies
```
Or from PowerShell:

```powershell
.\tools\seed.ps1 -Seeder movies
```

---

## Available Seeders
| Seeder   | Description                                     |
| -------- | ----------------------------------------------- |
| `movies` | Seeds movie data (genres, people, titles, etc.) |
| `drafts` | Seeds draft-specific data (titles, rules, etc.) |

---

## Available Modules

### Movies Seeder
| Module   | Description                                     |
| -------- | ----------------------------------------------- |
| `genres` | Seeds movie genres.                           |
| `people` | Seeds people data (actors, directors, etc.).   |
| `movies` | Seeds movie titles and their metadata.         |
| `moviesimdb`    | Searches imdb for the specified Ids and returns the titles and their metadata.                  |
| `productioncompanies` | Seeds production companies.                  |
| `moviesandactors` | Seeds the relationship between movies and actors. |
| `moviesanddirectors` | Seeds the relationship between movies and directors. |
| `moviesandproducers` | Seeds the relationship between movies and producers. |
| `moviesandwriters` | Seeds the relationship between movies and writers. |
| `moviesandproductioncompanies` | Seeds the relationship between movies and production companies. |
| `moviesandgenres` | Seeds the relationship between movies and genres. |

### Drafts Seeder
| Module   | Description                                     |
| -------- | ----------------------------------------------- |
| `drafts` | Seeds draft data (this should be `json` file and not `csv`). Should also be run first. |
| `drafters` | Seeds drafter data.                           |
| `hosts` | Seeds host data.                           |
| `draftsdrafters` | Seeds the relationship between drafts and drafters. |
| `draftshosts` | Seeds the relationship between drafts and hosts. |
| `drafterteams` | Seeds drafter teams.                           |
| `drafterteamsdrafters` | Seeds the relationship between drafter teams and drafters. |
| `draftsdrafterteams` | Seeds the relationship between drafts and drafter teams. |
| `draftpositions` | Seeds draft positions.                           |
| `drafterdraftstats` | Seeds the drafter stats. |
| `movies` | Seeds movies data. |
| `picks` | Seeds picks data. |
| `vetoes` | Seeds vetoes data. |
| `vetooverrides` | Seeds veto overrides data. |
| `commessioneroverrides` | Seeds commessioner overrides data. |

---

## Target a Specific Module
To only run a specific module inside a seeder (e.g., `movies`), pass the --module argument:

#### Bash
```bash
./tools/seed.sh drafts --module=drafts
```

#### PowerShell
```powershell
.\tools\seed.ps1 -Seeder drafts -Module drafts
```

This is equivalent to passing --module=genres to the seeder app. Multiple modules can be comma-separated.

---

## Rebuilding the Seeder Container
If you make changes to the seeder code, you may need to rebuild the Docker container. You can do this by running:

```bash
./tools/seed.sh movies --rebuild
```
Or from PowerShell:

```powershell
.\tools\seed.ps1 -Seeder movies -Rebuild
```

---

## Cleanup
By default, the script:
- Starts the PostgreSQL database container if it is not already running.
- Waits for the database to be ready.
- Runs the seeder container with the specified module(s).
- Shuts down and removes all containers after the seeding is complete.

No cleanup action is required after running the script.

---

## Directory Structure
The directory structure for the seeder tooling is as follows:

```
screendrafts.api/
├── data/
├── tools/
├──── seed.sh           # Bash script
├──── seed.ps1          # PowerShell script
├── docker-compose.yml
├── docker-compose.seeding.yml
```

---

## Requirements
- Docker and Docker Compose installed on your machine.
- On Windows, Docker Desktop is recommended for running Linux containers.
  - PowerShell 7+ is recommended for running the PowerShell script.
- On macOS/ Linux
  - Bash is required for running the Bash script.
- Seeder containers expect `.csv` files to be present in the `data` directory. Ensure that the data files are available before running the seeders.

