using AutoFixture;
using Moq;
using timer_app.Controllers;
using timer_app.UseCases.Interfaces;
using timer_app.Boundary.Response;
using FluentAssertions;
using timer_app.Boundary.Request;
using timer_app.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace timer_app_tests.Controller
{
    public class CalendarEventsControllerTests : ControllerTests
    {
        private CalendarEventsController _classUnderTest;

        private Mock<IGetAllEventsUseCase> _getAllEventsUseCaseMock;
        private Mock<IUpdateEventUseCase> _updateEventUseCaseMock;
        private Mock<IDeleteEventUseCase> _deleteEventUseCaseMock;
        private Mock<ICreateEventUseCase> _createEventUseCaseMock;

        private readonly Fixture _fixture = new Fixture();
        private readonly Random _random = new Random();

        [SetUp]
        public void Setup()
        {
            _getAllEventsUseCaseMock = new Mock<IGetAllEventsUseCase>();
            _updateEventUseCaseMock = new Mock<IUpdateEventUseCase>();
            _createEventUseCaseMock = new Mock<ICreateEventUseCase>();
            _deleteEventUseCaseMock = new Mock<IDeleteEventUseCase>();

            _classUnderTest = new CalendarEventsController(
                _getAllEventsUseCaseMock.Object,
                _createEventUseCaseMock.Object,
                _updateEventUseCaseMock.Object,
                _deleteEventUseCaseMock.Object
            );
        }

        [Test]
        public async Task GetAllEvents_WhenCalled_ReturnsAllEvents()
        {
            // Arrange
            var request = _fixture.Create<GetAllEventsRequest>();

            var numberOfResults = _random.Next(2, 5);
            var useCaseResponse = _fixture.CreateMany<CalendarEventResponse>(numberOfResults);

            _getAllEventsUseCaseMock
                .Setup(x => x.ExecuteAsync(request, It.IsAny<int>()))
                .ReturnsAsync(useCaseResponse);

            // Act
            var result = await _classUnderTest.GetAllEvents(request);
            var responseObject = GetResultData<IEnumerable<CalendarEventResponse>>(result);
            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(200);
            responseObject.Should().HaveCount(numberOfResults);
        }

        [Test]
        public async Task CreateEvent_WhenProjectNotFound_ReturnsBadRequest()
        {
            // Arrange
            var request = _fixture.Create<CreateEventRequest>();

            var numberOfResults = _random.Next(2, 5);
            var useCaseResponse = _fixture.CreateMany<CalendarEventResponse>(numberOfResults);
            var exception = new ProjectNotFoundException((int)request.ProjectId);

            _createEventUseCaseMock
                .Setup(x => x.ExecuteAsync(request, It.IsAny<int>()))
                .ThrowsAsync(exception);

            // Act
            var result = await _classUnderTest.CreateEvent(request);
            var responseObject = GetResultData<string>(result);
            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(400);
            responseObject.Should().Be(exception.Message);
        }

        [Test]
        public async Task CreateEvent_WhenUserDoesntOwnProject_ReturnsBadRequest()
        {
            // Arrange
            var request = _fixture.Create<CreateEventRequest>();

            var numberOfResults = _random.Next(2, 5);
            var useCaseResponse = _fixture.CreateMany<CalendarEventResponse>(numberOfResults);
            var exception = new UserUnauthorizedToAccessProjectException((int)request.ProjectId);

            _createEventUseCaseMock
                .Setup(x => x.ExecuteAsync(request, It.IsAny<int>()))
                .ThrowsAsync(exception);

            // Act
            var result = await _classUnderTest.CreateEvent(request);
            var responseObject = GetResultData<string>(result);
            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(400);
            responseObject.Should().Be(exception.Message);
        }

        [Test]
        public async Task CreateEvent_WhenSuccessful_Returns200()
        {
            // Arrange
            var request = _fixture.Create<CreateEventRequest>();
            var useCaseResponse = _fixture.Create<CalendarEventResponse>();

            _createEventUseCaseMock
                .Setup(x => x.ExecuteAsync(request, It.IsAny<int>()))
                .ReturnsAsync(useCaseResponse);

            // Act
            var result = await _classUnderTest.CreateEvent(request);
            var responseObject = GetResultData<CalendarEventResponse>(result);
            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(200);
            responseObject.Should().Be(useCaseResponse);
        }

        [Test]
        public async Task UpdateEvent_WhenNotFound_Returns404()
        {
            // Arrange
            var query = _fixture.Create<EventQuery>();
            var request = _fixture.Create<UpdateEventRequest>();

            _updateEventUseCaseMock
                .Setup(x => x.ExecuteAsync(query.EventId, request, It.IsAny<int>()))
                .ReturnsAsync((CalendarEventResponse) null);

            // Act
            var result = await _classUnderTest.UpdateEvent(query, request);
            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(404);
        }

        [Test]
        public async Task UpdateEvent_WhenUnauthorized_Returns401()
        {
            // Arrange
            var query = _fixture.Create<EventQuery>();
            var request = _fixture.Create<UpdateEventRequest>();

            _updateEventUseCaseMock
                .Setup(x => x.ExecuteAsync(query.EventId, request, It.IsAny<int>()))
                .ThrowsAsync(new UserUnauthorizedToAccessEventException(query.EventId));

            // Act
            var result = await _classUnderTest.UpdateEvent(query, request);
            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(401);
        }

        [Test]
        public async Task UpdateEvent_WhenProjectNotFound_ReturnsBadRequest()
        {
            // Arrange
            var query = _fixture.Create<EventQuery>();
            var request = _fixture.Create<UpdateEventRequest>();
            var exception = new ProjectNotFoundException(query.EventId);

            _updateEventUseCaseMock
                .Setup(x => x.ExecuteAsync(query.EventId, request, It.IsAny<int>()))
                .ThrowsAsync(exception);

            // Act
            var result = await _classUnderTest.UpdateEvent(query, request);
            var responseObject = GetResultData<string>(result);
            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(400);
            responseObject.Should().Be(exception.Message);
        }

        [Test]
        public async Task UpdateEvent_WhenProjectNotOwnedByUser_ReturnsBadRequest()
        {
            // Arrange
            var query = _fixture.Create<EventQuery>();
            var request = _fixture.Create<UpdateEventRequest>();
            var exception = new UserUnauthorizedToAccessProjectException(query.EventId);

            _updateEventUseCaseMock
                .Setup(x => x.ExecuteAsync(query.EventId, request, It.IsAny<int>()))
                .ThrowsAsync(exception);

            // Act
            var result = await _classUnderTest.UpdateEvent(query, request);
            var responseObject = GetResultData<string>(result);
            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(400);
            responseObject.Should().Be(exception.Message);
        }

        [Test]
        public async Task UpdateEvent_WhenProjectUpdated_Returns200()
        {
            // Arrange
            var query = _fixture.Create<EventQuery>();
            var request = _fixture.Create<UpdateEventRequest>();

            var useCaseResponse = _fixture.Create<CalendarEventResponse>();

            _updateEventUseCaseMock
                .Setup(x => x.ExecuteAsync(query.EventId, request, It.IsAny<int>()))
                .ReturnsAsync(useCaseResponse);

            // Act
            var result = await _classUnderTest.UpdateEvent(query, request);
            var responseObject = GetResultData<CalendarEventResponse>(result);
            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(200);
            responseObject.Should().Be(useCaseResponse);
        }

        [Test]
        public async Task DeleteEvent_WhenEventNotFound_Returns404()
        {
            // Arrange
            var query = _fixture.Create<EventQuery>();

            _deleteEventUseCaseMock
                .Setup(x => x.ExecuteAsync(query.EventId, It.IsAny<int>()))
                .ReturnsAsync(false);

            // Act
            var result = await _classUnderTest.DeleteEvent(query);
            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(404);
        }

        [Test]
        public async Task DeleteEvent_WhenUnauthorized_Returns401()
        {
            // Arrange
            var query = _fixture.Create<EventQuery>();

            _deleteEventUseCaseMock
                .Setup(x => x.ExecuteAsync(query.EventId, It.IsAny<int>()))
                .ThrowsAsync(new UserUnauthorizedToAccessEventException(query.EventId));

            // Act
            var result = await _classUnderTest.DeleteEvent(query);
            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(401);
        }

        [Test]
        public async Task DeleteEvent_WhenSuccessful_Returns204()
        {
            // Arrange
            var query = _fixture.Create<EventQuery>();

            _deleteEventUseCaseMock
                .Setup(x => x.ExecuteAsync(query.EventId, It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            var result = await _classUnderTest.DeleteEvent(query);
            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(204);
        }
    }
}
