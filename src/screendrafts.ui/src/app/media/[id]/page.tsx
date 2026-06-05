import YouTubeEmbed from "@/components/features/media/youtube-embed";
import { fetchMediaDetail } from "@/services/media/fetch-media-detail";
import { MediaResponse, MediaType, MediaAppearanceResponse, MediaStatsResponse, MediaHonorificResponse } from "@/lib/dto";
import { notFound } from "next/navigation";
import Link from "next/link";

type Props = { params: Promise<{ id: string }> };

export const dynamic = "force-dynamic";

export async function generateMetadata({ params }: Props) {
  const { id } = await params;
  try {
    const media = await fetchMediaDetail(id);
    return { title: media.title ?? "Media" };
  } catch {
    return { title: "Media" };
  }
}

function mediaTypeLabel(mediaType: MediaType | undefined): string {
  if (!mediaType) return "";
  const labels: Record<number, string> = {
    0: "Movie", 1: "TV Show", 2: "TV Episode", 3: "Video Game", 4: "Music Video",
  };
  return labels[mediaType.value] ?? mediaType.name ?? "";
}

function posterInitials(title: string): string {
  return title.split(" ").slice(0, 2).map((w) => w[0]?.toUpperCase() ?? "").join("");
}

// ── Honorific display ─────────────────────────────────────────────────────────

const APPEARANCE_HONORIFIC_LABELS: Record<number, { label: string; color: string }> = {
  1: { label: "MARQUEE OF FAME", color: "bg-sd-blue text-white" },
  2: { label: "HAT TRICK", color: "bg-sd-red text-white" },
  3: { label: "GRAND SLAM", color: "bg-sd-red text-white" },
  4: { label: "HIGH FIVE", color: "bg-sd-ink text-white" },
};

