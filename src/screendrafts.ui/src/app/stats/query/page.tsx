// app/stats/query/page.tsx
import type { Metadata } from "next";
import Link from "next/link";
import { auth } from "@/auth";
import SignInButton from "@/components/layout/header/sign-in-button";
import { QueryBuilder } from "@/components/features/stats/query-builder";
import { fetchStatsOptions } from "@/services/stats/fetch-stats";

const DESCRIPTION =
  "Build your own Screen Drafts stat line: pick a measure, group it by drafter, draft, series or title, and filter it.";

export const metadata: Metadata = {
  title: "Custom Query",
  description: DESCRIPTION,
  openGraph: { title: "Screen Drafts Custom Query", description: DESCRIPTION },
};

export const dynamic = "force-dynamic";

export default async function CustomQueryPage() {
  const session = await auth();

  if (!session?.accessToken || session.error) {
    return (
      <div className="bg-white border-2 border-sd-ink p-6 sm:p-8 max-w-xl">
        <h2 className="font-oswald font-bold text-[13px] tracking-widest text-sd-red mb-3">
          SIGN IN TO BUILD A QUERY
        </h2>
        <p className="text-sm text-sd-ink/70 mb-5">
          Custom queries are for signed-in users. Sign in or create an account to ask your own
          questions of the Record Book, such as who uses the fewest vetoes per draft.
        </p>
        <div className="flex flex-wrap items-center gap-3">
          <SignInButton />
          <Link
            href="/register"
            className="bg-sd-red text-white px-[18px] py-2.5 rounded font-oswald font-medium text-sm tracking-[0.14em] hover:bg-red-700 transition-colors"
          >
            REGISTER
          </Link>
        </div>
      </div>
    );
  }

  const options = await fetchStatsOptions(session.accessToken);

  if (!options) {
    return (
      <p className="border-2 border-sd-ink bg-white px-5 py-6 text-sm text-sd-ink/70">
        Custom queries are unavailable right now. Try again in a few minutes.
      </p>
    );
  }

  return <QueryBuilder options={options} />;
}
