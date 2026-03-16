using BarberShop.Api.Middlewares;
using BarberShop.Application.Config;
using BarberShop.Communication.Models.Auth;
using BarberShop.Infra.DataAccess;
using BarberShop.IOC;
using BarberShop.Infra.Interfaces;
using Hangfire;
using Hangfire.Console;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using MoneyScope.Application.Services;
using System.Text.Json.Serialization;
using BarberShop.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager config = builder.Configuration;
builder.Services.AddSingleton(d => config);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddDbContext<BarberShopContext>(options =>
{
    options.UseMySql(config["ConnectionStrings:LocalConn"],
        new MySqlServerVersion(new Version(8, 0)))
        .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddFilter((category, level) =>
            category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information)));
}, ServiceLifetime.Transient);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthenticationConfiguration(config);
builder.Services.AddAuthorizationConfiguration();
builder.Services.AddSwaggerConfiguration();

builder.Services.AddHangfire(options =>
{
    options.UseMemoryStorage();
    options.UseConsole();
});
builder.Services.AddHangfireServer();
builder.Services.AddHostedService<ServiceBackGround>();

builder.Services.Configure<SmtpConfig>(options =>
{
    builder.Configuration.GetSection(nameof(SmtpConfig)).Bind(options);
});
//configura mapeamentos de IOptions do appsettings
builder.Services.AddConfiguredOptions(builder.Configuration);

builder.Services.Configure<EnvironmentVars>(options =>
{
    builder.Configuration.GetSection("Vars").Bind(options);
});

builder.Services.AddMemoryCache();

var migrationConfig = builder.Configuration.GetSection(nameof(MigrationConfig)).Get<MigrationConfig>();

builder.Services.InjectDependencies(migrationConfig);
builder.Services.AddHttpContextAccessor();

//Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});

var app = builder.Build();


//swagger
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BarberAgenda-V1");
    c.InjectStylesheet("/swagger-ui/swagger-dark.css");
});

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = HangFireDashboard.AuthAuthorizationFilters()
});


app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

#region Middlewares
app.UseMiddleware<JwtMiddleware>();
app.UseMiddleware(typeof(HandlingMiddleware));
#endregion Middlewares

app.MapControllers();
app.Run();

