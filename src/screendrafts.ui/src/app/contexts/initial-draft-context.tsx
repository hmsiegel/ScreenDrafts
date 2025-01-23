'use client';

import { createContext, Dispatch, SetStateAction, useState } from "react";
import { ReactNode } from "react";

interface InitialDraftContextType {
   initialDraftData: InitialDraftData;
   setInitialDraftData: Dispatch<SetStateAction<InitialDraftData>>;
}

export interface InitialDraftData {
   draftType: string;
   expandedDraftType: string;
   title: string;
   noOfCommissioners: number;
   noOfDrafters: number;
   noOfMovies: number;
}

export const InitialDraftContext = createContext<InitialDraftContextType>({
   initialDraftData: {
      draftType: 'regular',
      expandedDraftType: '',
      title: '',
      noOfCommissioners: 2,
      noOfDrafters: 2,
      noOfMovies: 7,
   },
   setInitialDraftData: () => {},
});

export default function InitialDraftContextProvider({
   children,
}: {
   children: ReactNode;
}) {
   const [initialDraftData, setInitialDraftData] = useState<InitialDraftData>({
      draftType: 'regular',
      expandedDraftType: '',
      title: '',
      noOfCommissioners: 2,
      noOfDrafters: 2,
      noOfMovies: 7,
   });

   return (
      <InitialDraftContext.Provider value={{ initialDraftData, setInitialDraftData }}>
         {children}
      </InitialDraftContext.Provider>
   )
}