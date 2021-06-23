using Marvin.StreamExtensions;
using Movies.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Client.Services
{
    public class HttpClientFactoryInstanceManagementService : IIntegrationService
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly IHttpClientFactory httpClientFactory;
        private readonly MoviesClient moviesClient;

        public HttpClientFactoryInstanceManagementService(IHttpClientFactory httpClientFactory, MoviesClient moviesClient)
        {
            this.httpClientFactory = httpClientFactory;
            this.moviesClient = moviesClient;
        }

        public async Task Run()
        {
            // await TestDisposeHttpClient(cancellationTokenSource.Token);
            // await TestReuseHttpClient(cancellationTokenSource.Token);
            // await GetMoviesWithHttpClientFromFactory(cancellationTokenSource.Token);
            // await GetMoviesWithHttpNamedClientFromFactory(cancellationTokenSource.Token);
            //await GetMoviesWithHttpTypedClientFromFactory(cancellationTokenSource.Token);
            await GetMoviesViaMoviesClient(cancellationTokenSource.Token);
        }

        private async Task TestDisposeHttpClient(CancellationToken cancellationToken)
        {
            for(var i = 0; i < 10; i++)
            {
                using(var httpClient = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, "https://index.hr");

                    using(var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                    {
                        var stream = await response.Content.ReadAsStreamAsync();
                        response.EnsureSuccessStatusCode();

                        Console.WriteLine($"Request competed with status code {response.StatusCode}");
                    }
                }
            }
        }

        private async Task TestReuseHttpClient(CancellationToken cancellationToken)
        {
            var httpClient = new HttpClient();
            
            for (var i = 0; i < 10; i++)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://index.hr");

                using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    response.EnsureSuccessStatusCode();

                    Console.WriteLine($"Request competed with status code {response.StatusCode}");
                }
                
            }
        }

        private async Task GetMoviesWithHttpClientFromFactory(CancellationToken cancellationToken)
        {
            var httpClient = httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:5001/api/movies");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                var stream = await response.Content.ReadAsStreamAsync();

                response.EnsureSuccessStatusCode();
                var trailer = stream.ReadAndDeserializeFromJson<List<Movie>>();
            }
        }

        private async Task GetMoviesWithHttpNamedClientFromFactory(CancellationToken cancellationToken)
        {
            var httpClient = httpClientFactory.CreateClient("MoviesClient");

            var request = new HttpRequestMessage(HttpMethod.Get, "api/movies");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                var stream = await response.Content.ReadAsStreamAsync();

                response.EnsureSuccessStatusCode();
                var trailer = stream.ReadAndDeserializeFromJson<List<Movie>>();
            }
        }

        //private async Task GetMoviesWithHttpTypedClientFromFactory(CancellationToken cancellationToken)
        //{
            
        //    var request = new HttpRequestMessage(HttpMethod.Get, "api/movies");

        //    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //    request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

        //    using (var response = await moviesClient.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
        //    {
        //        var stream = await response.Content.ReadAsStreamAsync();
        //        response.EnsureSuccessStatusCode();
        //        var movies = stream.ReadAndDeserializeFromJson<List<Movie>>();
        //    }
        //}

        private async Task GetMoviesViaMoviesClient(CancellationToken cancellationToken)
        {
            var movies = await moviesClient.GetMovies(cancellationToken);
        }
    }
}
