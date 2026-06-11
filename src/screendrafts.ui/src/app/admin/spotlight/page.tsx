import { auth } from '@/auth';
import { redirect } from 'next/navigation';
import SpotlightManager from './spotlight-manager';

export const metadata = { title: 'Spotlight — Admin' };

export default async function AdminSpotlightPage() {
  const session = await auth();
  if (!session?.accessToken) redirect('/login');

 return (
    <div className="max-w-4xl mx-auto px-6 py-10">
      <div className="mb-8 pb-6 border-b border-sd-ink/10">
        <p className="font-mono text-[11px] tracking-widest text-sd-ink/50 mb-6">
          ADMIN / SPOTLIGHT
        </p>
        <h1 className="font-oswald font-bold text-[56px] leading-none text-sd-ink">
          DRAFT SPOTLIGHT
        </h1>
        <p className="text-sm text-sd-ink/100 mt-2 leading-relaxed max-w-lg">
          The active spotlight appears on the home page hero. Only one can be active at a time —
          activating a new one automatically deactivates the current.
        </p>
      </div>

      <SpotlightManager accessToken={session.accessToken} />
    </div>
  );
}