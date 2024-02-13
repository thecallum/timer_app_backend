using AutoFixture;
using FluentAssertions;
using FluentAssertions.Extensions;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using timer_app.Boundary.Request;
using timer_app.Boundary.Response;
using timer_app.Infrastructure;

namespace timer_app_tests.E2ETests
{
    [SingleThreaded]
    public class CreateEventE2ETests : MockWebApplicationFactory
    {
        public HttpClient Client => CreateClient();
        private readonly string AccessToken = GenerateToken();

        private readonly Fixture _fixture = new Fixture();
        private readonly Random _random = new Random();

        private HttpRequestMessage _requestMessage;

        [SetUp]
        public void Setup()
        {
            var url = new Uri($"/api/events", UriKind.Relative);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            _requestMessage = requestMessage;
        }

        [TearDown]
        public void TearDown()
        {
            CleanupDb();
        }

        [Test]
        public async Task CreateEvent_WhenInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var request = _fixture.Create<CreateEventRequest>();

            var jsonRequest = JsonConvert.SerializeObject(request);
            _requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            _requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "INVALID_TOKEN");

            // Act
            var response = await Client.SendAsync(_requestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task CreateEvent_WhenInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var startTime = _fixture.Create<DateTime>();
            var endTime = startTime.AddDays(-1);

            var request = new CreateEventRequest
            {
                Description = "",
                ProjectId = null,
                StartTime = startTime,
                EndTime = endTime
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            _requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.SendAsync(_requestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task CreateEvent_WhenProjectNotFound_ReturnsBadRequest()
        {
            // Arrange
            var startTime = _fixture.Create<DateTime>();
            var endTime = startTime.AddDays(1);
            var projectId = _fixture.Create<int>();

            var request = new CreateEventRequest
            {
                Description = "Description",
                ProjectId = projectId,
                StartTime = startTime,
                EndTime = endTime
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            _requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.SendAsync(_requestMessage);

            var stringResult = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            stringResult.Should().Be($"Project {projectId} not found.");
        }

        [Test]
        public async Task CreateEvent_WhenUserDoesntOwnProject_ReturnsBadRequest()
        {
            // Arrange
            var otherUserId = "9876";

            using var dbContext = CreateDbContext();

            var project = _fixture.Build<Project>()
                .Without(x => x.CalendarEvents)
                .With(x => x.UserId, otherUserId)
                .With(x => x.IsActive, true)
                .Create();

            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();

            var startTime = _fixture.Create<DateTime>();
            var endTime = startTime.AddDays(1);

            var request = new CreateEventRequest
            {
                Description = "Description",
                ProjectId = project.Id,
                StartTime = startTime,
                EndTime = endTime
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            _requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.SendAsync(_requestMessage);

            var stringResult = await response.Content.ReadAsStringAsync();
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            stringResult.Should().Be($"User {UserData.Sub} is not authorized to access the requested entity.");
        }

        [Test]
        public async Task CreateEvent_WhenProjectIsArchived_Returns400()
        {
            // Arrange
            using var dbContext = CreateDbContext();

            var project = _fixture.Build<Project>()
                .Without(x => x.CalendarEvents)
                .With(x => x.UserId, UserData.Sub)
                .With(x => x.IsActive, false)
                .Create();

            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();

            var startTime = _fixture.Create<DateTime>();
            var endTime = startTime.AddDays(1);

            var request = new CreateEventRequest
            {
                Description = "Description",
                ProjectId = project.Id,
                StartTime = startTime,
                EndTime = endTime
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            _requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.SendAsync(_requestMessage);

            var stringResult = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            stringResult.Should().Be($"The project {project.Id} has been archived.");
        }

        [Test]
        public async Task CreateEvent_WhenValid_Returns200()
        {
            // Arrange
            var project = _fixture.Build<Project>()
                .Without(x => x.CalendarEvents)
                .With(x => x.UserId, UserData.Sub)
                .With(x => x.IsActive, true)
                .Create();

            using (var dbContext = CreateDbContext())
            {
                dbContext.Projects.Add(project);
                await dbContext.SaveChangesAsync();
            }

            var startTime = _fixture.Create<DateTime>();
            var endTime = startTime.AddDays(1);

            var request = new CreateEventRequest
            {
                Description = "Description",
                ProjectId = project.Id,
                StartTime = startTime,
                EndTime = endTime
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            _requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.SendAsync(_requestMessage);

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