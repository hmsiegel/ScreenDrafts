import { useEffect } from "react";
import { clsx } from "keycloakify/tools/clsx";
import { kcSanitize } from "keycloakify/lib/kcSanitize";
import type { TemplateProps } from "keycloakify/login/TemplateProps";
import { getKcClsx } from "keycloakify/login/lib/kcClsx";
import { useSetClassName } from "keycloakify/tools/useSetClassName";
import { useInitialize } from "keycloakify/login/Template.useInitialize";
import { ScreenDraftsLogo } from "./Icons";
import type { I18n } from "./i18n";
import type { KcContext } from "./KcContext";

/**
 * Replaces Keycloakify's stock Template for every page that doesn't have its
 * own bespoke component (login.ftl and register.ftl bypass this entirely —
 * see KcPage.tsx). This is the shell: brand header, card, alert, language
 * switcher. Form elements inside {children} pick up the brand look via the
 * `classes` map passed alongside doUseDefaultCss={false} in KcPage.tsx —
 * this file only owns the chrome around them.
 */
export default function Template(props: TemplateProps<KcContext, I18n>) {
    const {
        displayInfo = false,
        displayMessage = true,
        displayRequiredFields = false,
        headerNode,
        socialProvidersNode = null,
        infoNode = null,
        documentTitle,
        bodyClassName,
        kcContext,
        i18n,
        doUseDefaultCss,
        classes,
        children
    } = props;

    const { kcClsx } = getKcClsx({ doUseDefaultCss, classes });

    const { msg, msgStr, currentLanguage, enabledLanguages } = i18n;

    const { realm, auth, url, message, isAppInitiatedAction } = kcContext;

    useEffect(() => {
        document.title = documentTitle ?? msgStr("loginTitle", realm.displayName || realm.name);
    }, []);

    useSetClassName({
        qualifiedName: "html",
        className: kcClsx("kcHtmlClass")
    });

    useSetClassName({
        qualifiedName: "body",
        className: bodyClassName ?? kcClsx("kcBodyClass")
    });

    const { isReadyToRender } = useInitialize({ kcContext, doUseDefaultCss });

    if (!isReadyToRender) {
        return null;
    }

    return (
        <div className="sd-page">
            <div className="sd-brand">
                <ScreenDraftsLogo size={64} />
                <div className="sd-brand-wordmark">Screen Drafts</div>
            </div>

            <div className="sd-card">
                {enabledLanguages.length > 1 && (
                    <div className="sd-tpl-locale" id="kc-locale">
                        <div id="kc-locale-wrapper">
                            <div id="kc-locale-dropdown" className="menu-button-links">
                                <button
                                    tabIndex={1}
                                    id="kc-current-locale-link"
                                    className="sd-tpl-locale-toggle"
                                    aria-label={msgStr("languages")}
                                    aria-haspopup="true"
                                    aria-expanded="false"
                                    aria-controls="language-switch1"
                                >
                                    {currentLanguage.label}
                                </button>
                                <ul
                                    role="menu"
                                    tabIndex={-1}
                                    aria-labelledby="kc-current-locale-link"
                                    aria-activedescendant=""
                                    id="language-switch1"
                                    className="sd-tpl-locale-list"
                                >
                                    {enabledLanguages.map(({ languageTag, label, href }, i) => (
                                        <li key={languageTag} role="none">
                                            <a role="menuitem" id={`language-${i + 1}`} href={href}>
                                                {label}
                                            </a>
                                        </li>
                                    ))}
                                </ul>
                            </div>
                        </div>
                    </div>
                )}

                {(() => {
                    const node = !(auth !== undefined && auth.showUsername && !auth.showResetCredentials) ? (
                        <h1 id="kc-page-title" className="sd-card-title">
                            {headerNode}
                        </h1>
                    ) : (
                        <div id="kc-username" className="sd-field">
                            <label id="kc-attempted-username" className="sd-label">
                                {auth.attemptedUsername}
                            </label>
                            <a id="reset-login" href={url.loginRestartFlowUrl} aria-label={msgStr("restartLoginTooltip")} className="sd-forgot-link">
                                {msg("restartLoginTooltip")}
                            </a>
                        </div>
                    );

                    if (displayRequiredFields) {
                        return (
                            <div>
                                <div className="sd-tpl-required-note">
                                    <span className="required">*</span> {msg("requiredFields")}
                                </div>
                                {node}
                            </div>
                        );
                    }

                    return node;
                })()}

                <div id="kc-content">
                    <div id="kc-content-wrapper">
                        {displayMessage && message !== undefined && (message.type !== "warning" || !isAppInitiatedAction) && (
                            <div className={clsx("sd-alert", `sd-alert-${message.type}`)}>
                                <span
                                    className="sd-alert-title"
                                    dangerouslySetInnerHTML={{
                                        __html: kcSanitize(message.summary)
                                    }}
                                />
                            </div>
                        )}
                        {children}
                        {auth !== undefined && auth.showTryAnotherWayLink && (
                            <form id="kc-select-try-another-way-form" action={url.loginAction} method="post">
                                <div className="sd-field">
                                    <input type="hidden" name="tryAnotherWay" value="on" />
                                    <a
                                        href="#"
                                        id="try-another-way"
                                        className="sd-forgot-link"
                                        onClick={event => {
                                            document.forms["kc-select-try-another-way-form" as never].requestSubmit();
                                            event.preventDefault();
                                            return false;
                                        }}
                                    >
                                        {msg("doTryAnotherWay")}
                                    </a>
                                </div>
                            </form>
                        )}
                        {socialProvidersNode}
                        {displayInfo && (
                            <div id="kc-info" className="sd-card-footer">
                                <div id="kc-info-wrapper">{infoNode}</div>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}