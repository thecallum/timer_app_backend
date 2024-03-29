﻿using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using timer_app.Boundary.Response;
using timer_app.Infrastructure;
using timer_app.Middleware;

namespace timer_app_tests.E2ETests
{
    [SingleThreaded]
    public class GetAllProjectsE2ETests : MockWebApplicationFactory
    {
        public HttpClient Client => CreateClient();
        private readonly string AccessToken = GenerateAccessToken();
        private readonly string IdToken = GenerateIdToken();

        private HttpRequestMessage _requestMessage;

        [SetUp]
        public void Setup()
        {
            var url = new Uri($"/api/projects/", UriKind.Relative);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            requestMessage.Headers.Add(HeaderConfig.IdToken, IdToken);

            _requestMessage = requestMessage;
        }

        [TearDown]
        public void TearDown()
        {
            CleanupDb();
        }

        [Test]
        public async Task GetAllProjects_WhenInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            _requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "INVALID_TOKEN");

            // Act
            var response = await Client.SendAsync(_requestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetAllProjects_WhenCalled_Returns200()
        {
            // Arrange
            var numberOfProjects = _random.Next(2, 5);

            using (var dbContext = CreateDbContext())
            {
                var projects = _fixture.Build<Project>()
                    .Without(x => x.CalendarEvents)
                    .With(x => x.UserId, UserData.Id)
                    .CreateMany(numberOfProjects);

                dbContext.Projects.AddRange(projects);
                await dbContext.SaveChangesAsync();
            }

            // Act
            var response = await Client.SendAsync(_requestMessage);

            var stringResult = await response.Content.ReadAsStringAsync();
            var responseContent = JsonConvert.DeserializeObject<IEnumerable<ProjectResponse>>(stringResult);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseContent.Should().NotBeNull();
            responseContent.Should().HaveCount(numberOfProjects);
        }
    }
}