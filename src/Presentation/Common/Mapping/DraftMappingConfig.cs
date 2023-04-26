namespace ScreenDrafts.Presentation.Common.Mapping;
public sealed class DraftMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateDraftRequest, CreateDraftCommand>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.DraftType, src => src.DraftType)
            .Map(dest => dest.EpisodeNumber, src => src.EpisodeNumber);
    }
}