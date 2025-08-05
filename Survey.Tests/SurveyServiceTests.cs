using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Survey.Models;
using Survey.Models.Dtos;
using Survey.Repositories;
using Survey.Services;
using Xunit;

namespace Survey.tests
{
    public class SurveyServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<SurveyService>> _mockLogger;
        private readonly SurveyService _service;

        public SurveyServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<SurveyService>>();
            _service = new SurveyService(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Create_ShouldReturnSurvey_WhenValidInput()
        {
            // Arrange
            var surveyDto = new SurveyCreateDto(
                "Test Survey",
                "Desc",
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(1),
                new List<QuestionCreateDto>
                {
                    new QuestionCreateDto("Q1", "short answer", true, null, null)
                }
            );
            var adminEmail = "admin@example.com";
            _mockUnitOfWork.Setup(u => u.Surveys.AddAsync(It.IsAny<SurveyModel>())).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.Create(surveyDto, adminEmail);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be(surveyDto.Title);
            result.Questions.Should().HaveCount(1);
            _mockUnitOfWork.Verify(u => u.Surveys.AddAsync(It.IsAny<SurveyModel>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task GetById_ShouldReturnNull_WhenSurveyNotFound()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.Surveys.GetByIdWithQuestionsAsync(It.IsAny<int>()))
                .ReturnsAsync((SurveyModel)null);

            // Act
            var result = await _service.GetById(1);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ShouldReturnFalse_WhenSurveyNotFound()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.Surveys.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((SurveyModel)null);

            // Act
            var result = await _service.Delete(1);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Delete_ShouldReturnTrue_WhenSurveyExists()
        {
            // Arrange
            var survey = new SurveyModel { Id = 1 };
            _mockUnitOfWork.Setup(u => u.Surveys.GetByIdAsync(1)).ReturnsAsync(survey);
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _service.Delete(1);

            // Assert
            result.Should().BeTrue();
            _mockUnitOfWork.Verify(u => u.Surveys.Delete(survey), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AddQuestion_ShouldReturnNull_WhenSurveyNotFound()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.Surveys.GetByIdWithQuestionsAsync(It.IsAny<int>()))
                .ReturnsAsync((SurveyModel)null);

            // Act
            var result = await _service.AddQuestion(1, new QuestionCreateDto("Q1", "short answer", true, null, null));

            // Assert
            result.Should().BeNull();
        }
    }
}