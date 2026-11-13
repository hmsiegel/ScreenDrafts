// app/cast-crew/[id]/page.tsx
import { fetchMediaPerson } from "@/services/media/fetch-media-person";
import type {
  MediaPersonCreditGroupResponse,
  MediaPersonCreditResponse,
  MediaPersonProfileResponse,
  MediaType,
} from "@/lib/dto";
import { notFound, permanentRedirect } from "next/navigation";
import Link from "next/link";

type Props = { params: Promise<{ id: string }> };

export const dynamic = "force-dynamic";

// Long filmographies fold after this many rows.
const CREDITS_VISIBLE = 20;

const ROLE_LABELS: Record<string, string> = {
  actor: "ACTOR",
  director: "DIRECTOR",
  writer: "WRITER",
  producer: "PRODUCER",
};

export async function generateMetadata({ params }: Props) {
  const { id } = await params;
  try {
    const person = await fetchMediaPerson(id);
    return { title: person?.name ?? "Cast & Crew" };
  } catch {
    return { title: "Cast & Crew" };
  }
}

function mediaTypeLabel(mediaType: MediaType | undefined): string {
  if (!mediaType) return "";
  const labels: Record<number, string> = {
    0: "MOVIE", 1: "TV SHOW", 2: "TV EPISODE", 3: "VIDEO GAME", 4: "MUSIC VIDEO",
  };
  return labels[mediaType.value] ?? mediaType.name?.toUpperCase() ?? "";
}

function initials(name: string): string {
  return name.split(" ").slice(0, 2).map((w) => w[0]?.toUpperCase() ?? "").join("");
}

// The Vault's person filter identifies people as "t:{tmdbId}" or "i:{imdbId}".
function vaultHref(person: MediaPersonProfileResponse): string {
  const key = person.tmdbId && person.tmdbId > 0 ? `t:${person.tmdbId}` : `i:${person.imdbId ?? ""}`;
  const qs = new URLSearchParams({ personKey: key, personName: person.name ?? "", role: "any" });
  return `/media?${qs.toString()}`;
}

