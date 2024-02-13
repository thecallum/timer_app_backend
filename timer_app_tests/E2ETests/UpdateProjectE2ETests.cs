using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using timer_app.Boundary.Request;
using timer_app.Boundary.Response;
using timer_app.Factories;
using timer_app.Infrastructure;

namespace timer_app_tests.E2ETests
{
    [SingleThreaded]
    public class UpdateProjectE2ETests : MockWebApplicationFactory
    {
        public HttpClient Client => CreateClient();
        private readonly string AccessToken = GenerateToken();

        private readonly Fixture _fixture = new Fixture();

        [TearDown]
        public void TearDown()
        {
            CleanupDb();
        }

        [Test]
        public async Task UpdateProject_WhenInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var projectId = _fixture.Create<int>();

            var url = new Uri($"/api/projects/{projectId}", UriKind.Relative);

            var requestMessage = new HttpRequestMessage(HttpMethod.Put, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "INVALID_TOKEN");

            var request = _fixture.Create<UpdateProjectRequest>();

            var jsonRequest = JsonConvert.SerializeObject(request);
            requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.SendAsync(requestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task UpdateProject_WhenInvalidRequest_Returns400()
        {
            // Arrange
            var projectId = _fixture.Create<int>();

            var url = new Uri($"/api/projects/{projectId}", UriKind.Relative);

            var requestMessage = new HttpRequestMessage(HttpMethod.Put, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            var request = new UpdateProjectRequest
            {
                Description = "",
                ProjectColor = new ProjectColorRequest
                {
                    Light = "#000000hsjofsf",
                    Lightest = "#000000hsjofsf",
                    Dark = "#000000hsjofsf",
                    Darkest = "#000000hsjofsf",
                }
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.SendAsync(requestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task UpdateProject_WhenNotFound_Returns404()
        {
            // Arrange
            var projectId = _fixture.Create<int>();

            var url = new Uri($"/api/projects/{projectId}", UriKind.Relative);

            var requestMessage = new HttpRequestMessage(HttpMethod.Put, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            var request = new UpdateProjectRequest
            {
                Description = "Description",
                ProjectColor = new ProjectColorRequest
                {
                    Light = "#000000",
                    Lightest = "#000000",
                    Dark = "#000000",
                    Darkest = "#000000",
                }
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.SendAsync(requestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task UpdateProject_WhenUnauthorized_Returns401()
        {
            // Arrange
            var otherUserId = "sdfsdf";

            using var dbContext = CreateDbContext();

            var project = _fixture.Build<Project>()
                .Without(x => x.CalendarEvents)
                .With(x => x.UserId, otherUserId)
                .With(x => x.IsActive, true)
                .Create();

            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();

            var url = new Uri($"/api/projects/{project.Id}", UriKind.Relative);

            var requestMessage = new HttpRequestMessage(HttpMethod.Put, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            var request = new UpdateProjectRequest
            {
                Description = "Description",
                ProjectColor = new ProjectColorRequest
                {
                    Light = "#000000",
                    Lightest = "#000000",
                    Dark = "#000000",
                    Darkest = "#000000",
                }
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.SendAsync(requestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task UpdateProject_WhenArchived_Returns422()
        {
            // Arrange
            var project = _fixture.Build<Project>()
                .Without(x => x.CalendarEvents)
                .With(x => x.UserId, UserData.Id)
                .With(x => x.IsActive, false)
                .Create();

            using (var dbContext = CreateDbContext())
            {
                dbContext.Projects.Add(project);
                await dbContext.SaveChangesAsync();
            }

            var url = new Uri($"/api/projects/{project.Id}", UriKind.Relative);

            var requestMessage = new HttpRequestMessage(HttpMethod.Put, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            var request = new UpdateProjectRequest
            {
                Description = "Description",
                ProjectColor = new ProjectColorRequest
                {
                    Light = "#000000",
                    Lightest = "#000000",
                    Dark = "#000000",
                    Darkest = "#000000",
                }
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.SendAsync(requestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [Test]
        public async Task UpdateProject_WhenUpdated_Returns200()
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

            var requestMessage = new HttpRequestMessage(HttpMethod.Put, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            var request = new UpdateProjectRequest
            {
                Description = "Description",
                ProjectColor = new ProjectColorRequest
                {
                    Light = "#000000",
                    Lightest = "#000000",
                    Dark = "#000000",
                    Darkest = "#000000",
                }
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.SendAsync(requestMessage);

            var stringResult = await response.Content.ReadAsStringAsync();
            var responseContent = JsonConvert.DeserializeObject<ProjectResponse>(stringResult);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseContent.Description.Should().Be(request.Description);
            responseContent.ProjectColor.Should().BeEquivalentTo(request.ProjectColor.ToDb().ToResponse());

            using (var dbContext = CreateDbContext())
            {
                var dbResponse = await dbContext.Projects.FindAsync(project.Id);
                dbResponse.Should().NotBeNull();
                dbResponse.Description.Should().Be(request.Description);
                dbResponse.ProjectColor.Should().BeEquivalentTo(request.ProjectColor.ToDb().ToResponse());
            }
        }
    }
}