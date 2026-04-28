export default function AnnouncementBar() {
  return (
    <div className="bg-sd-ink text-white px-8 py-2.5 flex items-center justify-between text-xs tracking-[0.18em]">
      <div className="flex gap-4">
        <span>317 EPISODES IN THE BOOKS</span>
        <span className="opacity-60">·</span>
        <span className="text-light-blue">SEASON 4 IN PROGRESS</span>
      </div>
      <div className="flex gap-4">
        <span>RSS</span>
        <span>APPLE</span>
        <span>SPOTIFY</span>
        <span className="text-sd-red">PATREON →</span>
      </div>
    </div>
  );
}
