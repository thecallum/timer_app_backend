using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using timer_app.Boundary.Request;
using timer_app.Boundary.Response;

namespace timer_app_tests.E2ETests
{
    [SingleThreaded]
    public class CreateProjectE2ETests : MockWebApplicationFactory
    {
        public HttpClient Client => CreateClient();

        [TearDown]
        public void TearDown()
        {
            CleanupDb();
        }

        [Test]
        public async Task CreateProject_WhenInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var url = new Uri($"/api/projects", UriKind.Relative);
            var request = new CreateProjectRequest
            {
                Description = "",
                DisplayColour = "#000000hsjofsf"
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync(url, content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task CreateProject_WhenProjectCreated_Returns200()
        {
            // Arrange
            var url = new Uri($"/api/projects", UriKind.Relative);
            var request = new CreateProjectRequest
            {
                Description = "Description1234",
                DisplayColour = "#000000"
            };

            var jsonRequest = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync(url, content);

            var stringResult = await response.Content.ReadAsStringAsync();
            var responseContent = JsonConvert.DeserializeObject<ProjectResponse>(stringResult);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseContent.Should().NotBeNull();

            responseContent.Description.Should().Be(request.Description);
            responseContent.DisplayColour.Should().Be(request.DisplayColour);

            using (var dbContext = CreateDbContext())
            {
                var dbResponse = await dbContext.Projects.FindAsync(responseContent.Id);

                dbResponse.Should().NotBeNull();

                dbResponse.Description.Should().Be(request.Description);
                dbResponse.DisplayColour.Should().Be(request.DisplayColour);
            }
        }
    }

}