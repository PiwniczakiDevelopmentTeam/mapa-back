global using mapa_back.Middlewares;
global using mapa_back.Helpers;
using DotNetEnv;
using mapa_back;
using mapa_back.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using NetTopologySuite.IO.Converters;
using mapa_back.Data.RSPOApi;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

Env.Load();
builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.JsonSerializerOptions.Converters.Add(
			new GeoJsonConverterFactory());
	});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Format: 'Bearer {token}'"
    });

    c.OperationFilter<AuthorizeOperationFilter>();
});
var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string is not set.");
}

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
if (string.IsNullOrEmpty(jwtIssuer))
{
    throw new InvalidOperationException("JWT issuer is not set.");
}

var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
if (string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT audience is not set.");
}

var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("JWT secret is not set.");
}
if (jwtSecret.Length < 16)
{
    throw new InvalidOperationException("JWT secret is too short.");
}

builder.Services.AddDbContext<DatabaseContext>(options => options.UseNpgsql(connectionString, o => o.UseNetTopologySuite()), optionsLifetime:ServiceLifetime.Scoped);
builder.Services.AddScoped<IRSPOApiService, RSPOApiService>();
builder.Services.AddScoped<ISchoolsService, SchoolsService>();
builder.Services.AddScoped<IUsersService,  UsersService>();
builder.Services.AddSingleton<RSPOProgressTracker>();
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddTransient<JwtMiddleware>();
builder.Services.AddHttpClient();
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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret!)),
        ClockSkew = TimeSpan.Zero
    };
});

// Configure the default authorization policy
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .Build();

});
builder.Host.UseSystemd();

var app = builder.Build();
app.UseMiddleware<JwtMiddleware>();
app.UseCors(options => 
{
    options.AllowAnyHeader();
    options.AllowAnyMethod();
    options.AllowAnyOrigin();
 });

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(options =>
{
    options.AllowAnyHeader();
    options.AllowAnyMethod();
    options.WithOrigins("http://localhost:3000", "http://localhost:8080", "https://localhost:3000", "https://localhost:8080")
           .AllowCredentials();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
