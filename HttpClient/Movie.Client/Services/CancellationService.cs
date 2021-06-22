using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Marvin.StreamExtensions;
using Movies.Client.Models;

namespace Movies.Client.Services
{
    public class CancellationService : IIntegrationService
    {
        private static HttpClient httpClient = new HttpClient(
            new HttpClientHandler()
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip
            });
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public CancellationService()
        {
            // set up HttpClient instance
            httpClient.BaseAddress = new Uri("https://localhost:5001");
            httpClient.Timeout = new TimeSpan(0, 0, 5);
            httpClient.DefaultRequestHeaders.Clear();
        }

        public async Task Run()
        {
            //cancellationTokenSource.CancelAfter(500);
            //await GetTrailerAndCancel(cancellationTokenSource.Token);
            await GetTrailerAndHandleTimeout();
        }

        private async Task GetTrailerAndCancel(CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/trailers/{Guid.NewGuid()}");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            //var cancellationTokenSource = new CancellationTokenSource();
            //// On the CancellationTokenSource, there's two methods we can use to effectively send a request for cancellation.
            //// Cancel will immediately ask for cancellation, while CancelAfter will do so after a specified amount of time. 
            //cancellationTokenSource.CancelAfter(1000);

            try
            {
                using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    var stream = await response.Content.ReadAsStreamAsync();

                    response.EnsureSuccessStatusCode();
                    var trailer = stream.ReadAndDeserializeFromJson<Trailer>();
                }
            }
            catch(OperationCanceledException operationCancelException)
            {
                Console.WriteLine($"An operation was cancelled with message {operationCancelException.Message}.");
                // additional cleanup, ...
            }
        }

        private async Task GetTrailerAndHandleTimeout()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/trailers/{Guid.NewGuid()}");

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            try
            {
                using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    var stream = await response.Content.ReadAsStreamAsync();

                    response.EnsureSuccessStatusCode();
                    var trailer = stream.ReadAndDeserializeFromJson<Trailer>();
                }
            }
            catch (OperationCanceledException operationCancelException)
            {
                Console.WriteLine($"An operation was cancelled with message {operationCancelException.Message}.");
                // additional cleanup, ...
            }
        }
    }
}
