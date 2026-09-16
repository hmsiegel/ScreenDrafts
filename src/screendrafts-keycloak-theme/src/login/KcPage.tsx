import { Suspense, lazy } from "react";
import type { KcContext } from "./KcContext";
import { useI18n } from "./i18n";
import DefaultPage from "keycloakify/login/DefaultPage";
import Template from "./Template";
import "./screendrafts.css";
import RegisterPage from "./pages/RegisterPage";
import { classes as defaultClasses } from "./KcClasses";

// login.ftl and register.ftl build their own markup and never consult this —
// see LoginPage.tsx / RegisterPage.tsx.
const emptyClasses = {};

const UserProfileFormFields = lazy(
    () => import("keycloakify/login/UserProfileFormFields")
);

const LoginPage = lazy(() => import("./pages/LoginPage"));

const doMakeUserConfirmPassword = true;

export default function KcPage(props: { kcContext: KcContext }) {
    const { kcContext } = props;
    const { i18n } = useI18n({ kcContext });

    return (
        <Suspense>
            {(() => {
                switch (kcContext.pageId) {
                    case "login.ftl":
                        return (
                            <LoginPage
                                kcContext={kcContext}
                                i18n={i18n}
                                classes={emptyClasses}
                                Template={Template}
                                doUseDefaultCss={false}
                            />
                        );
                    case "register.ftl":
                        return (
                            <RegisterPage
                                kcContext={kcContext}
                                i18n={i18n}
                                classes={emptyClasses}
                                Template={Template}
                                doUseDefaultCss={false}
                                UserProfileFormFields={UserProfileFormFields}
                                doMakeUserConfirmPassword={doMakeUserConfirmPassword}
                            />
                        );
                    default:
                        return (
                            <DefaultPage
                                kcContext={kcContext}
                                i18n={i18n}
                                classes={defaultClasses}
                                Template={Template}
                                doUseDefaultCss={false}
                                UserProfileFormFields={UserProfileFormFields}
                                doMakeUserConfirmPassword={doMakeUserConfirmPassword}
                            />
                        );
                }
            })()}
        </Suspense>
    );
}