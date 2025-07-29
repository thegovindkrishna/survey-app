using Xunit;
using Moq;
using Survey.Services;
using Survey.Data;
using Survey.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Survey.Repositories;

namespace Survey.Tests
{
    /// <summary>
    /// Unit tests for SurveyResultsService covering survey result aggregation, CSV/PDF export, and share link generation.
    /// Uses in-memory database and mock configuration for isolation.
    /// </summary>
    public class SurveyResultsServiceTests
    {
        private readonly SurveyResultsService _service;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly IConfiguration _config;
        private readonly Mock<ILogger<SurveyResultsService>> _mockLogger;

        public SurveyResultsServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"BaseUrl", "http://localhost:4200"}
            };
            _config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<SurveyResultsService>>();
            _service = new SurveyResultsService(_mockUnitOfWork.Object, _config, _mockLogger.Object);
        }

        /// <summary>
        /// Ensures GetSurveyResults returns aggregated results for a survey with responses.
        /// </summary>
        [Fact]
        public async Task GetSurveyResults_ReturnsResults_WhenSurveyExists()
        {
            var survey = new Survey.Models.Survey { Title = "A", Questions = new List<Question> { new Question { QuestionText = "Q", Type = "text" } } };
            _mockUnitOfWork.Setup(uow => uow.Surveys.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Survey.Models.Survey, bool>>>(), null))
                .ReturnsAsync(survey);
            _mockUnitOfWork.Setup(uow => uow.SurveyResponses.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<SurveyResponse, bool>>>()))
                .ReturnsAsync(new List<SurveyResponse> { new SurveyResponse { SurveyId = survey.Id, RespondentEmail = "a", responses = new List<QuestionResponse> { new QuestionResponse { QuestionId = survey.Questions.First().Id, response = "ans" } } } });

            var results = await _service.GetSurveyResults(survey.Id);
            Assert.Equal(survey.Id, results.SurveyId);
            Assert.Single(results.QuestionResults);
        }

        /// <summary>
        /// Ensures GetSurveyResults throws when the survey does not exist.
        /// </summary>
        [Fact]
        public async Task GetSurveyResults_Throws_WhenSurveyNotFound()
        {
            _mockUnitOfWork.Setup(uow => uow.Surveys.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Survey.Models.Survey, bool>>>(), null))
                .ReturnsAsync((Survey.Models.Survey?)null);
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetSurveyResults(999));
        }

        /// <summary>
        /// Ensures ExportToCsv returns non-empty bytes for a survey with responses.
        /// </summary>
        [Fact]
        public async Task ExportToCsv_ReturnsBytes_WhenSurveyExists()
        {
            var survey = new Survey.Models.Survey { Title = "A", Questions = new List<Question> { new Question { QuestionText = "Q", Type = "text" } } };
            _mockUnitOfWork.Setup(uow => uow.Surveys.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Survey.Models.Survey, bool>>>(), null))
                .ReturnsAsync(survey);
            _mockUnitOfWork.Setup(uow => uow.SurveyResponses.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<SurveyResponse, bool>>>()))
                .ReturnsAsync(new List<SurveyResponse> { new SurveyResponse { SurveyId = survey.Id, RespondentEmail = "a", responses = new List<QuestionResponse> { new QuestionResponse { QuestionId = survey.Questions.First().Id, response = "ans" } } } });

            var bytes = await _service.ExportToCsv(survey.Id);
            Assert.NotNull(bytes);
            Assert.True(bytes.Length > 0);
        }

        /// <summary>
        /// Ensures ExportToCsv throws when the survey does not exist.
        /// </summary>
        [Fact]
        public async Task ExportToCsv_Throws_WhenSurveyNotFound()
        {
            _mockUnitOfWork.Setup(uow => uow.Surveys.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Survey.Models.Survey, bool>>>(), null))
                .ReturnsAsync((Survey.Models.Survey?)null);
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ExportToCsv(999));
        }

        /// <summary>
        /// Ensures ExportToPdf returns non-empty bytes for a survey with responses.
        /// </summary>
        [Fact]
        public async Task ExportToPdf_ReturnsBytes_WhenSurveyExists()
        {
            var survey = new Survey.Models.Survey { Title = "A", Questions = new List<Question> { new Question { QuestionText = "Q", Type = "text" } } };
            _mockUnitOfWork.Setup(uow => uow.Surveys.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Survey.Models.Survey, bool>>>(), null))
                .ReturnsAsync(survey);
            _mockUnitOfWork.Setup(uow => uow.SurveyResponses.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<SurveyResponse, bool>>>()))
                .ReturnsAsync(new List<SurveyResponse> { new SurveyResponse { SurveyId = survey.Id, RespondentEmail = "a", responses = new List<QuestionResponse> { new QuestionResponse { QuestionId = survey.Questions.First().Id, response = "ans" } } } });

            var bytes = await _service.ExportToPdf(survey.Id);
            Assert.NotNull(bytes);
            Assert.True(bytes.Length > 0);
        }

        /// <summary>
        /// Ensures ExportToPdf throws when the survey does not exist.
        /// </summary>
        [Fact]
        public async Task ExportToPdf_Throws_WhenSurveyNotFound()
        {
            _mockUnitOfWork.Setup(uow => uow.Surveys.GetFirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Survey.Models.Survey, bool>>>(), null))
                .ReturnsAsync((Survey.Models.Survey?)null);
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ExportToPdf(999));
        }

        /// <summary>
        /// Ensures GenerateShareLink returns a valid link and updates the survey when the survey exists.
        /// </summary>
        [Fact]
        public async Task GenerateShareLink_ReturnsLink_WhenSurveyExists()
        {
            var survey = new Survey.Models.Survey { Id = 1, Title = "A" };
            _mockUnitOfWork.Setup(uow => uow.Surveys.GetByIdAsync(1)).ReturnsAsync(survey);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).Returns(Task.CompletedTask);

            var link = await _service.GenerateShareLink(survey.Id);
            Assert.Contains($"/survey/{survey.Id}", link);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        /// <summary>
        /// Ensures GenerateShareLink throws when the survey does not exist.
        /// </summary>
        [Fact]
        public async Task GenerateShareLink_Throws_WhenSurveyNotFound()
        {
            _mockUnitOfWork.Setup(uow => uow.Surveys.GetByIdAsync(999)).ReturnsAsync((Survey.Models.Survey?)null);
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GenerateShareLink(999));
        }
    }
}