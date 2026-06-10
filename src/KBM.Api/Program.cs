using System.Text;
using KBM.Api.Middleware;
using KBM.Infrastructure.Persistence;
using KBM.Application.Configuration;
using KBM.Application.Setup;
using KBM.Infrastructure.Persistence.Seeding;
using KBM.Reporting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.FromLogContext()
       .WriteTo.Console());

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddReporting();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jwtKey = builder.Configuration["Jwt:SigningKey"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<KbmDbContext>();
    await db.Database.MigrateAsync();
    var rbac = scope.ServiceProvider.GetRequiredService<RbacSeedService>();
    await rbac.EnsureSeededAsync();
    var config = scope.ServiceProvider.GetRequiredService<ISystemConfigService>();
    await config.EnsureDefaultReportsAsync();
    var setup = scope.ServiceProvider.GetRequiredService<ISetupService>();
    await setup.MarkCompletedIfLegacyDataAsync();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
