// app/draft-parts/[draftPartId]/live/page.tsx
import { auth } from '@/auth';
import { redirect } from 'next/navigation';
import { fetchGameplay } from './gameplay-fetchers';
import { LiveDraftPage } from './live-draft-page';

interface Props {
  params: Promise<{ draftPartId: string }>;
}

export default async function Page({ params }: Props) {
  const { draftPartId } = await params;

  const session = await auth();
  if (!session?.accessToken) redirect('/');

  const gameplay = await fetchGameplay(session.accessToken, draftPartId);

  // Resolve the current user's participant/host identity from the session publicId
  const userPublicId = session.publicId ?? null;

  // Determine roles for this draft part
  const isPrimaryHost =
    gameplay.hosts?.some((h) => h.isPrimary && h.hostPublicId === userPublicId) ?? false;
  const isCoHost =
    gameplay.hosts?.some((h) => !h.isPrimary && h.hostPublicId === userPublicId) ?? false;
  const isParticipant =
    gameplay.participants?.some((p) => p.participantId === userPublicId) ?? false;

  // Note: commissioner / surrogate detection requires checking against
  // DraftsOptions.CommissionerPersonPublicIds from config — deferred until
  // Predictions tab is built. For now isPredictions = false.
  const isPredictions = false;

  return (
    <LiveDraftPage
      draftPartId={draftPartId}
      accessToken={session.accessToken}
      initialGameplay={gameplay}
      userPublicId={userPublicId}
      isPrimaryHost={isPrimaryHost}
      isCoHost={isCoHost}
      isParticipant={isParticipant}
      isPredictions={isPredictions}
    />
  );
}