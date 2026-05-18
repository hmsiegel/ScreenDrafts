'use client';

interface EpisodeImageProps {
  title: string;
}

export default function EpisodeImage({ title }: EpisodeImageProps) {
  const encoded = encodeURIComponent(title);

  function tryWebp(e: React.SyntheticEvent<HTMLImageElement>) {
    const img = e.target as HTMLImageElement;
    if (!img.src.endsWith('.webp')) {
      img.src = `/episodes/${encoded}.webp`;
    } else {
      img.src = '/screen-drafts.jpg';
    }
  }

  return (
    <div className="mt-4 mb-4 border border-sd-ink/10 overflow-hidden">
      <img
        src={`/episodes/${encoded}.jpg`}
        alt={title}
        className="w-full object-cover"
        onError={tryWebp}
      />
    </div>
  );
}