// app/guest-drafts/guest-drafts-realtime-refresher.tsx
'use client';

import { useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import * as signalR from '@microsoft/signalr';

interface Props {
  accessToken: string;
  watchedGuestDraftIds: string[];
}

// Mirrors my-drafts-realtime-refresher.tsx exactly — no per-user group
// exists or is needed; this just joins the flat group (via
// JoinGuestDraftFlatAsync, not JoinGuestDraftAsync — no participantId to
// route reveals with here, and none needed for this purpose) for every
// draft currently sitting in "Created" status, and blanket-refreshes the
// page on DraftStarted so the server refetch moves it to the right section.
export default function GuestDraftsRealtimeRefresher({
  accessToken,
  watchedGuestDraftIds,
}: Props) {
  const router = useRouter();
  const previousTeardownRef = useRef<Promise<void> | null>(null);
  const watchedKey = watchedGuestDraftIds.join(',');

  useEffect(() => {
    if (watchedGuestDraftIds.length === 0) return;

    const hubUrl = `${process.env.NEXT_PUBLIC_API_URL}/drafts/hub`;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => accessToken })
      .withAutomaticReconnect()
      .build();

    connection.on('DraftStarted', () => {
      router.refresh();
    });

    let mounted = true;

    async function start() {
      if (previousTeardownRef.current) {
        await previousTeardownRef.current;
      }
      if (!mounted) return;

      try {
        await connection.start();
        if (!mounted) return;
        await Promise.all(
          watchedGuestDraftIds.map((id) => connection.invoke('JoinGuestDraftFlatAsync', id)),
        );
      } catch {
        // Silent — background convenience refresher, not core functionality.
        // A failed connection just means a manual reload is needed to see a
        // status change, same as before this existed.
      }
    }

    const startPromise = start();

    return () => {
      mounted = false;
      const teardown = startPromise.catch(() => undefined).then(() => connection.stop());
      previousTeardownRef.current = teardown;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [accessToken, watchedKey]);

  return null;
}