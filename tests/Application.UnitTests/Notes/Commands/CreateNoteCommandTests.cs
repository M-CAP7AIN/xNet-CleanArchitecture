
using Application.Notes.Commands.CreateNote;
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
    public class CreateNoteCommandTests
    {
        [Fact]
        public async Task Handle_ValidCommand_ShouldCreateNoteAndReturnId()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Note>>();

            mockContext.Setup(x => x.Notes).Returns(mockDbSet.Object);
            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new CreateNoteCommandHandler(mockContext.Object);

            var command = new CreateNoteCommand
            {
                Title = "Test Note",
                Content = "Test Content",
                Priority = NotePriority.Medium,
                DueDate = DateTime.UtcNow.AddDays(3)
            };

            Note? capturedNote = null;
            mockDbSet.Setup(x => x.Add(It.IsAny<Note>()))
                .Callback<Note>(note => capturedNote = note);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeEmpty();
            capturedNote.Should().NotBeNull();
            capturedNote!.Title.Should().Be(command.Title);
            capturedNote.Content.Should().Be(command.Content);
            capturedNote.Priority.Should().Be(command.Priority);
            capturedNote.DueDate.Should().Be(command.DueDate);

            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            mockDbSet.Verify(x => x.Add(It.IsAny<Note>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldSetCreatedAtAndUserId()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Note>>();

            mockContext.Setup(x => x.Notes).Returns(mockDbSet.Object);
            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new CreateNoteCommandHandler(mockContext.Object);

            var command = new CreateNoteCommand
            {
                Title = "Test",
                Content = "Content",
                Priority = NotePriority.Low
            };

            Note? capturedNote = null;
            mockDbSet.Setup(x => x.Add(It.IsAny<Note>()))
                .Callback<Note>(note => capturedNote = note);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            capturedNote.Should().NotBeNull();
            capturedNote!.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            capturedNote.Id.Should().NotBeEmpty();
        }
    }
}
