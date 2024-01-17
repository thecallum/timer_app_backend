using FluentValidation;

namespace timer_app.Boundary.Request.Validation
{
    public class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
    {
        public CreateEventRequestValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.StartTime)
                .NotNull();

            RuleFor(x => x.EndTime)
                .NotNull()
                .GreaterThan(x => x.StartTime);
        }
    }
}
