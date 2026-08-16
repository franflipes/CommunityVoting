var builder = DistributedApplication.CreateBuilder(args);

// 1. Connection string for local PostgreSQL installed on port 5432
var postgres = builder.AddConnectionString("postgres");

// 2. Monitoring resource card in Aspire Dashboard for local PostgreSQL port 5432
builder.AddExecutable("postgres-local", "powershell", ".",
    "-Command", "$port=5432; Write-Host 'Monitoring Local PostgreSQL on port 5432...'; while ($true) { $c = Test-NetConnection -ComputerName localhost -Port $port -WarningAction SilentlyContinue; if ($c.TcpTestSucceeded) { Write-Host \"[$(Get-Date -Format 'HH:mm:ss')] PostgreSQL Local (port $port) is ONLINE\" } else { Write-Host \"[$(Get-Date -Format 'HH:mm:ss')] PostgreSQL Local (port $port) is OFFLINE\" }; Start-Sleep -Seconds 10 }")
    .ExcludeFromManifest();

var votingApi = builder.AddProject<Projects.CommunityVoting_Voting_Api>("voting-api")
    .WithReference(postgres);

var mainApi = builder.AddProject<Projects.CommunityVoting_API>("main-api")
    .WithReference(votingApi)
    .WithReference(postgres)
    .WithEnvironment("VotingService__BaseUrl", "http://voting-api");

// Configure Voting.Api to connect back to main API
votingApi.WithEnvironment("ApiBaseUrl", "http://main-api");

builder.Build().Run();
