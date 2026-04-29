
using Application.Controller.Notes;
using Application.Controller.Notes.Queries.GetNotes;
using AutoMapper;
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

namespace Application.UnitTests.Notes.Queries
{
    public class GetNotesQueryTests
    {
        [Fact]
        public async Task Handle_ShouldReturnAllNotes_WhenNoFiltersApplied()
        {
            // Arrange
            var notes = new List<Note>
        {
            new() { Id = Guid.NewGuid(), Title = "Note 1", IsArchived = false, Priority = NotePriority.Low, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Note 2", IsArchived = true, Priority = NotePriority.High, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Note 3", IsArchived = false, Priority = NotePriority.Medium, CreatedAt = DateTime.UtcNow }
        }.AsQueryable();

            var mockDbSet = new Mock<DbSet<Note>>();
            mockDbSet.As<IQueryable<Note>>().Setup(m => m.Provider).Returns(notes.Provider);
            mockDbSet.As<IQueryable<Note>>().Setup(m => m.Expression).Returns(notes.Expression);
            mockDbSet.As<IQueryable<Note>>().Setup(m => m.ElementType).Returns(notes.ElementType);
            mockDbSet.As<IQueryable<Note>>().Setup(m => m.GetEnumerator()).Returns(notes.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(x => x.Notes).Returns(mockDbSet.Object);

            var mockMapper = new Mock<AutoMapper.IMapper>();
            mockMapper.Setup(x => x.Map<List<NoteDto>>(It.IsAny<List<Note>>()))
                .Returns((List<Note> source) => source.Select(n => new NoteDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Priority = n.Priority,
                    IsArchived = n.IsArchived
                }).ToList());

            var handler = new GetNotesQueryHandler(mockContext.Object, mockMapper.Object);
            var query = new GetNotesQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task Handle_WithArchiveFilter_ShouldReturnOnlyArchivedNotes()
        {
            // Arrange
            var notes = new List<Note>
        {
            new() { Id = Guid.NewGuid(), Title = "Archived Note", IsArchived = true, Priority = NotePriority.Low, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Active Note", IsArchived = false, Priority = NotePriority.Medium, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Archived Note 2", IsArchived = true, Priority = NotePriority.High, CreatedAt = DateTime.UtcNow }
        }.AsQueryable();

            var mockDbSet = new Mock<DbSet<Note>>();
            mockDbSet.As<IQueryable<Note>>().Setup(m => m.Provider).Returns(notes.Provider);
            mockDbSet.As<IQueryable<Note>>().Setup(m => m.Expression).Returns(notes.Expression);
            mockDbSet.As<IQueryable<Note>>().Setup(m => m.ElementType).Returns(notes.ElementType);
            mockDbSet.As<IQueryable<Note>>().Setup(m => m.GetEnumerator()).Returns(notes.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(x => x.Notes).Returns(mockDbSet.Object);

            var mockMapper = new Mock<AutoMapper.IMapper>();
            mockMapper.Setup(x => x.Map<List<NoteDto>>(It.IsAny<List<Note>>()))
                .Returns((List<Note> source) => source.Select(n => new NoteDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    IsArchived = n.IsArchived
                }).ToList());

            var handler = new GetNotesQueryHandler(mockContext.Object, mockMapper.Object);
            var query = new GetNotesQuery { IsArchived = true };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().AllSatisfy(note => note.IsArchived.Should().BeTrue());
        }

        [Fact]
        public async Task Handle_WithPriorityFilter_ShouldReturnNotesWithSpecificPriority()
        {
            // Arrange
            var notes = new List<Note>
        {
            new() { Id = Guid.NewGuid(), Title = "Low Priority", Priority = NotePriority.Low, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "High Priority", Priority = NotePriority.High, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Medium Priority", Priority = NotePriority.Medium, CreatedAt = DateTime.UtcNow }
        }.AsQueryable();

            var mockDbSet = new Mock<DbSet<Note>>();
            mockDbSet.As<IQueryable<Note>>().Setup(m => m.Provider).Returns(notes.Provider);
            mockDbSet.As<IQueryable<Note>>().Setup(m => m.Expression).Returns(notes.Expression);
            mockDbSet.As<IQueryable<Note>>().Setup(m => m.ElementType).Returns(notes.ElementType);
            mockDbSet.As<IQueryable<Note>>().Setup(m => m.GetEnumerator()).Returns(notes.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(x => x.Notes).Returns(mockDbSet.Object);

            var mockMapper = new Mock<AutoMapper.IMapper>();
            mockMapper.Setup(x => x.Map<List<NoteDto>>(It.IsAny<List<Note>>()))
                .Returns((List<Note> source) => source.Select(n => new NoteDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Priority = n.Priority
                }).ToList());

            var handler = new GetNotesQueryHandler(mockContext.Object, mockMapper.Object);
            var query = new GetNotesQuery { Priority = NotePriority.High };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().OnlyContain(note => note.Priority == NotePriority.High);
        }
    }
}