function HonorificBadges({ honorific }: { honorific: MediaHonorificResponse }) {
  const badges: { label: string; color: string }[] = [];

  const app = APPEARANCE_HONORIFIC_LABELS[honorific.appearanceHonorificValue];
  if (app) badges.push(app);
  if (honorific.isUnifiedNo1) badges.push({ label: "UNIFIED NO. 1", color: "bg-sd-blue text-white" });
  if (honorific.isTheCycle) badges.push({ label: "THE CYCLE", color: "bg-sd-ink text-white" });

  if (badges.length === 0) return null;

  return (
    <div className="flex flex-wrap gap-2">
      {badges.map((b) => (
        <span key={b.label} className={`font-mono text-[9px] tracking-widest px-2 py-1 rounded-sm ${b.color}`}>
          {b.label}
        </span>
      ))}
    </div>
  );
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default async function MediaDetailPage({ params }: Props) {
  const { id } = await params;

  let media: MediaResponse;
  try {
    media = await fetchMediaDetail(id);
  } catch {
    notFound();
  }

  const directors = [...new Set(media.directors?.map((d) => d.name).filter(Boolean) as string[] ?? [])];
  const actors = [...new Set(media.actors?.map((a) => a.name).filter(Boolean) as string[] ?? [])];
  const writers = [...new Set(media.writers?.map((w) => w.name).filter(Boolean) as string[] ?? [])];
  const companies = [...new Set(media.productionCompanies?.map((c) => c.name).filter(Boolean) as string[] ?? [])];
  const genres = [...new Set(media.genres?.map((g) => g.name).filter(Boolean) as string[] ?? [])];

  const appearances = (media.mediaAppearances ?? []) as MediaAppearanceResponse[];
  const stats = media.stats as MediaStatsResponse | undefined;
  const honorific = media.honorific as MediaHonorificResponse | undefined;

  return (
    <div className="min-h-screen bg-light-blue">
      {/* Hero */}
      <div className="relative bg-sd-ink overflow-hidden" style={{ minHeight: 360 }}>
        {media.image && (
          <img
            src={media.image}
            alt=""
            className="absolute inset-0 w-full h-full object-cover opacity-25"
          />
        )}
        <div className="relative px-10 py-16">
          <p className="font-mono text-[11px] tracking-widest text-light-blue mb-3">
            <Link href="/media" className="hover:text-white transition-colors">/ MEDIA</Link>
            <span className="text-white/40"> / </span>
            <span>{(media.title ?? "").toUpperCase()}</span>
          </p>

          <h1 className="font-oswald font-bold text-[64px] leading-[0.95] text-white mb-4">
            {(media.title ?? "").toUpperCase()}
          </h1>

          <div className="flex items-center gap-3 flex-wrap mb-4">
            {media.mediaType && (
              <span className="font-mono text-[9px] tracking-widest bg-white/20 text-white px-2 py-1 rounded-sm">
                {mediaTypeLabel(media.mediaType)}
              </span>
            )}
            {media.year && (
              <span className="font-mono text-[12px] text-white/70">{media.year}</span>
            )}
            {genres.map((g, i) => (
              <span key={i} className="font-mono text-[9px] tracking-widest border border-white/30 text-white/60 px-2 py-1 rounded-sm">
                {g}
              </span>
            ))}
          </div>

          {honorific && <HonorificBadges honorific={honorific} />}
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

            {/* Stats summary */}
            {stats && stats.timesPlayed > 0 && (
              <StatsCard stats={stats} honorific={honorific} />
            )}

            {/* Trailer */}
            {media.youTubeTrailer && (
              <div className="bg-white border-2 border-sd-ink overflow-hidden">
                <div className="bg-sd-ink px-5 py-3">
                  <h2 className="font-oswald font-bold text-[13px] tracking-widest text-sd-red">TRAILER</h2>
                </div>
                <YouTubeEmbed url={media.youTubeTrailer} title={`${media.title ?? ""} Trailer`} />
              </div>
            )}

            {directors.length > 0 && <CreditsCard title="DIRECTED BY" credits={directors} />}
            {writers.length > 0 && <CreditsCard title="WRITTEN BY" credits={writers} />}
            {actors.length > 0 && <CreditsCard title="CAST" credits={actors.slice(0, 5)} />}
            {companies.length > 0 && <CreditsCard title="PRODUCTION" credits={companies} />}
          </div>

          {/* ── Main column ── */}
          <div className="flex flex-col gap-8">
            <AppearancesSection appearances={appearances} />
          </div>
        </div>
      </div>

    </div>
  );
}

// ── Poster card ───────────────────────────────────────────────────────────────

function PosterCard({ media }: { media: MediaResponse }) {
  return (
    <div className="bg-white border-2 border-sd-ink overflow-hidden">
      <div className="relative w-full bg-sd-ink overflow-hidden" style={{ aspectRatio: "2 / 3" }}>
        {media.image ? (
          <img src={media.image} alt={media.title ?? ""} className="w-full h-full object-cover" />
        ) : (
          <div className="w-full h-full flex items-center justify-center font-oswald font-bold text-[48px] text-white/30 select-none">
            {posterInitials(media.title ?? "")}
          </div>
        )}
      </div>
      <div className="p-5 flex flex-col gap-4">
        {media.plot && (
          <p className="font-serif text-[15px] italic leading-relaxed text-sd-ink/75">{media.plot}</p>
        )}
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

// ── Honorific banner filenames ────────────────────────────────────────────────

const APPEARANCE_HONORIFIC_BANNERS: Record<number, string> = {
  1: "/artifacts/Marquee_of_Fame_Banner.webp",
  2: "/artifacts/Hat_Trick_Banner.webp",
  3: "/artifacts/Grand_Slam_Banner.webp",
  4: "/artifacts/High_Five_Banner.webp",
};

const POSITION_HONORIFIC_BANNERS: Record<string, string> = {
  unifiedNo1: "/artifacts/Unified_No._1_Banner.webp",
  theCycle: "/artifacts/The_Cycle_Banner.webp",
};

// ── Stats card ────────────────────────────────────────────────────────────────

function StatsCard({
  stats,
  honorific,
}: {
  stats: MediaStatsResponse;
  honorific?: MediaHonorificResponse;
}) {
  const cells = [
    { label: "PLAYED", value: stats.timesPlayed },
    { label: "LANDED", value: stats.landedOnBoard },
    { label: "VETOED", value: stats.timesVetoed },
    { label: "SAVED", value: stats.timesSaved },
  ];

  const appearanceBanner = honorific
    ? APPEARANCE_HONORIFIC_BANNERS[honorific.appearanceHonorificValue]
    : undefined;

  const positionBanners: string[] = [];
  if (honorific?.isUnifiedNo1) positionBanners.push(POSITION_HONORIFIC_BANNERS.unifiedNo1);
  if (honorific?.isTheCycle) positionBanners.push(POSITION_HONORIFIC_BANNERS.theCycle);

  const hasBanners = appearanceBanner || positionBanners.length > 0;

  return (
    <div className="bg-white border-2 border-sd-ink overflow-hidden">
      <div className="bg-sd-ink px-5 py-3">
        <h2 className="font-oswald font-bold text-[13px] tracking-widest text-sd-red">STATS</h2>
      </div>
      <div className="grid grid-cols-2 divide-x divide-y divide-sd-ink/10">
        {cells.map(({ label, value }) => (
          <div key={label} className="p-4 flex flex-col gap-1">
            <span className="font-oswald font-bold text-[28px] text-sd-red leading-none">{value}</span>
            <span className="font-mono text-[9px] tracking-widest text-sd-ink/50">{label}</span>
          </div>
        ))}
      </div>
      {hasBanners && (
        <div className="flex flex-col">
          {appearanceBanner && (
            <img
              src={appearanceBanner}
              alt={honorific?.appearanceHonorificName ?? ""}
              className="w-full object-contain"
            />
          )}
          {positionBanners.map((src) => (
            <img
              key={src}
              src={src}
              alt={src.includes("Unified") ? "Unified No. 1" : "The Cycle"}
              className="w-full object-contain"
            />
          ))}
        </div>
      )}
    </div>
  );
}

// ── Credits card ──────────────────────────────────────────────────────────────

function CreditsCard({ title, credits }: { title: string; credits: string[] }) {
  return (
    <div className="bg-white border-2 border-sd-ink p-5">
      <h2 className="font-oswald font-bold text-[13px] tracking-widest text-sd-red mb-3">{title}</h2>
      <div className="flex flex-col gap-1.5">
        {credits.map((name, i) => (
          <span key={i} className="font-oswald text-[15px] text-sd-ink leading-tight">{name}</span>
        ))}
      </div>
    </div>
  );
}

// ── Draft appearances ─────────────────────────────────────────────────────────

function AppearancesSection({ appearances }: { appearances: MediaAppearanceResponse[] }) {
  return (
    <div className="bg-white border-2 border-sd-ink">
      <div className="bg-sd-ink px-6 py-5 flex items-center gap-3">
        <span className="block w-1 h-4 bg-sd-red shrink-0" />
        <h2 className="font-oswald font-bold text-[13px] tracking-widest text-sd-red">
          DRAFT APPEARANCES
        </h2>
        <span className="text-white/40 font-mono text-[12px]">({appearances.filter((a) => (!a.wasVetoed || a.wasVetoOverridden) && !a.wasCommissionerOverride).length})</span>
      </div>

      {appearances.length === 0 ? (
        <div className="px-6 py-10 text-center font-mono text-[11px] text-sd-ink/40">
          This title has not appeared on a ScreenDrafts board yet.
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

function AppearanceRow({ appearance }: { appearance: MediaAppearanceResponse }) {
  const {
    draftPublicId, draftTitle, episodeNumber,
    pickedByDisplayName, pickedByPersonPublicId,
    position, wasVetoed, wasVetoOverridden, wasCommissionerOverride,
    vetoedByDisplayName, vetoOverrideByDisplayName, isPatreon,
  } = appearance;

  // Landed = not vetoed, OR veto was overridden. Commissioner override = did not land.
  const landed = (!wasVetoed || wasVetoOverridden) && !wasCommissionerOverride;

  return (
    <div className="px-6 py-4 flex items-start gap-4">
      {/* Position badge */}
      <div className={`w-9 h-9 shrink-0 flex items-center justify-center border-2 font-oswald font-bold text-[15px] ${landed ? "border-sd-red text-sd-red" : "border-sd-ink/20 text-sd-ink/30"
        }`}>
        {position ?? "—"}
      </div>

      <div className="flex-1 min-w-0">
        {/* Draft title */}
        <div className="flex items-center gap-2 flex-wrap">
          <Link
            href={`/drafts/${draftPublicId}`}
            className={`font-oswald font-bold text-[18px] leading-tight hover:text-sd-blue transition-colors ${landed ? "text-sd-ink" : "text-sd-ink/40 line-through"}`}
          >
            {draftTitle}
          </Link>
          {isPatreon && (
            <span className="font-mono text-[9px] tracking-widest bg-sd-blue/10 text-sd-blue px-2 py-0.5 rounded-sm">
              PATREON
            </span>
          )}
        </div>

        {/* Meta row */}
        <div className="mt-1 flex flex-wrap items-center gap-x-4 gap-y-1">
          {pickedByPersonPublicId ? (
            <Link href={`/drafters/${pickedByPersonPublicId}`} className={`font-mono text-[11px] tracking-widest hover:text-sd-ink transition-colors ${landed ? "text-[#5a6075]" : "text-sd-ink/30 line-through"}`}>
              PICKED BY {pickedByDisplayName.toUpperCase()}
            </Link>
          ) : (
            <span className={`font-mono text-[11px] tracking-widest ${landed ? "text-[#5a6075]" : "text-sd-ink/30 line-through"}`}>
              PICKED BY {pickedByDisplayName.toUpperCase()}
            </span>
          )}

          {episodeNumber !== null && episodeNumber !== undefined && (
            <span className="font-mono text-[11px] tracking-widest text-[#5a6075]">EP. {episodeNumber}</span>
          )}

          {wasVetoed && !wasVetoOverridden && !wasCommissionerOverride && (
            <span className="font-mono text-[12px] tracking-widest text-sd-red font-bold">
              ✕ VETOED{vetoedByDisplayName ? ` BY ${vetoedByDisplayName.toUpperCase()}` : ""}
            </span>
          )}

          {wasVetoOverridden && (
            <span className="font-mono text-[12px] tracking-widest text-sd-blue">
              ↩ OVERRIDE{vetoOverrideByDisplayName ? ` BY ${vetoOverrideByDisplayName.toUpperCase()}` : ""}
            </span>
          )}

          {wasCommissionerOverride && (
            <span className="font-mono text-[12px] tracking-widest text-sd-blue">
              ★ COMMISSIONER OVERRIDE
            </span>
          )}
        </div>
      </div>
    </div>
  );
}