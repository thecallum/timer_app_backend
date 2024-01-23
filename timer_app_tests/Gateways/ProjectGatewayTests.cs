using AutoFixture;
using FluentAssertions;
using timer_app.Boundary.Request;
using timer_app.Factories;
using timer_app.Gateway;
using timer_app.Infrastructure;
using timer_app.Infrastructure.Exceptions;

namespace timer_app_tests.GatewayTests
{
    public class ProjectGatewayTests
    {
        private ProjectGateway _classUnderTest;

        private readonly Fixture _fixture = new Fixture();
        private readonly Random _randm = new Random();

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new ProjectGateway(MockDbContext.Instance);
        }


        [TearDown]
        public void Teardown()
        {
            MockDbContext.Teardown();
        }

        [Test]
        public async Task GetAllProjects_WhenCalled_ReturnsAllProjectsAssignedToUser()
        {
            // Arrange
            var userId = _fixture.Create<int>();
            var otherUserId = _fixture.Create<int>();

            var numberOfProjects = _randm.Next(2, 10);
            var projects = _fixture.Build<Project>()
                .With(x => x.UserId, userId)
                .With(x => x.IsActive, true)
                .Without(x => x.CalendarEvents)
                .CreateMany(numberOfProjects);

            var projectOwnedByOtherUser = _fixture.Build<Project>()
                .With(x => x.UserId, otherUserId)
                .Without(x => x.CalendarEvents)
                .Create();

            await GatewayTestHelpers.AddProjectsToDb(projectOwnedByOtherUser);
            await GatewayTestHelpers.AddProjectsToDb(projects.ToArray());

            // Act
            var results = await _classUnderTest.GetAllProjects(userId);

            // Assert
            results.Should().HaveCount(numberOfProjects);
        }

        [Test]
        public async Task GetAllProjects_WhenNoProjectsFound_ReturnsEmptyList()
        {
            // Arrange
            var userId = _fixture.Create<int>();
            var otherUserId = _fixture.Create<int>();

            var numberOfProjects = _randm.Next(2, 10);

            var projects = _fixture.Build<Project>()
                .With(x => x.UserId, otherUserId)
                .With(x => x.IsActive, true)
                .Without(x => x.CalendarEvents)
                .CreateMany(numberOfProjects);

            await GatewayTestHelpers.AddProjectsToDb(projects.ToArray());

            // Act
            var results = await _classUnderTest.GetAllProjects(userId);

            // Assert
            results.Should().HaveCount(0);
        }

        [Test]
        public async Task CreateProject_WhenCalled_CreatesProject()
        {
            // Arrange
            var userId = _fixture.Create<int>();
            var request = _fixture.Create<CreateProjectRequest>();

            // Act
            var result = await _classUnderTest.CreateProject(request, userId);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);

            var dbResponse = await GatewayTestHelpers.GetProject(result.Id);
            dbResponse.Should().NotBeNull();

