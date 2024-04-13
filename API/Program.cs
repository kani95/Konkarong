using API.Data;
using API.Entities;
using API.Extensions;
using API.Middleware;
using API.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// add the services
builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var connString = "";
if (builder.Environment.IsDevelopment())
{
    connString = builder.Configuration.GetConnectionString("DefaultConnection");
}
else
{
    var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    var port = Environment.GetEnvironmentVariable("DB_PORT");
    var pgUser = Environment.GetEnvironmentVariable("DB_USER");
    var pgHost = Environment.GetEnvironmentVariable("DB_HOST");
    var pgPass = Environment.GetEnvironmentVariable("DB_PASS");
    var pgDb = Environment.GetEnvironmentVariable("DB_DB");

    connString = $"Server={pgHost};Port={port};User Id={pgUser};Password={pgPass};Database={pgDb};";
    builder.Services.AddDbContext<DataContext>(opt => 
    {
        opt.UseNpgsql(connString);
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(builder => builder
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .WithOrigins("https://localhost:4200"));

// Order is important of these below

// Do you have a valid token?
app.UseAuthentication();

// What are you allowed to do with that token?
app.UseAuthorization();

app.UseDefaultFiles(); // get index.html from wwwrooot folder
app.UseStaticFiles(); // look for wwwroot when searching for files

app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");
app.MapFallbackToController("index", "Fallback"); // angular is handleing routing

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<DataContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();

    await context.Database.MigrateAsync();
    await Seed.ClearConnections(context);
    await Seed.SeedUsers(userManager, roleManager);
}
catch (Exception ex)
{
    var logger = services.GetService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration");
}

app.Run();
