
import { getUpcomingDrafts } from "@/features/drafts/api/fetch-drafts";
import SectionWrapper from "./section-wrapper";
import { UpcomingDraftsTableBody } from "./upcoming-draft-table-body";

export default async function UpcomingDrafts() {
   const drafts = await getUpcomingDrafts();

   return (
      <SectionWrapper title="Upcoming Drafts">
         <UpcomingDraftsTableBody drafts={drafts} />
      </SectionWrapper>
   );
}
