var builder = WebApplication.CreateBuilder(args);

Assembly[] presentationAssemblies = [
  ScreenDrafts.Modules.Drafts.Presentation.AssemblyReference.Assembly
  ];

builder.Services.AddOpenApi();
builder.Services.AddFastEndpoints(opt =>
{
  opt.Assemblies = presentationAssemblies;
});

builder.Services.AddDraftsModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.MapScalarApiReference();

  app.ApplyMigrations();
}

app.UseFastEndpoints();

await app.RunAsync();

