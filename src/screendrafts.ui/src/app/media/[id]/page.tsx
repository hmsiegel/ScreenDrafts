import SiteFooter from "@/components/layout/footer/site-footer";
import SiteHeader from "@/components/layout/header/site-header";
import { fetchMediaDetail } from "@/services/media/fetch-media-detail";
import { DraftAppearance, MediaDetail } from "@/lib/media-dto";
import { mediaTypeLabel } from "@/lib/media-type-display";
import { notFound } from "next/navigation";
import Link from "next/link";

const TMDB_IMAGE_BASE_W500 = "https://image.tmdb.org/t/p/w500";
const TMDB_IMAGE_BASE_W1280 = "https://image.tmdb.org/t/p/w1280";

type Props = { params: Promise<{ id: string }> };

export const dynamic = "force-dynamic";

export async function generateMetadata({ params }: Props) {
  const { id } = await params;
  try {
    const media = await fetchMediaDetail(id);
    return { title: media.title };
  } catch {
    return { title: "Media" };
  }
}

function posterInitials(title: string): string {
  return title
    .split(" ")
    .slice(0, 2)
    .map((w) => w[0]?.toUpperCase() ?? "")
    .join("");
}

export default async function MediaDetailPage({ params }: Props) {
  const { id } = await params;

  let media: MediaDetail;
  try {
    media = await fetchMediaDetail(id);
  } catch {
    notFound();
  }

  return (
    <div className="min-h-screen bg-light-blue">
      <SiteHeader activePath="/media" />

      {/* Hero */}
      <div className="relative bg-sd-ink overflow-hidden" style={{ minHeight: 360 }}>
        {media.backdropPath && (
          <img
            src={`${TMDB_IMAGE_BASE_W1280}${media.backdropPath}`}
            alt=""
            className="absolute inset-0 w-full h-full object-cover opacity-25"
          />
        )}
        <div className="relative px-10 py-16">
          <p className="font-mono text-[11px] tracking-widest text-light-blue mb-3">
            <Link href="/media" className="hover:text-white transition-colors">
              / MEDIA
            </Link>
            <span className="text-white/40"> / </span>
            <span>{media.title.toUpperCase()}</span>
          </p>

          <h1 className="font-oswald font-bold text-[64px] leading-[0.95] text-white mb-4">
            {media.title.toUpperCase()}
          </h1>

          <div className="flex items-center gap-3 flex-wrap">
            <span className="font-mono text-[9px] tracking-widest bg-white/20 text-white px-2 py-1 rounded-sm">
              {mediaTypeLabel(media.mediaType)}
            </span>
            {media.year && (
              <span className="font-mono text-[12px] text-white/70">{media.year}</span>
            )}
            {media.genres.map((g) => (
              <span
                key={g}
                className="font-mono text-[9px] tracking-widest border border-white/30 text-white/60 px-2 py-1 rounded-sm"
              >
                {g}
              </span>
            ))}
          </div>
        </div>
      </div>

      {/* Red accent bar */}
      <div className="h-1 bg-sd-red" />

      {/* Content */}
      <div className="px-10 py-10 max-w-[1400px] mx-auto">
        <div className="grid gap-10" style={{ gridTemplateColumns: "300px 1fr" }}>

          {/* ── Sidebar ── */}
          <div className="flex flex-col gap-6">
            <PosterCard media={media} />
            {media.directors.length > 0 && (
              <CreditsCard title="DIRECTED BY" credits={media.directors} />
            )}
            {media.actors.length > 0 && (
              <CreditsCard title="CAST" credits={media.actors.slice(0, 5)} />
            )}
          </div>

          {/* ── Main column ── */}
          <div className="flex flex-col gap-8">
            <AppearancesSection appearances={media.draftAppearances} />
          </div>
        </div>
      </div>

      <SiteFooter />
    </div>
  );
}

// ── Poster card ───────────────────────────────────────────────────────────────

