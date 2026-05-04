import { createGetKcContextMock } from "keycloakify/login/KcContext/getKcContextMock";
import KcPage from "./KcPage";

const { getKcContextMock } = createGetKcContextMock({
    kcContextExtension: {
        themeName: "screendrafts" as const,
        properties: {}
    },
    kcContextExtensionPerPage: {}
});

export { getKcContextMock };

export function createKcPageStory(params: { pageId: string }) {
    const { pageId } = params;

    function KcPageStory(props: { kcContext?: Record<string, unknown> }) {
        const { kcContext: overrides } = props;

        const kcContext = getKcContextMock({
            pageId: pageId as any,
            overrides: overrides as any
        });

        return <KcPage kcContext={kcContext as any} />;
    }

    return { KcPageStory };
}