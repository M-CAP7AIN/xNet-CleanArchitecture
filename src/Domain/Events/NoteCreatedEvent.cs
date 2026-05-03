using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Events
{
    public class NoteCreatedEvent
    {
        public Guid NoteId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid UserId { get; set; }
    }
}
