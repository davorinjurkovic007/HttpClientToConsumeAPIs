using Microsoft.AspNetCore.JsonPatch;
using Movies.Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Client.Services
{
    public class PartialUpdateService : IIntegrationService
    {
        private static HttpClient httpClient = new HttpClient();

        public PartialUpdateService()
        {
            // set up HttpClient instance
            httpClient.BaseAddress = new Uri("https://localhost:5001");
            httpClient.Timeout = new TimeSpan(0, 0, 30);
            httpClient.DefaultRequestHeaders.Clear();
        }

        public async Task Run()
        {
            await PatchResource();
        }        public async Task PatchResource()
        {
            var patchDoc = new JsonPatchDocument<MovieForUpdate>();

            patchDoc.Replace(m => m.Title, "Update Title");
            patchDoc.Remove(m => m.Description);

            var serializedChangeSet = JsonConvert.SerializeObject(patchDoc);

            var request = new HttpRequestMessage(HttpMethod.Patch, "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(serializedChangeSet);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var updateMovie = JsonConvert.DeserializeObject<Movie>(content);
        }

        /// <summary>
        /// Here to we run into that request header issue.
        /// Headers like Accept, Accept-Encoding, Accept-Language, and so on are not set o per request basis.
        /// You can only set them at he level of HttpClient itself if you want to use PatchAsync method.
        /// </summary>
        /// <returns></returns>
        public async Task PatchResourceShortcut()
        {
            var patchDoc = new JsonPatchDocument<MovieForUpdate>();
            patchDoc.Replace(m => m.Title, "Updated Title");
            patchDoc.Remove(m => m.Description);

            var response = await httpClient.PatchAsync(
               "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b",
               new StringContent(
                   JsonConvert.SerializeObject(patchDoc),
                  Encoding.UTF8,
                   "application/json-patch+json"));

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var updatedMovie = JsonConvert.DeserializeObject<Movie>(content);
        }    }
}
