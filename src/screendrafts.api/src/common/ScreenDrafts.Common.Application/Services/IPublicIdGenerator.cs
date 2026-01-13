namespace ScreenDrafts.Common.Application.Services;

public interface IPublicIdGenerator
{
  string GeneratePublicId(string prefix);
}
