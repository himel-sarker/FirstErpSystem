// ================================================================
// Program.cs — Added By Himel Sarkar 10-06-2026
// LEARNING: YARP = Yet Another Reverse Proxy
// Client শুধু Gateway কে call করে, Gateway internal service কে route করে
// ================================================================

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseRouting();
app.MapReverseProxy();
app.Run();
