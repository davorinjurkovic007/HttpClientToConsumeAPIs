using Movies.Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Marvin.StreamExtensions;

namespace Movies.Client.Services
{
    public class StreamService : IIntegrationService
    {
        private static HttpClient httpClient = new HttpClient(new HttpClientHandler()
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip
        });

        public StreamService()
        {
            // set zo HttpClient intance
            httpClient.BaseAddress = new Uri("https://localhost:5001");
            httpClient.Timeout = new TimeSpan(0, 0, 30);
            httpClient.DefaultRequestHeaders.Clear();
        }

        public async Task Run()
        {
            //await GetPosterWithStream();
            //await GetPosterWithStreamAndCompletionMode();

            //await TestGetPosterWithoutStream();
            //await TestGetPosterWithStream();
            //await TestGetPosterWithStreamAndCompletionMode();

            //await PostPosterWithStream();
            //await PostAndReadPosterWithStream();

            //await TestPostPosterWithoutStream();
            //await TestPostPosterWithStream();
            //await TestPostAndPosterWithStreams();

            await GetPosterWithGipCompression();
        }

        private async Task GetPosterWithStream()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using (var response = await httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();
                var poster = stream.ReadAndDeserializeFromJson<Poster>();

                //using (var streamReader = new StreamReader(stream))
                //{
                //    using (var jsonTextReader = new JsonTextReader(streamReader))
                //    {
                //        var jsonSerializer = new JsonSerializer();
                //        var poster = jsonSerializer.Deserialize<Poster>(jsonTextReader);

                //        // do something with the poster
                //      }
                //}
            }
        }

        private async Task GetPosterWithStreamAndCompletionMode()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();
                var poster = stream.ReadAndDeserializeFromJson<Poster>();

                //using (var streamReader = new StreamReader(stream))
                //{
                //    using (var jsonTextReader = new JsonTextReader(streamReader))
                //    {
                //        var jsonSerializer = new JsonSerializer();
                //        var poster = jsonSerializer.Deserialize<Poster>(jsonTextReader);

                //        // do something with the poster
                //    }
                //}
            }
        }

        private async Task GetPosterWithoutStream()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var poster = JsonConvert.DeserializeObject<Poster>(content);
        }

        private async Task PostPosterWithStream()
        {
            // generete a movie poster of 500KB
            var random = new Random();
            var generatedBytes = new byte[524288];
            random.NextBytes(generatedBytes);

            var posterForCreation = new PosterForCreation()
            {
                Name = "A new poster for The Big Lebowski",
                Bytes = generatedBytes
            };

            // This is what we're going to avoid
            // JsonConvert.SerializeObject(posterForCreation);

            var memoryContentStream = new MemoryStream();
            memoryContentStream.SerializeToJsonAndWrite(posterForCreation, new UTF8Encoding(), 1024, true);

            //using(var streamWriter = new StreamWriter(memoryContentStream, new UTF8Encoding(), 1024, true))
            //{
            //    using(var jsonTextWriter = new JsonTextWriter(streamWriter))
            //    {
            //        var jsonSerializer = new JsonSerializer();
            //        jsonSerializer.Serialize(jsonTextWriter, posterForCreation);
            //        jsonTextWriter.Flush();
            //    }
            //}

            memoryContentStream.Seek(0, SeekOrigin.Begin);
            using( var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters"))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (var streamContent = new StreamContent(memoryContentStream))
                {
                    request.Content = streamContent;
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var response = await httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var createdContent = await response.Content.ReadAsStringAsync();
                    var createdPoster = JsonConvert.DeserializeObject<Poster>(createdContent);
                }
            }
        }

        private async Task PostAndReadPosterWithStream()
        {
            // generete a movie poster of 500KB
            var random = new Random();
            var generatedBytes = new byte[524288];
            random.NextBytes(generatedBytes);

            var posterForCreation = new PosterForCreation()
            {
                Name = "A new poster for The Big Lebowski",
                Bytes = generatedBytes
            };

            // This is what we're going to avoid
            // JsonConvert.SerializeObject(posterForCreation);

            var memoryContentStream = new MemoryStream();
            memoryContentStream.SerializeToJsonAndWrite(posterForCreation, new UTF8Encoding(), 1024, true);

            memoryContentStream.Seek(0, SeekOrigin.Begin);
            using (var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters"))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (var streamContent = new StreamContent(memoryContentStream))
                {
                    request.Content = streamContent;
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    using(var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        var stream = await response.Content.ReadAsStreamAsync();
                        var poster = stream.ReadAndDeserializeFromJson<Poster>();
                    }
                }
            }
        }

        private async Task PostPosterWithoutStream()
        {
            // generete a movie poster of 500KB
            var random = new Random();
            var generatedBytes = new byte[524288];
            random.NextBytes(generatedBytes);

            var posterForCreation = new PosterForCreation()
            {
                Name = "A new poster for The Big Lebowski",
                Bytes = generatedBytes
            };

            var serializedPosterForCreation = JsonConvert.SerializeObject(posterForCreation);

            var request = new HttpRequestMessage(HttpMethod.Post, $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(serializedPosterForCreation);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var createdMovie = JsonConvert.DeserializeObject<Poster>(content);
        }

        private async Task GetPosterWithGipCompression()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using (var response = await httpClient.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();
                var poster = stream.ReadAndDeserializeFromJson<Poster>();

            }
        }

        public async Task TestPostPosterWithoutStream()
        {
            // warmup
            await PostPosterWithoutStream();

            // start stopwatch
            var stopWatch = Stopwatch.StartNew();

            // run requests
            for (int i = 0; i < 200; i++)
            {
                await PostPosterWithoutStream();
            }

            // stop stopwatch
            stopWatch.Stop();
            Console.WriteLine($"Elapsed miliseconds without stream: {stopWatch.ElapsedMilliseconds}, averaging {stopWatch.ElapsedMilliseconds / 200} " +
                "moliseconds/request");
        }

        public async Task TestPostPosterWithStream()
        {
            // warmup
            await PostPosterWithStream();

            // start stopwatch
            var stopWatch = Stopwatch.StartNew();

            // run requests
            for (int i = 0; i < 200; i++)
            {
                await PostPosterWithStream();
            }

            // stop stopwatch
            stopWatch.Stop();
            Console.WriteLine($"Elapsed miliseconds without stream: {stopWatch.ElapsedMilliseconds}, averaging {stopWatch.ElapsedMilliseconds / 200} " +
                "moliseconds/request");
        }

        public async Task TestPostAndPosterWithStreams()
        {
            // warmup
            await PostAndReadPosterWithStream();

            // start stopwatch
            var stopWatch = Stopwatch.StartNew();

            // run requests
            for (int i = 0; i < 200; i++)
            {
                await PostAndReadPosterWithStream();
            }

            // stop stopwatch
            stopWatch.Stop();
            Console.WriteLine($"Elapsed miliseconds without stream: {stopWatch.ElapsedMilliseconds}, averaging {stopWatch.ElapsedMilliseconds / 200} " +
                "moliseconds/request");
        }

        public async Task TestGetPosterWithoutStream()
        {
            // warmup
            await GetPosterWithoutStream();

            // start stopwatch
            var stopWatch = Stopwatch.StartNew();

            // run requests
            for(int i = 0; i < 200; i++)
            {
                await GetPosterWithoutStream();
            }

            // stop stopwatch
            stopWatch.Stop();
            Console.WriteLine($"Elapsed miliseconds without stream: {stopWatch.ElapsedMilliseconds}, averaging {stopWatch.ElapsedMilliseconds / 200} "  +
                "moliseconds/request");
        }

        public async Task TestGetPosterWithStream()
        {
            // warmup
            await GetPosterWithStream();

            // start stopwatch
            var stopWatch = Stopwatch.StartNew();

            // run requests
            for (int i = 0; i < 200; i++)
            {
                await GetPosterWithStream();
            }

            // stop stopwatch
            stopWatch.Stop();
            Console.WriteLine($"Elapsed miliseconds without stream: {stopWatch.ElapsedMilliseconds}, averaging {stopWatch.ElapsedMilliseconds / 200} " +
                "moliseconds/request");
        }

        public async Task TestGetPosterWithStreamAndCompletionMode()
        {
            // warmup
            await GetPosterWithStreamAndCompletionMode();

            // start stopwatch
            var stopWatch = Stopwatch.StartNew();

            // run requests
            for (int i = 0; i < 200; i++)
            {
                await GetPosterWithStreamAndCompletionMode();
            }

            // stop stopwatch
            stopWatch.Stop();
            Console.WriteLine($"Elapsed miliseconds without stream: {stopWatch.ElapsedMilliseconds}, averaging {stopWatch.ElapsedMilliseconds / 200} " +
                "moliseconds/request");
        }
    }
}
