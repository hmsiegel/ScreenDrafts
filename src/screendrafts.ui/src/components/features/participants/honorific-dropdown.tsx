'use client';

import { useRouter } from "next/navigation";

const HONORIFIC_OPTIONS = [
  { value: "",  label: "No Honorifics" },
  { value: "1", label: "★ All-Star" },
  { value: "2", label: "★ Hall of Famer" },
  { value: "3", label: "★ MVP" },
  { value: "4", label: "★ Legend" },
];

interface HonorificDropdownProps {
  honorific: string;
  sort: string;
  q?: string;
}

export default function HonorificDropdown({ honorific, sort, q }: HonorificDropdownProps) {
  const router = useRouter();

  function handleChange(value: string) {
    // Selecting an honorific clears the role filter — honorifics are GM-only
    const qs = new URLSearchParams({ sort, page: "1" });
    if (q) qs.set("q", q);
    if (value) qs.set("honorific", value);
    router.push(`?${qs.toString()}`);
  }

  return (
    <div className="flex items-center gap-2">
      <span className="font-mono text-[10px] tracking-widest text-sd-blue shrink-0">
        HONORIFIC
      </span>
      <div className="relative">
        <select
          className={`appearance-none border font-mono text-[11px] px-3 py-1.5 pr-7 focus:outline-none cursor-pointer transition-colors ${
            honorific
              ? "bg-sd-red text-white border-sd-red"
              : "bg-white text-sd-ink border-sd-ink/30 hover:border-sd-ink"
          }`}
          value={honorific}
          onChange={(e) => handleChange(e.target.value)}
        >
          {HONORIFIC_OPTIONS.map((opt) => (
            <option key={opt.value} value={opt.value}>
              {opt.label}
            </option>
          ))}
        </select>
        <div className="pointer-events-none absolute right-2 top-1/2 -translate-y-1/2">
          <svg
            className={`w-3 h-3 ${honorific ? "text-white" : "text-sd-ink"}`}
            fill="none"
            viewBox="0 0 12 12"
            stroke="currentColor"
            strokeWidth={2}
          >
            <path d="M2 4l4 4 4-4" />
          </svg>
        </div>
      </div>
    </div>
  );
}