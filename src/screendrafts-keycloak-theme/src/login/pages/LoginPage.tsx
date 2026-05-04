import { useState } from "react";
import { kcSanitize } from "keycloakify/lib/kcSanitize";
import { useIsPasswordRevealed } from "keycloakify/tools/useIsPasswordRevealed";
import type { PageProps } from "keycloakify/login/pages/PageProps";
import type { KcContext } from "../KcContext";
import type { I18n } from "../i18n";
import { ScreenDraftsLogo, GoogleIcon, MicrosoftIcon, GenericProviderIcon, EyeOpenIcon, EyeClosedIcon } from "../Icons";

/** Map Keycloak provider aliases to branded icon components */
function SocialIcon({ alias }: { alias: string }) {
    const lower = alias.toLowerCase();
    if (lower.includes("google")) return <GoogleIcon />;
    if (lower.includes("microsoft") || lower.includes("azure")) return <MicrosoftIcon />;
    return <GenericProviderIcon />;
}

export default function LoginPage(
    props: PageProps<Extract<KcContext, { pageId: "login.ftl" }>, I18n>
) {
    const { kcContext, i18n } = props;
    const { realm, url, usernameHidden, login, auth, messagesPerField } = kcContext;
    const { msgStr } = i18n;

    const [isLoginButtonDisabled, setIsLoginButtonDisabled] = useState(false);
    const { isPasswordRevealed, toggleIsPasswordRevealed } = useIsPasswordRevealed({ passwordInputId: "password" });

    const hasError = messagesPerField.existsError("username", "password");
    const globalMessage = (kcContext as any).message;

    const rawProviders: Array<{ alias: string; displayName: string; loginUrl: string }> =
        (window as any).kcContext?.social?.providers ?? [];
    const hasSocial = rawProviders.length > 0;

    const registerUrl = (window as any).kcContext?.properties?.REGISTER_URL ?? "http://localhost:3005/register";

    console.log("registerUrl:", registerUrl);
    console.log("properties:", JSON.stringify(kcContext.properties));

    return (
        <div className="sd-page">
            {/* Brand */}
            <div className="sd-brand">
                <ScreenDraftsLogo size={80} />
                <div className="sd-brand-wordmark">Screen Drafts</div>
            </div>

            {/* Card */}
            <div className="sd-card">
                <h1 className="sd-card-title">Sign In to Your Account</h1>

                {/* Global flash message */}
                {globalMessage && (
                    <div className={`sd-alert sd-alert-${globalMessage.type}`}
                        dangerouslySetInnerHTML={{ __html: kcSanitize(globalMessage.summary) }}
                    />
                )}

                {/* Social providers — shown first for prominence */}
                {hasSocial && (
                    <>
                        <ul className="sd-social-list">
                            {rawProviders.map(p => (
                                <li key={p.alias}>
                                    <a className="sd-social-btn" href={p.loginUrl}>
                                        <SocialIcon alias={p.alias} />
                                        <span dangerouslySetInnerHTML={{ __html: kcSanitize(p.displayName) }} />
                                    </a>
                                </li>
                            ))}
                        </ul>

                        {realm.password && (
                            <div className="sd-divider">or sign in with email</div>
                        )}
                    </>
                )}

                {/* Email / password form */}
                {realm.password && (
                    <form
                        id="kc-form-login"
                        className="sd-form"
                        onSubmit={() => { setIsLoginButtonDisabled(true); return true; }}
                        action={url.loginAction}
                        method="post"
                    >
                        {!usernameHidden && (
                            <div className="sd-field">
                                <label htmlFor="username" className="sd-label">
                                    {!realm.loginWithEmailAllowed
                                        ? msgStr("username")
                                        : !realm.registrationEmailAsUsername
                                            ? msgStr("usernameOrEmail")
                                            : msgStr("email")}
                                </label>
                                <input
                                    id="username"
                                    className="sd-input"
                                    name="username"
                                    defaultValue={login.username ?? ""}
                                    type="text"
                                    autoFocus
                                    autoComplete="username"
                                    aria-invalid={hasError}
                                />
                                {hasError && !usernameHidden && (
                                    <span className="sd-field-error"
                                        dangerouslySetInnerHTML={{ __html: kcSanitize(messagesPerField.getFirstError("username", "password")) }}
                                    />
                                )}
                            </div>
                        )}

                        <div className="sd-field">
                            <label htmlFor="password" className="sd-label">{msgStr("password")}</label>
                            <div className="sd-input-wrap">
                                <input
                                    id="password"
                                    className="sd-input sd-input-with-toggle"
                                    name="password"
                                    type={isPasswordRevealed ? "text" : "password"}
                                    autoComplete="current-password"
                                    aria-invalid={hasError}
                                />
                                <button
                                    type="button"
                                    className="sd-pwd-toggle"
                                    aria-label={msgStr(isPasswordRevealed ? "hidePassword" : "showPassword")}
                                    aria-controls="password"
                                    onClick={toggleIsPasswordRevealed}
                                >
                                    {isPasswordRevealed ? <EyeClosedIcon /> : <EyeOpenIcon />}
                                </button>
                            </div>
                            {usernameHidden && hasError && (
                                <span className="sd-field-error"
                                    dangerouslySetInnerHTML={{ __html: kcSanitize(messagesPerField.getFirstError("username", "password")) }}
                                />
                            )}
                        </div>

                        {/* Remember me + forgot password */}
                        <div className="sd-form-meta">
                            {realm.rememberMe && !usernameHidden ? (
                                <label className="sd-remember-label">
                                    <input
                                        id="rememberMe"
                                        name="rememberMe"
                                        type="checkbox"
                                        defaultChecked={!!login.rememberMe}
                                    />
                                    {msgStr("rememberMe")}
                                </label>
                            ) : <span />}

                            {realm.resetPasswordAllowed && (
                                <a className="sd-forgot-link" href={url.loginResetCredentialsUrl}>
                                    {msgStr("doForgotPassword")}
                                </a>
                            )}
                        </div>

                        <input type="hidden" id="id-hidden-input" name="credentialId" value={auth.selectedCredential} />

                        <button
                            type="submit"
                            className="sd-btn-primary"
                            name="login"
                            id="kc-login"
                            disabled={isLoginButtonDisabled}
                        >
                            {msgStr("doLogIn")}
                        </button>
                    </form>
                )}

                {/* Register footer */}
                <div className="sd-card-footer">
                    {msgStr("noAccount")}{" "}
                    <a href={registerUrl}>{msgStr("doRegister")}</a>
                </div>
            </div>
        </div>
    );
}
