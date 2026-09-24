using System.Reflection;
using FastEndpoints.Swagger;
using Attendify.Host.Extensions;
using Attendify.Host;
using Serilog;

var appAssembly = Assembly.GetExecutingAssembly();
var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.AddServiceDefaults();

builder.Logging.ClearProviders();
builder.Host.UseSerilog();

builder.Services.AddCustomProblemDetails();

builder.AddWebApi();
builder.AddApplication();
builder.AddInfrastructure();

builder.Services.ConfigureFeatures(builder.Configuration, appAssembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.UseCustomFastEndpoints();
app.UseSwaggerGen();
app.UseEventualConsistencyMiddleware();

app.MapDefaultEndpoints();
app.UseExceptionHandler();

app.Run();

namespace Attendify
{
    public partial class Program;
}
