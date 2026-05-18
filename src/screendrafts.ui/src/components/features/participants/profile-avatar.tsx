"use client";

export default function ProfileAvatar({ displayName, picturePath }: { displayName: string; picturePath?: string }) {
  const initials = displayName
    .split(" ")
    .slice(0, 2)
    .map((w) => w[0]?.toUpperCase() ?? "")
    .join("");

  // Strip extension so we can try both
  const base = picturePath
    ? `/drafters/${picturePath}`
    : null;

  function showFallback(e: React.SyntheticEvent<HTMLImageElement>) {
    const img = e.target as HTMLImageElement;
    img.style.display = "none";
    const parent = img.parentElement;
    if (parent) {
      parent.innerHTML = `<div class="w-full h-full bg-sd-blue flex items-center justify-center font-oswald font-bold text-[32px] text-white">${initials}</div>`;
    }
  }

  return (
    <div
      className="relative overflow-hidden rounded-full border-4 border-sd-red"
      style={{ width: 96, height: 96 }}
    >
      {base ? (
        <img
          src={base}
          alt={displayName}
          className="w-full h-full object-cover"
          onError={showFallback}
        />
      ) : (
        <div className="w-full h-full bg-sd-blue flex items-center justify-center font-oswald font-bold text-[32px] text-white">
          {initials}
        </div>
      )}
    </div>
  );
}