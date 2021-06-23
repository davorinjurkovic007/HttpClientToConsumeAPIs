using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Movies.Client.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Movies.Client
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

            serviceCollection.AddHttpClient("MoviesClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:5001");
                client.Timeout = new TimeSpan(0, 0, 30);
                client.DefaultRequestHeaders.Clear();
            })
                // start building the pipeline
            // Important here is ensuring that the timeout we set on out handler is lower than the timeout specified at HttpClient level;
            // otherwise, we will just end up with a TaskCanceledException again
            .AddHttpMessageHandler(handler => new TimeOutDelegatingHandler(TimeSpan.FromSeconds(20)))
            .AddHttpMessageHandler(handlers => new RetryPolicyDelegatingHandler(2))
            // The handlers are added in the order we registered them, and the primary message handler is the last one in the pipeline
            .ConfigurePrimaryHttpMessageHandler(handler =>
                new HttpClientHandler()
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip
                });

            //// This registers our MoviesClient with a transient scope.
            //// The factory will automatically create an instance of HttpClient with whichever configuration
            //// we input when an instance of MoviesClient is requested from the DI system.
            //serviceCollection.AddHttpClient<MoviesClient>(client =>
            //{
            //    client.BaseAddress = new Uri("https://localhost:5001");
            //    client.Timeout = new TimeSpan(0, 0, 30);
            //    client.DefaultRequestHeaders.Clear();
            //})
            //.ConfigurePrimaryHttpMessageHandler(handler =>
            //    new HttpClientHandler()
            //    {
            //        AutomaticDecompression = System.Net.DecompressionMethods.GZip
            //    });

            // New configuration for MovieClient class to add data. 
            serviceCollection.AddHttpClient<MoviesClient>()
                .ConfigurePrimaryHttpMessageHandler(handlers =>
                    new HttpClientHandler()
                    {
                        AutomaticDecompression = System.Net.DecompressionMethods.GZip
                    });

            // register the integration service on our container with a scoped lifetime

            // For the CRUD demos
            // serviceCollection.AddScoped<IIntegrationService, CRUDService>();

            // For the partial update demos
            // serviceCollection.AddScoped<IIntegrationService, PartialUpdateService>();

            // For the stream demos
            // serviceCollection.AddScoped<IIntegrationService, StreamService>();

            // For the cancellation demos
            // serviceCollection.AddScoped<IIntegrationService, CancellationService>();

            // For the HttpClientFactory demos
            // serviceCollection.AddScoped<IIntegrationService, HttpClientFactoryInstanceManagementService>();

            // For the dealing wiht errors and faults demos
            // serviceCollection.AddScoped<IIntegrationService, DealingWithErrorAndFaultsService>();

            // For the custom http handlers demos
            serviceCollection.AddScoped<IIntegrationService, HttpHandlersService>();
        }
    }
}
