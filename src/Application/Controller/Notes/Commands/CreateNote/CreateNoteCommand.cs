using Domain.Entities;
using Domain.Enums;
using Domain.Events;
using Domain.Interfaces;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Controller.Notes.Commands.CreateNote
{
    public record CreateNoteCommand : IRequest<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public NotePriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class CreateNoteCommandHandler(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMessageBus messageBus) : IRequestHandler<CreateNoteCommand, Guid>
    {

        public async Task<Guid> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
        {
            var note = new Note
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Content = request.Content,
                Priority = request.Priority,
                DueDate = request.DueDate,
                CreatedAt = DateTime.UtcNow,
                UserId = currentUserService.UserId
            };

            await context.Notes.AddAsync(note);

            await context.SaveChangesAsync(cancellationToken);

            // ✅ انتشار رویداد به RabbitMQ
            var noteEvent = new NoteCreatedEvent
            {
                NoteId = note.Id,
                Title = note.Title,
                Content = note.Content,
                CreatedAt = note.CreatedAt,
                UserId = note.UserId
            };

            //await messageBus.PublishAsync(noteEvent, cancellationToken);

            return note.Id;
        }
    }
}
