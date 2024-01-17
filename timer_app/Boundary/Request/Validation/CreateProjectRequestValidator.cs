using FluentValidation;

namespace timer_app.Boundary.Request.Validation
{
    public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
    {
        public CreateProjectRequestValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.DisplayColour)
                .NotEmpty()
                .Length(7);
        }
    }
}
