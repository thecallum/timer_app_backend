using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
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

        private readonly Fixture _fixture = new Fixture();

        [TearDown]
        public void TearDown()
        {
            CleanupDb();
        }

        [Test]
        public async Task UpdateProject_WhenInvalidRequest_Returns400()
        {
            // Arrange
            var projectId = _fixture.Create<int>();

            var url = new Uri($"/api/projects/{projectId}", UriKind.Relative);

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
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PutAsync(url, content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task UpdateProject_WhenNotFound_Returns404()
        {
            // Arrange
            var projectId = _fixture.Create<int>();

            var url = new Uri($"/api/projects/{projectId}", UriKind.Relative);

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
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PutAsync(url, content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task UpdateProject_WhenUnauthorized_Returns401()
        {
            // Arrange
            var otherUserId = 2;

            using var dbContext = CreateDbContext();

            var project = _fixture.Build<Project>()
                .Without(x => x.CalendarEvents)
                .With(x => x.UserId, otherUserId)
                .Create();

            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();

            var url = new Uri($"/api/projects/{project.Id}", UriKind.Relative);

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
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PutAsync(url, content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task UpdateProject_WhenUpdated_Returns200()
        {
            // Arrange
            var userId = 1;
            var projectId = _fixture.Create<int>();

            using (var dbContext = CreateDbContext())
            {
                var project = _fixture.Build<Project>()
                    .Without(x => x.CalendarEvents)
                    .With(x => x.UserId, userId)
                    .With(x => x.Id, projectId)
                    .Create();

                dbContext.Projects.Add(project);
                await dbContext.SaveChangesAsync();
            }

            var url = new Uri($"/api/projects/{projectId}", UriKind.Relative);

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
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PutAsync(url, content);

            var stringResult = await response.Content.ReadAsStringAsync();
            var responseContent = JsonConvert.DeserializeObject<ProjectResponse>(stringResult);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseContent.Description.Should().Be(request.Description);
            responseContent.ProjectColor.Should().BeEquivalentTo(request.ProjectColor.ToDb().ToResponse());

            using (var dbContext = CreateDbContext())
            {
                var dbResponse = await dbContext.Projects.FindAsync(projectId);
                dbResponse.Should().NotBeNull();
                dbResponse.Description.Should().Be(request.Description);
                dbResponse.ProjectColor.Should().BeEquivalentTo(request.ProjectColor.ToDb().ToResponse());
            }
        }
    }
}