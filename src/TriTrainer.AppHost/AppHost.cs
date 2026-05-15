var builder = DistributedApplication.CreateBuilder(args);

var dbPassword = new GenerateParameterDefault();
var dbPasswordParameter = builder.AddParameter("db-Password", dbPassword, secret: true, persist: true);
var postgres = builder.AddPostgres("postgres", password: dbPasswordParameter)
    .WithPgAdmin();

var database = postgres.AddDatabase("tritrainerdb");

var apiService = builder.AddProject<Projects.TriTrainer_ApiService>("apiservice")
    .WithReference(database)
    .WaitFor(database)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.TriTrainer_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
