using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieStore.API.Data;
using MovieStore.API.EndPoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MovieStore.API.Models;

var builder = WebApplication.CreateBuilder(args);

var authServer = builder.Configuration
    .GetSection("AuthServer")
    .Get<AuthServerData>()
    ?? throw new InvalidOperationException("Не найдена секция AuthServer");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.MetadataAddress =
            $"{authServer.Host}/realms/{authServer.Realm}/.well-known/openid-configuration";

        options.Authority =
            $"{authServer.Host}/realms/{authServer.Realm}";

        options.Audience = "account";
        options.RequireHttpsMetadata = false;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("admin", policy =>
        policy.RequireRole("POWER-USER"));
});


builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")));
builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssemblyContaining<Program>());

var app = builder.Build();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMovieEndpoints();

await DbInitializer.SeedData(app);

app.Run();

public partial class Program;
