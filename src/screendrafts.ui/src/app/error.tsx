// app/error.tsx
'use client';

// Shown in place of any page whose server render throws — e.g. listDrafts when the API
// is down. The site header and footer stay, because this renders inside the root layout.

import { startTransition, useEffect } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";

export default function ErrorPage({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  const router = useRouter();

  useEffect(() => {
    console.error(error);
  }, [error]);

  // reset() alone only re-renders on the client, which would show the same failed
  // server result. refresh() refetches the page's server data first.
  function retry() {
    startTransition(() => {
      router.refresh();
      reset();
    });
  }

  return (
    <div className="min-h-[60vh] bg-light-blue page-x py-16 lg:py-24">
      <div className="max-w-xl mx-auto bg-white border-2 border-sd-ink p-6 sm:p-10">
        <h1 className="font-oswald font-bold text-[28px] sm:text-[36px] leading-tight text-sd-ink">
          This page didn&rsquo;t load
        </h1>
        <p className="mt-3 text-[15px] leading-relaxed text-sd-ink/75">
          The server couldn&rsquo;t fetch what this page needs. Try again in a moment. If it
          keeps happening, the archive itself may be down.
        </p>
        <div className="mt-6 flex flex-wrap gap-3">
          <button
            type="button"
            onClick={retry}
            className="min-h-11 bg-sd-red text-white px-5 font-oswald font-bold tracking-[0.14em] text-sm hover:bg-red-700 transition-colors"
          >
            TRY AGAIN
          </button>
          <Link
            href="/"
            className="min-h-11 inline-flex items-center border-2 border-sd-ink text-sd-ink px-5 font-oswald font-bold tracking-[0.14em] text-sm hover:bg-sd-ink hover:text-white transition-colors"
          >
            GO HOME
          </Link>
        </div>
        {error.digest && (
          <p className="mt-6 font-mono text-[11px] text-sd-ink/40">Reference: {error.digest}</p>
        )}
      </div>
    </div>
  );
}