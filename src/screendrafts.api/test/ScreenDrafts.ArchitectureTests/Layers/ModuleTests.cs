namespace ScreenDrafts.ArchitectureTests.Layers;

public class ModuleTests : BaseTest
{
  [Fact]
  public void AdministrationModule_ShouldNotHaveDependenciesOn_OtherModules()
  {
    string[] otherModules =
    [
      AuditNamespace,
      CommunicationsNamespace,
      DraftsNamespace,
      IntegrationsNamespace,
      MoviesNamespace,
      RealTimeUpdatesNamespace,
      ReportingNamespace,
      UsersNamespace
    ];

    string[] integrationEventsModules =
    [
      AuditIntegrationEventsNamespace,
      CommunicationsIntegrationEventsNamespace,
      DraftsIntegrationEventsNamespace,
      IntegrationsIntegrationEventsNamespace,
      MoviesIntegrationEventsNamespace,
      RealTimeUpdatesIntegrationEventsNamespace,
      ReportingIntegrationEventsNamespace,
      UsersIntegrationEventsNamespace
    ];

    List<Assembly> administrationAssemblies =
      [
      Modules.Administration.Application.AssemblyReference.Assembly,
      Modules.Administration.Presentation.AssemblyReference.Assembly,
      typeof(AdministrationModule).Assembly
      ];

    Types.InAssemblies(administrationAssemblies)
      .That()
      .DoNotHaveDependencyOnAny(integrationEventsModules)
      .Should()
      .NotHaveDependencyOnAny(otherModules)
      .GetResult()
      .ShouldBeSuccessful();
    }

  [Fact]
  public void AuditModule_ShouldNotHaveDependenciesOn_OtherModules()
  {
    string[] otherModules =
    [
      AdministrationNamespace,
      CommunicationsNamespace,
      DraftsNamespace,
      IntegrationsNamespace,
      MoviesNamespace,
      RealTimeUpdatesNamespace,
      ReportingNamespace,
      UsersNamespace
    ];

    string[] integrationEventsModules =
    [
      AdministrationIntegrationEventsNamespace,
      CommunicationsIntegrationEventsNamespace,
      DraftsIntegrationEventsNamespace,
      IntegrationsIntegrationEventsNamespace,
      MoviesIntegrationEventsNamespace,
      RealTimeUpdatesIntegrationEventsNamespace,
      ReportingIntegrationEventsNamespace,
      UsersIntegrationEventsNamespace
    ];

    List<Assembly> auditAssemblies =
      [
      Modules.Audit.Application.AssemblyReference.Assembly,
      Modules.Audit.Presentation.AssemblyReference.Assembly,
      typeof(AuditModule).Assembly
      ];

    Types.InAssemblies(auditAssemblies)
      .That()
      .DoNotHaveDependencyOnAny(integrationEventsModules)
      .Should()
      .NotHaveDependencyOnAny(otherModules)
      .GetResult()
      .ShouldBeSuccessful();
    }

  [Fact]
  public void CommunicationsModule_ShouldNotHaveDependenciesOn_OtherModules()
  {
    string[] otherModules =
    [
      AdministrationNamespace,
      AuditNamespace,
      DraftsNamespace,
      IntegrationsNamespace,
      MoviesNamespace,
      RealTimeUpdatesNamespace,
      ReportingNamespace,
      UsersNamespace
    ];

    string[] integrationEventsModules =
    [
      AdministrationIntegrationEventsNamespace,
      AuditIntegrationEventsNamespace,
      DraftsIntegrationEventsNamespace,
      IntegrationsIntegrationEventsNamespace,
      MoviesIntegrationEventsNamespace,
      RealTimeUpdatesIntegrationEventsNamespace,
      ReportingIntegrationEventsNamespace,
      UsersIntegrationEventsNamespace
    ];

    List<Assembly> communicationsAssemblies =
      [
      Modules.Communications.Application.AssemblyReference.Assembly,
      Modules.Communications.Presentation.AssemblyReference.Assembly,
      typeof(CommunicationsModule).Assembly
      ];

    Types.InAssemblies(communicationsAssemblies)
      .That()
      .DoNotHaveDependencyOnAny(integrationEventsModules)
      .Should()
      .NotHaveDependencyOnAny(otherModules)
      .GetResult()
      .ShouldBeSuccessful();
    }

