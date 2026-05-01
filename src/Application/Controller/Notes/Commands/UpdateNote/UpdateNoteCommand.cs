using Application.Common.Exceptions;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using MediatR;

namespace Application.Controller.Notes.Commands.UpdateNote
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

    public class UpdateNoteCommandHandler(ApplicationDbContext context) : IRequestHandler<UpdateNoteCommand>
    {

        public async Task Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
        {
            var note = await context.Notes.FindAsync([request.Id], cancellationToken);

            if (note == null)
                throw new NotFoundException(nameof(Note), request.Id);

            note.Title = request.Title;
            note.Content = request.Content;
            note.Priority = request.Priority;
            note.IsArchived = request.IsArchived;
            note.DueDate = request.DueDate;
            note.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
