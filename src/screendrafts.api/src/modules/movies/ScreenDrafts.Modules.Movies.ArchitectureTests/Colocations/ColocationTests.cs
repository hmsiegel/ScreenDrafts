namespace ScreenDrafts.Modules.Movies.ArchitectureTests.Colocations;

public class ColocationTests : BaseTest
{
  [Theory]
  [MemberData(nameof(GetHandlerAndCommandPairs))]
  public void Handlers_ShouldBeInSameNamespacs_AsTheirCommandOrQuery(Type handlerType, Type commandOrQueryType)
  {
    ArgumentNullException.ThrowIfNull(handlerType);
    ArgumentNullException.ThrowIfNull(commandOrQueryType);

    handlerType.Namespace.Should().Be(
      commandOrQueryType.Namespace,
      $"Handler {handlerType.Name} should be in the same namespace as its command or query {commandOrQueryType.Name}");
  }

  public static TheoryData<Type, Type> GetHandlerAndCommandPairs()
  {
    Type[] handlerInterfaces = [
      typeof(ICommandHandler<>),
      typeof(ICommandHandler<,>),
      typeof(IQueryHandler<,>),
      ];

    var pairs = new TheoryData<Type, Type>();

    IEnumerable<Type> handlers = FeaturesAssembly
      .GetTypes()
      .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false })
      .Where(t => t.DeclaringType is null);

    foreach (var handler in handlers)
    {
      foreach (var iface in handler.GetInterfaces())
      {
        if (!iface.IsGenericType)
        {
          continue;
        }

        var genericDef = iface.GetGenericTypeDefinition();

        if (!handlerInterfaces.Contains(genericDef))
        {
          continue;
        }

        var commandOrQueryType = iface.GetGenericArguments()[0];
        pairs.Add(handler, commandOrQueryType);
      }
    }

    return pairs;
  }
}
