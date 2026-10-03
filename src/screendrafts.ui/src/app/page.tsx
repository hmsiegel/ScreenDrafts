// app/page.tsx
import SpotlightHero from "@/components/features/home/spotlight-hero";
import StatBar from "@/components/features/home/stat-bar";
import RecentDrafts from "@/components/features/home/recent-drafts";
import CommissionerStandings from "@/components/features/home/commissioner-standings";
import UpcomingDrafts from "@/components/features/home/upcoming-drafts";
import AuthStrip from "@/components/features/home/auth-strip";
import {
  fetchLatestDrafts,
  fetchUpcomingDrafts,
  fetchCurrentStandings,
  fetchSpotlight,
  fetchSiteStats,
  mapLatestDraft,
  mapUpcomingDraft,
  mapStandings,
  mapSpotlight,
  mapSiteStats,
} from "@/services/home/fetch-home-data";

export const dynamic = "force-dynamic";

export default async function Home() {
  // None of these throw: a failed fetch comes back empty ([] or null) and only its
  // own section changes — the hero and stat bar disappear, the cards show an empty line.
  const [latestDrafts, upcomingDrafts, currentStandings, spotlightData, statsData] = await Promise.all([
    fetchLatestDrafts(),
    fetchUpcomingDrafts(),
    fetchCurrentStandings(),
    fetchSpotlight(),
    fetchSiteStats(),
  ]);

  const recentDrafts = latestDrafts.map(mapLatestDraft);
  const upcoming = upcomingDrafts.map(mapUpcomingDraft);
  const standings = currentStandings ? mapStandings(currentStandings) : null;
  const spotlight = spotlightData ? mapSpotlight(spotlightData) : null;
  const stats = statsData ? mapSiteStats(statsData) : null;

  return (
    <div className="bg-light-blue min-h-screen font-sans">
      {spotlight && <SpotlightHero spotlight={spotlight} />}
      {stats && <StatBar stats={stats} />}

      {/* Phones: one column. md: two, with Upcoming spanning the second row.
          xl: three across — at lg each card would be ~300px, too narrow for the
          recent-drafts columns. */}
      <section className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6 page-x py-8 lg:py-10">
        <RecentDrafts drafts={recentDrafts} />
        <CommissionerStandings standings={standings} />
        <div className="md:col-span-2 xl:col-span-1 min-w-0 [&>*]:h-full">
          <UpcomingDrafts drafts={upcoming} />
        </div>
      </section>

      <AuthStrip />
    </div>
  );
}