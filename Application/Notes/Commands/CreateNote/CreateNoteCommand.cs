using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Notes.Commands.CreateNote
{
    public record CreateNoteCommand : IRequest<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public NotePriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class CreateNoteCommandHandler : IRequestHandler<CreateNoteCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CreateNoteCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

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
                UserId = Guid.Parse("user-id-from-context") // از کاربر فعلی
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync(cancellationToken);

            return note.Id;
        }
    }
}
