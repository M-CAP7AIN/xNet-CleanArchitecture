using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Notes.Commands.CreateNote
{
    public class CreateNoteCommandValidator : AbstractValidator<CreateNoteCommand>
    {
        public CreateNoteCommandValidator()
        {
            RuleFor(v => v.Title)
                .MaximumLength(200)
                .NotEmpty();

            RuleFor(v => v.Content)
                .MaximumLength(2000);
        }
    }
}
