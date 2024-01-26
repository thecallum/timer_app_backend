using AutoFixture;
using FluentAssertions;
using FluentAssertions.Extensions;
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
    public class UpdateEventE2ETests : MockWebApplicationFactory
    {
        public HttpClient Client => CreateClient();

        private readonly Fixture _fixture = new Fixture();
        private readonly Random _random = new Random();

        [TearDown]
        public void TearDown()
        {
            CleanupDb();
        }

        [Test]
        public async Task UpdateEvent_WhenInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var eventId = _fixture.Create<int>();

            var url = new Uri($"/api/events/{eventId}", UriKind.Relative);

            var startTime = _fixture.Create<DateTime>();
            var endTime = startTime.AddDays(-1);

            var request = new UpdateEventRequest
            {
                Description = "",
                ProjectId = null,
                StartTime = startTime,
                EndTime = endTime
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PutAsync(url, content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task UpdateEvent_WhenEventNotFound_Returns404()
        {
            // Arrange
            var eventId = _fixture.Create<int>();

            var url = new Uri($"/api/events/{eventId}", UriKind.Relative);

            var startTime = _fixture.Create<DateTime>();
            var endTime = startTime.AddDays(1);

            var request = new UpdateEventRequest
            {
                Description = "Description",
                ProjectId = null,
                StartTime = startTime,
                EndTime = endTime
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PutAsync(url, content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task UpdateEvent_WhenEventNotOwnedByUser_Returns401()
        {
            // Arrange
            var otherUserId = 2;

            var calendarEvent = _fixture.Build<CalendarEvent>()
                .With(x => x.UserId, otherUserId)
                .Without(x => x.Project)
                .Without(x => x.ProjectId)
                .Create();

            using (var dbContext = CreateDbContext())
            {
                dbContext.CalendarEvents.Add(calendarEvent);
                await dbContext.SaveChangesAsync();
            }

            var url = new Uri($"/api/events/{calendarEvent.Id}", UriKind.Relative);

            var startTime = _fixture.Create<DateTime>();
            var endTime = startTime.AddDays(1);

            var request = new UpdateEventRequest
            {
                Description = "Description",
                ProjectId = null,
                StartTime = startTime,
                EndTime = endTime
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PutAsync(url, content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task UpdateEvent_ProjectNotFound_Returns400()
        {
            // Arrange
            var userId = 1;

            var calendarEvent = _fixture.Build<CalendarEvent>()
                .With(x => x.UserId, userId)
                .Without(x => x.Project)
                .Without(x => x.ProjectId)
                .Create();

            using (var dbContext = CreateDbContext())
            {
                dbContext.CalendarEvents.Add(calendarEvent);
                await dbContext.SaveChangesAsync();
            }

            var url = new Uri($"/api/events/{calendarEvent.Id}", UriKind.Relative);

            var startTime = _fixture.Create<DateTime>();
            var endTime = startTime.AddDays(1);

            var request = new UpdateEventRequest
            {
                Description = "Description",
                ProjectId = _fixture.Create<int>(),
                StartTime = startTime,
                EndTime = endTime
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PutAsync(url, content);
            var stringResult = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            stringResult.Should().Be($"Project {request.ProjectId} not found.");
        }

        [Test]
        public async Task UpdateEvent_WhenUserDoesntOwnProject_Returns400()
        {
            // Arrange
            var userId = 1;
            var otherUserId = 2;

            var project = _fixture.Build<Project>()
                .With(x => x.UserId, otherUserId)
                .Without(x => x.CalendarEvents)
                .With(x => x.IsActive, true)
                .Create();

            var calendarEvent = _fixture.Build<CalendarEvent>()
                .With(x => x.UserId, userId)
                .Without(x => x.Project)
                .Without(x => x.ProjectId)
                .Create();

            using (var dbContext = CreateDbContext())
            {
                dbContext.Projects.Add(project);
                dbContext.CalendarEvents.Add(calendarEvent);
                await dbContext.SaveChangesAsync();
            }

            var url = new Uri($"/api/events/{calendarEvent.Id}", UriKind.Relative);

            var startTime = _fixture.Create<DateTime>();
            var endTime = startTime.AddDays(1);

            var request = new UpdateEventRequest
            {
                Description = "Description",
                ProjectId = project.Id,
                StartTime = startTime,
                EndTime = endTime
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PutAsync(url, content);
            var stringResult = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            stringResult.Should().Be($"User {userId} is not authorized to access the requested entity.");
        }

        [Test]
        public async Task UpdateEvent_WhenProjectArchived_Returns400()
        {
            // Arrange
            var userId = 1;

            var project = _fixture.Build<Project>()
                .With(x => x.UserId, userId)
                .With(x => x.IsActive, false)
                .Without(x => x.CalendarEvents)
                .Create();

            var calendarEvent = _fixture.Build<CalendarEvent>()
                .With(x => x.UserId, userId)
                .Without(x => x.Project)
                .Without(x => x.ProjectId)
                .Create();

            using (var dbContext = CreateDbContext())
            {
                dbContext.Projects.Add(project);
                dbContext.CalendarEvents.Add(calendarEvent);
                await dbContext.SaveChangesAsync();
            }

            var url = new Uri($"/api/events/{calendarEvent.Id}", UriKind.Relative);

            var startTime = _fixture.Create<DateTime>();
            var endTime = startTime.AddDays(1);

            var request = new UpdateEventRequest
            {
                Description = "Description",
                ProjectId = project.Id,
                StartTime = startTime,
                EndTime = endTime
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PutAsync(url, content);
            var stringResult = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            stringResult.Should().Be($"The project {project.Id} has been archived.");
        }

        [Test]
        public async Task UpdateEvent_WhenValid_Returns200()
        {
            // Arrange
            var userId = 1;

            var project = _fixture.Build<Project>()
                .With(x => x.UserId, userId)
                .With(x => x.IsActive, true)
                .Without(x => x.CalendarEvents)
                .Create();

            var calendarEvent = _fixture.Build<CalendarEvent>()
                .With(x => x.UserId, userId)
                .Without(x => x.Project)
                .Without(x => x.ProjectId)
                .Create();

            using (var dbContext = CreateDbContext())
            {
                dbContext.Projects.Add(project);
                dbContext.CalendarEvents.Add(calendarEvent);
                await dbContext.SaveChangesAsync();
            }

            var url = new Uri($"/api/events/{calendarEvent.Id}", UriKind.Relative);

            var startTime = _fixture.Create<DateTime>();
            var endTime = startTime.AddDays(1);

            var request = new UpdateEventRequest
            {
                Description = "Description",
                ProjectId = project.Id,
                StartTime = startTime,
                EndTime = endTime
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PutAsync(url, content);
            var stringResult = await response.Content.ReadAsStringAsync();
            var responseContent = JsonConvert.DeserializeObject<CalendarEventResponse>(stringResult);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseContent.Should().NotBeNull();

            responseContent.Description.Should().Be(request.Description);
            responseContent.StartTime.Should().Be(request.StartTime);
            responseContent.EndTime.Should().Be(request.EndTime);
            responseContent.ProjectId.Should().Be(project.Id);

            using (var dbContext = CreateDbContext())
            {
                var dbResponse = await dbContext.CalendarEvents.FindAsync(responseContent.Id);
                dbResponse.Should().NotBeNull();

                dbResponse.Description.Should().Be(request.Description);
                dbResponse.StartTime.Should().BeCloseTo(request.StartTime, 1.Seconds());
                dbResponse.EndTime.Should().BeCloseTo(request.EndTime, 1.Seconds());
                dbResponse.ProjectId.Should().Be(project.Id);
            }

        }
    }
}