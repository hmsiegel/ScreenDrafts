import type { Meta, StoryObj } from "@storybook/react-vite";
import { createKcPageStory } from "../KcPageStory";

const { KcPageStory } = createKcPageStory({ pageId: "login.ftl" });

const meta = {
   title: "login/LoginPage",
   component: KcPageStory
} satisfies Meta<typeof KcPageStory>;

export default meta;

type Story = StoryObj<typeof meta>;

export const Default: Story = {};

export const WithSocialProviders: Story = {
   args: {
      kcContext: {
         social: {
            displayInfo: true,
            providers: [
               { alias: "google", displayName: "Google", loginUrl: "#", iconClasses: "" },
               { alias: "microsoft", displayName: "Microsoft", loginUrl: "#", iconClasses: "" }
            ]
         }
      }
   }
};