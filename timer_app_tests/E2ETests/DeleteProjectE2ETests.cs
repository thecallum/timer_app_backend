using AutoFixture;
using FluentAssertions;
using System.Net;
using timer_app.Infrastructure;

namespace timer_app_tests.E2ETests
{
    [SingleThreaded]
    public class DeleteProjectE2ETests : MockWebApplicationFactory
    {
        public HttpClient Client => CreateClient();

        private readonly Fixture _fixture = new Fixture();

        [TearDown]
        public void TearDown()
        {
            CleanupDb();
        }

        [Test]
        public async Task DeleteProject_WhenNotFound_Returns404()
        {
            // Arrange
            var projectId = _fixture.Create<int>();

            var url = new Uri($"/api/projects/{projectId}", UriKind.Relative);

            // Act
            var response = await Client.DeleteAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task DeleteProject_WhenUnauthorized_Returns401()
        {
            // Arrange
            var otherUserId = 2;

            using var dbContext = CreateDbContext();

            var project = _fixture.Build<Project>()
                .Without(x => x.CalendarEvents)
                .With(x => x.UserId, otherUserId)
                .With(x => x.IsActive, true)
                .Create();

            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();

            var url = new Uri($"/api/projects/{project.Id}", UriKind.Relative);

            // Act
            var response = await Client.DeleteAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task DeleteProject_WhenArchived_Returns422()
        {
            // Arrange
            var userId = 1;

            using var dbContext = CreateDbContext();

            var project = _fixture.Build<Project>()
                .Without(x => x.CalendarEvents)
                .With(x => x.UserId, userId)
                .With(x => x.IsActive, false)
                .Create();

            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();

            var url = new Uri($"/api/projects/{project.Id}", UriKind.Relative);

            // Act
            var response = await Client.DeleteAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [Test]
        public async Task DeleteProject_WhenProjectDeleted_Returns201()
        {
            // Arrange
            var userId = 1;

            var project = _fixture.Build<Project>()
                .Without(x => x.CalendarEvents)
                .With(x => x.UserId, userId)
                .With(x => x.IsActive, true)
                .Create();

            using (var dbContext = CreateDbContext())
            {
                dbContext.Projects.Add(project);
                await dbContext.SaveChangesAsync();
            }            

            var url = new Uri($"/api/projects/{project.Id}", UriKind.Relative);

            // Act
            var response = await Client.DeleteAsync(url);

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