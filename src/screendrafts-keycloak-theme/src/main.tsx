import { createRoot } from "react-dom/client";
import { StrictMode } from "react";
import KcPage from "./login/KcPage";

// In production (loaded from Keycloak), window.kcContext is populated by the
// FreeMarker template before the bundle runs. During `vite dev` it is not set,
// and we fall back to a mock so the page renders standalone.
async function bootstrap() {
    const kcContext = window.kcContext;

    if (!kcContext) {
        // Dev-only path. Dynamic import keeps the mock out of the production bundle.
        const { getKcContextMock } = await import("./login/KcPageStory");
        const mock = getKcContextMock({ pageId: "login.ftl" });

        createRoot(document.getElementById("root")!).render(
            <StrictMode>
                <KcPage kcContext={mock as any} />
            </StrictMode>
        );
        return;
    }

    createRoot(document.getElementById("root")!).render(
        <StrictMode>
            <KcPage kcContext={kcContext} />
        </StrictMode>
    );
}

bootstrap();