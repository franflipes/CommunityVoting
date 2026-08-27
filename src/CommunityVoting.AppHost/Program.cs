var builder = DistributedApplication.CreateBuilder(args);

// 1. PostgreSQL container on host port 5433 + PgAdmin container dashboard on port 5050
var password = builder.AddParameter("postgres-password", "postgres", secret: true);

var postgres = builder.AddPostgres("postgres", password, port: 5433)
    .WithPgAdmin(c => c.WithHostPort(5050))
    .WithDataVolume("communityvoting-postgres-data");

var db = postgres.AddDatabase("communityvoting");

// 2. Azurite (Azure Blob & Queue Storage Emulator) Container
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(c => c.WithDataVolume("communityvoting-azurite-data"));
var blobs = storage.AddBlobs("blobs");
var queues = storage.AddQueues("queues");

var votingApi = builder.AddProject<Projects.CommunityVoting_Voting_Api>("voting-api")
    .WithReference(db, "postgres")
    .WaitFor(postgres);

var documentApi = builder.AddProject<Projects.CommunityVoting_Document_Api>("document-api")
    .WithReference(db, "postgres")
    .WithReference(blobs)
    .WaitFor(postgres);

var mainApi = builder.AddProject<Projects.CommunityVoting_API>("main-api")
    .WithReference(votingApi)
    .WithReference(documentApi)
    .WithReference(db, "postgres")
    .WithReference(blobs)
    .WithReference(queues)
    .WithEnvironment("VotingService__BaseUrl", "http://voting-api")
    .WithEnvironment("DocumentService__BaseUrl", "http://document-api")
    .WaitFor(postgres);

// Configure Voting.Api to connect back to main API
votingApi.WithReference(mainApi);

// 3. Add Vite React Frontend
builder.AddNpmApp("frontend", "../../frontend", "dev")
    .WithReference(mainApi)
    .WithReference(votingApi)
    .WithReference(documentApi)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

builder.Build().Run();
