using Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.ReadModels
{
    public class NoteReadModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsArchived { get; set; }
        public DateTime? DueDate { get; set; }
        public int Priority { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }

        public string UserFullName { get; set; } = string.Empty; 
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // متد کمکی برای تبدیل از Note اصلی
        public static NoteReadModel FromNote(Note note, string userFullName = "")
        {
            return new NoteReadModel
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                IsArchived = note.IsArchived,
                DueDate = note.DueDate,
                Priority = (int)note.Priority,
                UserId = note.UserId,
                UserFullName = userFullName,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            };
        }
    }
}
