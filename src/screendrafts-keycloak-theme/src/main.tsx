import { createRoot } from "react-dom/client";
import { getKcContextMock } from "./login/KcPageStory";
import KcPage from "./login/KcPage";

const kcContext = getKcContextMock({
    pageId: "login.ftl"
});

createRoot(document.getElementById("root")!).render(
    <KcPage kcContext={kcContext as any} />
);