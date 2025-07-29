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
using AutoMapper;
using Microsoft.Extensions.Logging;
using Survey.Models.Dtos;
using Survey.Models.Dtos;

namespace Survey.Tests
{
    public class UserControllerTests
    {
        private readonly Mock<ISurveyService> _mockSurveyService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<UserController>> _mockLogger;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockSurveyService = new Mock<ISurveyService>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<UserController>>();
            _controller = new UserController(_mockSurveyService.Object, _mockMapper.Object, _mockLogger.Object);

            // Setup dummy user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "user@example.com"),
                new Claim(ClaimTypes.Role, "User")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetAvailableSurveys_ReturnsOnlyActiveSurveys()
        {
            var now = DateTime.UtcNow;
            var activeSurvey = new SurveyModel { Id = 1, Title = "Active", StartDate = now.AddDays(-1), EndDate = now.AddDays(1) };
            var inactiveSurvey = new SurveyModel { Id = 2, Title = "Inactive", StartDate = now.AddDays(-10), EndDate = now.AddDays(-5) };
            _mockSurveyService.Setup(s => s.GetAll()).ReturnsAsync(new List<SurveyDto> { _mockMapper.Object.Map<SurveyDto>(activeSurvey), _mockMapper.Object.Map<SurveyDto>(inactiveSurvey) });

            var result = await _controller.GetAvailableSurveys();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<List<SurveyDto>>(okResult.Value);
            Assert.Single(returned);
            Assert.Equal(activeSurvey.Id, returned[0].Id);
        }

        [Fact]
        public async Task GetSurvey_ReturnsOk_WhenSurveyIsActive()
        {
            var now = DateTime.UtcNow;
            var survey = new SurveyModel { Id = 1, Title = "Active", StartDate = now.AddDays(-1), EndDate = now.AddDays(1) };
            _mockSurveyService.Setup(s => s.GetById(1)).ReturnsAsync(survey);

            var result = await _controller.GetSurvey(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(survey, okResult.Value);
        }

        [Fact]
        public async Task GetSurvey_ReturnsNotFound_WhenSurveyNotFound()
        {
            _mockSurveyService.Setup(s => s.GetById(1)).ReturnsAsync((SurveyDto?)null);

            var result = await _controller.GetSurvey(1);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Survey not found", notFound.Value);
        }

        [Fact]
        public async Task GetSurvey_ReturnsNotFound_WhenSurveyNotActive()
        {
            var now = DateTime.UtcNow;
            var survey = new SurveyModel { Id = 1, Title = "Inactive", StartDate = now.AddDays(-10), EndDate = now.AddDays(-5) };
            _mockSurveyService.Setup(s => s.GetById(1)).ReturnsAsync(survey);

            var result = await _controller.GetSurvey(1);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Survey is not currently active", notFound.Value);
        }

        [Fact]
        public async Task GetUserResponses_ReturnsResponses_WhenUserHasResponses()
        {
            var now = DateTime.UtcNow;
            var survey = new SurveyModel { Id = 1, Title = "Survey", StartDate = now.AddDays(-1), EndDate = now.AddDays(1) };
            var response = new SurveyResponse { Id = 10, SurveyId = 1, RespondentEmail = "user@example.com", SubmissionDate = now, responses = new List<QuestionResponse>() };
            _mockSurveyService.Setup(s => s.GetAll()).ReturnsAsync(new List<SurveyModel> { survey });
            _mockSurveyService.Setup(s => s.GetResponses(1)).ReturnsAsync(new List<SurveyResponse> { response });

            var result = await _controller.GetUserResponses();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<List<object>>(okResult.Value);
            Assert.Single(returned);
        }

        [Fact]
        public async Task GetUserResponses_ReturnsEmpty_WhenUserHasNoResponses()
        {
            var now = DateTime.UtcNow;
            var survey = new SurveyModel { Id = 1, Title = "Survey", StartDate = now.AddDays(-1), EndDate = now.AddDays(1) };
            _mockSurveyService.Setup(s => s.GetAll()).ReturnsAsync(new List<SurveyModel> { survey });
            _mockSurveyService.Setup(s => s.GetResponses(1)).ReturnsAsync(new List<SurveyResponse>());

            var result = await _controller.GetUserResponses();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<List<object>>(okResult.Value);
            Assert.Empty(returned);
        }

        [Fact]
        public async Task GetUserResponses_ReturnsBadRequest_WhenEmailMissing()
        {
            // Remove email claim
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "User")
            }, "mock"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var result = await _controller.GetUserResponses();

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User email not found", badRequest.Value);
        }
    }
} 