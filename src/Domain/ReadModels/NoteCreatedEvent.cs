using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.ReadModels
{
    public class NoteCreatedEvent
    {
        public Guid NoteId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
