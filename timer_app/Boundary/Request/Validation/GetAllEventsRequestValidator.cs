using FluentValidation;

namespace timer_app.Boundary.Request.Validation
{
    public class GetAllEventsRequestValidator : AbstractValidator<GetAllEventsRequest>
    {
        public GetAllEventsRequestValidator()
        {
            // optional parameters - but both must be set if either passed in request

            RuleFor(x => x.StartTime)
                .NotNull()
                .LessThan(x => x.EndTime)
                .When(x => x.EndTime != null);

            RuleFor(x => x.EndTime)
                .NotNull()
                .GreaterThan(x => x.StartTime)
                .When(x => x.StartTime != null);
        }
    }
}
