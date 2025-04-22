using EduKidsFunctionApp.Models;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        string connectionString = Environment.GetEnvironmentVariable("SqlConnectionString:EduDb");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,                        // Number of retries dd
                    maxRetryDelay: TimeSpan.FromSeconds(10), // Delay between retries
                    errorNumbersToAdd: null);                // Specific SQL error numbers (optional)
            }));

     //   services.AddDbContext<AppDbContext>(options =>
     //       options.UseSqlServer(connectionString));
    })
    .Build();

host.Run();