using Application.Common.Exceptions;
using Application.Controller.Notes;
using Application.Controller.Notes.Queries.GetNoteById;

using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.UnitTests.Notes.Queries
{

    public class GetNoteByIdQueryTests
    {
        [Fact]
        public async Task Handle_NoteExists_ShouldReturnNote()
        {
            // Arrange
            var noteId = Guid.NewGuid();
            var existingNote = new Note
            {
                Id = noteId,
                Title = "Test Note",
                Content = "Test Content",
                Priority = NotePriority.High,
                CreatedAt = DateTime.UtcNow
            };

            var notes = new List<Note> { existingNote }.AsQueryable();

            var mockDbSet = new Mock<DbSet<Note>>();
            mockDbSet.As<IQueryable<Note>>().Setup(m => m.Provider).Returns(notes.Provider);
            mockDbSet.As<IQueryable<Note>>().Setup(m => m.Expression).Returns(notes.Expression);
            mockDbSet.As<IQueryable<Note>>().Setup(m => m.ElementType).Returns(notes.ElementType);
            mockDbSet.As<IQueryable<Note>>().Setup(m => m.GetEnumerator()).Returns(notes.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(x => x.Notes).Returns(mockDbSet.Object);

            var mockMapper = new Mock<AutoMapper.IMapper>();
            mockMapper.Setup(x => x.Map<NoteDto>(It.IsAny<Note>()))
                .Returns((Note source) => new NoteDto
                {
                    Id = source.Id,
                    Title = source.Title,
                    Content = source.Content,
                    Priority = source.Priority,
                    CreatedAt = source.CreatedAt
                });

            var handler = new GetNoteByIdQueryHandler(mockContext.Object, mockMapper.Object);
            var query = new GetNoteByIdQuery(noteId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(noteId);
            result.Title.Should().Be("Test Note");
        }

        [Fact]
        public async Task Handle_NoteNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var noteId = Guid.NewGuid();
            var notes = new List<Note>().AsQueryable();

            var mockDbSet = new Mock<DbSet<Note>>();
            mockDbSet.As<IQueryable<Note>>().Setup(m => m.Provider).Returns(notes.Provider);
            mockDbSet.As<IQueryable<Note>>().Setup(m => m.Expression).Returns(notes.Expression);
            mockDbSet.As<IQueryable<Note>>().Setup(m => m.ElementType).Returns(notes.ElementType);
            mockDbSet.As<IQueryable<Note>>().Setup(m => m.GetEnumerator()).Returns(notes.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(x => x.Notes).Returns(mockDbSet.Object);

            var mockMapper = new Mock<AutoMapper.IMapper>();

            var handler = new GetNoteByIdQueryHandler(mockContext.Object, mockMapper.Object);
            var query = new GetNoteByIdQuery(noteId);

            // Act
            Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
