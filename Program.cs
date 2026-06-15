global using mapa_back.Helpers;
global using mapa_back.Middlewares;

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using DotNetEnv;
using mapa_back;
using mapa_back.Configuration;
using mapa_back.Data.RSPOApi;
using mapa_back.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using NetTopologySuite.IO.Converters;

var builder = WebApplication.CreateBuilder(args);

Env.Load();
builder.Configuration.AddEnvironmentVariables();

#region CONTROLLERS

builder.Services.AddControllers()
	.AddJsonOptions(opt =>
	{
		opt.JsonSerializerOptions.NumberHandling =
			JsonNumberHandling.AllowNamedFloatingPointLiterals;

		opt.JsonSerializerOptions.ReferenceHandler =
			ReferenceHandler.Preserve;

		opt.JsonSerializerOptions.Converters.Add(
			new GeoJsonConverterFactory());

		opt.JsonSerializerOptions.Converters.Add(
			new NullableDateOnlyJsonConverter());
	});

#endregion


builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy
			.AllowAnyOrigin()
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});

#region RSPO OPTIONS

builder.Services.AddOptions<RspoApiOptions>()
	.Bind(builder.Configuration.GetSection("RspoApi"))
	.Validate(o => !string.IsNullOrWhiteSpace(o.BaseUrl), "RspoApi:BaseUrl missing")
	.Validate(o => !string.IsNullOrWhiteSpace(o.Username), "RspoApi:Username missing")
	.Validate(o => !string.IsNullOrWhiteSpace(o.Password), "RspoApi:Password missing")
	.ValidateOnStart();

#endregion

#region HTTP CLIENT

builder.Services.AddHttpClient<IRSPOApiService, RSPOApiService>((sp, client) =>
{
	var cfg = sp.GetRequiredService<IOptions<RspoApiOptions>>().Value;

	client.BaseAddress = new Uri(cfg.BaseUrl.TrimEnd('/'));

	var credentials = Convert.ToBase64String(
		Encoding.UTF8.GetBytes($"{cfg.Username}:{cfg.Password}")
	);

	client.DefaultRequestHeaders.Authorization =
		new AuthenticationHeaderValue("Basic", credentials);

	client.DefaultRequestHeaders.Accept.Add(
		new MediaTypeWithQualityHeaderValue("application/json"));
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
	UseCookies = true,
	CookieContainer = new CookieContainer(),
	AutomaticDecompression =
		DecompressionMethods.GZip | DecompressionMethods.Deflate,
	AllowAutoRedirect = true
});

#endregion

#region SWAGGER

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header
	});
});

#endregion

#region DB

var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
	?? throw new InvalidOperationException("DB missing");

builder.Services.AddDbContext<DatabaseContext>(opt =>
	opt.UseNpgsql(connectionString, o => o.UseNetTopologySuite()));

#endregion

#region SERVICES

builder.Services.AddScoped<ISchoolsService, SchoolsService>();
builder.Services.AddScoped<IUsersService, UsersService>();

builder.Services.AddSingleton<RSPOProgressTracker>();
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddTransient<JwtMiddleware>();

#endregion

builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Host.UseSystemd();

var app = builder.Build();

#region PIPELINE (FIX ORDER)

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowAll");

app.Use(async (context, next) =>
{
	if (context.Request.Method == "OPTIONS")
	{
		context.Response.StatusCode = 200;
		return;
	}

	await next();
});

app.UseMiddleware<JwtMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();