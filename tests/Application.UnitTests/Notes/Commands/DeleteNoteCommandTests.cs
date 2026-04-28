using Application.Common.Exceptions;

using Application.Notes.Commands.DeleteNote;
using Domain.Entities;
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
    public class DeleteNoteCommandTests
    {
        [Fact]
        public async Task Handle_NoteExists_ShouldDeleteSuccessfully()
        {
            // Arrange
            var noteId = Guid.NewGuid();
            var existingNote = new Note { Id = noteId, Title = "To Delete" };

            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Note>>();

            mockDbSet.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingNote);

            mockContext.Setup(x => x.Notes).Returns(mockDbSet.Object);
            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new DeleteNoteCommandHandler(mockContext.Object);
            var command = new DeleteNoteCommand(noteId);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            mockDbSet.Verify(x => x.Remove(It.Is<Note>(n => n.Id == noteId)), Times.Once);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NoteNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var noteId = Guid.NewGuid();

            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Note>>();

            mockDbSet.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(null as Note);

            mockContext.Setup(x => x.Notes).Returns(mockDbSet.Object);

            var handler = new DeleteNoteCommandHandler(mockContext.Object);
            var command = new DeleteNoteCommand(noteId);

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
            mockDbSet.Verify(x => x.Remove(It.IsAny<Note>()), Times.Never);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
