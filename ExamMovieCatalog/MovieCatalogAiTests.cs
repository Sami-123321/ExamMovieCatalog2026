using System;
using System.Net;
using System.Text.Json;
using ExamMovieCatalog.Models;
using RestSharp;
using RestSharp.Authenticators;
using ExamMovieCatalog.Models;

namespace ExamMovieCatalog
{

    [TestFixture]
    public class Tests
    {
        private RestClient client;
        private static string lastCreatedMovieId;

        private const string BaseUrl = "http://144.91.123.158:5000";

        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiI3YzYwZjllYS0xZmIxLTQxYTYtYmRlNi0xN2UwN2I4Njc3NTkiLCJpYXQiOiIwNC8xOC8yMDI2IDA2OjM0OjU4IiwiVXNlcklkIjoiY2IwMzA4ZjQtZjljOS00NTdjLTYyNjktMDhkZTc2OTcxYWI5IiwiRW1haWwiOiJTYW1pRXhhbUBhYm4uY29tIiwiVXNlck5hbWUiOiJTYW1pRXhhbSIsImV4cCI6MTc3NjUxNTY5OCwiaXNzIjoiTW92aWVDYXRhbG9nX0FwcF9Tb2Z0VW5pIiwiYXVkIjoiTW92aWVDYXRhbG9nX1dlYkFQSV9Tb2Z0VW5pIn0.v9sdAhxi9OhvxuUKalR01n1eZDx7Y139SiHAyfC2xok";

        [OneTimeSetUp]
        public void Setup()
        {
            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(StaticToken)
            };

            client = new RestClient(options);
        }

        [Order(1)]
        [Test]
        public void CreateMovie_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Movie/Create", Method.Post);

            request.AddJsonBody(new
            {
                title = "Test Movie " + Guid.NewGuid(),
                description = "Test description",
                posterUrl = "",
                trailerLink = "",
                isWatched = false
            });

            var response = client.Execute(request);

            Console.WriteLine(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            lastCreatedMovieId = json
                .GetProperty("movie")
                .GetProperty("id")
                .GetString();

            Assert.That(lastCreatedMovieId, Is.Not.Null.And.Not.Empty);
        }

        [Order(2)]
        [Test]
        public void EditMovie_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Movie/Edit", Method.Put);

            request.AddQueryParameter("movieId", lastCreatedMovieId);

            request.AddJsonBody(new
            {
                title = "Edited Movie",
                description = "Edited description",
                posterUrl = "",
                trailerLink = "",
                isWatched = false
            });

            var response = client.Execute(request);

            Console.WriteLine(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            var msg = json.GetProperty("msg").GetString();

            Assert.That(msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Order(3)]
        [Test]
        public void GetAllMovies_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Catalog/All", Method.Get);

            var response = client.Execute(request);

            Console.WriteLine(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var movies = JsonSerializer.Deserialize<object>(response.Content);

            Assert.That(movies, Is.Not.Null);
        }

        [Order(4)]
        [Test]
        public void DeleteMovie_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Movie/Delete", Method.Delete);

            request.AddQueryParameter("movieId", lastCreatedMovieId);

            var response = client.Execute(request);

            Console.WriteLine(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            Assert.That(response.Content, Does.Contain("deleted"));
        }

        [Order(5)]
        [Test]
        public void CreateMovie_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/api/Movie/Create", Method.Post);

          
            request.AddJsonBody(new
            {
                posterUrl = "",
                trailerLink = "",
                isWatched = false
            });

            var response = client.Execute(request);

            Console.WriteLine(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]
        public void Edit_NonExistingMovie_ShouldReturnBadRequest()
        {
            var fakeMovieId = "invalid-movie-id-12345";

            var request = new RestRequest("/api/Movie/Edit", Method.Put);

            request.AddQueryParameter("movieId", fakeMovieId);

            request.AddJsonBody(new
            {
                title = "Edited Title",
                description = "Edited Description",
                posterUrl = "",
                trailerLink = "",
                isWatched = false
            });

            var response = client.Execute(request);

            Console.WriteLine(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            var message = json.GetProperty("msg").GetString();

            Assert.That(message, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Order(7)]
        [Test]
        public void Delete_NonExistingMovie_ShouldReturnBadRequest()
        {
            var fakeMovieId = "invalid-movie-id-12345";

            var request = new RestRequest("/api/Movie/Delete", Method.Delete);

            request.AddQueryParameter("movieId", fakeMovieId);

            var response = client.Execute(request);

            Console.WriteLine(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            var message = json.GetProperty("msg").GetString();

            Assert.That(
                message,
                Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!")
            );
        }
        [OneTimeTearDown]
        public void TearDown()
        {
            client.Dispose();
        }
    }

    
}