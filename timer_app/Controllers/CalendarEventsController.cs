using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using timer_app.Boundary.Request;
using timer_app.Boundary.Request.Validation;
using timer_app.Infrastructure.Exceptions;
using timer_app.UseCases.Interfaces;

namespace timer_app.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class CalendarEventsController : ControllerBase
    {
        private readonly IGetAllEventsUseCase _getAllEventsUseCase;
        private readonly ICreateEventUseCase _createEventUseCase;
        private readonly IUpdateEventUseCase _updateEventUseCase;
        private readonly IDeleteEventUseCase _deleteEventUseCase;

        private readonly IValidator<CreateEventRequest> _createEventRequestValidator;
        private readonly IValidator<GetAllEventsRequest> _getAllEventsRequestValidator;
        private readonly IValidator<UpdateEventRequest> _updateEventRequestValidator;


        private const int PlaceholderUserId = 1;

        public CalendarEventsController(
            IGetAllEventsUseCase getAllEventsUseCase,
            ICreateEventUseCase createEventUseCase,
            IUpdateEventUseCase updateEventUseCase,
            IDeleteEventUseCase deleteEventUseCase,
            IValidator<CreateEventRequest> createEventRequestValidator,
            IValidator<GetAllEventsRequest> getAllEventsRequestValidator,
            IValidator<UpdateEventRequest> updateEventRequestValidator)
        {
            _getAllEventsUseCase = getAllEventsUseCase;
            _createEventUseCase = createEventUseCase;
            _updateEventUseCase = updateEventUseCase;
            _deleteEventUseCase = deleteEventUseCase;
            _createEventRequestValidator = createEventRequestValidator;
            _getAllEventsRequestValidator = getAllEventsRequestValidator;
            _updateEventRequestValidator = updateEventRequestValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEvents([FromQuery] GetAllEventsRequest request)
        {
            var validationResult = await _getAllEventsRequestValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var calendarEvents = await _getAllEventsUseCase.ExecuteAsync(request, PlaceholderUserId);

            return Ok(calendarEvents);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
        {
            var validationResult = await _createEventRequestValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var calendarEvent = await _createEventUseCase.ExecuteAsync(request, PlaceholderUserId);
                return Ok(calendarEvent);
            }
            catch (ProjectNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (UserUnauthorizedToAccessProjectException e)
            {
                return BadRequest(e.Message);
            }
            catch (ProjectIsArchivedException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        [Route("{eventId}")]
        public async Task<IActionResult> UpdateEvent([FromRoute] EventQuery query, [FromBody] UpdateEventRequest request)
        {
            var validationResult = await _updateEventRequestValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var response = await _updateEventUseCase.ExecuteAsync(query.EventId, request, PlaceholderUserId);
                if (response == null) return NotFound(query.EventId);

                return Ok(response);
            }
            catch (UserUnauthorizedToAccessEventException e)
            {
                return Unauthorized(e.Message);
            }
            catch (ProjectNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (UserUnauthorizedToAccessProjectException e)
            {
                return BadRequest(e.Message);
            }
            catch (ProjectIsArchivedException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete]
        [Route("{eventId}")]
        public async Task<IActionResult> DeleteEvent([FromRoute] EventQuery query)
        {
            try
            {
                var response = await _deleteEventUseCase.ExecuteAsync(query.EventId, PlaceholderUserId);
                if (!response) return NotFound(query.EventId);

                return NoContent();
            }
            catch (UserUnauthorizedToAccessEventException e)
            {
                return Unauthorized(e.Message);
            }
        }
    }
}
