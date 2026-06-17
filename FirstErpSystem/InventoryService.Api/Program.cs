using InventoryService.Application.Common;
using InventoryService.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =================================================================
// LEARNING: Dependency Injection (DI) - The Heart of Clean Architecture
// We are telling the app: "Whenever someone asks for IApplicationDbContext,
// give them a real ApplicationDbContext with this SQL connection string."
// This is how the Waiter (Application) talks to the Chef (Infrastructure)
// without knowing exactly how the Chef cooks!
// =================================================================
builder.Services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =================================================================
// LEARNING: MediatR Registration
// We tell MediatR to look inside the Application layer for all Commands
// and Queries. This connects the API to the Waiter.
// =================================================================
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(IApplicationDbContext).Assembly));

// Add Controllers (instead of Minimal APIs)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers(); // This tells the app to use our ProductsController!
app.Run();