  [Fact]
  public void DraftsModule_ShouldNotHaveDependenciesOn_OtherModules()
  {
    string[] otherModules =
    [
      AdministrationNamespace,
      AuditNamespace,
      CommunicationsNamespace,
      IntegrationsNamespace,
      MoviesNamespace,
      RealTimeUpdatesNamespace,
      ReportingNamespace,
      UsersNamespace
    ];

    string[] integrationEventsModules =
    [
      AdministrationIntegrationEventsNamespace,
      AuditIntegrationEventsNamespace,
      CommunicationsIntegrationEventsNamespace,
      IntegrationsIntegrationEventsNamespace,
      MoviesIntegrationEventsNamespace,
      RealTimeUpdatesIntegrationEventsNamespace,
      ReportingIntegrationEventsNamespace,
      UsersIntegrationEventsNamespace,
      UsersPublicApiNameSpace
    ];

    List<Assembly> draftsAssemblies =
      [
      typeof(Draft).Assembly,
      Modules.Drafts.Application.AssemblyReference.Assembly,
      Modules.Drafts.Presentation.AssemblyReference.Assembly,
      typeof(DraftsModule).Assembly
      ];

    Types.InAssemblies(draftsAssemblies)
      .That()
      .DoNotHaveDependencyOnAny(integrationEventsModules)
      .Should()
      .NotHaveDependencyOnAny(otherModules)
      .GetResult()
      .ShouldBeSuccessful();
    }

  [Fact]
  public void IntegrationsModule_ShouldNotHaveDependenciesOn_OtherModules()
  {
    string[] otherModules =
    [
      AdministrationNamespace,
      AuditNamespace,
      CommunicationsNamespace,
      DraftsNamespace,
      MoviesNamespace,
      RealTimeUpdatesNamespace,
      ReportingNamespace,
      UsersNamespace
    ];

    string[] integrationEventsModules =
    [
      AdministrationIntegrationEventsNamespace,
      AuditIntegrationEventsNamespace,
      CommunicationsIntegrationEventsNamespace,
      DraftsIntegrationEventsNamespace,
      MoviesIntegrationEventsNamespace,
      RealTimeUpdatesIntegrationEventsNamespace,
      ReportingIntegrationEventsNamespace,
      UsersIntegrationEventsNamespace
    ];

    List<Assembly> integrationsAssemblies =
      [
      Modules.Integrations.Application.AssemblyReference.Assembly,
      Modules.Integrations.Presentation.AssemblyReference.Assembly,
      typeof(IntegrationsModule).Assembly
      ];

    Types.InAssemblies(integrationsAssemblies)
      .That()
      .DoNotHaveDependencyOnAny(integrationEventsModules)
      .Should()
      .NotHaveDependencyOnAny(otherModules)
      .GetResult()
      .ShouldBeSuccessful();
    }

  [Fact]
  public void MoviesModule_ShouldNotHaveDependenciesOn_OtherModules()
  {
    string[] otherModules =
    [
      AdministrationNamespace,
      AuditNamespace,
      CommunicationsNamespace,
      DraftsNamespace,
      IntegrationsNamespace,
      RealTimeUpdatesNamespace,
      ReportingNamespace,
      UsersNamespace
    ];

    string[] integrationEventsModules =
    [
      AdministrationIntegrationEventsNamespace,
      AuditIntegrationEventsNamespace,
      CommunicationsIntegrationEventsNamespace,
      DraftsIntegrationEventsNamespace,
      IntegrationsIntegrationEventsNamespace,
      RealTimeUpdatesIntegrationEventsNamespace,
      ReportingIntegrationEventsNamespace,
      UsersIntegrationEventsNamespace
    ];

    List<Assembly> moviesAssemblies =
      [
      Modules.Movies.Application.AssemblyReference.Assembly,
      Modules.Movies.Presentation.AssemblyReference.Assembly,
      typeof(MoviesModule).Assembly
      ];

    Types.InAssemblies(moviesAssemblies)
      .That()
      .DoNotHaveDependencyOnAny(integrationEventsModules)
      .Should()
      .NotHaveDependencyOnAny(otherModules)
      .GetResult()
      .ShouldBeSuccessful();
    }

