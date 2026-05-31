using System.Data;
using System.Text;
using CareCollar.Application.Contracts;
using CareCollar.Application.Services;
using CareCollar.Infrastructure.Security;
using CareCollar.Persistence;
using CareCollar.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<CareCollarDbContext>(options =>
    options.UseNpgsql(connectionString,
        o => o.UseNodaTime()));

builder.Services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(connectionString));
builder.Services.AddScoped<ICareCollarDbContext>(provider =>
    provider.GetRequiredService<CareCollarDbContext>());

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(
    jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<ICollarService, CollarService>();
builder.Services.AddScoped<IHealthDataRepository, HealthDataRepository>();
builder.Services.AddScoped<IIngestionService, IngestionService>();
builder.Services.AddScoped<IThresholdService, ThresholdService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000",
                "http://localhost:4173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.SuppressMapClientErrors = true;
});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CareCollar API",
        Version = "v1",
        Description = "API for Smart Pet Collar Management and Telemetry"
    });

    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token: Bearer {your_token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

for (int i = 0; i < 10; i++)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CareCollarDbContext>();
        db.Database.Migrate();
        await SeedAdminAsync(scope.ServiceProvider);
        break;
    }
    catch (NpgsqlException)
    {
        Console.WriteLine("Db is not ready. Waiting 3 seconds.");
        Thread.Sleep(3000);
    }
}

static async Task SeedAdminAsync(IServiceProvider services)
{
    var config = services.GetRequiredService<IConfiguration>();
    var adminEmail = config["AdminSeed:Email"];
    var adminPassword = config["AdminSeed:Password"];

    if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        return;

    var db = services.GetRequiredService<CareCollarDbContext>();
    var hasher = services.GetRequiredService<CareCollar.Application.Contracts.IPasswordHasher>();

    var existing = await db.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
    if (existing is not null)
    {
        if (!existing.IsAdmin)
        {
            existing.IsAdmin = true;
            await db.SaveChangesAsync();
            Console.WriteLine($"Promoted existing user '{adminEmail}' to admin.");
        }
        return;
    }

    db.Users.Add(new CareCollar.Domain.Entities.User
    {
        Email = adminEmail,
        PasswordHash = hasher.HashPassword(adminPassword),
        IsAdmin = true
    });
    await db.SaveChangesAsync();
    Console.WriteLine($"Admin user '{adminEmail}' created.");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseCors("Frontend");
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
