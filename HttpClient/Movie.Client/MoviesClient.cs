using Movies.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Marvin.StreamExtensions;

namespace Movies.Client
{
    public class MoviesClient
    {
        private HttpClient Client;

        public MoviesClient(HttpClient client)
        {
            Client = client;
            Client.BaseAddress = new Uri("https://localhost:44383");
            Client.Timeout = new TimeSpan(0, 0, 30);
            Client.DefaultRequestHeaders.Clear();
        }

        //public HttpClient Client { get; }

        public async Task<IEnumerable<Movie>> GetMovies(CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/movies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using(var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                var stream = await response.Content.ReadAsStreamAsync();
                response.EnsureSuccessStatusCode();
                return stream.ReadAndDeserializeFromJson<List<Movie>>();
            }
        }
    }
}
