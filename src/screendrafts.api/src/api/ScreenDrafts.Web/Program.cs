var builder = WebApplication.CreateBuilder(args);

Assembly[] presentationAssemblies = [
  ScreenDrafts.Modules.Drafts.Presentation.AssemblyReference.Assembly
  ];

Assembly[] applicationAssembles = [
  ScreenDrafts.Modules.Drafts.Application.AssemblyReference.Assembly
  ];

builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddOpenApi();
builder.Services.AddFastEndpoints(opt =>
{
  opt.Assemblies = presentationAssemblies;
});

builder.Services.AddApplication(applicationAssembles);
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("Database")!);

builder.Configuration.AddModuleConfiguration(["drafts"]);

builder.Services.AddDraftsModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.MapScalarApiReference();

  app.ApplyMigrations();
}

app.UseFastEndpoints();

app.UseSerilogRequestLogging();

await app.RunAsync();

