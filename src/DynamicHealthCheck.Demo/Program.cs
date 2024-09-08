using DynamicHealthCheck;
using DynamicHealthCheck.LogSeverity;

namespace DynamicHealthCheck.Demo;

public static class Program
{
    public static void Main(string[] args)
    {
        // Set up configuration and services
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddDynamicHealthCheck(builder.Configuration)
            .AddLogSeverity();
        
        // Build the application
        var app = builder.Build();

        // Configure the HTTP request pipeline
        app.UseDynamicHealthCheck();
        app.UseRouting();
        app.MapControllers();

        // Start the application
        app.Run();
    }
}