using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var tunnel = builder.AddCloudflareTunnel("Lunnel");

var hostname = builder.Configuration.GetSection("Parameters")["Hostname"];

var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var db = postgres
    .AddDatabase("AppDb", "app-db");

var migrationService = builder
    .AddProject<MigrationService>("migrations")
    .WithReference(db)
    .WaitFor(postgres);

var api = builder
    .AddProject<WebApi>("api")
    .WithExternalHttpEndpoints()
    .WithReference(db)
    .WaitForCompletion(migrationService);

var webapp = builder
    .AddViteApp("webapp", "../../../../webapp/attendify")
    .WithReference(api)
    .WaitFor(api)
    .WithExternalHttpEndpoints();


var gateway = builder.AddYarp("gateway")
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute("/api/{**catch-all}", api);
        yarp.AddRoute("{**catch-all}", webapp);
    });

gateway.WithCloudflareTunnel(tunnel, hostname: hostname);

builder.Build().Run();