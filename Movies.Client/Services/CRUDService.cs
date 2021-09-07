using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Movies.Client.Models;

namespace Movies.Client.Services
{
    public class CRUDService : IIntegrationService
    {
        private static HttpClient _httpClient = new HttpClient();

        public CRUDService()
        {
            _httpClient.BaseAddress = new Uri("http://localhost:57863");
            // to avoid exception in case if some other part of code has already set header
            _httpClient.DefaultRequestHeaders.Clear();

            // we can process content with both types: json and xml
            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9)); // preference for contentType xml = 0.9 is less than json = 1
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
        }

        public async Task Run()
        {
            //await GetResource();
            //await GetResourceThroughHttpRequestMessage();
            //await CreateResource();
            //await UpdateResource();
            await DeleteResource();
        }

        public async Task GetResource()
        {
            // bad practice to use using statement for HttpClient
            //using (_httpClient = new HttpClient())
            HttpResponseMessage response = await _httpClient.GetAsync("api/movies");
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            var movies = new List<Movie>();

            if (response.Content.Headers.ContentType?.MediaType == "application/json")
            {
                movies = await response.Content.ReadFromJsonAsync<List<Movie>>();
            }
            else if (response.Content.Headers.ContentType?.MediaType == "application/xml")
            {
                var serializer = new XmlSerializer(typeof(List<Movie>));
                movies = (List<Movie>)serializer.Deserialize(new StringReader(content));
            }
        }

        public async Task GetResourceThroughHttpRequestMessage()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/movies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var movies = await response.Content.ReadFromJsonAsync<IEnumerable<Movie>>();
        }

        public async Task CreateResource()
        {
            var movieToCreate = new MovieForCreation()
            {
                Title = "Reservoir Dogs",
                Description = "After a simple jewelry heist goes terribly wrong, the " +
                              "surviving criminals begin to suspect that one of them is a police informant.",
                DirectorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                ReleaseDate = new DateTimeOffset(new DateTime(1992, 9, 2)),
                Genre = "Crime, Drama"
            };

            var serializedMovieToCreate = JsonSerializer.Serialize(movieToCreate);

            var request = new HttpRequestMessage(HttpMethod.Post, "api/movies");
            // content type that we want to get back in the response
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            request.Content = new StringContent(serializedMovieToCreate);
            // content type of request body
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var movie = await response.Content.ReadFromJsonAsync<Movie>();
        }

        public async Task UpdateResource()
        {
            var movieToUpdate = new MovieForUpdate()
            {
                Title = "Pulp Fiction",
                Description = "The movie with Zed.",
                DirectorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                ReleaseDate = new DateTimeOffset(new DateTime(1992, 9, 2)),
                Genre = "Crime, Drama"
            };

            var serializedMovieToCreate = JsonSerializer.Serialize(movieToUpdate);

            var request = new HttpRequestMessage(HttpMethod.Put, "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            request.Content = new StringContent(serializedMovieToCreate);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var movie = await response.Content.ReadFromJsonAsync<Movie>();
        }

        public async Task DeleteResource()
        {
            var request = new HttpRequestMessage(HttpMethod.Delete,
                "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b");

            // header accept is provided for Delete operation just in case, beucase sometimes some api returns error message in response if smt bad happened
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
        }

        #region shortcuts
        private async Task PostResourceShortcut()
        {
            var movieToCreate = new MovieForCreation()
            {
                Title = "Reservoir Dogs",
                Description = "After a simple jewelry heist goes terribly wrong, the " +
                "surviving criminals begin to suspect that one of them is a police informant.",
                DirectorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                ReleaseDate = new DateTimeOffset(new DateTime(1992, 9, 2)),
                Genre = "Crime, Drama"
            };

            var response = await _httpClient.PostAsync(
                "api/movies",
                new StringContent(
                    JsonSerializer.Serialize(movieToCreate),
                    Encoding.UTF8,
                    "application/json"));

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var createdMovie = JsonSerializer.Deserialize<Movie>(content,
               new JsonSerializerOptions
               {
                   PropertyNamingPolicy = JsonNamingPolicy.CamelCase
               });
        }

        private async Task PutResourceShortcut()
        {
            var movieToUpdate = new MovieForUpdate()
            {
                Title = "Pulp Fiction",
                Description = "The movie with Zed.",
                DirectorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"),
                ReleaseDate = new DateTimeOffset(new DateTime(1992, 9, 2)),
                Genre = "Crime, Drama"
            };

            var response = await _httpClient.PutAsync(
               "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b",
               new StringContent(
                   JsonSerializer.Serialize(movieToUpdate),
                   System.Text.Encoding.UTF8,
                   "application/json"));

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var updatedMovie = JsonSerializer.Deserialize<Movie>(content,
               new JsonSerializerOptions
               {
                   PropertyNamingPolicy = JsonNamingPolicy.CamelCase
               });
        }

        private async Task DeleteResourceShortcut()
        {
            var response = await _httpClient.DeleteAsync(
                "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
        }
        #endregion
    }
}