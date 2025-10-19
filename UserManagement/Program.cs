using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Data;
using Microsoft.Data.SqlClient;
using System.Text;
using UserManagement.Repositories;
using UserManagement.Mappings;
using UserManagement.Services;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Text.Json;
using UserManagement.Models;


var builder = WebApplication.CreateBuilder(args);

var client = new AmazonSecretsManagerClient(RegionEndpoint.APSoutheast1);

async Task LoadSecret(string secretName)
{
    var request = new GetSecretValueRequest
    {
        SecretId = secretName
    };
    var response = await client.GetSecretValueAsync(request);

    if (response.SecretString != null)
    {
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(response.SecretString);
        if (dict != null)
        {
            foreach (var kv in dict)
            {
                // Flatten one level (e.g. "Jwt:Key")
                if (kv.Value is JsonElement el && el.ValueKind == JsonValueKind.Object)
                {
                    foreach (var inner in el.EnumerateObject())
                    {
                        builder.Configuration[$"{kv.Key}:{inner.Name}"] = inner.Value.ToString();
                    }
                }
                else
                {
                    builder.Configuration[kv.Key] = kv.Value?.ToString();
                }
            }
        }
    }
}

//Load your 3 secrets
await LoadSecret("tradeport/dev/user-mgmt/mssql-eks");
await LoadSecret("tradeport/dev/user-mgmt/jwt");
await LoadSecret("tradeport/dev/user-mgmt/google");

// Register EF Core with SQL Server (uses ConnectionStrings:UserMgmtDb from appsettings.Development.json)
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("tradeportdb"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null));
});


// ✅ AutoMapper Configuration
builder.Services.AddAutoMapper(typeof(UserAutoMapperProfiles));

// ✅ Add Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped<IUserRepository, UserRepository>();

// ✅ CORS Policy for React Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://tradeport.cloud")    
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ✅ Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer("Google", options =>
{
    options.Authority = "https://accounts.google.com";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = "https://accounts.google.com",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Google:ClientId"], 
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
})
.AddJwtBearer("Bearer", options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"];
    var jwtIssuer = builder.Configuration["Jwt:Issuer"];
    var jwtAudience = builder.Configuration["Jwt:Audience"];

    if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
    {
        throw new InvalidOperationException("JWT configuration is missing required values.");
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,  
        ValidateAudience = true,
        ValidAudience = jwtAudience, 
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), 
        ValidateIssuerSigningKey = true
    };
});

// ✅ JWT Token Generator Service
builder.Services.AddSingleton<IJwtService, JwtService>();

var app = builder.Build();

// ✅ Swagger UI for Dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API v1");
        c.RoutePrefix = "swagger"; // Access at http://localhost:7237/swagger
    });
}
// ✅ Cross-Origin Isolation Headers to prevent COOP/COEP warnings
app.Use(async (context, next) =>
{
    context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
    context.Response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";
    await next();
});

// ✅ Correct Middleware Order
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }

