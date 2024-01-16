using AutoFixture;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                .Without(x => x.CalendarEvents)
                .CreateMany(numberOfProjects);

            MockDbContext.Instance.Projects.AddRange(projects);

            var projectOwnedByOtherUser = _fixture.Build<Project>()
                .With(x => x.UserId, otherUserId)
                .Without(x => x.CalendarEvents)
                .Create();

            MockDbContext.Instance.Projects.Add(projectOwnedByOtherUser);

            await MockDbContext.Instance.SaveChangesAsync();

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
                .Without(x => x.CalendarEvents)
                .CreateMany(numberOfProjects);

            MockDbContext.Instance.Projects.AddRange(projects);

            await MockDbContext.Instance.SaveChangesAsync();

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
            var project = _fixture.Build<Project>()
                .Without(x => x.CalendarEvents)
                .Create();

            // Act
            var result = await _classUnderTest.CreateProject(project, userId);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);

            var dbResponse = await MockDbContext.Instance.Projects.FindAsync(result.Id);
            dbResponse.Should().NotBeNull();

            dbResponse.Should().BeEquivalentTo(result);
            dbResponse.UserId.Should().Be(userId);
        }

        [Test]
        public async Task UpdateProject_WhenProjectNotFound_ReturnsFalse()
        {
            // Arrange
            var userId = _fixture.Create<int>();
            var project = _fixture.Build<Project>()
                .Without(x => x.CalendarEvents)
                .Create();

            // Act
            var result = await _classUnderTest.UpdateProject(project, userId);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task UpdateProject_WhenUserDoesntOwnProject_ThrowsUnauthorizedException()
        {
            // Arrange
            var userId = _fixture.Create<int>();
            var project = _fixture.Build<Project>()
                .Without(x => x.CalendarEvents)
                .Create();

            MockDbContext.Instance.Projects.Add(project);
            await MockDbContext.Instance.SaveChangesAsync();

            // Act
            Func<Task> task = async () => await _classUnderTest.UpdateProject(project, userId);

            // Assert
            await task.Should().ThrowAsync<UserUnauthorizedException>();
        }

        [Test]
        public async Task UpdateProject_WhenCalled_UpdatesProject()
        {
            // Arrange
            var userId = _fixture.Create<int>();
            var project = _fixture.Build<Project>()
                .With(x => x.UserId, userId)
                .Without(x => x.CalendarEvents)
                .Create();

            MockDbContext.Instance.Projects.Add(project);
            await MockDbContext.Instance.SaveChangesAsync();

            var projectWithUpdates = new Project
            {
                Id = project.Id,
                UserId = userId,
                Description = _fixture.Create<string>(),
                DisplayColour = _fixture.Create<string>(),
            };

            // Act
            var result = await _classUnderTest.UpdateProject(projectWithUpdates, userId);

            // Assert
            result.Should().BeTrue();

            var dbResponse = await MockDbContext.Instance.Projects.FindAsync(project.Id);
            dbResponse.Should().NotBeNull();

            dbResponse.Should().BeEquivalentTo(projectWithUpdates);
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
                .Without(x => x.CalendarEvents)
                .Create();

            MockDbContext.Instance.Projects.Add(project);
            await MockDbContext.Instance.SaveChangesAsync();

            // Act
            Func<Task> task = async () => await _classUnderTest.DeleteProject(project.Id, userId);

            // Assert
            await task.Should().ThrowAsync<UserUnauthorizedException>();
        }

        [Test]
        public async Task DeleteProject_WhenCalled_DeletesProject()
        {
            // Arrange
            var userId = _fixture.Create<int>();
            var project = _fixture.Build<Project>()
                .With(x => x.UserId, userId)
                .Without(x => x.CalendarEvents)
                .Create();

            MockDbContext.Instance.Projects.Add(project);
            await MockDbContext.Instance.SaveChangesAsync();

            // Act
            var result = await _classUnderTest.DeleteProject(project.Id, userId);

            // Assert
            result.Should().BeTrue();

            var dbResponse = await MockDbContext.Instance.Projects.FindAsync(project.Id);
            dbResponse.Should().BeNull();
        }
    }
}
