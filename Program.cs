using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Data;
using Microsoft.Data.SqlClient;
using System.Text;
using UserManagement.Repositories;
using Google.Apis.Auth;
using UserManagement.Mappings;


var builder = WebApplication.CreateBuilder(args);

//var googleClientId = builder.Configuration["Google:ClientId"];
//var jwtKey = builder.Configuration["Jwt:Key"];
//var jwtIssuer = builder.Configuration["Jwt:Issuer"];
//var jwtAudience = builder.Configuration["Jwt:Audience"];
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

// Configure Google OAuth Token Validation
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://accounts.google.com";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://accounts.google.com",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["GoogleOAuth:ClientId"], // Fix: Use Google Client ID
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
    builder =>
    {
        builder.WithOrigins("http://localhost:3001") // Add the ReactJS app origin
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(7237); // HTTP port
    options.ListenAnyIP(7238);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

// Enable Swagger for all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API v1");
    c.RoutePrefix = "swagger"; // Access at http://localhost:7237/swagger
});

app.UseHttpsRedirection();
app.UseAuthentication(); // Use Authentication
app.UseAuthorization();  // Use Authorization

app.MapControllers();

// Enable CORS with the specified policy
app.UseCors("AllowSpecificOrigins");

app.Run();
