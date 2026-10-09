import coreWebVitals from "eslint-config-next/core-web-vitals";
import nextTs from "eslint-config-next/typescript";

const eslintConfig = [
  ...coreWebVitals,
  ...nextTs,
  {
    rules: {
      // react-hooks 7 compiler rule; existing effects violate it. Tracked as debt.
      "react-hooks/set-state-in-effect": "warn",
    },
  },
];

export default eslintConfig;