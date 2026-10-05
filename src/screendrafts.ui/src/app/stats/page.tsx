// app/stats/page.tsx
import type { Metadata } from "next";
import { auth } from "@/auth";
import { RecordBookTotals } from "@/components/features/stats/record-book-totals";
import { RecordSection } from "@/components/features/stats/record-section";
import { ScopeToggle } from "@/components/features/stats/scope-toggle";
import { fetchRecordBook, fetchStatsOptions } from "@/services/stats/fetch-stats";

const DESCRIPTION =
  "Every Screen Drafts record: most appearances, vetoes, overrides, copacetic drafts, and the titles drafted most.";

export const metadata: Metadata = {
  title: "Record Book",
  description: DESCRIPTION,
  openGraph: { title: "Screen Drafts Record Book", description: DESCRIPTION },
};

export const dynamic = "force-dynamic";

type Props = { searchParams: Promise<{ scope?: string }> };

export default async function RecordBookPage({ searchParams }: Props) {
  const { scope } = await searchParams;
  const session = await auth();
  const accessToken = session?.accessToken;

  // The options call tells us whether this caller may include Patreon and Speed drafts. Signed-out
  // visitors always get the canonical Record Book.
  const options = accessToken ? await fetchStatsOptions(accessToken) : null;
  const canIncludeAll = options?.canIncludeAll ?? false;
  const includeAll = canIncludeAll && scope === "all";

  const book = await fetchRecordBook({ includeAll, accessToken });

  if (!book) {
    return (
      <p className="border-2 border-sd-ink bg-white px-5 py-6 text-sm text-sd-ink/70">
        The Record Book is unavailable right now. Try again in a few minutes.
      </p>
    );
  }

  return (
    <div className="flex flex-col gap-8">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <p className="text-sm text-sd-ink/70 max-w-2xl">
          {book.includesNonCanonical
            ? "Every draft, Patreon and Speed included."
            : "Canonical episodes only."}{" "}
          Appearances count once per draft, and a team&apos;s members share credit for its picks.
        </p>
        {canIncludeAll && <ScopeToggle includeAll={includeAll} />}
      </div>

      <RecordBookTotals totals={book.totals} />

      <div>
        {book.sections.map((section) => (
          <RecordSection key={section.key} section={section} />
        ))}
      </div>

      {book.generatedAt && (
        <p className="font-mono text-[10px] tracking-widest text-sd-ink/50">
          UPDATED{" "}
          {new Date(book.generatedAt).toLocaleString("en-US", {
            dateStyle: "medium",
            timeStyle: "short",
            timeZone: "UTC",
          })}{" "}
          UTC
        </p>
      )}
    </div>
  );
}
