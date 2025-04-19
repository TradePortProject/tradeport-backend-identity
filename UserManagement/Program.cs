using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Data;
using Microsoft.Data.SqlClient;
using System.Text;
using UserManagement.Repositories;
using UserManagement.Mappings;
using UserManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ AutoMapper Configuration
builder.Services.AddAutoMapper(typeof(UserAutoMapperProfiles));

// ✅ Add Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ SQL Server Connection (with encryption)
var sqlBuilder = new SqlConnectionStringBuilder(builder.Configuration.GetConnectionString("DefaultConnection"))
{
    Encrypt = true,
    TrustServerCertificate = true
};
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(sqlBuilder.ConnectionString));
builder.Services.AddScoped<IUserRepository, UserRepository>();

// ✅ CORS Policy for React Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3001")
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
        ValidAudience = builder.Configuration["Google:ClientId"], // Use Google Client ID
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
        ValidIssuer = jwtIssuer,  // As configured earlier
        ValidateAudience = true,
        ValidAudience = jwtAudience, // As configured earlier
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), // Your JWT Key
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

