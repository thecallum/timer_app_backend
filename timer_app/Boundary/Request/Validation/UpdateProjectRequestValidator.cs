using FluentValidation;

namespace timer_app.Boundary.Request.Validation
{
    public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
    {
        public UpdateProjectRequestValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.ProjectColor)
                .NotNull()
                .SetValidator(new ProjectColorValidator());
        }
    }
}