  [Fact]
  public void RealTimeUpdatesModule_ShouldNotHaveDependenciesOn_OtherModules()
  {
    string[] otherModules =
    [
      AdministrationNamespace,
      AuditNamespace,
      CommunicationsNamespace,
      DraftsNamespace,
      IntegrationsNamespace,
      MoviesNamespace,
      ReportingNamespace,
      UsersNamespace
    ];

    string[] integrationEventsModules =
    [
      AdministrationIntegrationEventsNamespace,
      AuditIntegrationEventsNamespace,
      CommunicationsIntegrationEventsNamespace,
      DraftsIntegrationEventsNamespace,
      IntegrationsIntegrationEventsNamespace,
      MoviesIntegrationEventsNamespace,
      ReportingIntegrationEventsNamespace,
      UsersIntegrationEventsNamespace
    ];

    List<Assembly> realTimeUpdatesAssemblies =
      [
      Modules.RealTimeUpdates.Application.AssemblyReference.Assembly,
      Modules.RealTimeUpdates.Presentation.AssemblyReference.Assembly,
      typeof(RealTimeUpdatesModule).Assembly
      ];

    Types.InAssemblies(realTimeUpdatesAssemblies)
      .That()
      .DoNotHaveDependencyOnAny(integrationEventsModules)
      .Should()
      .NotHaveDependencyOnAny(otherModules)
      .GetResult()
      .ShouldBeSuccessful();
    }

  [Fact]
  public void ReportingModule_ShouldNotHaveDependenciesOn_OtherModules()
  {
    string[] otherModules =
    [
      AdministrationNamespace,
      AuditNamespace,
      CommunicationsNamespace,
      DraftsNamespace,
      IntegrationsNamespace,
      MoviesNamespace,
      RealTimeUpdatesNamespace,
      UsersNamespace
    ];

    string[] integrationEventsModules =
    [
      AdministrationIntegrationEventsNamespace,
      AuditIntegrationEventsNamespace,
      CommunicationsIntegrationEventsNamespace,
      DraftsIntegrationEventsNamespace,
      IntegrationsIntegrationEventsNamespace,
      MoviesIntegrationEventsNamespace,
      RealTimeUpdatesIntegrationEventsNamespace,
      UsersIntegrationEventsNamespace
    ];

    List<Assembly> reportingAssemblies =
      [
      Modules.Reporting.Application.AssemblyReference.Assembly,
      Modules.Reporting.Presentation.AssemblyReference.Assembly,
      typeof(ReportingModule).Assembly
      ];

    Types.InAssemblies(reportingAssemblies)
      .That()
      .DoNotHaveDependencyOnAny(integrationEventsModules)
      .Should()
      .NotHaveDependencyOnAny(otherModules)
      .GetResult()
      .ShouldBeSuccessful();
    }

  [Fact]
  public void UsersModule_ShouldNotHaveDependenciesOn_OtherModules()
  {
    string[] otherModules =
    [
      AdministrationNamespace,
      AuditNamespace,
      CommunicationsNamespace,
      DraftsNamespace,
      IntegrationsNamespace,
      MoviesNamespace,
      RealTimeUpdatesNamespace,
      ReportingNamespace,
    ];

    string[] integrationEventsModules =
    [
      AdministrationIntegrationEventsNamespace,
      AuditIntegrationEventsNamespace,
      CommunicationsIntegrationEventsNamespace,
      DraftsIntegrationEventsNamespace,
      IntegrationsIntegrationEventsNamespace,
      MoviesIntegrationEventsNamespace,
      RealTimeUpdatesIntegrationEventsNamespace,
      ReportingIntegrationEventsNamespace,
    ];

    List<Assembly> usersAssemblies =
      [
      typeof(User).Assembly,
      Modules.Users.Application.AssemblyReference.Assembly,
      Modules.Users.Presentation.AssemblyReference.Assembly,
      typeof(UsersModule).Assembly
      ];

    Types.InAssemblies(usersAssemblies)
      .That()
      .DoNotHaveDependencyOnAny(integrationEventsModules)
      .Should()
      .NotHaveDependencyOnAny(otherModules)
      .GetResult()
      .ShouldBeSuccessful();
    }
}
