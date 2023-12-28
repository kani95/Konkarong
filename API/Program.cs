using API.Extensions;
using API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// add the services
builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200"));

// Order is important of these below

// Do you have a valid token?
app.UseAuthentication();

// What are you allowed to do with that token?
app.UseAuthorization();

app.MapControllers();

app.Run();
