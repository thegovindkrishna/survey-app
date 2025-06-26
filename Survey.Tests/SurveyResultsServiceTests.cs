using Xunit;
using Survey.Services;
using Survey.Data;
using Survey.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Survey.Tests
{
    /// <summary>
    /// Unit tests for SurveyResultsService covering survey result aggregation, CSV/PDF export, and share link generation.
    /// Uses in-memory database and mock configuration for isolation.
    /// </summary>
    public class SurveyResultsServiceTests
    {
        private readonly SurveyResultsService _service;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public SurveyResultsServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);
            var inMemorySettings = new Dictionary<string, string> {
                {"BaseUrl", "http://localhost:4200"}
            };
            _config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
            _service = new SurveyResultsService(_context, _config);
        }

        /// <summary>
        /// Ensures GetSurveyResults returns aggregated results for a survey with responses.
        /// </summary>
        [Fact]
        public async Task GetSurveyResults_ReturnsResults_WhenSurveyExists()
        {
            var survey = new Survey.Models.Survey { Title = "A", Questions = new List<Question> { new Question { QuestionText = "Q", type = "text" } } };
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
            var questionId = survey.Questions.First().Id;
            _context.SurveyResponses.Add(new SurveyResponse { SurveyId = survey.Id, RespondentEmail = "a", responses = new List<QuestionResponse> { new QuestionResponse { QuestionId = questionId, response = "ans" } } });
            await _context.SaveChangesAsync();
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
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetSurveyResults(999));
        }

        /// <summary>
        /// Ensures ExportToCsv returns non-empty bytes for a survey with responses.
        /// </summary>
        [Fact]
        public async Task ExportToCsv_ReturnsBytes_WhenSurveyExists()
        {
            var survey = new Survey.Models.Survey { Title = "A", Questions = new List<Question> { new Question { QuestionText = "Q", type = "text" } } };
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
            var questionId = survey.Questions.First().Id;
            _context.SurveyResponses.Add(new SurveyResponse { SurveyId = survey.Id, RespondentEmail = "a", responses = new List<QuestionResponse> { new QuestionResponse { QuestionId = questionId, response = "ans" } } });
            await _context.SaveChangesAsync();
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
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ExportToCsv(999));
        }

        /// <summary>
        /// Ensures ExportToPdf returns non-empty bytes for a survey with responses.
        /// </summary>
        [Fact]
        public async Task ExportToPdf_ReturnsBytes_WhenSurveyExists()
        {
            var survey = new Survey.Models.Survey { Title = "A", Questions = new List<Question> { new Question { QuestionText = "Q", type = "text" } } };
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
            var questionId = survey.Questions.First().Id;
            _context.SurveyResponses.Add(new SurveyResponse { SurveyId = survey.Id, RespondentEmail = "a", responses = new List<QuestionResponse> { new QuestionResponse { QuestionId = questionId, response = "ans" } } });
            await _context.SaveChangesAsync();
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
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ExportToPdf(999));
        }

        /// <summary>
        /// Ensures GenerateShareLink returns a valid link and updates the survey when the survey exists.
        /// </summary>
        [Fact]
        public async Task GenerateShareLink_ReturnsLink_WhenSurveyExists()
        {
            var survey = new Survey.Models.Survey { Title = "A" };
            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
            var link = await _service.GenerateShareLink(survey.Id);
            Assert.Contains($"/survey/{survey.Id}", link);
            var updated = await _context.Surveys.FindAsync(survey.Id);
            Assert.Equal(link, updated.ShareLink);
        }

        /// <summary>
        /// Ensures GenerateShareLink throws when the survey does not exist.
        /// </summary>
        [Fact]
        public async Task GenerateShareLink_Throws_WhenSurveyNotFound()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GenerateShareLink(999));
        }
    }
} 