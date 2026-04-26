using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Notes.Commands.UpdateNote
{
    public record UpdateNoteCommand : IRequest
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public NotePriority Priority { get; set; }
        public bool IsArchived { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class UpdateNoteCommandHandler : IRequestHandler<UpdateNoteCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateNoteCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
        {
            var note = await _context.Notes.FindAsync([request.Id], cancellationToken);

            if (note == null)
                throw new NotFoundException(nameof(Note), request.Id);

            note.Title = request.Title;
            note.Content = request.Content;
            note.Priority = request.Priority;
            note.IsArchived = request.IsArchived;
            note.DueDate = request.DueDate;
            note.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
