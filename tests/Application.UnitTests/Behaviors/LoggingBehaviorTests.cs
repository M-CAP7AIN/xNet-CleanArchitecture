using Application.Behaviors;
using Application.Controller.Notes;
using Application.Controller.Notes.Queries.GetNotes;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.UnitTests.Behaviors
{
    public class LoggingBehaviorTests
    {
        [Fact]
        public async Task Handle_ShouldLogRequestInformation()
        {
            // Arrange
            var logger = Substitute.For<ILogger<LoggingBehavior<GetNotesQuery, List<NoteDto>>>>();
            var behavior = new LoggingBehavior<GetNotesQuery, List<NoteDto>>(logger);
            var request = new GetNotesQuery();
            var next = Substitute.For<RequestHandlerDelegate<List<NoteDto>>>();
            next().Returns(Task.FromResult(new List<NoteDto>()));

            // Act
            await behavior.Handle(request, next, CancellationToken.None);

            // Assert
            logger.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>()
            );
        }
    }
}
