using AutoFixture;
using FluentAssertions;
using timer_app.Boundary.Request;
using timer_app.Factories;
using timer_app.Gateway;
using timer_app.Infrastructure;
using timer_app.Infrastructure.Exceptions;

namespace timer_app_tests.GatewayTests
{
    public class CalendarEventGatewayTests
    {
        private CalendarEventsGateway _classUnderTest;

        private readonly Fixture _fixture = new Fixture();
        private readonly Random _random = new Random();

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new CalendarEventsGateway(MockDbContext.Instance);
        }

        [TearDown]
        public void Teardown()
        {
            MockDbContext.Teardown();
        }

        [Test]
        public async Task GetAllEvents_WhenNoEventsFound_ReturnsEmptyList()
        {
            // Arrange
            var startTime = _fixture.Create<DateTime>();
            var endTime = startTime.AddDays(1);
            var userId = _fixture.Create<string>();

            // Act
            var results = await _classUnderTest.GetAllEvents(userId, startTime, endTime);

            // Assert
            results.Should().BeEmpty();
        }

        [Test]
        public async Task GetAllEvents_WhenCalled_ReturnsAllEventsAssignedToUser()
        {
            // Arrange
            var startTime = _fixture.Create<DateTime>();
            var endTime = startTime.AddDays(1);
            var userId = _fixture.Create<string>();
            var otherUserId = _fixture.Create<string>();

            var numberOfEvents = _random.Next(2, 6);
            var events = _fixture.Build<CalendarEvent>()
                .With(x => x.UserId, userId)
                .With(x => x.StartTime, startTime.AddHours(1))
                .With(x => x.EndTime, endTime.AddHours(-1))
                .Without(x => x.Project)
                .CreateMany(numberOfEvents);

            var eventForAnotherUser = _fixture.Build<CalendarEvent>()
                .With(x => x.UserId, otherUserId)
                .With(x => x.StartTime, startTime.AddHours(1))
                .With(x => x.EndTime, endTime.AddHours(-1))
                .Without(x => x.Project)
                .Create();


            await GatewayTestHelpers.AddEventsToDb(events.ToArray());
            await GatewayTestHelpers.AddEventsToDb(eventForAnotherUser);

            // Act
            var results = await _classUnderTest.GetAllEvents(userId, startTime, endTime);

            // Assert
            results.Should().HaveCount(numberOfEvents);
            results.Should().BeEquivalentTo(events);
        }

        [Test]
        public async Task GetAllEventsWhenCalled_ReturnsEventsWithinTheGivenTimeframe()
        {
            // Arrange
            var startTime = _fixture.Create<DateTime>();
            var endTime = startTime.AddDays(1);
            var userId = _fixture.Create<string>();

            var numberOfEvents = _random.Next(3, 10);

            var calendarEvents = _fixture.Build<CalendarEvent>()
                .With(x => x.UserId, userId)
                .With(x => x.StartTime, startTime.AddHours(1))
                .With(x => x.EndTime, startTime.AddHours(2))
                .Without(x => x.Project)
                .CreateMany(numberOfEvents)
                .ToList();

            // set first event before startTime
            calendarEvents[0].StartTime = startTime.AddDays(-2);
            calendarEvents[0].EndTime = startTime.AddDays(-2);

            // set second event after endTime
            calendarEvents[1].StartTime = startTime.AddDays(2);
            calendarEvents[1].EndTime = startTime.AddDays(3);

            await GatewayTestHelpers.AddEventsToDb(calendarEvents.ToArray());

            // Act
            var results = await _classUnderTest.GetAllEvents(userId, startTime, endTime);

            // Assert
            results.Should().HaveCount(numberOfEvents - 2);
            results.Should().NotContain(calendarEvents[0]);
            results.Should().NotContain(calendarEvents[1]);
        }