function PosterCard({ media }: { media: MediaDetail }) {
  return (
    <div className="bg-white border-2 border-sd-ink overflow-hidden">
      {/* Poster image */}
      <div className="relative w-full bg-sd-ink overflow-hidden" style={{ aspectRatio: "2 / 3" }}>
        {media.posterPath ? (
          <img
            src={`${TMDB_IMAGE_BASE_W500}${media.posterPath}`}
            alt={media.title}
            className="w-full h-full object-cover"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center font-oswald font-bold text-[48px] text-white/30 select-none">
            {posterInitials(media.title)}
          </div>
        )}
      </div>

      <div className="p-5 flex flex-col gap-4">
        {/* Plot */}
        {media.plot && (
          <p className="font-serif text-[15px] italic leading-relaxed text-sd-ink/75">
            {media.plot}
          </p>
        )}

        {/* IMDb link */}
        {media.imdbId && (
          <a
            href={`https://www.imdb.com/title/${media.imdbId}`}
            target="_blank"
            rel="noopener noreferrer"
            className="font-mono text-[10px] tracking-widest text-sd-blue hover:text-sd-red transition-colors"
          >
            VIEW ON IMDb →
          </a>
        )}
      </div>
    </div>
  );
}

// ── Credits card ──────────────────────────────────────────────────────────────

function CreditsCard({ title, credits }: { title: string; credits: string[] }) {
  return (
    <div className="bg-white border-2 border-sd-ink p-5">
      <h2 className="font-oswald font-bold text-[13px] tracking-widest text-sd-red mb-3">
        {title}
      </h2>
      <div className="flex flex-col gap-1.5">
        {credits.map((name) => (
          <span key={name} className="font-oswald text-[15px] text-sd-ink leading-tight">
            {name}
          </span>
        ))}
      </div>
    </div>
  );
}

// ── Draft appearances ─────────────────────────────────────────────────────────

function AppearancesSection({ appearances }: { appearances: DraftAppearance[] }) {
  return (
    <div className="bg-white border-2 border-sd-ink">
      <div className="bg-sd-ink px-6 py-5 flex items-center gap-3">
        <span className="block w-1 h-4 bg-sd-red shrink-0" />
        <h2 className="font-oswald font-bold text-[13px] tracking-widest text-sd-red">
          DRAFT APPEARANCES
        </h2>
        <span className="text-white/40 font-mono text-[12px]">({appearances.length})</span>
      </div>

      {appearances.length === 0 ? (
        <div className="px-6 py-10 text-center font-mono text-[11px] text-sd-ink/40">
          No draft appearances recorded.
        </div>
      ) : (
        <div className="divide-y divide-sd-ink/5">
          {appearances.map((a, i) => (
            <AppearanceRow key={`${a.draftPublicId}-${i}`} appearance={a} />
          ))}
        </div>
      )}
    </div>
  );
}

function AppearanceRow({ appearance }: { appearance: DraftAppearance }) {
  const { draftPublicId, draftTitle, episodeNumber, pickedBy, position, wasVetoed } = appearance;

  return (
    <div className="px-6 py-4 flex items-center gap-4">
      {/* Position badge */}
      <div
        className={`w-9 h-9 shrink-0 flex items-center justify-center border-2 font-oswald font-bold text-[15px] ${
          wasVetoed
            ? "border-sd-ink/20 text-sd-ink/30"
            : "border-sd-red text-sd-red"
        }`}
      >
        <span className={wasVetoed ? "line-through" : ""}>{position ?? "—"}</span>
      </div>

      <div className="flex-1 min-w-0">
        {/* Draft title */}
        <Link
          href={`/drafts/${draftPublicId}`}
          className="font-oswald font-bold text-[18px] text-sd-ink leading-tight hover:text-sd-blue transition-colors"
        >
          {draftTitle}
        </Link>

        {/* Meta row */}
        <div className="mt-1 flex flex-wrap items-center gap-x-4 gap-y-1">
          <span className="font-mono text-[10px] tracking-widest text-[#5a6075]">
            PICKED BY {pickedBy.toUpperCase()}
          </span>
          {episodeNumber !== null && (
            <span className="font-mono text-[10px] tracking-widest text-[#5a6075]">
              EP. {episodeNumber}
            </span>
          )}
          {wasVetoed && (
            <span className="font-mono text-[11px] tracking-widest text-sd-red font-bold">
              ✕ VETOED
            </span>
          )}
        </div>
      </div>
    </div>
  );
}