            dbResponse.Should().BeEquivalentTo(result);
            dbResponse.UserId.Should().Be(userId);
        }

        [Test]
        public async Task UpdateProject_WhenProjectNotFound_ReturnsFalse()
        {
            // Arrange
            var userId = _fixture.Create<int>();
            var projectId = _fixture.Create<int>();
            var request = _fixture.Create<UpdateProjectRequest>();

            // Act
            var result = await _classUnderTest.UpdateProject(projectId, request, userId);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task UpdateProject_WhenUserDoesntOwnProject_ThrowsUnauthorizedException()
        {
            // Arrange
            var userId = _fixture.Create<int>();
            var otherUserId = userId + 1;

            var project = _fixture.Build<Project>()
                .Without(x => x.CalendarEvents)
                .With(x => x.UserId, otherUserId)
                .With(x => x.IsActive, true)
                .Create();

            await GatewayTestHelpers.AddProjectsToDb(project);

            var request = new UpdateProjectRequest();

            // Act
            Func<Task> task = async () => await _classUnderTest.UpdateProject(project.Id, request, userId);

            // Assert
            await task.Should().ThrowAsync<UserUnauthorizedToAccessProjectException>();
        }

        [Test]
        public async Task UpdateProject_WhenProjectIsArchived_ThrowsArchivedException()
        {
            // Arrange
            var userId = _fixture.Create<int>();
            var project = _fixture.Build<Project>()
                .Without(x => x.CalendarEvents)
                .With(x => x.IsActive, false)
                .With(x => x.UserId, userId)
                .Create();

            await GatewayTestHelpers.AddProjectsToDb(project);

            var request = new UpdateProjectRequest();

            // Act
            Func<Task> task = async () => await _classUnderTest.UpdateProject(project.Id, request, userId);

            // Assert
            await task.Should().ThrowAsync<ProjectIsArchivedException>();
        }

        [Test]
        public async Task UpdateProject_WhenCalled_UpdatesProject()
        {
            // Arrange
            var userId = _fixture.Create<int>();

            var project = _fixture.Build<Project>()
                .With(x => x.UserId, userId)
                .With(x => x.IsActive, true)
                .Without(x => x.CalendarEvents)
                .Create();

            await GatewayTestHelpers.AddProjectsToDb(project);

            var request = _fixture.Create<UpdateProjectRequest>();

            // Act
            var result = await _classUnderTest.UpdateProject(project.Id, request, userId);

            // Assert
            result.Should().NotBeNull();

            result.Description.Should().Be(request.Description);
            result.ProjectColor.Should().BeEquivalentTo(request.ProjectColor.ToDb().ToResponse());

            var dbResponse = await GatewayTestHelpers.GetProject(project.Id);
            dbResponse.Should().NotBeNull();

            dbResponse.Description.Should().Be(request.Description);
            dbResponse.ProjectColor.Should().BeEquivalentTo(request.ProjectColor.ToDb().ToResponse());
        }

        [Test]
        public async Task Delete_WhenProjectNotFound_ReturnsFalse()
        {
            // Arrange
            var userId = _fixture.Create<int>();
            var projectId = _fixture.Create<int>();

            // Act
            var result = await _classUnderTest.DeleteProject(projectId, userId);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task DeleteProject_WhenUserDoesntOwnProject_ThrowsUnauthorizedException()
        {
            // Arrange
            var userId = _fixture.Create<int>();
            var project = _fixture.Build<Project>()
                .With(x => x.IsActive, true)
                .Without(x => x.CalendarEvents)
                .Create();

            await GatewayTestHelpers.AddProjectsToDb(project);

            // Act
            Func<Task> task = async () => await _classUnderTest.DeleteProject(project.Id, userId);

            // Assert
            await task.Should().ThrowAsync<UserUnauthorizedToAccessProjectException>();
        }

        [Test]
        public async Task DeleteProject_WhenArchived_ThrowsArchivedException()
        {
            // Arrange
            var userId = _fixture.Create<int>();
            
            var project = _fixture.Build<Project>()
                .Without(x => x.CalendarEvents)
                .With(x => x.UserId, userId)
                .With(x => x.IsActive, false)
                .Create();

            await GatewayTestHelpers.AddProjectsToDb(project);

            // Act
            Func<Task> task = async () => await _classUnderTest.DeleteProject(project.Id, userId);

            // Assert
            await task.Should().ThrowAsync<ProjectIsArchivedException>();
        }

        [Test]
        public async Task DeleteProject_WhenCalled_DeletesProject()
        {
            // Arrange
            var userId = _fixture.Create<int>();

            var project = _fixture.Build<Project>()
                .With(x => x.UserId, userId)
                .With(x => x.IsActive, true)
                .Without(x => x.CalendarEvents)
                .Create();

            await GatewayTestHelpers.AddProjectsToDb(project);

            // Act
            var result = await _classUnderTest.DeleteProject(project.Id, userId);

            // Assert
            result.Should().BeTrue();

            var dbResponse = await GatewayTestHelpers.GetProject(project.Id);
            dbResponse.Should().NotBeNull();
            dbResponse.IsActive.Should().BeFalse();
        }
    }
}
