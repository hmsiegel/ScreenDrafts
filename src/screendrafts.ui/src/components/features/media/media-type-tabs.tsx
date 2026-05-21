"use client";

import { useRouter } from "next/navigation";

const TABS = [
  { value: "",  label: "ALL" },
  { value: "0", label: "MOVIES" },
  { value: "1", label: "TV" },
  { value: "3", label: "GAMES" },
];

interface MediaTypeTabsProps {
  mediaType: string;
  sort: string;
  q?: string;
  year?: string;
}

export default function MediaTypeTabs({ mediaType, sort, q, year }: MediaTypeTabsProps) {
  const router = useRouter();

  function handleTab(value: string) {
    const qs = new URLSearchParams({ sort, page: "1" });
    if (value) qs.set("mediaType", value);
    if (q) qs.set("q", q);
    if (year) qs.set("year", year);
    router.push(`?${qs.toString()}`);
  }

  return (
    <div className="flex gap-1">
      {TABS.map((tab) => (
        <button
          key={tab.value}
          onClick={() => handleTab(tab.value)}
          className={`px-4 py-2 font-oswald text-[12px] tracking-wide transition-colors ${
            mediaType === tab.value
              ? "bg-sd-ink text-white"
              : "text-sd-ink hover:bg-sd-ink/5"
          }`}
        >
          {tab.label}
        </button>
      ))}
    </div>
  );
}