// app/drafters/drafters-infinite-grid.tsx
'use client';

import { useCallback } from "react";
import type { ReactNode } from "react";
import ParticipantCard from "@/components/features/participants/participant-card";
import { WikiSelectCheckbox } from "@/components/features/wiki-export/wiki-export";
import { InfiniteListFooter, useInfiniteList } from "@/components/ui/infinite-list";
import { loadDraftersPage } from "@/app/list-actions";
import type { ParticipantListItem } from "@/lib/dto";
import type { QueryRecord } from "@/lib/list-queries";

interface DraftersInfiniteGridProps {
  /** Must also be passed as this component's React key. */
  cacheKey: string;
  query: QueryRecord;
  initialItems: ParticipantListItem[];
  initialPage: number;
  total: number;
  totalPages: number;
  isAdmin: boolean;
  paginator: ReactNode;
}

const participantKey = (p: ParticipantListItem) => p.personPublicId ?? "";

export function DraftersInfiniteGrid({
  cacheKey,
  query,
  initialItems,
  initialPage,
  total,
  totalPages,
  isAdmin,
  paginator,
}: DraftersInfiniteGridProps) {
  const fetchPage = useCallback((page: number) => loadDraftersPage(query, page), [query]);
  const list = useInfiniteList({
    cacheKey,
    initialItems,
    initialPage,
    total,
    totalPages,
    fetchPage,
    getKey: participantKey,
  });

  return (
    <>
      {/* 1 column on phones, 2 from sm, 3 from lg */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 sm:gap-[22px]">
        {list.items.map((participant, i) => (
          <div key={participant.personPublicId} className="relative min-w-0">
            <ParticipantCard
              participant={participant}
              index={i}
              honorific={participant.honorific ?? null}
            />
            {/* Outside the card link so ticking never navigates; the label widens the touch target. */}
            {isAdmin && participant.drafterPublicId && (
              <label className="absolute bottom-3 right-3 z-10 bg-white/90 p-2 cursor-pointer">
                <WikiSelectCheckbox id={participant.drafterPublicId} />
              </label>
            )}
          </div>
        ))}
      </div>

      {list.items.length === 0 && (
        <div className="text-center font-mono text-sm text-sd-ink/50 py-16">
          No participants found.
        </div>
      )}

      <InfiniteListFooter
        shown={list.items.length}
        total={list.total}
        noun="PARTICIPANTS"
        status={list.status}
        onRetry={list.loadMore}
        sentinelRef={list.sentinelRef}
        paginator={paginator}
        desktopShown={initialItems.length}
      />
    </>
  );
}
