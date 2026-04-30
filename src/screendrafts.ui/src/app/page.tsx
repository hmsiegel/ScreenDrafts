import AnnouncementBar from "@/components/layout/announcement-bar";
import SiteHeader from "@/components/layout/header/site-header";
import SiteFooter from "@/components/layout/footer/site-footer";
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
  mapLatestDraft,
  mapUpcomingDraft,
  mapStandings,
} from "@/services/home/fetch-home-data";

// ── Mock data (no endpoint yet) ────────────────────────────────────────────

const spotlight = {
  episodeNumber: 264,
  type: 'Super Draft',
  parts: 3,
  subject: 'Martin Scorsese',
  description: `Nine drafters. Twenty-nine theatrical releases (plus No Direction Home, advanced from the Patreon). Multiple veto overrides — including the first-ever override of a Patreon-awarded veto. The Age of Innocence made the largest leap from a vetoed pick in show history.`,
  topFive: [
    { position: 1, title: 'Goodfellas', year: 1990 },
    { position: 2, title: 'Taxi Driver', year: 1976 },
    { position: 3, title: 'Raging Bull', year: 1980 },
    { position: 4, title: 'The Departed', year: 2006 },
    { position: 5, title: 'Casino', year: 1995 },
  ],
  totalPicks: 30,
};

// TODO: wire up when endpoint exists
const stats = [
  { value: '317', label: 'EPISODES PRODUCED' },
  { value: '2,140', label: 'FILMS DRAFTED' },
  { value: '186', label: 'GUEST G.M.s' },
  { value: '418', label: 'VETOES DEPLOYED' },
  { value: '6', label: 'LEGENDS' },
];

// ── Page ───────────────────────────────────────────────────────────────────

export default async function Home() {
  const [latestDrafts, upcomingDrafts, currentStandings] = await Promise.all([
    fetchLatestDrafts(),
    fetchUpcomingDrafts(),
    fetchCurrentStandings(),
  ]);

  const recentDrafts = latestDrafts.map(mapLatestDraft);
  const upcoming = upcomingDrafts.map(mapUpcomingDraft);
  const standings = mapStandings(currentStandings);

  return (
    <div className="bg-light-blue min-h-screen font-sans">
      <AnnouncementBar />
      <SiteHeader />
      <SpotlightHero spotlight={spotlight} />
      <StatBar stats={stats} />

      <section className="grid grid-cols-3 gap-6 px-8 py-10">
        <RecentDrafts drafts={recentDrafts} />
        <CommissionerStandings standings={standings} />
        <UpcomingDrafts drafts={upcoming} />
      </section>

      <AuthStrip />
      <SiteFooter />
    </div>
  );
}
