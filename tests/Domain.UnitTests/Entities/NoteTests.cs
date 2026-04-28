using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.UnitTests.Entities
{
    public class NoteTests
    {
        [Fact]
        public void CreateNote_WithValidData_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var title = "Test Note";
            var content = "Test Content";
            var priority = NotePriority.High;

            // Act
            var note = new Note
            {
                Id = id,
                Title = title,
                Content = content,
                Priority = priority,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            note.Id.Should().Be(id);
            note.Title.Should().Be(title);
            note.Content.Should().Be(content);
            note.Priority.Should().Be(NotePriority.High);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void CreateNote_WithInvalidTitle_ShouldAcceptButBusinessRuleViolates(string invalidTitle)
        {
            // Act
            var note = new Note { Title = invalidTitle };

            // Assert - Domain entity فقط نگهداری می‌کند، اعتبارسنجی در لایه اپلیکیشن است
            note.Title.Should().Be(invalidTitle);
        }

        [Fact]
        public void Note_Archive_ShouldSetIsArchivedToTrue()
        {
            // Arrange
            var note = new Note { IsArchived = false };

            // Act
            note.IsArchived = true;

            // Assert
            note.IsArchived.Should().BeTrue();
        }

        [Fact]
        public void Note_WithDueDate_ShouldStoreCorrectly()
        {
            // Arrange
            var dueDate = DateTime.UtcNow.AddDays(7);

            // Act
            var note = new Note { DueDate = dueDate };

            // Assert
            note.DueDate.Should().Be(dueDate);
        }
    }
}
