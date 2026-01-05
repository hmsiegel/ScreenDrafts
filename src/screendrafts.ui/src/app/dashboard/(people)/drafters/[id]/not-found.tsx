import Link from 'next/link';

export default function NotFound() {
  return (
    <main className="flex h-full flex-col items-center justify-center gap-2">
      <h2 className="text-xl font-semibold">404 Not Found</h2>
      <div className="relative w-full" >
         <iframe src="https://giphy.com/embed/jcxtvm2bsZDH2" className="w-full h-full absolute"  allowFullScreen>
            </iframe>
            </div>
            <p><a href="https://giphy.com/gifs/hair-template-gaps-jcxtvm2bsZDH2">via GIPHY</a></p>
      <Link
        href="/drafts/"
        className="mt-4 rounded-md bg-blue-500 px-4 py-2 text-sm text-white transition-colors hover:bg-blue-400"
      >
        Go Back
      </Link>
    </main>
  );
}