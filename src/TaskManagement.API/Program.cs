using TaskManagement.API.Extensions;
using TaskManagement.API.Middleware;
using TaskManagement.Application;
using TaskManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddApplication();                           
builder.Services.AddInfrastructure(builder.Configuration);  

// API layer services
builder.Services.AddControllers();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApiVersioningConfig();
builder.Services.AddSwaggerConfig();

// CORS — adjust origins in production
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger at the app root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TaskManagement.Infrastructure.Data.AppDbContext>();
    context.Database.EnsureCreated(); 
}

app.Run();

public partial class Program { }
