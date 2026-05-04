import { useState, useLayoutEffect, type JSX } from "react";
import { kcSanitize } from "keycloakify/lib/kcSanitize";
import type { LazyOrNot } from "keycloakify/tools/LazyOrNot";
import type { UserProfileFormFieldsProps } from "keycloakify/login/UserProfileFormFieldsProps";
import type { PageProps } from "keycloakify/login/pages/PageProps";
import type { KcContext } from "../KcContext";
import type { I18n } from "../i18n";
import { ScreenDraftsLogo, GoogleIcon, MicrosoftIcon, GenericProviderIcon } from "../Icons";
import { getKcClsx } from "keycloakify/login/lib/kcClsx";

type RegisterPageProps = PageProps<Extract<KcContext, { pageId: "register.ftl" }>, I18n> & {
    UserProfileFormFields: LazyOrNot<(props: UserProfileFormFieldsProps) => JSX.Element>;
    doMakeUserConfirmPassword: boolean;
};

function SocialIcon({ alias }: { alias: string }) {
    const lower = alias.toLowerCase();
    if (lower.includes("google"))    return <GoogleIcon />;
    if (lower.includes("microsoft") || lower.includes("azure")) return <MicrosoftIcon />;
    return <GenericProviderIcon />;
}

export default function RegisterPage(props: RegisterPageProps) {
    const { kcContext, i18n, classes, UserProfileFormFields, doMakeUserConfirmPassword } = props;

    const { kcClsx } = getKcClsx({
        doUseDefaultCss: false,  // disable all Patternfly defaults
        classes: {
            // Map every keycloakify class key to our sd-* equivalents
            kcFormGroupClass:    "sd-field",
            kcLabelWrapperClass: "",
            kcLabelClass:        "sd-label",
            kcInputWrapperClass: "sd-input-wrap-inner",
            kcInputClass:        "sd-input",
            kcInputErrorMessageClass: "sd-field-error",
            ...classes,
        }
    });

    const { url, messagesPerField, termsAcceptanceRequired } = kcContext;
    const { msg, msgStr } = i18n;

    const [isFormSubmittable, setIsFormSubmittable] = useState(false);
    const [areTermsAccepted, setAreTermsAccepted] = useState(false);

    useLayoutEffect(() => {
        (window as Window & { onSubmitRecaptcha?: () => void })["onSubmitRecaptcha"] = () => {
            (document.getElementById("kc-register-form") as HTMLFormElement | null)?.requestSubmit();
        };
        return () => {
            delete (window as Window & { onSubmitRecaptcha?: () => void })["onSubmitRecaptcha"];
        };
    }, []);

    const globalMessage = (kcContext as KcContext & { message?: { type: string; summary: string } }).message;

    // social providers from kcContext (present on register page when IdPs configured)
    const social = (kcContext as KcContext & { social?: { providers?: Array<{ alias: string; displayName: string; loginUrl: string }> } }).social;
    const hasSocial = social?.providers && social.providers.length > 0;

    return (
        <div className="sd-page">
            {/* Brand */}
            <div className="sd-brand">
                <ScreenDraftsLogo size={80} />
                <div className="sd-brand-wordmark">Screen Drafts</div>
            </div>

            <div className="sd-card">
                <h1 className="sd-card-title">Create Your Account</h1>

                {globalMessage && (
                    <div
                        className={`sd-alert sd-alert-${globalMessage.type}`}
                        dangerouslySetInnerHTML={{ __html: kcSanitize(globalMessage.summary) }}
                    />
                )}

                {/* Social providers */}
                {hasSocial && (
                    <>
                        <ul className="sd-social-list">
                            {social!.providers!.map((p: { alias: string; displayName: string; loginUrl: string; iconClasses?: string }) => (
                                <li key={p.alias}>
                                    <a className="sd-social-btn" href={p.loginUrl}>
                                        <SocialIcon alias={p.alias} />
                                        <span dangerouslySetInnerHTML={{ __html: kcSanitize(p.displayName) }} />
                                    </a>
                                </li>
                            ))}
                        </ul>
                        <div className="sd-divider">or register with email</div>
                    </>
                )}

                {/* Profile fields rendered by keycloakify — styled via kcClsx overrides */}
                <form
                    id="kc-register-form"
                    className="sd-form"
                    action={url.registrationAction}
                    method="post"
                >
                    <UserProfileFormFields
                        kcContext={kcContext}
                        i18n={i18n}
                        kcClsx={kcClsx}
                        onIsFormSubmittableValueChange={setIsFormSubmittable}
                        doMakeUserConfirmPassword={doMakeUserConfirmPassword}
                    />

                    {termsAcceptanceRequired && (
                        <div className="sd-field">
                            <div className="sd-terms-text" style={{ fontSize: 12, color: "var(--sd-text-muted)", marginBottom: 8 }}>
                                {msg("termsTitle")}
                            </div>
                            <label className="sd-remember-label">
                                <input
                                    type="checkbox"
                                    id="termsAccepted"
                                    name="termsAccepted"
                                    checked={areTermsAccepted}
                                    onChange={e => setAreTermsAccepted(e.target.checked)}
                                    aria-invalid={messagesPerField.existsError("termsAccepted")}
                                />
                                {msgStr("acceptTerms")}
                            </label>
                            {messagesPerField.existsError("termsAccepted") && (
                                <span
                                    className="sd-field-error"
                                    dangerouslySetInnerHTML={{ __html: kcSanitize(messagesPerField.get("termsAccepted")) }}
                                />
                            )}
                        </div>
                    )}

                    <button
                        type="submit"
                        className="sd-btn-primary"
                        disabled={!isFormSubmittable || (termsAcceptanceRequired && !areTermsAccepted)}
                    >
                        {msgStr("doRegister")}
                    </button>
                </form>

                {/* Sign in footer */}
                <div className="sd-card-footer">
                    Already have an account?{" "}
                    <a href={url.loginUrl}>{msgStr("doLogIn")}</a>
                </div>
            </div>
        </div>
    );
}
