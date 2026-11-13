// app/providers.tsx
'use client'

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { SessionProvider } from "next-auth/react";
import { useState } from "react";

export default function Providers({ children }: { children: React.ReactNode }) {
   const [client] = useState(() => new QueryClient());
   return (
      <SessionProvider>
         <QueryClientProvider client={client}>
            {children}
            {process.env.NODE_ENV === "development" && <ReactQueryDevtools />}
         </QueryClientProvider>
      </SessionProvider>
   );
}