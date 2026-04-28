using Application.Controller.Notes.Commands.CreateNote;
using Domain.Enums;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.UnitTests.Validators
{
    public class CreateNoteCommandValidatorTests
    {
        private readonly CreateNoteCommandValidator _validator;

        public CreateNoteCommandValidatorTests()
        {
            _validator = new CreateNoteCommandValidator();
        }

        [Fact]
        public void Validate_ValidCommand_ShouldNotHaveErrors()
        {
            // Arrange
            var command = new CreateNoteCommand
            {
                Title = "Valid Title",
                Content = "Valid Content",
                Priority = NotePriority.Medium
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(" ")]
        public void Validate_EmptyTitle_ShouldHaveError(string invalidTitle)
        {
            // Arrange
            var command = new CreateNoteCommand
            {
                Title = invalidTitle,
                Content = "Valid Content",
                Priority = NotePriority.Low
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Title");
        }

        [Fact]
        public void Validate_TitleTooLong_ShouldHaveError()
        {
            // Arrange
            var command = new CreateNoteCommand
            {
                Title = new string('A', 201),
                Content = "Valid Content",
                Priority = NotePriority.Low
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Title");
        }

        [Fact]
        public void Validate_ContentTooLong_ShouldHaveError()
        {
            // Arrange
            var command = new CreateNoteCommand
            {
                Title = "Valid Title",
                Content = new string('B', 2001),
                Priority = NotePriority.Low
            };

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Content");
        }
    }
}
