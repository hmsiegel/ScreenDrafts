// app/my-drafts/my-drafts-realtime-refresher.tsx
'use client';

import { useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import * as signalR from '@microsoft/signalr';

interface Props {
  accessToken: string;
  watchedDraftPartIds: string[];
}

export default function MyDraftsRealtimeRefresher({ accessToken, watchedDraftPartIds }: Props) {
  const router = useRouter();
  const previousTeardownRef = useRef<Promise<void> | null>(null);
  // Stable string key so the effect doesn't re-run on every render just
  // because a new array instance was passed with the same contents.
  const watchedKey = watchedDraftPartIds.join(',');

  useEffect(() => {
    if (watchedDraftPartIds.length === 0) return;

    const hubUrl = `${process.env.NEXT_PUBLIC_API_URL}/drafts/hub`;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => accessToken })
      .withAutomaticReconnect()
      .build();

    connection.on('DraftPartStarted', () => {
      router.refresh();
    });

    let mounted = true;

    async function start() {
      // Same Strict-Mode-safe teardown pattern as LiveDraftProvider — wait
      // for any prior connection to fully leave its groups before this one
      // joins, so a broadcast never gets delivered twice.
      if (previousTeardownRef.current) {
        await previousTeardownRef.current;
      }
      if (!mounted) return;

      try {
        await connection.start();
        if (!mounted) return;
        await Promise.all(
          watchedDraftPartIds.map((id) => connection.invoke('JoinDraftPartAsync', id)),
        );
      } catch {
        // Silent — this is a background convenience refresher, not core
        // page functionality. A failed connection just means the person
        // needs a manual reload to see a status change, same as before
        // this existed.
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