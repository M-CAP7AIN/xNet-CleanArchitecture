using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Notes.Commands.DeleteNote
{
    public record DeleteNoteCommand(Guid Id) : IRequest;

    public class DeleteNoteCommandHandler : IRequestHandler<DeleteNoteCommand>
    {
        private readonly IApplicationDbContext _context;

        public DeleteNoteCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
        {
            var note = await _context.Notes.FindAsync([request.Id], cancellationToken);

            if (note == null)
                throw new NotFoundException(nameof(Note), request.Id);

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}