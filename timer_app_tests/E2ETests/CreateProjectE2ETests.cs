using AutoFixture;
using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using timer_app.Boundary.Request;
using timer_app.Boundary.Response;
using timer_app.Factories;

namespace timer_app_tests.E2ETests
{
    [SingleThreaded]
    public class CreateProjectE2ETests : MockWebApplicationFactory
    {
        public HttpClient Client => CreateClient();
        private readonly string AccessToken = GenerateToken();

        private HttpRequestMessage _requestMessage;
        private readonly Fixture _fixture = new Fixture();

        [SetUp]
        public void Setup()
        {
            var url = new Uri($"/api/projects", UriKind.Relative);

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
        public async Task CreateProject_WhenInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var request = _fixture.Create<CreateProjectRequest>();

            var jsonRequest = JsonConvert.SerializeObject(request);
            _requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            _requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "INVALID_TOKEN");

            // Act
            var response = await Client.SendAsync(_requestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task CreateProject_WhenInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var request = new CreateProjectRequest
            {
                Description = "",
                ProjectColor = new ProjectColorRequest
                {
                    Light = "#000000ddd",
                    Lightest = "#000000ddd",
                    Dark = "#000000ddd",
                    Darkest = "#000000ddd",
                }
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            _requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.SendAsync(_requestMessage);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task CreateProject_WhenProjectCreated_Returns200()
        {
            // Arrange
            var request = new CreateProjectRequest
            {
                Description = "Description1234",
                ProjectColor = new ProjectColorRequest
                {
                    Light = "#000000",
                    Lightest = "#000000",
                    Dark = "#000000",
                    Darkest = "#000000",
                }
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            _requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.SendAsync(_requestMessage);

            var stringResult = await response.Content.ReadAsStringAsync();
            var responseContent = JsonConvert.DeserializeObject<ProjectResponse>(stringResult);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseContent.Should().NotBeNull();
            responseContent.Description.Should().Be(request.Description);
            responseContent.ProjectColor.Should().BeEquivalentTo(request.ProjectColor.ToDb().ToResponse());

            using (var dbContext = CreateDbContext())
            {
                var dbResponse = await dbContext.Projects.FindAsync(responseContent.Id);

                dbResponse.Should().NotBeNull();
                dbResponse.Description.Should().Be(request.Description);
                dbResponse.ProjectColor.Should().BeEquivalentTo(request.ProjectColor.ToDb().ToResponse());
            }
        }
    }

}