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

namespace Movies.Client
{
    public class TestableClassWithApiAccess
    {
        private readonly HttpClient httpClient;

        public TestableClassWithApiAccess(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<Movie> GetMovie(CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/movies/030a43b0-f9a5-405a-811c-bf342524b2be");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                if (!response.IsSuccessStatusCode)
                {
                    // inspect the status code
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // show this to the user
                        Console.WriteLine("The request movie cannot be found.");
                        return null;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        // trigger a login flow
                        // TODO
                        throw new UnauthorizedApiAccessException();
                    }

                    // Is other status code is founded, throw exception
                    response.EnsureSuccessStatusCode();
                }

                var stream = await response.Content.ReadAsStreamAsync();
                return stream.ReadAndDeserializeFromJson<Movie>();
            }
        }
    }
}
