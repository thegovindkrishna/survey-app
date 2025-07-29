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
using Survey.Models.Dtos;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Survey.Tests
{
    /// <summary>
    /// Unit tests for QuestionController covering question CRUD endpoints using mocked ISurveyService.
    /// </summary>
    public class QuestionControllerTests
    {
        private readonly Mock<ISurveyService> _mockSurveyService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<QuestionController>> _mockLogger;
        private readonly QuestionController _controller;

        public QuestionControllerTests()
        {
            _mockSurveyService = new Mock<ISurveyService>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<QuestionController>>();
            _controller = new QuestionController(_mockSurveyService.Object, _mockMapper.Object, _mockLogger.Object);

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
            var createDto = new QuestionCreateDto("Q1", "text", true, null, null);
            var surveyDto = new SurveyDto(1, "Survey", "Desc", DateTime.Now, DateTime.Now.AddDays(7), "admin@example.com", null, new List<QuestionDto> { new QuestionDto(1, "Q1", "text", true, null, null) });

            _mockSurveyService.Setup(s => s.AddQuestion(1, createDto)).ReturnsAsync(new Survey.Models.Survey { Id = surveyDto.Id, Title = surveyDto.Title, Description = surveyDto.Description, StartDate = surveyDto.StartDate, EndDate = surveyDto.EndDate, CreatedBy = surveyDto.CreatedBy, ShareLink = surveyDto.ShareLink, Questions = surveyDto.Questions.Select(q => new Question { Id = q.Id, QuestionText = q.QuestionText, Type = q.Type, Required = q.Required, Options = q.Options, MaxRating = q.MaxRating }).ToList() });

            var result = await _controller.AddQuestion(1, createDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(surveyDto, okResult.Value);
        }

        /// <summary>
        /// Ensures AddQuestion returns NotFound when the survey does not exist.
        /// </summary>
        [Fact]
        public async Task AddQuestion_ReturnsNotFound_WhenSurveyMissing()
        {
            var createDto = new QuestionCreateDto("Q1", "text", true, null, null);
            _mockSurveyService.Setup(s => s.AddQuestion(1, createDto)).ReturnsAsync((Survey.Models.Survey?)null);

            var result = await _controller.AddQuestion(1, createDto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Survey not found", notFound.Value);
        }

        /// <summary>
        /// Ensures GetQuestions returns Ok with questions when the survey exists.
        /// </summary>
        [Fact]
        public async Task GetQuestions_ReturnsOk_WhenSurveyExists()
        {
            var questionDtos = new List<QuestionDto> { new QuestionDto(1, "Q1", "text", true, null, null) };
            var surveyDto = new SurveyDto(1, "Survey", "Desc", DateTime.Now, DateTime.Now.AddDays(7), "admin@example.com", null, questionDtos);
            _mockSurveyService.Setup(s => s.GetById(1)).ReturnsAsync(surveyDto);

            var result = await _controller.GetQuestions(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(questionDtos, okResult.Value);
        }

        /// <summary>
        /// Ensures GetQuestions returns NotFound when the survey does not exist.
        /// </summary>
        [Fact]
        public async Task GetQuestions_ReturnsNotFound_WhenSurveyMissing()
        {
            _mockSurveyService.Setup(s => s.GetById(1)).ReturnsAsync((SurveyDto?)null);

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
            var updateDto = new QuestionUpdateDto("Q1 updated", "text", true, null, null);
            var surveyDto = new SurveyDto(1, "Survey", "Desc", DateTime.Now, DateTime.Now.AddDays(7), "admin@example.com", null, new List<QuestionDto> { new QuestionDto(1, "Q1 updated", "text", true, null, null) });

            _mockSurveyService.Setup(s => s.UpdateQuestion(1, 1, updateDto)).ReturnsAsync(surveyDto);

            var result = await _controller.UpdateQuestion(1, 1, updateDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(surveyDto, okResult.Value);
        }

        /// <summary>
        /// Ensures UpdateQuestion returns NotFound when the survey or question does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateQuestion_ReturnsNotFound_WhenSurveyOrQuestionMissing()
        {
            var updateDto = new QuestionUpdateDto("Q1 updated", "text", true, null, null);
            _mockSurveyService.Setup(s => s.UpdateQuestion(1, 1, updateDto)).ReturnsAsync((SurveyDto?)null);

            var result = await _controller.UpdateQuestion(1, 1, updateDto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Survey or question not found", notFound.Value);
        }

        /// <summary>
        /// Ensures DeleteQuestion returns Ok when a question is successfully deleted.
        /// </summary>
        [Fact]
        public async Task DeleteQuestion_ReturnsOk_WhenSuccessful()
        {
            var surveyDto = new SurveyDto(1, "Survey", "Desc", DateTime.Now, DateTime.Now.AddDays(7), "admin@example.com", null, new List<QuestionDto>());
            _mockSurveyService.Setup(s => s.DeleteQuestion(1, 1)).ReturnsAsync(surveyDto);

            var result = await _controller.DeleteQuestion(1, 1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(surveyDto, okResult.Value);
        }

        /// <summary>
        /// Ensures DeleteQuestion returns NotFound when the survey or question does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteQuestion_ReturnsNotFound_WhenSurveyOrQuestionMissing()
        {
            _mockSurveyService.Setup(s => s.DeleteQuestion(1, 1)).ReturnsAsync((SurveyDto?)null);

            var result = await _controller.DeleteQuestion(1, 1);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Survey or question not found", notFound.Value);
        }
    }
} 