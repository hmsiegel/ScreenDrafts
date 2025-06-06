
import { getUpcomingDrafts } from "@/app/lib/fetch-drafts";
import SectionWrapper from "../section-wrapper";
import { UpcomingDraftsTableBody } from "./draft-table-body";

export default async function UpcomingDrafts() {
   const drafts = await getUpcomingDrafts();

   return (
      <SectionWrapper title="Upcoming Drafts">
         <UpcomingDraftsTableBody drafts={drafts} />
      </SectionWrapper>
   );
}
