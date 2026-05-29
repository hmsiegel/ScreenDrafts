"use client";

interface ProfileAvatarProps {
  displayName: string;
  picturePath?: string;  // full URL already constructed by fetch-profile.ts
}

export default function ProfileAvatar({ displayName, picturePath }: ProfileAvatarProps) {
  const initials = displayName
    .split(" ")
    .slice(0, 2)
    .map((w) => w[0]?.toUpperCase() ?? "")
    .join("");

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
      {picturePath ? (
        <img
          src={picturePath}
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