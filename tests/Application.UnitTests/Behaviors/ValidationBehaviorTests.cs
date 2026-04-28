using Application.Behaviors;
using Application.Common.Exceptions;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.UnitTests.Behaviors
{
    public class ValidationBehaviorTests
    {
        [Fact]
        public async Task Handle_WithValidationErrors_ShouldThrowValidationException()
        {
            // Arrange
            var validatorMock = new Mock<IValidator<CreateNoteCommand>>();
            validatorMock.Setup(x => x.ValidateAsync(It.IsAny<CreateNoteCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[]
                {
                new FluentValidation.Results.ValidationFailure("Title", "Title is required")
                }));

            var validators = new List<IValidator<CreateNoteCommand>> { validatorMock.Object };
            var behavior = new ValidationBehavior<CreateNoteCommand, Guid>(validators);
            var request = new CreateNoteCommand();
            var next = new Mock<RequestHandlerDelegate<Guid>>();

            // Act
            Func<Task> act = async () => await behavior.Handle(request, next.Object, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<AppValidationException>();
            next.Verify(x => x(), Times.Never);
        }

        [Fact]
        public async Task Handle_WithoutValidationErrors_ShouldCallNext()
        {
            // Arrange
            var validatorMock = new Mock<IValidator<CreateNoteCommand>>();
            validatorMock.Setup(x => x.ValidateAsync(It.IsAny<CreateNoteCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            var validators = new List<IValidator<CreateNoteCommand>> { validatorMock.Object };
            var behavior = new ValidationBehavior<CreateNoteCommand, Guid>(validators);
            var request = new CreateNoteCommand();
            var next = new Mock<RequestHandlerDelegate<Guid>>();
            next.Setup(x => x()).ReturnsAsync(Guid.NewGuid());

            // Act
            var result = await behavior.Handle(request, next.Object, CancellationToken.None);

            // Assert
            result.Should().NotBeEmpty();
            next.Verify(x => x(), Times.Once);
        }
    }

    public record CreateNoteCommand : IRequest<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
