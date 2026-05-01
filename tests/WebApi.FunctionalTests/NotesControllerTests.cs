using Application.Common.Exceptions;
using Application.Controller.Notes;
using Application.Controller.Notes.Commands.CreateNote;
using Application.Controller.Notes.Commands.DeleteNote;
using Application.Controller.Notes.Commands.UpdateNote;
using Application.Controller.Notes.Queries.GetNoteById;
using Application.Controller.Notes.Queries.GetNotes;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace WebApi.FunctionalTests
{
    public class NotesControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly Mock<IMediator> _mediatorMock;

        public NotesControllerTests(WebApplicationFactory<Program> factory)
        {
            _mediatorMock = new Mock<IMediator>();

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the real IMediator
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IMediator));

                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add mocked IMediator
                    services.AddSingleton(_mediatorMock.Object);
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetNotes_ShouldReturnOkWithNotesList()
        {
            // Arrange
            var expectedNotes = new List<NoteDto>
        {
            new() { Id = Guid.NewGuid(), Title = "Note 1", Content = "Content 1", Priority = NotePriority.Low },
            new() { Id = Guid.NewGuid(), Title = "Note 2", Content = "Content 2", Priority = NotePriority.High }
        };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetNotesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedNotes);

            // Act
            var response = await _client.GetAsync("/api/notes");
            var result = await response.Content.ReadFromJsonAsync<List<NoteDto>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result![0].Title.Should().Be("Note 1");
            result[1].Title.Should().Be("Note 2");

            _mediatorMock.Verify(m => m.Send(It.IsAny<GetNotesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetNotes_WithFilters_ShouldReturnFilteredNotes()
        {
            // Arrange
            var query = new GetNotesQuery { IsArchived = true, Priority = NotePriority.High };
            var expectedNotes = new List<NoteDto>
        {
            new() { Id = Guid.NewGuid(), Title = "Archived High Priority", IsArchived = true, Priority = NotePriority.High }
        };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetNotesQuery>(q => q.IsArchived == true && q.Priority == NotePriority.High),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedNotes);

            // Act
            var response = await _client.GetAsync("/api/notes?isArchived=true&priority=2");
            var result = await response.Content.ReadFromJsonAsync<List<NoteDto>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result![0].IsArchived.Should().BeTrue();
            result[0].Priority.Should().Be(NotePriority.High);
        }

        [Fact]
        public async Task GetNoteById_WithValidId_ShouldReturnOkWithNote()
        {
            // Arrange
            var noteId = Guid.NewGuid();
            var expectedNote = new NoteDto
            {
                Id = noteId,
                Title = "Test Note",
                Content = "Test Content",
                Priority = NotePriority.Medium,
                CreatedAt = DateTime.UtcNow
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetNoteByIdQuery>(q => q.Id == noteId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedNote);

            // Act
            var response = await _client.GetAsync($"/api/notes/{noteId}");
            var result = await response.Content.ReadFromJsonAsync<NoteDto>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().NotBeNull();
            result!.Id.Should().Be(noteId);
            result.Title.Should().Be("Test Note");
            result.Content.Should().Be("Test Content");
        }

        [Fact]
        public async Task GetNoteById_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetNoteByIdQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException(nameof(Note), invalidId));

            // Act
            var response = await _client.GetAsync($"/api/notes/{invalidId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateNote_WithValidData_ShouldReturnCreatedWithId()
        {
            // Arrange
            var newNoteId = Guid.NewGuid();
            var createCommand = new CreateNoteCommand
            {
                Title = "New Note",
                Content = "New Content",
                Priority = NotePriority.Medium,
                DueDate = DateTime.UtcNow.AddDays(3)
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateNoteCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newNoteId);

            // Act
            var response = await _client.PostAsJsonAsync("/api/notes", createCommand);
            var result = await response.Content.ReadFromJsonAsync<Guid>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            result.Should().Be(newNoteId);
            response.Headers.Location.Should().NotBeNull();
            response.Headers.Location!.ToString().Should().Contain($"/api/notes/{newNoteId}");

            _mediatorMock.Verify(m => m.Send(It.IsAny<CreateNoteCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateNote_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var invalidCommand = new CreateNoteCommand
            {
                Title = "", // Empty title - invalid
                Content = "Content",
                Priority = NotePriority.Low
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/notes", invalidCommand);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            _mediatorMock.Verify(m => m.Send(It.IsAny<CreateNoteCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateNote_WithValidData_ShouldReturnNoContent()
        {
            // Arrange
            var noteId = Guid.NewGuid();
            var updateCommand = new UpdateNoteCommand
            {
                Id = noteId,
                Title = "Updated Title",
                Content = "Updated Content",
                Priority = NotePriority.High,
                IsArchived = true
            };

            _mediatorMock
                .Setup(m => m.Send(It.Is<UpdateNoteCommand>(c => c.Id == noteId), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Unit.Value));

            // Act
            var response = await _client.PutAsJsonAsync($"/api/notes/{noteId}", updateCommand);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateNoteCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateNote_WithMismatchedIds_ShouldReturnBadRequest()
        {
            // Arrange
            var noteId = Guid.NewGuid();
            var updateCommand = new UpdateNoteCommand
            {
                Id = Guid.NewGuid(), // Different ID
                Title = "Updated Title",
                Content = "Content"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/notes/{noteId}", updateCommand);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateNoteCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateNote_WithNonExistentNote_ShouldReturnNotFound()
        {
            // Arrange
            var noteId = Guid.NewGuid();
            var updateCommand = new UpdateNoteCommand
            {
                Id = noteId,
                Title = "Updated Title",
                Content = "Content"
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<UpdateNoteCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException(nameof(Note), noteId));

            // Act
            var response = await _client.PutAsJsonAsync($"/api/notes/{noteId}", updateCommand);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteNote_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            var noteId = Guid.NewGuid();

            _mediatorMock
                .Setup(m => m.Send(It.Is<DeleteNoteCommand>(c => c.Id == noteId), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Unit.Value));

            // Act
            var response = await _client.DeleteAsync($"/api/notes/{noteId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            _mediatorMock.Verify(m => m.Send(It.IsAny<DeleteNoteCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteNote_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var noteId = Guid.NewGuid();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<DeleteNoteCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException(nameof(Note), noteId));

            // Act
            var response = await _client.DeleteAsync($"/api/notes/{noteId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateNote_WithVeryLongTitle_ShouldReturnBadRequest()
        {
            // Arrange
            var invalidCommand = new CreateNoteCommand
            {
                Title = new string('A', 201), // 201 characters
                Content = "Valid Content",
                Priority = NotePriority.Low
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/notes", invalidCommand);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateNote_WithVeryLongContent_ShouldReturnBadRequest()
        {
            // Arrange
            var invalidCommand = new CreateNoteCommand
            {
                Title = "Valid Title",
                Content = new string('B', 2001), // 2001 characters
                Priority = NotePriority.Low
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/notes", invalidCommand);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
