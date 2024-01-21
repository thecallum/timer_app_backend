using FluentValidation;

namespace timer_app.Boundary.Request.Validation
{
    public class ProjectColorValidator : AbstractValidator<ProjectColorRequest>
    {
        public ProjectColorValidator()
        {
            RuleFor(x => x.Lightest)
                .Matches("^#[0-9a-fA-F]{6}$")
                .WithMessage("The field must be a 6-digit hex color code.");

            RuleFor(x => x.Light)
                .Matches("^#[0-9a-fA-F]{6}$")
                .WithMessage("The field must be a 6-digit hex color code.");

            RuleFor(x => x.Dark)
                .Matches("^#[0-9a-fA-F]{6}$")
                .WithMessage("The field must be a 6-digit hex color code.");

            RuleFor(x => x.Darkest)
                .Matches("^#[0-9a-fA-F]{6}$")
                .WithMessage("The field must be a 6-digit hex color code.");
        }
    }
}
