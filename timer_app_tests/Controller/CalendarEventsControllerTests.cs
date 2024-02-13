using AutoFixture;
using Moq;
using timer_app.Controllers;
using timer_app.UseCases.Interfaces;
using timer_app.Boundary.Response;
using FluentAssertions;
using timer_app.Boundary.Request;
using timer_app.Infrastructure.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using timer_app.Middleware.Interfaces;

namespace timer_app_tests.Controller
{
    public class CalendarEventsControllerTests : ControllerTests
    {
        private CalendarEventsController _classUnderTest;

        private Mock<IGetAllEventsUseCase> _getAllEventsUseCaseMock;
        private Mock<IUpdateEventUseCase> _updateEventUseCaseMock;
        private Mock<IDeleteEventUseCase> _deleteEventUseCaseMock;
        private Mock<ICreateEventUseCase> _createEventUseCaseMock;

        private Mock<IValidator<CreateEventRequest>> _createEventRequestValidatorMock;
        private Mock<IValidator<GetAllEventsRequest>> _getAllEventsRequestValidatorMock;
        private Mock<IValidator<UpdateEventRequest>> _updateEventRequestValidatorMock;

        private Mock<IUserService> _currentUserServiceMock;

        private readonly Fixture _fixture = new Fixture();
        private readonly Random _random = new Random();

        [SetUp]
        public void Setup()
        {
            _getAllEventsUseCaseMock = new Mock<IGetAllEventsUseCase>();
            _updateEventUseCaseMock = new Mock<IUpdateEventUseCase>();
            _createEventUseCaseMock = new Mock<ICreateEventUseCase>();
            _deleteEventUseCaseMock = new Mock<IDeleteEventUseCase>();

            _createEventRequestValidatorMock = new Mock<IValidator<CreateEventRequest>>();
            _getAllEventsRequestValidatorMock = new Mock<IValidator<GetAllEventsRequest>>();
            _updateEventRequestValidatorMock = new Mock<IValidator<UpdateEventRequest>>();

            _createEventRequestValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<CreateEventRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _getAllEventsRequestValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<GetAllEventsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _updateEventRequestValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<UpdateEventRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _currentUserServiceMock = new Mock<IUserService>();

            _classUnderTest = new CalendarEventsController(
                _getAllEventsUseCaseMock.Object,
                _createEventUseCaseMock.Object,
                _updateEventUseCaseMock.Object,
                _deleteEventUseCaseMock.Object,
                _createEventRequestValidatorMock.Object,
                _getAllEventsRequestValidatorMock.Object,
                _updateEventRequestValidatorMock.Object,
                _currentUserServiceMock.Object
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
                .Setup(x => x.ExecuteAsync(request, It.IsAny<string>()))
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
                .Setup(x => x.ExecuteAsync(request, It.IsAny<string>()))
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
            var userId = _fixture.Create<string>();

            var numberOfResults = _random.Next(2, 5);
            var useCaseResponse = _fixture.CreateMany<CalendarEventResponse>(numberOfResults);
            var exception = new UserUnauthorizedToAccessProjectException(userId);

            _createEventUseCaseMock
                .Setup(x => x.ExecuteAsync(request, It.IsAny<string>()))
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
        public async Task CreateEvent_WhenProjectIsArchived_Returns400()
        {
            // Arrange
            var request = _fixture.Create<CreateEventRequest>();

            var numberOfResults = _random.Next(2, 5);
            var useCaseResponse = _fixture.CreateMany<CalendarEventResponse>(numberOfResults);
            var exception = new ProjectIsArchivedException((int)request.ProjectId);

            _createEventUseCaseMock
                .Setup(x => x.ExecuteAsync(request, It.IsAny<string>()))
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
                .Setup(x => x.ExecuteAsync(request, It.IsAny<string>()))
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
                .Setup(x => x.ExecuteAsync(query.EventId, request, It.IsAny<string>()))
                .ReturnsAsync((CalendarEventResponse)null);

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
            var userId = _fixture.Create<string>();

            _updateEventUseCaseMock
                .Setup(x => x.ExecuteAsync(query.EventId, request, It.IsAny<string>()))
                .ThrowsAsync(new UserUnauthorizedToAccessEventException(userId));

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
                .Setup(x => x.ExecuteAsync(query.EventId, request, It.IsAny<string>()))
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
            var userId = _fixture.Create<string>();

            var exception = new UserUnauthorizedToAccessProjectException(userId);

            _updateEventUseCaseMock
                .Setup(x => x.ExecuteAsync(query.EventId, request, It.IsAny<string>()))
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
        public async Task UpdateEvent_WhenProjectIsArchived_Returns400()
        {
            // Arrange
            var query = _fixture.Create<EventQuery>();
            var request = _fixture.Create<UpdateEventRequest>();
            var exception = new ProjectIsArchivedException((int)request.ProjectId);

            _updateEventUseCaseMock
                .Setup(x => x.ExecuteAsync(query.EventId, request, It.IsAny<string>()))
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
                .Setup(x => x.ExecuteAsync(query.EventId, request, It.IsAny<string>()))
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
                .Setup(x => x.ExecuteAsync(query.EventId, It.IsAny<string>()))
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
            var userId = _fixture.Create<string>();

            _deleteEventUseCaseMock
                .Setup(x => x.ExecuteAsync(query.EventId, It.IsAny<string>()))
                .ThrowsAsync(new UserUnauthorizedToAccessEventException(userId));

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
                .Setup(x => x.ExecuteAsync(query.EventId, It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _classUnderTest.DeleteEvent(query);
            var statusCode = GetStatusCode(result);

            // Assert
            statusCode.Should().Be(204);
        }
    }
}
