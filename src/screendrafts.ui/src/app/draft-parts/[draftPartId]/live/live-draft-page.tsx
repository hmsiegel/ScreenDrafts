// app/draft-parts/[draftPartId]/live/live-draft-page.tsx
'use client';

import { LiveDraftProvider, useLiveDraft } from './live-draft-context';
import { useState } from 'react';
import { GetDraftPartGameplayResponse } from '@/lib/dto';
import { PatterDrawer } from './components/patter-drawer';
import { VetoStatusBar } from './components/veto-status-bar';
import { PrimaryHostTab } from './components/tabs/primary-host-tab';
import { CoHostTab } from './components/tabs/co-host-tab';
import { DrafterTab } from './components/tabs/drafter-tab';
import { PredictionsTab } from './components/tabs/predictions-tab';
import { PickNotificationModal } from './components/pick-notification-model';

interface LiveDraftPageProps {
   draftPartId: string;
   accessToken: string;
   initialGameplay: GetDraftPartGameplayResponse;
   userPublicId: string | null;
   isPrimaryHost: boolean;
   isCoHost: boolean;
   isParticipant: boolean;
   isPredictions: boolean;
}

export function LiveDraftPage(props: LiveDraftPageProps) {
   return (
      <LiveDraftProvider
         draftPartId={props.draftPartId}
         accessToken={props.accessToken}
         initialGameplay={props.initialGameplay}
      >
         <LiveDraftPageInner {...props} />
      </LiveDraftProvider>
   );
}

function LiveDraftPageInner({
   accessToken,
   userPublicId,
   isPrimaryHost,
   isCoHost,
   isParticipant,
   isPredictions,
}: LiveDraftPageProps) {
   const { gameplay, reconnecting, notification, dismissNotification } = useLiveDraft();

   // Build ordered tab list
   type TabKey = 'host' | 'cohost' | 'drafter' | 'predictions';
   const tabs: { key: TabKey; label: string }[] = [
      ...(isPrimaryHost ? [{ key: 'host' as TabKey, label: 'PRIMARY HOST' }] : []),
      ...(isCoHost ? [{ key: 'cohost' as TabKey, label: 'CO-HOST' }] : []),
      ...(isParticipant ? [{ key: 'drafter' as TabKey, label: 'DRAFTER' }] : []),
      ...(isPredictions ? [{ key: 'predictions' as TabKey, label: 'PREDICTIONS' }] : []),
   ];

   const [activeTab, setActiveTab] = useState<TabKey>(tabs[0]?.key ?? 'drafter');
   const showPatter = isPrimaryHost || isCoHost;

   return (
      <div className="min-h-screen bg-sd-ink text-sd-paper">
         {/* Reconnecting banner */}
         {reconnecting && (
            <div className="fixed top-0 inset-x-0 z-50 bg-sd-red py-2 text-center text-sm font-oswald tracking-widest text-white">
               RECONNECTING…
            </div>
         )}

         {/* Page header */}
         <div className="border-b border-white/10 px-6 py-4 flex items-center justify-between">
            <div>
               <h1 className="font-oswald text-2xl font-bold tracking-widest text-sd-paper uppercase">
                  {gameplay.draftTitle}
               </h1>
               <p className="text-sm text-white/50 font-mono mt-0.5 uppercase tracking-wider">
                  {gameplay.draftType}
                  {gameplay.isMultiPart && !gameplay.isFinalPart && ' · Part in progress'}
               </p>
            </div>
            {showPatter && <PatterDrawer gameplay={gameplay} />}
         </div>

         {/* Veto status bar — always visible */}
         <VetoStatusBar />

         {/* Tab bar */}
         {tabs.length > 1 && (
            <div className="flex border-b border-white/10">
               {tabs.map((tab) => (
                  <button
                     key={tab.key}
                     onClick={() => setActiveTab(tab.key)}
                     className={`px-6 py-3 font-oswald text-sm tracking-widest transition-colors ${activeTab === tab.key
                           ? 'text-sd-paper border-b-2 border-sd-red'
                           : 'text-white/40 hover:text-white/70'
                        }`}
                  >
                     {tab.label}
                  </button>
               ))}
            </div>
         )}

         {/* Tab content */}
         <div className="p-6">
            {activeTab === 'host' && isPrimaryHost && gameplay.draftPartId && (
               <PrimaryHostTab
                  accessToken={accessToken}
                  draftPartId={gameplay.draftPartId}
                  userPublicId={userPublicId}
               />
            )}
            {activeTab === 'cohost' && isCoHost && (
               <CoHostTab userPublicId={userPublicId} />
            )}
            {activeTab === 'drafter' && isParticipant && gameplay.draftPartId && (
               <DrafterTab
                  accessToken={accessToken}
                  draftPartId={gameplay.draftPartId}
                  userPublicId={userPublicId}
               />
            )}
            {activeTab === 'predictions' && isPredictions && <PredictionsTab />}
         </div>

         {/* Notification modal — rendered outside tab content so it overlays everything */}
         {notification && gameplay.draftPartId && (
            <PickNotificationModal
               notification={notification}
               onDismiss={dismissNotification}
               isPrimaryHost={isPrimaryHost}
               accessToken={accessToken}
               draftPartId={gameplay.draftPartId}
            />
         )}
      </div>
   );
}