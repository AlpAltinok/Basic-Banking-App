using System.Text;
using BankaApp.Api.Middleware;
using BankaApp.Application;
using BankaApp.Application.Options;
using BankaApp.Infrastructure;
using BankaApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Default SQLite file under ContentRoot; Docker can override via ConnectionStrings__SqliteConnection.
var sqliteConnection = builder.Configuration.GetConnectionString("SqliteConnection");
if (string.IsNullOrWhiteSpace(sqliteConnection)
    || sqliteConnection.Equals("Data Source=BankaApp.db", StringComparison.OrdinalIgnoreCase))
{
    var sqlitePath = Path.Combine(builder.Environment.ContentRootPath, "BankaApp.db");
    builder.Configuration["ConnectionStrings:SqliteConnection"] = $"Data Source={sqlitePath}";
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Digital Wallet API",
        Version = "v1",
        Description = """
            How to try it:
            1) Auth → register or login (no lock icon)
            2) Copy accessToken
            3) Authorize → paste: Bearer {token}
            4) Authorize → Close
            5) Call Wallet / Transfers endpoints
            """
    });

    // ApiKey scheme: paste the full "Bearer {token}" value (Swagger does not add the prefix).
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Example: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });

    options.OperationFilter<BankaApp.Api.Swagger.AuthorizeCheckOperationFilter>();
});

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
          ?? throw new InvalidOperationException("Jwt ayarları eksik.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "BankaApp API";
        options.DefaultModelsExpandDepth(-1); // şema paneli kafa karıştırmasın
    });
}

// Development'ta http://localhost ile test ederken HTTPS yönlendirmesi takılmaya yol açabiliyor.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Relational DB: migration uygula. InMemory: EnsureCreated.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsRelational())
    {
        await db.Database.MigrateAsync();
    }
    else
    {
        await db.Database.EnsureCreatedAsync();
    }
}

app.Run();

// Integration test'ler için partial class.
public partial class Program;
