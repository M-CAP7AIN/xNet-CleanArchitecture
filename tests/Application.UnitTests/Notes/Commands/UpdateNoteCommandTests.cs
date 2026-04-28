using Application.Common.Exceptions;

using Application.Notes.Commands.UpdateNote;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.UnitTests.Notes.Commands
{
    public class UpdateNoteCommandTests
    {
        [Fact]
        public async Task Handle_NoteExists_ShouldUpdateSuccessfully()
        {
            // Arrange
            var noteId = Guid.NewGuid();
            var existingNote = new Note
            {
                Id = noteId,
                Title = "Old Title",
                Content = "Old Content",
                Priority = NotePriority.Low,
                IsArchived = false,
                CreatedAt = DateTime.UtcNow
            };

            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Note>>();

            // Setup FindAsync to return existing note
            mockDbSet.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingNote);

            mockContext.Setup(x => x.Notes).Returns(mockDbSet.Object);
            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new UpdateNoteCommandHandler(mockContext.Object);

            var command = new UpdateNoteCommand
            {
                Id = noteId,
                Title = "Updated Title",
                Content = "Updated Content",
                Priority = NotePriority.High,
                IsArchived = true,
                DueDate = DateTime.UtcNow.AddDays(5)
            };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            existingNote.Title.Should().Be(command.Title);
            existingNote.Content.Should().Be(command.Content);
            existingNote.Priority.Should().Be(command.Priority);
            existingNote.IsArchived.Should().BeTrue();
            existingNote.DueDate.Should().Be(command.DueDate);
            existingNote.UpdatedAt.Should().NotBeNull();

            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NoteNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var noteId = Guid.NewGuid();

            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Note>>();

            // Setup FindAsync to return null
            mockDbSet.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(null as Note);

            mockContext.Setup(x => x.Notes).Returns(mockDbSet.Object);

            var handler = new UpdateNoteCommandHandler(mockContext.Object);
            var command = new UpdateNoteCommand { Id = noteId };

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"*{nameof(Note)}*{noteId}*");

            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenDbUpdateException_ShouldPropagateError()
        {
            // Arrange
            var noteId = Guid.NewGuid();
            var existingNote = new Note { Id = noteId, Title = "Test", Content = "Test" };

            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Note>>();

            mockDbSet.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingNote);

            mockContext.Setup(x => x.Notes).Returns(mockDbSet.Object);
            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("Database error"));

            var handler = new UpdateNoteCommandHandler(mockContext.Object);
            var command = new UpdateNoteCommand { Id = noteId, Title = "Updated" };

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DbUpdateException>();
        }
    }
}
