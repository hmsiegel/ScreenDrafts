import AnnouncementBar from "@/components/layout/announcement-bar";
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

export default async function Home() {
  const [latestDrafts, upcomingDrafts, currentStandings, spotlightData, statsData] = await Promise.all([
    fetchLatestDrafts(),
    fetchUpcomingDrafts(),
    fetchCurrentStandings(),
    fetchSpotlight(),
    fetchSiteStats(),
  ]);

  const recentDrafts = latestDrafts.map(mapLatestDraft);
  const upcoming = upcomingDrafts.map(mapUpcomingDraft);
  const standings = mapStandings(currentStandings);
  const spotlight = mapSpotlight(spotlightData);
  const stats = mapSiteStats(statsData);

  return (
    <div className="bg-light-blue min-h-screen font-sans">
      <SpotlightHero spotlight={spotlight} />
      <StatBar stats={stats} />

      <section className="grid grid-cols-3 gap-6 px-8 py-10">
        <RecentDrafts drafts={recentDrafts} />
        <CommissionerStandings standings={standings} />
        <UpcomingDrafts drafts={upcoming} />
      </section>

      <AuthStrip />
    </div>
  );
}
