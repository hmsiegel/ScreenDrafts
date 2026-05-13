const TYPE_COLORS: Record<string, { bg: string; text: string }> = {
  Standard:     { bg: "bg-sd-blue",    text: "text-white" },
  Mega:         { bg: "bg-sd-red",     text: "text-white" },
  Super:        { bg: "bg-sd-ink",     text: "text-white" },
  "Speed Draft":{ bg: "bg-[#6a6f7e]", text: "text-white" },
  "Mini-Mega":  { bg: "bg-sd-red",     text: "text-white" },
  "Mini-Super": { bg: "bg-sd-ink",     text: "text-white" },
};

export default function DraftTypeBadge({ type }: { type: string }) {
  const colors = TYPE_COLORS[type] ?? { bg: "bg-[#6a6f7e]", text: "text-white" };
  return (
    <span
      className={`inline-block font-oswald font-semibold text-[11px] tracking-wide px-2 py-0.5 rounded-[2px] ${colors.bg} ${colors.text}`}
    >
      {type}
    </span>
  );
}