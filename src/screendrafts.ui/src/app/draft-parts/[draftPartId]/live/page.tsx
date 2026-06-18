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

  const {
    isPrimaryHost = false,
    isCoHost = false,
    isParticipant = false,
    isCommissioner = false
  } = gameplay.currentUserRoles ?? {};

  // isPredictions: true when this co-host is acting as surrogate for the
  // commissioner predictions game. Deferred until predictions feature is built —
  // will come from a separate query or session attribute at that point.
  const isPredictions = false;

  return (
    <LiveDraftPage
      draftPartId={draftPartId}
      accessToken={session.accessToken}
      initialGameplay={gameplay}
      isPrimaryHost={isPrimaryHost}
      isCoHost={isCoHost}
      isParticipant={isParticipant}
      isCommissioner={isCommissioner}
      isPredictions={isPredictions}
    />
  );
}