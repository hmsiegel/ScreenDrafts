namespace ScreenDrafts.Modules.Communications.IntegrationTests.Email;

public sealed class EmailTemplatesGuestDraftInviteTests
{
  private static string Render(
    string recipientName = "Mary Smith",
    string draftTitle = "Friday Night Draft",
    string invitedByName = "Olivia Owner",
    string registerLink = "https://screendrafts.test/register"
  ) => EmailTemplates.GuestDraftInvite(recipientName, draftTitle, invitedByName, registerLink);

  [Fact]
  public void GuestDraftInvite_ShouldContainTheDraftTitleAndInviterName()
  {
    var html = Render(draftTitle: "Friday Night Draft", invitedByName: "Olivia Owner");

    html.Should().Contain("<strong>Friday Night Draft</strong>");
    html.Should().Contain("Olivia Owner");
  }

  [Fact]
  public void GuestDraftInvite_ShouldPutTheRegisterLinkInTheHrefWithAmpersandsEncoded()
  {
    var html = Render(registerLink: "https://screendrafts.test/register?name=Mary%20Jane&email=m%40x.com");

    html
      .Should()
      .Contain("href=\"https://screendrafts.test/register?name=Mary%20Jane&amp;email=m%40x.com\"");
    html.Should().NotContain("name=Mary%20Jane&email=");
  }

  [Fact]
  public void GuestDraftInvite_ShouldEncodeHtmlSpecialCharactersInTitleInviterAndRecipient()
  {
    var html = Render(
      recipientName: "<script>alert('r')</script>",
      draftTitle: "<script>alert('t')</script>",
      invitedByName: "<script>alert('i')</script>"
    );

    html.Should().NotContain("<script>");
    html.Should().Contain("&lt;script&gt;alert(&#39;t&#39;)&lt;/script&gt;");
    html.Should().Contain("&lt;script&gt;alert(&#39;i&#39;)&lt;/script&gt;");
    html.Should().Contain("&lt;script&gt;alert(&#39;r&#39;)&lt;/script&gt;");
  }

  [Fact]
  public void GuestDraftInvite_ShouldEncodeQuotesInTheLinkSoItCannotBreakOutOfTheHref()
  {
    var html = Render(registerLink: "https://screendrafts.test/register?x=\" onmouseover=\"evil()");

    html.Should().NotContain("\" onmouseover=\"");
  }

  [Fact]
  public void GuestDraftInvite_ShouldUseTheInviteFooter()
  {
    Render()
      .Should()
      .Contain("You received this because someone invited you to a Screen Drafts guest draft.");
  }
}
