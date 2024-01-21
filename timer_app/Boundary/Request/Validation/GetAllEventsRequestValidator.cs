using FluentValidation;

namespace timer_app.Boundary.Request.Validation
{
    public class GetAllEventsRequestValidator : AbstractValidator<GetAllEventsRequest>
    {
        public GetAllEventsRequestValidator()
        {
            RuleFor(x => x.StartTime)
                .NotNull();

            RuleFor(x => x.EndTime)
                .NotNull()
                .GreaterThan(x => x.StartTime);
        }
    }
}
