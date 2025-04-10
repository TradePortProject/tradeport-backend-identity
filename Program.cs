using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Data;
using Microsoft.Data.SqlClient;
using System.Text;
using UserManagement.Repositories;
using Google.Apis.Auth;
using UserManagement.Mappings;
using UserManagement.Services;

/*
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(typeof(UserAutoMapperProfiles));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Set up SQL connection
SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
sqlBuilder.Encrypt = true;
sqlBuilder.TrustServerCertificate = true;
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(sqlBuilder.ConnectionString));
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Add CORS service
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Configure Google OAuth Token Validation
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer("Google",options =>
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
.AddJwtBearer("CustomJWT", options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"];
    var jwtIssuer = builder.Configuration["Jwt:Issuer"];
    var jwtAudience = builder.Configuration["Jwt:Audience"];

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

#region
//// Configure CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowSpecificOrigins",
//    builder =>
//    {
//        builder.WithOrigins("http://localhost:3001") // Add the ReactJS app origin
//               .AllowAnyHeader()
//               .AllowAnyMethod()
//               .AllowCredentials();
//    });
//});

//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(7237); // HTTP port
//    options.ListenAnyIP(7238);
//});


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//// Enable Swagger for all environments
//app.UseSwagger();
//app.UseSwaggerUI(c =>
//{
//    c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API v1");
//    c.RoutePrefix = "swagger"; // Access at http://localhost:7237/swagger
//});

//app.UseHttpsRedirection();
//app.UseAuthentication(); // Use Authentication
//app.UseAuthorization();  // Use Authorization

//app.MapControllers();

//// Enable CORS with the specified policy
//app.UseCors("AllowSpecificOrigins");

//app.Run();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

app.Run();
*/

var builder = WebApplication.CreateBuilder(args);

// Add AutoMapper service
builder.Services.AddAutoMapper(typeof(UserAutoMapperProfiles));

// Add controllers and Swagger services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Set up SQL connection with secure connection string
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Scoped services for repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Configure CORS service for secure origin handling
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy => policy
            .WithOrigins("http://localhost:3001") // Update with your allowed ReactJS app origin
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// Configure Google OAuth Token Validation for JWT Bearer
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
        ValidIssuer = "https://accounts.google.com", // Google Issuer
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Google:ClientId"], // Google Client ID from configuration
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
})
.AddJwtBearer("CustomJWT", options =>
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

// Set up JWT generation and expiration in services
builder.Services.AddSingleton<IJwtService, JwtService>();

var app = builder.Build();

//app.Logger.LogInformation("Application starting...");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API v1");
        c.RoutePrefix = "swagger"; // Access at http://localhost:7237/swagger
    });
}

app.UseHttpsRedirection();

// Enable CORS with a specified policy
app.UseCors("AllowSpecificOrigins");

// Use Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();
