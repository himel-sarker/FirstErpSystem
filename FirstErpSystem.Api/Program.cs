using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FirstErpSystem.Api.Data;
using FirstErpSystem.Api.Services;

/*
================================================================
Program.cs — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- Program.cs = ASP.NET Core app এর entry point
- builder.Services = Dependency Injection container
  এখানে register করলে যেকোনো Controller-এ inject করা যায়
- Phase 2 তে ছিল: DbContext, JWT, Swagger
- Phase 4 তে add হলো: EmailService, SmsService, HttpClient
================================================================
*/

var builder = WebApplication.CreateBuilder(args);

//Added By Himel Sarkar 09-06-2026

// ── CORS ──────────────────────────────────────────────────
/*
LEARNING: CORS = Cross-Origin Resource Sharing
Frontend (localhost:3000) → API (localhost:5123)
Different port = different origin → CORS block করে
AllowAnyOrigin = সব origin থেকে request allow
*/
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ── Database ──────────────────────────────────────────────
/*
LEARNING: AddDbContext = EF Core register করা
appsettings.json এর ConnectionStrings:DefaultConnection use করে
*/
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ── JWT Authentication ────────────────────────────────────
/*
LEARNING: JWT = JSON Web Token
Login করলে token পাওয়া যায়
সেই token request header-এ পাঠালে API verify করে
TokenValidationParameters = token check করার rules
*/
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(secretKey))
    };
});

// ── Email Service ─────────────────────────────────────────
/*
LEARNING: AddScoped = প্রতিটা HTTP request-এ নতুন instance তৈরি
AddSingleton = পুরো app lifetime-এ একটাই instance
AddTransient  = প্রতিবার inject করলে নতুন instance
Email/SMS-এর জন্য Scoped best practice
*/
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();

// ── HttpClient ────────────────────────────────────────────
/*
LEARNING: AddHttpClient = IHttpClientFactory register করে
SmsService এ HttpClient inject করার জন্য দরকার
Direct new HttpClient() করা bad practice —
IHttpClientFactory connection properly manage করে
*/
builder.Services.AddHttpClient();

// ── Controllers + Swagger ─────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FirstErpSystem.Api",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT token দাও। Example: Bearer eyJhbGci..."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

//End By Himel Sarkar 09-06-2026

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ================================================================
// Auto-Migration — Added By Himel Sarkar 11-06-2026
// LEARNING: Docker-এ ডাটাবেজ নতুন হয়, তাই API চালু হলে
// অটোমেটিক ডাটাবেজ তৈরি এবং Migration চালানো দরকার
// ================================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // ডাটাবেজ না থাকলে তৈরি করবে, পুরানো হলে আপডেট করবে
}





app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
