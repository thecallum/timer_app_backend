using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using timer_app.Boundary.Request;
using timer_app.Infrastructure;
using timer_app.Middleware;

namespace timer_app_tests.E2ETests
{
    [SingleThreaded]
    public class DeleteProjectE2ETests : MockWebApplicationFactory
    {
        public HttpClient Client => CreateClient();
        private readonly string AccessToken = GenerateAccessToken();
        private readonly string IdToken = GenerateIdToken();

        [TearDown]
        public void TearDown()
        {
            CleanupDb();
        }

        [Test]
        public async Task DeleteProject_WhenInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var projectId = _fixture.Create<int>();

            var url = new Uri($"/api/projects/{projectId}", UriKind.Relative);

            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "INVALID_TOKEN");

            // Act
            var response = await Client.SendAsync(requestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task DeleteProject_WhenNotFound_Returns404()
        {
            // Arrange
            var projectId = _fixture.Create<int>();

            var url = new Uri($"/api/projects/{projectId}", UriKind.Relative);

            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            requestMessage.Headers.Add(HeaderConfig.IdToken, IdToken);

            // Act
            var response = await Client.SendAsync(requestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task DeleteProject_WhenProjectNotOwnedByUser_Returns401()
        {
            // Arrange
            var otherUserId = _fixture.Create<string>();

            using var dbContext = CreateDbContext();

            var project = _fixture.Build<Project>()
                .Without(x => x.CalendarEvents)
                .With(x => x.UserId, otherUserId)
                .With(x => x.IsActive, true)
                .Create();

            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();

            var url = new Uri($"/api/projects/{project.Id}", UriKind.Relative);

            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            requestMessage.Headers.Add(HeaderConfig.IdToken, IdToken);

            // Act
            var response = await Client.SendAsync(requestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task DeleteProject_WhenArchived_Returns422()
        {
            // Arrange
            using var dbContext = CreateDbContext();

            var project = _fixture.Build<Project>()
                .Without(x => x.CalendarEvents)
                .With(x => x.UserId, UserData.Id)
                .With(x => x.IsActive, false)
                .Create();

            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();

            var url = new Uri($"/api/projects/{project.Id}", UriKind.Relative);

            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            requestMessage.Headers.Add(HeaderConfig.IdToken, IdToken);

            // Act
            var response = await Client.SendAsync(requestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [Test]
        public async Task DeleteProject_WhenProjectDeleted_Returns201()
        {
            // Arrange
            var project = _fixture.Build<Project>()
                .Without(x => x.CalendarEvents)
                .With(x => x.UserId, UserData.Id)
                .With(x => x.IsActive, true)
                .Create();

            using (var dbContext = CreateDbContext())
            {
                dbContext.Projects.Add(project);
                await dbContext.SaveChangesAsync();
            }

            var url = new Uri($"/api/projects/{project.Id}", UriKind.Relative);

            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            requestMessage.Headers.Add(HeaderConfig.IdToken, IdToken);

            // Act
            var response = await Client.SendAsync(requestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using (var dbContext = CreateDbContext())
            {
                var dbResponse = await dbContext.Projects.FindAsync(project.Id);
                dbResponse.Should().NotBeNull();
                dbResponse.IsActive.Should().BeFalse();
            }
        }
    }
}