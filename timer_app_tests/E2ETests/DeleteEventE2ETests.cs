using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using timer_app.Boundary.Request;
using timer_app.Infrastructure;

namespace timer_app_tests.E2ETests
{
    [SingleThreaded]
    public class DeleteEventE2ETests : MockWebApplicationFactory
    {
        public HttpClient Client => CreateClient();
        private readonly string AccessToken = GenerateToken();

        [TearDown]
        public void TearDown()
        {
            CleanupDb();
        }

        [Test]
        public async Task DeleteEvent_WhenInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var eventId = _fixture.Create<int>();

            var url = new Uri($"/api/events/{eventId}", UriKind.Relative);

            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "INVALID_TOKEN");

            // Act
            var response = await Client.SendAsync(requestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task DeleteEvent_WhenNotFound_Returns404()
        {
            // Arrange
            var eventId = _fixture.Create<int>();

            var url = new Uri($"/api/events/{eventId}", UriKind.Relative);

            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            // Act
            var response = await Client.SendAsync(requestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task DeleteEvent_WhenEventNotOwnedByUser_Returns401()
        {
            // Arrange
            var otherUserId = _fixture.Create<string>();

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

            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            // Act
            var response = await Client.SendAsync(requestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task DeleteEvent_WhenSuccessful_Returns201()
        {
            // Arrange
            var calendarEvent = _fixture.Build<CalendarEvent>()
               .Without(x => x.Project)
               .Without(x => x.ProjectId)
               .With(x => x.UserId, UserData.Id)
               .Create();

            using (var dbContext = CreateDbContext())
            {
                dbContext.CalendarEvents.Add(calendarEvent);
                await dbContext.SaveChangesAsync();
            }

            var url = new Uri($"/api/events/{calendarEvent.Id}", UriKind.Relative);

            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            // Act
            var response = await Client.SendAsync(requestMessage);

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