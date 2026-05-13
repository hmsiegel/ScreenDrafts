'use client';

interface EpisodeImageProps {
   title: string;
}

export default function EpisodeImage({ title }: EpisodeImageProps) {
   return (
      <div className="mt-4 mb-4 border border-sd-ink/10 overflow-hidden">
         <img
            src={`/episodes/${encodeURIComponent(title)}.jpg`}
            alt="{title}"
            className="w-full object-cover"
            onError={(e) => { (e.target as HTMLImageElement).style.display = 'none'; }}
         />
      </div>
   );
}