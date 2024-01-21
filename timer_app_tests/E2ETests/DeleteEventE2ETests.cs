using AutoFixture;
using FluentAssertions;
using System.Net;
using timer_app.Infrastructure;

namespace timer_app_tests.E2ETests
{
    [SingleThreaded]
    public class DeleteEventE2ETests : MockWebApplicationFactory
    {
        public HttpClient Client => CreateClient();

        private readonly Fixture _fixture = new Fixture();

        [TearDown]
        public void TearDown()
        {
            CleanupDb();
        }

        [Test]
        public async Task DeleteEvent_WhenNotFound_Returns404()
        {
            // Arrange
            var eventId = _fixture.Create<int>();

            var url = new Uri($"/api/events/{eventId}", UriKind.Relative);

            // Act
            var response = await Client.DeleteAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task DeleteEvent_WhenUnauthorized_Returns401()
        {
            // Arrange
            var otherUserId = 2;

            var calendarEvent = _fixture.Build<CalendarEvent>()
               .Without(x => x.Project)
               .Without(x => x.ProjectId)
               .With(x => x.UserId, otherUserId)
               .Create();

            using (var dbContext = CreateDbContext())
            {
                dbContext.CalendarEvents.Add(calendarEvent);
                await dbContext.SaveChangesAsync();
            }

            var url = new Uri($"/api/events/{calendarEvent.Id}", UriKind.Relative);

            // Act
            var response = await Client.DeleteAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task DeleteEvent_WhenSuccessful_Returns201()
        {
            // Arrange
            var userId = 1;

            var calendarEvent = _fixture.Build<CalendarEvent>()
               .Without(x => x.Project)
               .Without(x => x.ProjectId)
               .With(x => x.UserId, userId)
               .Create();

            using (var dbContext = CreateDbContext())
            {
                dbContext.CalendarEvents.Add(calendarEvent);
                await dbContext.SaveChangesAsync();
            }

            var url = new Uri($"/api/events/{calendarEvent.Id}", UriKind.Relative);

            // Act
            var response = await Client.DeleteAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using (var dbContext = CreateDbContext())
            {
                var dbResponse = await dbContext.CalendarEvents.FindAsync(calendarEvent.Id);
                dbResponse.Should().BeNull();
            }
        }
    }
}