        [Test]
        public async Task CreateEvent_WhenCalled_CreatesEvent()
        {
            // Arrange
            var userId = _fixture.Create<string>();
            var request = _fixture.Build<CreateEventRequest>()
                .Without(x => x.ProjectId)
                .Create();

            // Act
            var result = await _classUnderTest.CreateEvent(request, userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(request.ToDb(userId).ToResponse(), x => x.Excluding(x => x.Id));

            var dbResult = await GatewayTestHelpers.GetEvent(result.Id);
            dbResult.Should().NotBeNull();
            dbResult.Should().BeEquivalentTo(result);

            dbResult.UserId.Should().Be(userId);
        }

        [Test]
        public async Task CreateEvent_WhenProjectNotFound_ThrowsException()
        {
            // Arrange
            var userId = _fixture.Create<string>();
            var request = _fixture.Create<CreateEventRequest>();

            // Act
            Func<Task> func = async () => await _classUnderTest.CreateEvent(request, userId);

            // Assert
            await func.Should().ThrowAsync<ProjectNotFoundException>();
        }

        [Test]
        public async Task CreateEvent_WhenProjectNotOwnedByUser_ThrowsException()
        {
            // Arrange
            var userId = _fixture.Create<string>();

            var project = _fixture.Build<Project>()
                .Without(x => x.CalendarEvents)
                .With(x => x.IsActive, true)
                .Create();

            await GatewayTestHelpers.AddProjectsToDb(project);

            var request = _fixture.Build<CreateEventRequest>()
                .With(x => x.ProjectId, project.Id)
                .Create();

            // Act
            Func<Task> func = async () => await _classUnderTest.CreateEvent(request, userId);

            // Assert
            await func.Should().ThrowAsync<UserUnauthorizedToAccessProjectException>();
        }

        [Test]
        public async Task CreateEvent_WhenProjectArchived_ThrowsException()
        {
            // Arrange
            var userId = _fixture.Create<string>();

            var project = _fixture.Build<Project>()
                .Without(x => x.CalendarEvents)
                .With(x => x.UserId, userId)
                .With(x => x.IsActive, false)
                .Create();

            await GatewayTestHelpers.AddProjectsToDb(project);

            var request = _fixture.Build<CreateEventRequest>()
                .With(x => x.ProjectId, project.Id)
                .Create();

            // Act
            Func<Task> func = async () => await _classUnderTest.CreateEvent(request, userId);

            // Assert
            await func.Should().ThrowAsync<ProjectIsArchivedException>();
        }

        [Test]
        public async Task CreateEvent_WhenCalledWithProject_AssignsProjectToEvent()
        {
            // Arrange
            var userId = _fixture.Create<string>();

            var project = _fixture.Build<Project>()
                .With(x => x.UserId, userId)
                .Without(x => x.CalendarEvents)
                .With(x => x.IsActive, true)
                .Create();

            await GatewayTestHelpers.AddProjectsToDb(project);

            var request = _fixture.Build<CreateEventRequest>()
                .With(x => x.ProjectId, project.Id)
                .Create();

            // Act
            var result = await _classUnderTest.CreateEvent(request, userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(request.ToDb(userId).ToResponse(), x => x.Excluding(x => x.Id));
            result.Project.Should().BeEquivalentTo(project.ToResponse(), options =>
                options.Excluding(x => x.TotalEventDurationInMinutes));

            var dbResult = await GatewayTestHelpers.GetEvent(result.Id);
            dbResult.Should().NotBeNull();

            dbResult.Project.Should().NotBeNull();
            dbResult.Project.Should().BeEquivalentTo(project.ToResponse(), options =>
                options.Excluding(x => x.TotalEventDurationInMinutes));
        }

        [Test]
        public async Task UpdateEvent_WhenEventNotFound_ReturnsFalse()
        {
            // Arrange
            var userId = _fixture.Create<string>();
            var calendarEventId = _fixture.Create<int>();

            var request = _fixture.Build<UpdateEventRequest>()
               .Without(x => x.ProjectId)
               .Create();

            // Act
            var response = await _classUnderTest.UpdateEvent(calendarEventId, request, userId);

            // Assert
            response.Should().BeNull();
        }

        [Test]
        public async Task UpdateEvent_WhenEventDoesntBelongToUser_ThrowsUnauthorizedException()
        {
            // Arrange
            var userId = _fixture.Create<string>();
            var otherUserId = userId + 1;

            var request = _fixture.Build<UpdateEventRequest>()
               .Without(x => x.ProjectId)
               .Create();

            var calendarEvent = _fixture.Build<CalendarEvent>()
                .With(x => x.UserId, otherUserId)
                .Without(x => x.Project)
                .Without(x => x.ProjectId)
                .Create();

            await GatewayTestHelpers.AddEventsToDb(calendarEvent);

            // Act
            Func<Task> task = async () => await _classUnderTest.UpdateEvent(calendarEvent.Id, request, userId);

            // Assert
            await task.Should().ThrowAsync<UserUnauthorizedToAccessEventException>();
        }

        [Test]
        public async Task UpdateEvent_WhenCalled_UpdatesEvent()
        {
            // Arrange
            var userId = _fixture.Create<string>();

            var calendarEvent = _fixture.Build<CalendarEvent>()
                .With(x => x.UserId, userId)
                .Without(x => x.Project)
                .Without(x => x.ProjectId)
                .Create();

            await GatewayTestHelpers.AddEventsToDb(calendarEvent);

            var request = _fixture.Build<UpdateEventRequest>()
               .Without(x => x.ProjectId)
               .Create();

            // Act
            var response = await _classUnderTest.UpdateEvent(calendarEvent.Id, request, userId);

            // Assert
            var expectedResponse = new CalendarEvent
            {
                Id = calendarEvent.Id,
                Description = request.Description,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                UserId = userId,
                Project = null
            };

            response.Should().BeEquivalentTo(expectedResponse.ToResponse());

            var dbResult = await GatewayTestHelpers.GetEvent(calendarEvent.Id);
            dbResult.Should().NotBeNull();
            dbResult.Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public async Task UpdateEvent_WhenProjectRemoved_RemoveProjectReference()
        {
            // Arrange
            var userId = _fixture.Create<string>();

            var project = _fixture.Build<Project>()
                .With(x => x.UserId, userId)
                .Without(x => x.CalendarEvents)
                .With(x => x.IsActive, true)
                .Create();

            await GatewayTestHelpers.AddProjectsToDb(project);

            var calendarEvent = _fixture.Build<CalendarEvent>()
                .With(x => x.UserId, userId)
                .Without(x => x.Project)
                .With(x => x.ProjectId, project.Id)
                .Create();

            await GatewayTestHelpers.AddEventsToDb(calendarEvent);

            var request = _fixture.Build<UpdateEventRequest>()
               .Without(x => x.ProjectId)
               .Create();

            // Act
            var response = await _classUnderTest.UpdateEvent(calendarEvent.Id, request, userId);

            // Assert
            response.Project.Should().BeNull();
            response.ProjectId.Should().BeNull();

            var dbResult = await GatewayTestHelpers.GetEvent(calendarEvent.Id);
            dbResult.Should().NotBeNull();

            dbResult.ProjectId.Should().BeNull();
            dbResult.Project.Should().BeNull();
        }

        [Test]
        public async Task UpdateEvent_WhenProjectIdIncludedButNotFound_ThrowsException()
        {
            // Arrange
            var userId = _fixture.Create<string>();
            var projectId = _fixture.Create<int>();

            var calendarEvent = _fixture.Build<CalendarEvent>()
                .With(x => x.UserId, userId)
                .Without(x => x.Project)
                .Without(x => x.ProjectId)
                .Create();

            await GatewayTestHelpers.AddEventsToDb(calendarEvent);

            var request = _fixture.Build<UpdateEventRequest>()
               .With(x => x.ProjectId, projectId)
               .Create();

            // Act
            Func<Task> task = async () => await _classUnderTest.UpdateEvent(calendarEvent.Id, request, userId);

            // Assert
            await task.Should().ThrowAsync<ProjectNotFoundException>();
        }

        [Test]
        public async Task WhenProjectIdIncludedButNotOwnedByUser_ThrowsException()
        {
            // Arrange
            var userId = _fixture.Create<string>();
            var otherUserId = userId + 1;

            var project = _fixture.Build<Project>()
                .With(x => x.UserId, otherUserId)
                .Without(x => x.CalendarEvents)
                .With(x => x.IsActive, true)
                .Create();

            await GatewayTestHelpers.AddProjectsToDb(project);

            var calendarEvent = _fixture.Build<CalendarEvent>()
                .With(x => x.UserId, userId)
                .Without(x => x.Project)
                .Without(x => x.ProjectId)
                .Create();

            await GatewayTestHelpers.AddEventsToDb(calendarEvent);

            var request = _fixture.Build<UpdateEventRequest>()
               .With(x => x.ProjectId, project.Id)
               .Create();

            // Act
            Func<Task> task = async () => await _classUnderTest.UpdateEvent(calendarEvent.Id, request, userId);

            // Assert
            await task.Should().ThrowAsync<UserUnauthorizedToAccessProjectException>();
        }

        [Test]
        public async Task WhenProjectIdIncludedButArchived_ThrowsException()
        {
            // Arrange
            var userId = _fixture.Create<string>();
            var otherUserId = userId + 1;

            var project = _fixture.Build<Project>()
                .With(x => x.UserId, userId)
                .With(x => x.IsActive, false)
                .Without(x => x.CalendarEvents)
                .Create();

            await GatewayTestHelpers.AddProjectsToDb(project);

            var calendarEvent = _fixture.Build<CalendarEvent>()
                .With(x => x.UserId, userId)
                .Without(x => x.Project)
                .Without(x => x.ProjectId)
                .Create();

            await GatewayTestHelpers.AddEventsToDb(calendarEvent);

            var request = _fixture.Build<UpdateEventRequest>()
               .With(x => x.ProjectId, project.Id)
               .Create();

            // Act
            Func<Task> task = async () => await _classUnderTest.UpdateEvent(calendarEvent.Id, request, userId);

            // Assert
            await task.Should().ThrowAsync<ProjectIsArchivedException>();
        }

        [Test]
        public async Task UpdateEvent_WhenProjectIdIncluded_AddsProjectReference()
        {
            // Arrange
            var userId = _fixture.Create<string>();

            var project = _fixture.Build<Project>()
                .With(x => x.UserId, userId)
                .With(x => x.IsActive, true)
                .Without(x => x.CalendarEvents)
                .Create();

            await GatewayTestHelpers.AddProjectsToDb(project);

            var calendarEvent = _fixture.Build<CalendarEvent>()
                .With(x => x.UserId, userId)
                .Without(x => x.Project)
                .Without(x => x.ProjectId)
                .Create();

            await GatewayTestHelpers.AddEventsToDb(calendarEvent);

            var request = _fixture.Build<UpdateEventRequest>()
               .With(x => x.ProjectId, project.Id)
               .Create();

            // Act
            var response = await _classUnderTest.UpdateEvent(calendarEvent.Id, request, userId);

            // Assert
            response.Project.Should().BeEquivalentTo(project.ToResponse(), options =>
                options.Excluding(x => x.TotalEventDurationInMinutes));

            var dbResponse = await GatewayTestHelpers.GetEvent(calendarEvent.Id);

            dbResponse.Should().NotBeNull();

            dbResponse.ProjectId.Should().Be(project.Id);
            dbResponse.Project.Should().BeEquivalentTo(project);
        }

        [Test]
        public async Task DeleteEvent_WhenNotFound_ReturnsFalse()
        {
            // Arrange
            var userId = _fixture.Create<string>();
            var calendarEventId = _fixture.Create<int>();

            // Act
            var response = await _classUnderTest.DeleteEvent(calendarEventId, userId);

            // Assert
            response.Should().BeFalse();
        }

        [Test]
        public async Task DeleteEvent_WhenItDoesntBelongToUser_ThrowsException()
        {
            // Arrange
            var userId = _fixture.Create<string>();
            var otherUserId = userId + 1;

            var calendarEvent = _fixture.Build<CalendarEvent>()
                .With(x => x.UserId, otherUserId)
                .Without(x => x.Project)
                .Create();

            await GatewayTestHelpers.AddEventsToDb(calendarEvent);

            // Act
            Func<Task> func = async () => await _classUnderTest.DeleteEvent(calendarEvent.Id, userId);

            // Assert
            await func.Should().ThrowAsync<UserUnauthorizedToAccessEventException>();
        }

        [Test]
        public async Task DeleteEvent_WhenCalled_DeletesEvent()
        {
            // Arrange
            var userId = _fixture.Create<string>();

            var calendarEvent = _fixture.Build<CalendarEvent>()
                .With(x => x.UserId, userId)
                .Without(x => x.Project)
                .Create();

            await GatewayTestHelpers.AddEventsToDb(calendarEvent);

            // Act
            var response = await _classUnderTest.DeleteEvent(calendarEvent.Id, userId);

            // Assert
            response.Should().BeTrue();

            var dbResponse = await GatewayTestHelpers.GetEvent(calendarEvent.Id);

            dbResponse.Should().BeNull();
        }
    }
}