export default async function CastCrewPage({ params }: Props) {
  const { id } = await params;

  let person: MediaPersonProfileResponse | null;
  try {
    person = await fetchMediaPerson(id);
  } catch (err) {
    console.error("[CastCrewPage]", err);
    throw err;
  }

  if (!person) notFound();

  // One canonical URL per person: the TMDb id when there is one, otherwise the IMDb id.
  if (person.id && person.id !== id) {
    permanentRedirect(`/cast-crew/${person.id}`);
  }

  const name = person.name ?? "";
  const groups = (person.groups ?? []).filter((g) => (g.credits?.length ?? 0) > 0);
  const titleCount = person.titleCount ?? 0;

  return (
    <div className="min-h-screen bg-light-blue">
      {/* Hero */}
      <div className="bg-sd-ink page-x pt-10 pb-8 lg:pt-14 lg:pb-11">
        <p className="font-mono text-[11px] tracking-widest text-light-blue mb-3">
          <Link href="/media" className="hover:text-white transition-colors">/ MEDIA</Link>
          <span className="text-white/40"> / </span>
          <span>CAST &amp; CREW</span>
        </p>
        <h1 className="font-oswald font-bold text-[40px] sm:text-[52px] lg:text-[64px] leading-[0.95] text-white mb-4 wrap-anywhere">
          {name.toUpperCase()}
        </h1>
        <p className="font-mono text-[12px] text-white/70 mb-4">
          {titleCount} {titleCount === 1 ? "TITLE" : "TITLES"} IN THE VAULT
        </p>
        <div className="flex flex-wrap gap-2">
          {groups.map((g) => (
            <a
              key={g.role}
              href={`#${g.role}`}
              className="font-mono text-[10px] tracking-widest border border-white/30 text-white/80 px-2.5 py-1 rounded-xs transition-colors hover:border-white hover:text-white"
            >
              {ROLE_LABELS[g.role ?? ""] ?? (g.role ?? "").toUpperCase()} · {g.credits?.length ?? 0}
            </a>
          ))}
        </div>
      </div>

      <div className="h-1 bg-sd-red" />

      <div className="page-x py-6 lg:py-10 max-w-[1400px] mx-auto">
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-[260px_minmax(0,1fr)] lg:gap-10">
          {/* Sidebar */}
          <div className="flex flex-col gap-6">
            <div className="bg-white border-2 border-sd-ink overflow-hidden">
              {person.photoUrl ? (
                <img src={person.photoUrl} alt={name} className="block w-full aspect-2/3 object-cover" />
              ) : (
                <div className="flex w-full aspect-2/3 items-center justify-center bg-sd-paper">
                  <span className="font-oswald font-bold text-[64px] text-sd-ink/30">{initials(name)}</span>
                </div>
              )}
            </div>

            <div className="bg-white border-2 border-sd-ink p-5">
              <h2 className="font-oswald font-bold text-[13px] tracking-widest text-sd-red mb-3">LINKS</h2>
              <div className="flex flex-col gap-1.5">
                <Link
                  href={vaultHref(person)}
                  className="font-oswald text-[15px] text-sd-ink leading-tight transition-colors hover:text-sd-blue"
                >
                  All in the Vault
                </Link>
                {(person.tmdbId ?? 0) > 0 && (
                  <a
                    href={`https://www.themoviedb.org/person/${person.tmdbId}`}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="font-oswald text-[15px] text-sd-ink leading-tight transition-colors hover:text-sd-blue"
                  >
                    TMDb
                  </a>
                )}
                {person.imdbId && (
                  <a
                    href={`https://www.imdb.com/name/${person.imdbId}/`}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="font-oswald text-[15px] text-sd-ink leading-tight transition-colors hover:text-sd-blue"
                  >
                    IMDb
                  </a>
                )}
              </div>
            </div>
          </div>

          {/* Credits */}
          <div className="flex flex-col gap-6">
            {groups.map((group) => (
              <CreditGroup key={group.role} group={group} />
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

function CreditGroup({ group }: { group: MediaPersonCreditGroupResponse }) {
  const role = group.role ?? "";
  const credits = group.credits ?? [];
  const shown = credits.slice(0, CREDITS_VISIBLE);
  const rest = credits.slice(CREDITS_VISIBLE);

  return (
    <section id={role} className="bg-white border-2 border-sd-ink scroll-mt-6">
      <div className="bg-sd-ink px-4 py-4 sm:px-6 sm:py-5 flex items-center gap-3">
        <span className="block w-1 h-4 bg-sd-red shrink-0" />
        <h2 className="font-oswald font-bold text-[13px] tracking-widest text-sd-red">
          {ROLE_LABELS[role] ?? role.toUpperCase()} · {credits.length}
        </h2>
      </div>
      <ul>
        {shown.map((credit) => (
          <CreditRow key={credit.mediaPublicId} credit={credit} />
        ))}
      </ul>
      {rest.length > 0 && (
        <details className="group border-t border-sd-ink/10">
          <summary className="cursor-pointer list-none px-4 py-3 sm:px-6 font-mono text-[11px] text-sd-ink/50 hover:text-sd-blue select-none group-open:hidden">
            + {rest.length} more
          </summary>
          <ul>
            {rest.map((credit) => (
              <CreditRow key={credit.mediaPublicId} credit={credit} />
            ))}
          </ul>
        </details>
      )}
    </section>
  );
}

function CreditRow({ credit }: { credit: MediaPersonCreditResponse }) {
  const title = credit.title ?? "";
  return (
    <li className="border-t border-sd-ink/10 first:border-t-0">
      <Link
        href={`/media/${credit.mediaPublicId}`}
        className="flex items-center gap-4 px-4 py-3 sm:px-6 transition-colors hover:bg-sd-paper"
      >
        {credit.posterUrl ? (
          <img src={credit.posterUrl} alt="" className="h-14 w-10 shrink-0 border border-sd-ink/20 object-cover" />
        ) : (
          <div className="flex h-14 w-10 shrink-0 items-center justify-center border border-sd-ink/20 bg-sd-paper">
            <span className="font-oswald text-[13px] text-sd-ink/40">{initials(title)}</span>
          </div>
        )}
        <span className="min-w-0 flex-1">
          <span className="block truncate font-oswald text-[16px] leading-tight text-sd-ink">{title}</span>
          <span className="font-mono text-[10px] tracking-widest text-sd-ink/50">
            {mediaTypeLabel(credit.mediaType)}
          </span>
        </span>
        {credit.year && (
          <span className="shrink-0 font-mono text-[12px] text-sd-ink/60">{credit.year}</span>
        )}
      </Link>
    </li>
  );
}
