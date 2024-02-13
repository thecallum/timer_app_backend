using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using timer_app.Boundary.Response;
using timer_app.Infrastructure;

namespace timer_app_tests.E2ETests
{
    [SingleThreaded]
    public class GetAllProjectsE2ETests : MockWebApplicationFactory
    {
        public HttpClient Client => CreateClient();
        private readonly string AccessToken = GenerateToken();

        private HttpRequestMessage _requestMessage;

        private readonly Fixture _fixture = new Fixture();
        private readonly Random _random = new Random();

        [SetUp]
        public void Setup()
        {
            var url = new Uri($"/api/projects/", UriKind.Relative);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

            _requestMessage = requestMessage;
        }

        [TearDown]
        public void TearDown()
        {
            CleanupDb();
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
                    .With(x => x.UserId, UserData.Sub)
                    .CreateMany(numberOfProjects);

                dbContext.Projects.AddRange(projects);
                await dbContext.SaveChangesAsync();
            }

            var url = new Uri($"/api/projects/", UriKind.Relative);

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