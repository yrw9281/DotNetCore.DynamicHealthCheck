using DeepHealthCheck;

namespace Dynamic_Health_Check_Demo;

public static class Program
{
    public static void Main(string[] args)
    {
        // Set up configuration and services
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddDeepHealthCheck(builder.Configuration);

        // Build the application
        var app = builder.Build();

        // Configure the HTTP request pipeline
        app.UseDeepHealthCheck();
        app.UseRouting();
        app.MapControllers();

        // Start the application
        app.Run();
    }
}