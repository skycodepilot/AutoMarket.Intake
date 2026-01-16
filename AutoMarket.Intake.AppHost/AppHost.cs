var builder = DistributedApplication.CreateBuilder(args);

// 1. Infrastructure (Data & Cache)
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

var redis = builder.AddRedis("redis");

// 2. Backend API
var api = builder.AddProject<Projects.AutoMarket_Intake_ApiService>("apiservice")
    .WithReference(postgres)
    .WithReference(redis);

// 3. Frontend (External)
// Since we run React via 'npm run dev' externally, we just register the URL 
// so it appears in the Aspire Dashboard for convenience.
builder.AddConnectionString("frontend", "http://localhost:5173");

builder.Build().Run();