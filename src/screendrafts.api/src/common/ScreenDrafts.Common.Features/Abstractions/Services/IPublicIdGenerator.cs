namespace ScreenDrafts.Common.Features.Abstractions.Services;

public interface IPublicIdGenerator
{
  string GeneratePublicId(string prefix);
}
