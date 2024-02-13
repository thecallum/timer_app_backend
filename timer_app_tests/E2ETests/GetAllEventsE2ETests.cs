using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using timer_app.Boundary.Request;
using timer_app.Boundary.Response;
using timer_app.Infrastructure;

namespace timer_app_tests.E2ETests
{
    [SingleThreaded]
    public class GetAllEventsE2ETests : MockWebApplicationFactory
    {
        public HttpClient Client => CreateClient();
        private readonly string AccessToken = GenerateToken();

        private readonly Fixture _fixture = new Fixture();
        private readonly Random _random = new Random();

        [TearDown]
        public void TearDown()
        {
            CleanupDb();
        }

        private static string FormatDate(DateTime dateTime)
        {
            // Format the DateTime in a URL-safe way (ISO 8601 format)
            string formattedDate = dateTime.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

            // Return the URL-encoded date string
            return Uri.EscapeDataString(formattedDate);
        }

        [Test]
        public async Task GetAllEvents_WhenInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var startTime = _fixture.Create<DateTime>();
            var endTime = startTime.AddDays(-2);

            var url = new Uri($"/api/events?startTime={FormatDate(startTime)}&endTime={FormatDate(endTime)}", UriKind.Relative);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "INVALID_TOKEN");

            // Act
            var response = await Client.SendAsync(requestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetAllEvents_WhenInvalidParameters_ReturnsBadRequest()
        {
            // Arrange
            var startTime = _fixture.Create<DateTime>();
            var endTime = startTime.AddDays(-2);

            var url = new Uri($"/api/events?startTime={FormatDate(startTime)}&endTime={FormatDate(endTime)}", UriKind.Relative);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            // Act
            var response = await Client.SendAsync(requestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetAllEvents_WhenNoneFound_Returns200()
        {
            // Arrange
            var startTime = _fixture.Create<DateTime>();
            var endTime = startTime.AddDays(2);

            var url = new Uri($"/api/events?startTime={FormatDate(startTime)}&endTime={FormatDate(endTime)}", UriKind.Relative);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            // Act
            var response = await Client.SendAsync(requestMessage);

            var stringResult = await response.Content.ReadAsStringAsync();
            var responseContent = JsonConvert.DeserializeObject<IEnumerable<CalendarEventResponse>>(stringResult);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseContent.Should().NotBeNull();
            responseContent.Should().BeEmpty();
        }


        [Test]
        public async Task GetAllEvents_WhenCalled_Returns200()
        {
            // Arrange
            var numberOfEvents = _random.Next(2, 5);

            var startTime = _fixture.Create<DateTime>();
            var endTime = startTime.AddDays(2);

            using (var dbContext = CreateDbContext())
            {
                var project = _fixture.Build<Project>()
                    .Without(x => x.CalendarEvents)
                    .With(x => x.UserId, UserData.Id)
                    .Create();

                dbContext.Projects.Add(project);
                await dbContext.SaveChangesAsync();

                var events = _fixture.Build<CalendarEvent>()
                    .With(x => x.UserId, UserData.Id)
                    .With(x => x.ProjectId, project.Id)
                    .With(x => x.StartTime, startTime.AddHours(2))
                    .With(x => x.EndTime, endTime.AddHours(2))
                    .Without(x => x.Project)
                    .CreateMany(numberOfEvents);

                dbContext.CalendarEvents.AddRange(events);
                await dbContext.SaveChangesAsync();
            }

            var url = new Uri($"/api/events?startTime={FormatDate(startTime)}&endTime={FormatDate(endTime)}", UriKind.Relative);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            var request = new GetAllEventsRequest
            {
                StartTime = startTime,
                EndTime = endTime,
            };

            // Act
            var response = await Client.SendAsync(requestMessage);

            var stringResult = await response.Content.ReadAsStringAsync();
            var responseContent = JsonConvert.DeserializeObject<IEnumerable<CalendarEventResponse>>(stringResult);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseContent.Should().NotBeNull();
            responseContent.Should().HaveCount(numberOfEvents);

            foreach (var calendarEvent in responseContent)
            {
                calendarEvent.ProjectId.Should().NotBeNull();
            }
        }
    }
}