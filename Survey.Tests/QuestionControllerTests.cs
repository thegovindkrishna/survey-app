using Xunit;
using Moq;
using Survey.Controllers;
using Survey.Services;
using Survey.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using SurveyModel = Survey.Models.Survey;

namespace Survey.Tests
{
    /// <summary>
    /// Unit tests for QuestionController covering question CRUD endpoints using mocked ISurveyService.
    /// </summary>
    public class QuestionControllerTests
    {
        private readonly Mock<ISurveyService> _mockSurveyService;
        private readonly QuestionController _controller;

        public QuestionControllerTests()
        {
            _mockSurveyService = new Mock<ISurveyService>();
            _controller = new QuestionController(_mockSurveyService.Object);

            // Setup dummy admin user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "admin@example.com"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        /// <summary>
        /// Ensures AddQuestion returns Ok when a question is successfully added.
        /// </summary>
        [Fact]
        public async Task AddQuestion_ReturnsOk_WhenSuccessful()
        {
            var question = new Question { QuestionText = "Q1", type = "text", required = true };
            var survey = new SurveyModel { Id = 1, Questions = new List<Question> { question } };
            _mockSurveyService.Setup(s => s.AddQuestion(1, question)).ReturnsAsync(survey);

            var result = await _controller.AddQuestion(1, question);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(survey, okResult.Value);
        }

        /// <summary>
        /// Ensures AddQuestion returns NotFound when the survey does not exist.
        /// </summary>
        [Fact]
        public async Task AddQuestion_ReturnsNotFound_WhenSurveyMissing()
        {
            var question = new Question { QuestionText = "Q1", type = "text", required = true };
            _mockSurveyService.Setup(s => s.AddQuestion(1, question)).ReturnsAsync((SurveyModel?)null);

            var result = await _controller.AddQuestion(1, question);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Survey not found", notFound.Value);
        }

        /// <summary>
        /// Ensures GetQuestions returns Ok with questions when the survey exists.
        /// </summary>
        [Fact]
        public async Task GetQuestions_ReturnsOk_WhenSurveyExists()
        {
            var questions = new List<Question> { new Question { Id = 1, QuestionText = "Q1" } };
            var survey = new SurveyModel { Id = 1, Questions = questions };
            _mockSurveyService.Setup(s => s.GetById(1)).ReturnsAsync(survey);

            var result = await _controller.GetQuestions(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(questions, okResult.Value);
        }

        /// <summary>
        /// Ensures GetQuestions returns NotFound when the survey does not exist.
        /// </summary>
        [Fact]
        public async Task GetQuestions_ReturnsNotFound_WhenSurveyMissing()
        {
            _mockSurveyService.Setup(s => s.GetById(1)).ReturnsAsync((SurveyModel?)null);

            var result = await _controller.GetQuestions(1);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Survey not found", notFound.Value);
        }

        /// <summary>
        /// Ensures UpdateQuestion returns Ok when a question is successfully updated.
        /// </summary>
        [Fact]
        public async Task UpdateQuestion_ReturnsOk_WhenSuccessful()
        {
            var question = new Question { Id = 1, QuestionText = "Q1 updated" };
            var survey = new SurveyModel { Id = 1, Questions = new List<Question> { question } };
            _mockSurveyService.Setup(s => s.UpdateQuestion(1, 1, question)).ReturnsAsync(survey);

            var result = await _controller.UpdateQuestion(1, 1, question);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(survey, okResult.Value);
        }

        /// <summary>
        /// Ensures UpdateQuestion returns NotFound when the survey or question does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateQuestion_ReturnsNotFound_WhenSurveyOrQuestionMissing()
        {
            var question = new Question { Id = 1, QuestionText = "Q1 updated" };
            _mockSurveyService.Setup(s => s.UpdateQuestion(1, 1, question)).ReturnsAsync((SurveyModel?)null);

            var result = await _controller.UpdateQuestion(1, 1, question);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Survey or question not found", notFound.Value);
        }

        /// <summary>
        /// Ensures DeleteQuestion returns Ok when a question is successfully deleted.
        /// </summary>
        [Fact]
        public async Task DeleteQuestion_ReturnsOk_WhenSuccessful()
        {
            var survey = new SurveyModel { Id = 1 };
            _mockSurveyService.Setup(s => s.DeleteQuestion(1, 1)).ReturnsAsync(survey);

            var result = await _controller.DeleteQuestion(1, 1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(survey, okResult.Value);
        }

        /// <summary>
        /// Ensures DeleteQuestion returns NotFound when the survey or question does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteQuestion_ReturnsNotFound_WhenSurveyOrQuestionMissing()
        {
            _mockSurveyService.Setup(s => s.DeleteQuestion(1, 1)).ReturnsAsync((SurveyModel?)null);

            var result = await _controller.DeleteQuestion(1, 1);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Survey or question not found", notFound.Value);
        }
    }
} 