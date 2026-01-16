var builder = DistributedApplication.CreateBuilder(args);

// 1. Infrastructure
var postgres = builder.AddPostgres("postgres")
                      .WithDataVolume();

var redis = builder.AddRedis("redis");

// 2. The API
var apiService = builder.AddProject<Projects.AutoMarket_Intake_ApiService>("apiservice")
                        .WithReference(postgres)
                        .WithReference(redis);

// 3. Launch
builder.Build().Run();