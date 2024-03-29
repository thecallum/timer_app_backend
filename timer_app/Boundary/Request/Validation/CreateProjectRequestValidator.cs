﻿using FluentValidation;

namespace timer_app.Boundary.Request.Validation
{
    public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
    {
        public CreateProjectRequestValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(30);

            RuleFor(x => x.ProjectColor)
                .NotNull()
                .SetValidator(new ProjectColorValidator());
        }
    }
}
