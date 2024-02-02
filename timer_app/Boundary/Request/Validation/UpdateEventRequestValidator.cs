using FluentValidation;

namespace timer_app.Boundary.Request.Validation
{
    public class UpdateEventRequestValidator : AbstractValidator<UpdateEventRequest>
    {
        public UpdateEventRequestValidator()
        {
            RuleFor(x => x.Description)
                .MaximumLength(60);

            RuleFor(x => x.StartTime)
                .NotNull();

            RuleFor(x => x.EndTime)
                .NotNull()
                .GreaterThan(x => x.StartTime);
        }
    }
}
