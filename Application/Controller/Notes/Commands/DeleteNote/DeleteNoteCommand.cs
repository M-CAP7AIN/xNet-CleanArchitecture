using Application.Common.Exceptions;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Controller.Notes.Commands.DeleteNote
{
    public record DeleteNoteCommand(Guid Id) : IRequest;

    public class DeleteNoteCommandHandler(ApplicationDbContext context) : IRequestHandler<DeleteNoteCommand>
    {

        public async Task Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
        {
            var note = await context.Notes.FindAsync([request.Id], cancellationToken);

            if (note == null)
                throw new NotFoundException(nameof(Note), request.Id);

            context.Notes.Remove(note);

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}