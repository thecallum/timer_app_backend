using AutoFixture;
using Moq;
using timer_app.Controllers;
using timer_app.UseCases.Interfaces;
using timer_app.Boundary.Response;
using FluentAssertions;
using timer_app.Boundary.Request;
using timer_app.Infrastructure.Exceptions;

namespace timer_app_tests.Controller
{
    public class ProjectsControllerTests : ControllerTests
    {
        private ProjectsController _classUnderTest;

        private Mock<IGetAllProjectsUseCase> _getAllProjectsUseCaseMock;
        private Mock<IUpdateProjectUseCase> _updateProjectUseCaseMock;
        private Mock<IDeleteProjectUseCase> _deleteProjectUseCaseMock;
        private Mock<ICreateProjectUseCase> _createProjectUseCaseMock;

        private readonly Fixture _fixture = new Fixture();
        private readonly Random _random = new Random();

        [SetUp]
        public void Setup()
        {
            _getAllProjectsUseCaseMock = new Mock<IGetAllProjectsUseCase>();
            _updateProjectUseCaseMock = new Mock<IUpdateProjectUseCase>();
            _deleteProjectUseCaseMock = new Mock<IDeleteProjectUseCase>();
            _createProjectUseCaseMock = new Mock<ICreateProjectUseCase>();

            _classUnderTest = new ProjectsController(
                _getAllProjectsUseCaseMock.Object,
                _updateProjectUseCaseMock.Object,
                _deleteProjectUseCaseMock.Object,
                _createProjectUseCaseMock.Object
            );
        }

        [Test]
        public async Task GetAllProjects_WhenCalled_ReturnsProjects()
        {
            // Arrange
            var numberOfResults = _random.Next(2, 5);
            var useCaseResponse = _fixture.CreateMany<ProjectResponse>(numberOfResults);

            _getAllProjectsUseCaseMock
                .Setup(x => x.ExecuteAsync(It.IsAny<int>()))
                .ReturnsAsync(useCaseResponse);

            // Act
            var result = await _classUnderTest.GetAllProjects();
            var responseObject = GetResultData<IEnumerable<ProjectResponse>>(result);
            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(200);
            responseObject.Should().HaveCount(numberOfResults);
        }

        [Test]
        public async Task CreateProject_WhenCalled_Returns200()
        {
            // Arrange
            var request = _fixture.Create<CreateProjectRequest>();
            var useCaseResponse = _fixture.Create<ProjectResponse>();

            _createProjectUseCaseMock
                .Setup(x => x.ExecuteAsync(request, It.IsAny<int>()))
                .ReturnsAsync(useCaseResponse);

            // Act
            var result = await _classUnderTest.CreateProject(request);
            var responseObject = GetResultData<ProjectResponse>(result);
            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(200);
            responseObject.Should().Be(useCaseResponse);
        }

        [Test]
        public async Task DeleteProject_WhenNotFound_Returns404()
        {
            // Arrange
            var query = _fixture.Create<ProjectQuery>();

            _deleteProjectUseCaseMock
                .Setup(x => x.ExecuteAsync(query.ProjectId, It.IsAny<int>()))
                .ReturnsAsync(false);

            // Act
            var result = await _classUnderTest.DeleteProject(query);
            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(404);
        }

        [Test]
        public async Task DeleteProject_WhenUnauthorized_Returns401()
        {
            // Arrange
            var query = _fixture.Create<ProjectQuery>();

            _deleteProjectUseCaseMock
                .Setup(x => x.ExecuteAsync(query.ProjectId, It.IsAny<int>()))
                .ThrowsAsync(new UserUnauthorizedToAccessProjectException(query.ProjectId));

            // Act
            var result = await _classUnderTest.DeleteProject(query);
            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(401);
        }

        [Test]
        public async Task DeleteProject_WhenSuccessful_Returns204NoContent()
        {
            // Arrange
            var query = _fixture.Create<ProjectQuery>();

            _deleteProjectUseCaseMock
                .Setup(x => x.ExecuteAsync(query.ProjectId, It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            var result = await _classUnderTest.DeleteProject(query);
            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(204);
        }

        [Test]
        public async Task UpdateProject_WhenNotFound_Returns404()
        {
            // Arrange
            var query = _fixture.Create<ProjectQuery>();
            var request = _fixture.Create<UpdateProjectRequest>();

            _updateProjectUseCaseMock
                .Setup(x => x.ExecuteAsync(query.ProjectId, request, It.IsAny<int>()))
                .ReturnsAsync((ProjectResponse) null);

            // Act
            var result = await _classUnderTest.UpdateProject(query, request);
            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(404);
        }

        [Test]
        public async Task UpdateProject_WhenUnauthorized_Returns401()
        {
            // Arrange
            var query = _fixture.Create<ProjectQuery>();
            var request = _fixture.Create<UpdateProjectRequest>();

            _updateProjectUseCaseMock
                .Setup(x => x.ExecuteAsync(query.ProjectId, request, It.IsAny<int>()))
               .ThrowsAsync(new UserUnauthorizedToAccessProjectException(query.ProjectId));

            // Act
            var result = await _classUnderTest.UpdateProject(query, request);

            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(401);
        }

        [Test]
        public async Task UpdateProject_WhenSuccessful_Returns200()
        {
            // Arrange
            var query = _fixture.Create<ProjectQuery>();
            var request = _fixture.Create<UpdateProjectRequest>();

            var useCaseResponse = _fixture.Create<ProjectResponse>();

            _updateProjectUseCaseMock
                .Setup(x => x.ExecuteAsync(query.ProjectId, request, It.IsAny<int>()))
                .ReturnsAsync(useCaseResponse);

            // Act
            var result = await _classUnderTest.UpdateProject(query, request);

            var responseObject = GetResultData<ProjectResponse>(result);
            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(200);
            responseObject.Should().Be(useCaseResponse);
        }
    }
}
