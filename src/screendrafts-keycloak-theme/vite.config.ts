import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import { keycloakify } from "keycloakify/vite-plugin";

export default defineConfig({
    plugins: [
        react(),
        keycloakify({
            accountThemeImplementation: "none",
            themeName: "screendrafts",
            keycloakVersionTargets: {
                "all-other-versions": "keycloak-theme-screendrafts.jar",
                "22-to-25": false,
            },
            environmentVariables: [
                { name: "REGISTER_URL", default: "http://localhost:3005/register" }
            ]
        })
    ]
});
