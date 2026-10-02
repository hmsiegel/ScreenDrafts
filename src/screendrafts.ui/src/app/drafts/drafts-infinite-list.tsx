// app/drafts/drafts-infinite-list.tsx
'use client';

import { useCallback } from "react";
import type { ReactNode } from "react";
import { DraftsTable } from "@/components/features/drafts/drafts-table";
import { InfiniteListFooter, useInfiniteList } from "@/components/ui/infinite-list";
import { loadDraftsPage } from "@/app/list-actions";
import type { ListDraftsResponse } from "@/lib/dto";
import type { QueryRecord } from "@/lib/list-queries";

interface DraftsInfiniteListProps {
  /** Must also be passed as this component's React key. */
  cacheKey: string;
  query: QueryRecord;
  initialItems: ListDraftsResponse[];
  initialPage: number;
  total: number;
  totalPages: number;
  isAdmin: boolean;
  paginator: ReactNode;
}

const draftKey = (d: ListDraftsResponse) => d.draftPartPublicId ?? d.draftPublicId ?? "";

export function DraftsInfiniteList({
  cacheKey,
  query,
  initialItems,
  initialPage,
  total,
  totalPages,
  isAdmin,
  paginator,
}: DraftsInfiniteListProps) {
  const fetchPage = useCallback((page: number) => loadDraftsPage(query, page), [query]);
  const list = useInfiniteList({
    cacheKey,
    initialItems,
    initialPage,
    total,
    totalPages,
    fetchPage,
    getKey: draftKey,
  });

  return (
    <>
      <DraftsTable drafts={list.items} searchParams={query} isAdmin={isAdmin} />
      <InfiniteListFooter
        shown={list.items.length}
        total={list.total}
        noun="EPISODES"
        status={list.status}
        onRetry={list.loadMore}
        sentinelRef={list.sentinelRef}
        paginator={paginator}
        desktopShown={initialItems.length}
      />
    </>
  );
}
