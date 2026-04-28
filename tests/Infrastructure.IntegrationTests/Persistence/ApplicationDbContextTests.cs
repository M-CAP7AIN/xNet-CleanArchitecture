
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.IntegrationTests.Persistence
{
    public class ApplicationDbContextTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public ApplicationDbContextTests()
        {
            // استفاده از In-Memory Database به جای SQLite
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(_options);
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task AddNote_ShouldSaveToDatabase()
        {
            // Arrange
            var note = new Note
            {
                Id = Guid.NewGuid(),
                Title = "Integration Test Note",
                Content = "Test Content",
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Priority = NotePriority.High
            };

            // Act
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            // Assert
            var savedNote = await _context.Notes.FirstOrDefaultAsync(n => n.Id == note.Id);
            savedNote.Should().NotBeNull();
            savedNote!.Title.Should().Be(note.Title);
            savedNote.Content.Should().Be(note.Content);
            savedNote.Priority.Should().Be(note.Priority);
        }

        [Fact]
        public async Task AddNote_ShouldGenerateTimestamps()
        {
            // Arrange
            var note = new Note
            {
                Id = Guid.NewGuid(),
                Title = "Test Note",
                Content = "Content",
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Priority = NotePriority.Medium
            };

            // Act
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            // Assert
            var savedNote = await _context.Notes.FindAsync(note.Id);
            savedNote.Should().NotBeNull();
            savedNote!.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task UpdateNote_ShouldUpdateInDatabase()
        {
            // Arrange
            var note = new Note
            {
                Id = Guid.NewGuid(),
                Title = "Original Title",
                Content = "Original Content",
                CreatedAt = DateTime.UtcNow,
                UserId = Guid.NewGuid(),
                Priority = NotePriority.Low
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            // Act
            note.Title = "Updated Title";
            note.Content = "Updated Content";
            note.Priority = NotePriority.High;
            note.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Assert
            var updatedNote = await _context.Notes.FindAsync(note.Id);
            updatedNote.Should().NotBeNull();
            updatedNote!.Title.Should().Be("Updated Title");
            updatedNote.Content.Should().Be("Updated Content");
            updatedNote.Priority.Should().Be(NotePriority.High);
            updatedNote.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteNote_ShouldRemoveFromDatabase()
        {
            // Arrange
            var note = new Note
            {
                Id = Guid.NewGuid(),
                Title = "To Delete",
                Content = "Content",
                CreatedAt = DateTime.UtcNow,
                UserId = Guid.NewGuid(),
                Priority = NotePriority.Low
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            // Act
            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            // Assert
            var deletedNote = await _context.Notes.FindAsync(note.Id);
            deletedNote.Should().BeNull();
        }

        [Fact]
        public async Task QueryNotes_WithFilters_ShouldReturnCorrectResults()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var notes = new List<Note>
        {
            new() { Id = Guid.NewGuid(), Title = "Active 1", UserId = userId, IsArchived = false, Priority = NotePriority.Low, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Archived 1", UserId = userId, IsArchived = true, Priority = NotePriority.Medium, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Active 2", UserId = userId, IsArchived = false, Priority = NotePriority.High, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Other User", UserId = Guid.NewGuid(), IsArchived = false, Priority = NotePriority.Low, CreatedAt = DateTime.UtcNow }
        };

            _context.Notes.AddRange(notes);
            await _context.SaveChangesAsync();

            // Act
            var activeNotesForUser = await _context.Notes
                .Where(n => n.UserId == userId && !n.IsArchived)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            // Assert
            activeNotesForUser.Should().HaveCount(2);
            activeNotesForUser.Should().NotContain(n => n.IsArchived);
            activeNotesForUser.Should().OnlyContain(n => n.UserId == userId);
        }

        [Fact]
        public async Task QueryNotes_ByPriority_ShouldReturnCorrectResults()
        {
            // Arrange
            var notes = new List<Note>
        {
            new() { Id = Guid.NewGuid(), Title = "Low Priority", Priority = NotePriority.Low, UserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Medium Priority", Priority = NotePriority.Medium, UserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "High Priority", Priority = NotePriority.High, UserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Another High", Priority = NotePriority.High, UserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow }
        };

            _context.Notes.AddRange(notes);
            await _context.SaveChangesAsync();

            // Act
            var highPriorityNotes = await _context.Notes
                .Where(n => n.Priority == NotePriority.High)
                .ToListAsync();

            // Assert
            highPriorityNotes.Should().HaveCount(2);
            highPriorityNotes.Should().AllSatisfy(n => n.Priority.Should().Be(NotePriority.High));
        }

        [Fact]
        public async Task AddMultipleNotes_ShouldSaveAll()
        {
            // Arrange
            var notes = new List<Note>
        {
            new() { Id = Guid.NewGuid(), Title = "Note 1", Content = "Content 1", UserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, Priority = NotePriority.Low },
            new() { Id = Guid.NewGuid(), Title = "Note 2", Content = "Content 2", UserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, Priority = NotePriority.Medium },
            new() { Id = Guid.NewGuid(), Title = "Note 3", Content = "Content 3", UserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, Priority = NotePriority.High }
        };

            // Act
            _context.Notes.AddRange(notes);
            await _context.SaveChangesAsync();

            // Assert
            var allNotes = await _context.Notes.ToListAsync();
            allNotes.Should().HaveCount(3);
        }

        [Fact]
        public async Task Note_WithDueDate_ShouldStoreCorrectly()
        {
            // Arrange
            var dueDate = DateTime.UtcNow.AddDays(7);
            var note = new Note
            {
                Id = Guid.NewGuid(),
                Title = "Task with Due Date",
                Content = "Content",
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                DueDate = dueDate,
                Priority = NotePriority.High
            };

            // Act
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            // Assert
            var savedNote = await _context.Notes.FindAsync(note.Id);
            savedNote.Should().NotBeNull();
            savedNote!.DueDate.Should().Be(dueDate);
        }

        [Fact]
        public async Task Note_WithoutDueDate_ShouldStoreNull()
        {
            // Arrange
            var note = new Note
            {
                Id = Guid.NewGuid(),
                Title = "Task without Due Date",
                Content = "Content",
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                DueDate = null,
                Priority = NotePriority.Low
            };

            // Act
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            // Assert
            var savedNote = await _context.Notes.FindAsync(note.Id);
            savedNote.Should().NotBeNull();
            savedNote!.DueDate.Should().BeNull();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
