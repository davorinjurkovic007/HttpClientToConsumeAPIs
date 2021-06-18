﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Movie.Client.Services;
using System;
using System.Threading.Tasks;

namespace Movie.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using IHost host = CreatehostBuilder(args).Build();
            var serviceProvider = host.Services;

            // For demo purposes: overall catch-all to log any exception that might
            // happen to the console & wait for key input afterwards so we can easily 
            // inspect the issue.

            try
            {
                var logger = host.Services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Host created.");

                // Run out IntegrationService containing all samples and 
                // await thois call to ensure the application doesn't 
                // prematurely exit.
                await serviceProvider.GetService<IIntegrationService>().Run();
            }
            catch(Exception generalException)
            {
                // log the exception
                var logger = serviceProvider.GetService<ILogger<Program>>();
                logger.LogError(generalException, "An exception happened while running the integration service.");
            }

            Console.ReadKey();

            await host.RunAsync();
        }

        private static IHostBuilder CreatehostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args).ConfigureServices(
                (serviceCollection) => ConfigureServices(serviceCollection));
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // add loggers
            serviceCollection.AddLogging(configure => configure.AddDebug().AddConsole());

            // register the integration service on our container with a scoped lifetime

            // For the CRUD demos
            serviceCollection.AddScoped<IIntegrationService, CRUDService>();

            // For the partial update demos
        }


    }
}