import type { ClassKey } from "keycloakify/login/lib/kcClsx";

/**
 * Passed as `classes` alongside doUseDefaultCss={false} for every page that
 * falls through to the default: branch in KcPage.tsx (i.e. everything except
 * login.ftl / register.ftl, which build their own markup). Reuses the sd-
 * classes already defined in screendrafts.css for the login form, so a
 * password-update or verify-email page's inputs and buttons match without a
 * bespoke component per page.
 *
 * Unmapped keys fall back to "" (via doUseDefaultCss={false}), not to
 * Keycloak's PatternFly defaults — only map a key here once you've checked
 * it needs a class at all.
 */
export const classes = {
    kcFormClass: "sd-form",
    kcFormGroupClass: "sd-field",
    kcLabelClass: "sd-label",
    kcInputClass: "sd-input",
    kcInputErrorMessageClass: "sd-field-error",
    kcFormOptionsClass: "sd-form-meta",
    kcButtonClass: "sd-btn-primary",
    kcButtonSecondaryClass: "sd-btn-secondary",
    kcInputClassCheckboxLabel: "sd-remember-label",
    kcFormSocialAccountListClass: "sd-social-list",
    kcFormSocialAccountListButtonClass: "sd-social-btn"
} satisfies Partial<Record<ClassKey, string>>;