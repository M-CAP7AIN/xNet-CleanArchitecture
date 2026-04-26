using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Note : BaseAuditableEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public bool IsArchived { get; set; }
        public DateTime? DueDate { get; set; }
        public NotePriority Priority { get; set; }
    }
}
